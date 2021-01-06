using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    [Generator]
    public class TcBuildSourceGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context)
        {
        }


        public void Execute(GeneratorExecutionContext context)
        {
            var visitor = new TcPluginSymbolVisitor(context.ReportDiagnostic);
            visitor.Visit(context.Compilation.GlobalNamespace);
            var plugins = visitor.Plugins;

            foreach (var plugin in plugins) {
                // TODO handle plugin fs + content plugin

                var fileName = $"{plugin.Type}.generated.cs";
                var fileSource = GenerateWrapperSource(plugin);
                fileSource = ModifySource(plugin, fileSource);
                context.AddSource(fileName, fileSource);
            }
        }


        private static string ModifySource(PluginData plugin, string fileSource)
        {
            // 1. take all Mandatory and BaseImplemented methods
            var methods = plugin.Definition.Methods.Values
                .Where(_ => _.WrapperData.Mandatory || _.WrapperData.BaseImplemented)
                .ToList();

            // 2. add all plugin implemented methods
            foreach (var method in plugin.ImplementedMethods) {
                var definition = plugin.Definition.Methods[method.Signature];
                methods.Add(definition);
            }

            // 3. get methods to implement in the wrapper
            var wrapperMethods = methods.SelectMany(_ => _.WrapperData.MethodNames).ToImmutableHashSet();

            // 4. find all methods to be excluded
            var methodsToRemove = plugin.Definition.Methods.Values
                .SelectMany(_ => _.WrapperData.MethodNames).Distinct()
                .Where(_ => !wrapperMethods.Contains(_))
                .ToList();

            // temp fix: remove Fs Content
            methodsToRemove.AddRange(new[] {
                "FsContentGetDefaultSortOrder",
                "FsContentGetDefaultView",
                "FsContentGetDefaultViewW",
                "FsContentGetSupportedField",
                "FsContentGetSupportedFieldFlags",
                "FsContentGetValue",
                "FsContentGetValueW",
                "FsContentPluginUnloading",
                "FsContentSetValue",
                "FsContentSetValueW",
                "FsContentStopGetValue",
                "FsContentStopGetValueW",
            });

            // 5.remove excluded methods
            foreach (var method in methodsToRemove) {
                fileSource = fileSource.Replace($"[UnmanagedCallersOnly(EntryPoint = \"{method}\")]", "");
            }

            fileSource = fileSource.Replace("PluginClassPlaceholder", $"global::{plugin.ClassFullName}");

            return fileSource;
        }


        private static string GenerateWrapperSource(PluginData plugin)
        {
            return plugin.Type switch {
                PluginType.FsPlugin => GetManifestResource("TcBuildGenerator.Wrapper.FsWrapper.cs"),
                _ => throw new ArgumentOutOfRangeException(nameof(plugin.Type), $"PluginType: '{plugin.Type}' not implemented!")
            };
        }


        private static string GetManifestResource(string name)
        {
            using var stream = typeof(TcBuildSourceGenerator).Assembly.GetManifestResourceStream(name);
            if (stream == null) throw new FileNotFoundException($"ManifestResource: '{name}'");
            using var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}
