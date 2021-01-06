using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    public class TcPluginSymbolVisitor : SymbolVisitor {
        private readonly Action<Diagnostic> _errorSink;
        public List<PluginData> Plugins = new List<PluginData>();
        public Dictionary<PluginType, PluginDefinition> Definitions = new Dictionary<PluginType, PluginDefinition>();

        public TcPluginSymbolVisitor(Action<Diagnostic> errorSink)
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
            if (TcHelper.IsPluginClass(clazz, out var pluginType, out var pluginInterface)) {
                var definition = GetPluginDefinition(pluginType, pluginInterface);

                var plugin = new PluginData {
                    Type = pluginType,
                    ClassFullName = clazz.ToString(),
                    Namespace = clazz.ContainingNamespace.Name,
                    ImplementedMethods = new List<MethodData>(),
                    Definition = definition,
                };
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
                var visitor = new TcInterfaceSymbolVisitor(_errorSink);
                visitor.Visit(pluginInterface);
                definition = visitor.Definitions.Single();

                Definitions.Add(pluginType, definition);
            }

            return definition;
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            if (
                symbol.IsOverride &&
                symbol.IsDefinition
            ) {
                var methodData = new MethodData {
                    MethodName = symbol.Name,
                    Signature = $"{symbol.ReturnType} {symbol.Name}({string.Join(", ", symbol.Parameters)})",
                    ContainingType = symbol.ContainingType.ToString(),
                };

                var plugin = Plugins.Last();
                if (plugin.Definition.Methods.ContainsKey(methodData.Signature)) {
                    plugin.ImplementedMethods.Add(methodData);
                }
            }
        }
    }


    public struct PluginData {
        public PluginType Type { get; set; }
        public string Namespace { get; set; }
        public string ClassFullName { get; set; }
        public List<MethodData> ImplementedMethods { get; set; }
        public PluginDefinition Definition { get; set; }
    }

    public struct MethodData {
        public string MethodName { get; set; }
        public string Signature { get; set; }
        public string ContainingType { get; set; }
    }
}
