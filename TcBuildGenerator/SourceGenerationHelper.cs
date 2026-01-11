using System.Collections.Immutable;


namespace TcBuildGenerator;

public class SourceGenerationHelper {
    public static (string FileName, string FileSource) GenerateAssemblyAttribute(PluginData plugin)
    {
        var fileName = $"{plugin.Type}.attributes.g.cs";
        var fileSource = $$"""
            using System;

            [assembly:{{TcInfos.TcPluginDefinitionAttribute}}("{{plugin.Type}}", typeof(global::{{plugin.ClassFullName}}))]

            internal class {{TcInfos.TcPluginDefinitionAttribute}} : Attribute {
                public {{TcInfos.TcPluginDefinitionAttribute}}(string pluginType, Type type) {
                    PluginType = pluginType;
                    Type = type;
                }
                public string PluginType { get; set; }
                public Type Type { get; set; }
            }
            """;

        return (fileName, fileSource);
    }

    public static (string FileName, string FileSource) GenerateWrapperSource(PluginData plugin)
    {
        var fileName = $"{plugin.Type}.wrapper.g.cs";
        var wrapperSource = LoadWrapperSource(plugin);
        var fileSource = ModifySource(plugin, wrapperSource);

        return (fileName, fileSource);
    }


    internal static string ModifySource(PluginData plugin, string fileSource)
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
        methodsToRemove.AddRange([
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
            "FsContentStopGetValueW"
        ]);

        // 5.remove excluded methods
        foreach (var method in methodsToRemove) {
            fileSource = fileSource.Replace($"[UnmanagedCallersOnly(EntryPoint = \"{method}\")]", "");
        }

        fileSource = fileSource.Replace("PluginClassPlaceholder", $"global::{plugin.ClassFullName}");

        foreach (var (key, value) in plugin.Definition.GenericTypeArguments) {
            fileSource = fileSource.Replace(key, $"global::{value}");
        }

        return fileSource;
    }


    internal static string LoadWrapperSource(PluginData plugin)
    {
        return plugin.Type switch {
            PluginType.FileSystem => GetManifestResource("TcBuildGenerator.Wrapper.FsWrapper.cs"),
            PluginType.Lister => GetManifestResource("TcBuildGenerator.Wrapper.WlxWrapper.cs"),
            PluginType.Packer => GetManifestResource("TcBuildGenerator.Wrapper.WcxWrapper.cs"),
            PluginType.QuickSearch => GetManifestResource("TcBuildGenerator.Wrapper.QSWrapper.cs"),
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
