using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    public class TcInterfaceSymbolVisitor : SymbolVisitor {
        private readonly Action<Diagnostic> _errorSink;
        public List<PluginDefinition> Definitions = new List<PluginDefinition>();

        public TcInterfaceSymbolVisitor(Action<Diagnostic> errorSink)
        {
            _errorSink = errorSink;
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var child in symbol.GetMembers()) {
                child.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol clazz)
        {
            if (TcHelper.IsPluginInterface(clazz, out var pluginType)) {
                var definition = new PluginDefinition {
                    Type = pluginType,
                    Name = clazz.Name,
                    Methods = new Dictionary<string, PluginDefinitionMethodData>()
                };
                Definitions.Add(definition);

                foreach (var child in clazz.GetMembers()) {
                    child.Accept(this);
                }
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            if (symbol.IsDefinition && TryGetWrapperData(symbol, out var wrapperData)) {
                var methodData = new PluginDefinitionMethodData {
                    MethodName = symbol.Name,
                    Signature = $"{symbol.ReturnType} {symbol.Name}({string.Join(", ", symbol.Parameters)})",
                    ContainingType = symbol.ContainingType.ToString(),
                    WrapperData = wrapperData,
                };

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
                    var wrapperMethods = attribute.ConstructorArguments.Where(_ => _.Type?.ToString() == "string").Select(_ => _.Value).OfType<string>().ToArray();

                    return new WrapperData {
                        MethodNames = wrapperMethods,
                        Mandatory = (bool?) attribute.NamedArguments.FirstOrDefault(_ => _.Key == "Mandatory").Value.Value == true,
                        BaseImplemented = (bool?) attribute.NamedArguments.FirstOrDefault(_ => _.Key == "BaseImplemented").Value.Value == true,
                    };
                }
            }
        }
    }

    public struct PluginDefinition {
        public PluginType Type { get; set; }
        public string Name { get; set; }
        public Dictionary<string, PluginDefinitionMethodData> Methods { get; set; }
    }

    public struct PluginDefinitionMethodData {
        public string MethodName { get; set; }
        public string Signature { get; set; }
        public string ContainingType { get; set; }
        public WrapperData WrapperData { get; set; }
    }

    public struct WrapperData {
        public string[] MethodNames { get; set; }
        public bool Mandatory { get; set; }
        public bool BaseImplemented { get; set; }
    }
}
