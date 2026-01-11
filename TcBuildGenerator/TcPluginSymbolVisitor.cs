using System.Collections.Immutable;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator;

public class TcPluginSymbolVisitor : SymbolVisitor {
    public List<PluginData> Plugins = [];
    public Dictionary<PluginType, PluginDefinition> Definitions = new();

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var child in symbol.GetMembers()) {
            child.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol clazz)
    {
        if (TcHelper.IsPluginClass(clazz, out var pluginType, out var pluginInterface)) {
            var definition = GetPluginDefinition(pluginType, pluginInterface);

            var plugin = new PluginData(
                Locations: clazz.Locations,
                Type: pluginType,
                ClassFullName: clazz.ToString(),
                Namespace: clazz.ContainingNamespace.Name,
                ImplementedMethods: [],
                Definition: definition
            );
            Plugins.Add(plugin);

            // visit all methods of all base classes excluding the abstract XxPlugin base!

            var clazzType = clazz;
            while (clazzType != null && clazzType.ContainingAssembly.Name != "TcPluginBase") {
                foreach (var child in clazzType.GetMembers()) {
                    child.Accept(this);
                }

                clazzType = clazzType.BaseType;
            }
        }
    }

    private PluginDefinition GetPluginDefinition(PluginType pluginType, INamedTypeSymbol pluginInterface)
    {
        if (!Definitions.TryGetValue(pluginType, out var definition)) {
            var visitor = new TcInterfaceSymbolVisitor();
            visitor.Visit(pluginInterface);
            definition = visitor.Definitions.Single();

            Definitions.Add(pluginType, definition);
        }

        return definition;
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        if (symbol is { IsOverride: true, IsDefinition: true }) {
            var plugin = Plugins.Last();

            if (TcHelper.TryGetPluginMethod(plugin, symbol, out var interfaceMember)) {
                var methodData = new MethodData(
                    MethodName: symbol.Name,
                    Signature: interfaceMember.ToString(),
                    ContainingType: symbol.ContainingType.ToString()
                );

                if (plugin.Definition.Methods.ContainsKey(methodData.Signature)) {
                    plugin.ImplementedMethods.Add(methodData);
                }
            }
        }
    }
}

public record PluginData(
    ImmutableArray<Location> Locations,
    PluginType Type,
    string Namespace,
    string ClassFullName,
    List<MethodData> ImplementedMethods,
    PluginDefinition Definition
);

public record struct MethodData(
    string MethodName,
    string Signature,
    string ContainingType
);
