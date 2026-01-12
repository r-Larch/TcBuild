using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;


[assembly: InternalsVisibleTo("TcBuildGenerator.Tests")]

namespace TcBuildGenerator;

[Generator(LanguageNames.CSharp)]
public class TcBuildSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var descriptors = context.CompilationProvider
            .Select((compilation, token) => TcBuildSourceHelper.FindPluginDescriptor(compilation.GlobalNamespace));

        context.RegisterSourceOutput(descriptors, static (spc, source) => Execute(source, spc));
    }


    private static void Execute(PluginDescriptor pluginDescriptor, SourceProductionContext context)
    {
        var plugin = GetPlugin(pluginDescriptor, context);
        if (plugin == null) {
            return;
        }

        {
            var (wrapperName, wrapperSource) = TcBuildSourceHelper.GenerateWrapperSource(plugin);
            context.AddSource(wrapperName, wrapperSource);
        }

        {
            var (fileName, fileSource) = TcBuildSourceHelper.GenerateAssemblyAttribute(plugin);
            context.AddSource(fileName, fileSource);
        }
    }


    private static PluginData? GetPlugin(PluginDescriptor pluginDescriptor, SourceProductionContext context)
    {
        var plugins = pluginDescriptor.Plugins;

        if (plugins is not { Length: 1 }) {
            // TODO handle plugin fs + content plugin
            if (plugins is { Length: > 1 }) {
                foreach (var location in plugins.SelectMany(_ => _.Locations)) {
                    context.ReportDiagnostic(Diagnostic.Create(Messages.TooManyPluginsError, location));
                }

                return null;
            }

            context.ReportDiagnostic(Diagnostic.Create(Messages.NoPluginError, Location.None));
            return null;
        }

        return plugins[0];
    }
}

public record PluginDescriptor(
    ImmutableArray<PluginData> Plugins
);
