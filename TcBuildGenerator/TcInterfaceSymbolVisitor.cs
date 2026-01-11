using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    public class TcInterfaceSymbolVisitor(Action<Diagnostic> errorSink) : SymbolVisitor {
        private readonly Action<Diagnostic> _errorSink = errorSink;
        public List<PluginDefinition> Definitions = [];

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var child in symbol.GetMembers()) {
                child.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol iface)
        {
            if (TcHelper.IsPluginInterface(iface, out var pluginType)) {
                var genericTypeArguments = new List<(string key, string value)>();
                for (var i = 0; i < iface.OriginalDefinition.TypeArguments.Length; i++) {
                    var key = iface.OriginalDefinition.TypeArguments[i].ToString();
                    var value = iface.TypeArguments[i].ToString();
                    genericTypeArguments.Add((key, value));
                }

                var definition = new PluginDefinition(
                    Type: pluginType,
                    Name: iface.Name,
                    GenericTypeArguments: genericTypeArguments.ToImmutableArray(),
                    Methods: new()
                );
                Definitions.Add(definition);

                foreach (var child in iface.GetMembers()) {
                    child.Accept(this);
                }
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            if (TryGetWrapperData(symbol, out var wrapperData)) {
                var methodData = new PluginDefinitionMethodData(
                    MethodName: symbol.Name,
                    Signature: symbol.ToString(),
                    ContainingType: symbol.ContainingType.ToString(),
                    WrapperData: wrapperData
                );

                symbol.GetAttributes();

                var plugin = Definitions.Last();
                plugin.Methods.Add(methodData.Signature, methodData);
            }

            static bool TryGetWrapperData(IMethodSymbol method, out WrapperData propertyData)
            {
                var attribute = method.GetAttributes().FirstOrDefault(_ => _.AttributeClass?.ToString() == "TcPluginBase.TcMethodAttribute");
                if (attribute != null) {
                    propertyData = GetAttributeData(attribute);
                    return true;
                }

                propertyData = default;
                return false;

                static WrapperData GetAttributeData(AttributeData attribute)
                {
                    var wrapperMethods = attribute.ConstructorArguments.Where(_ => _.Type?.ToString() == "string").Select(_ => _.Value).OfType<string>().ToImmutableArray();

                    return new WrapperData(
                        MethodNames: wrapperMethods,
                        Mandatory: (bool?) attribute.NamedArguments.FirstOrDefault(_ => _.Key == "Mandatory").Value.Value == true,
                        BaseImplemented: (bool?) attribute.NamedArguments.FirstOrDefault(_ => _.Key == "BaseImplemented").Value.Value == true
                    );
                }
            }
        }
    }

    public record struct PluginDefinition(
        PluginType Type,
        string Name,
        Dictionary<string, PluginDefinitionMethodData> Methods,
        ImmutableArray<(string key, string value)> GenericTypeArguments
    );

    public record struct PluginDefinitionMethodData(
        string MethodName,
        string Signature,
        string ContainingType,
        WrapperData WrapperData
    );

    public record struct WrapperData(
        ImmutableArray<string> MethodNames,
        bool Mandatory,
        bool BaseImplemented
    );
}
