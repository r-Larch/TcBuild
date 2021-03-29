using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    public class TcHelper {
        private static readonly Dictionary<string, PluginType> Plugins = new() {
            {"TcPluginBase.FileSystem.IFsPlugin", PluginType.FileSystem},
            {"TcPluginBase.Lister.IListerPlugin", PluginType.Lister},
            {"TcPluginBase.Packer.IPackerPlugin", PluginType.Packer},
            {"TcPluginBase.QuickSearch.IQuickSearchPlugin", PluginType.QuickSearch},
        };

        public static bool IsPluginClass(in INamedTypeSymbol clazz, out PluginType pluginType, out INamedTypeSymbol pluginInterface)
        {
            if (
                clazz.TypeKind == TypeKind.Class &&
                !clazz.IsAbstract && clazz.ContainingAssembly.Name != "TcPluginBase"
            ) {
                foreach (var iface in clazz.AllInterfaces) {
                    if (IsPluginInterface(iface, out pluginType)) {
                        pluginInterface = iface;
                        return true;
                    }
                }
            }

            pluginType = default;
            pluginInterface = default!;
            return false;
        }

        public static bool IsPluginInterface(in INamedTypeSymbol iface, out PluginType pluginType)
        {
            pluginType = default;

            var ifaceName = $"{iface.ContainingNamespace}.{iface.Name}";

            return (
                iface.TypeKind == TypeKind.Interface &&
                Plugins.TryGetValue(ifaceName, out pluginType)
            );
        }

        public static bool TryGetPluginMethod(PluginData plugin, in IMethodSymbol symbol, [NotNullWhen(true)] out ISymbol? implementedInterfaceMember)
        {
            implementedInterfaceMember = OverriddenMethods(symbol)
                .SelectMany(m => ExplicitOrImplicitPluginInterfaceImplementations(m, plugin))
                .FirstOrDefault();

            return implementedInterfaceMember != null;

            static IEnumerable<IMethodSymbol> OverriddenMethods(IMethodSymbol? m)
            {
                while (m != null) {
                    yield return m;
                    m = m.OverriddenMethod;
                }
            }

            static IEnumerable<ISymbol> ExplicitOrImplicitPluginInterfaceImplementations(ISymbol symbol, PluginData plugin)
            {
                if (symbol.Kind != SymbolKind.Method && symbol.Kind != SymbolKind.Property && symbol.Kind != SymbolKind.Event) {
                    return Enumerable.Empty<ISymbol>();
                }

                var containingType = symbol.ContainingType;
                var query =
                    from iface in containingType.AllInterfaces
                    where IsPluginInterface(iface, out var pluginType) && pluginType == plugin.Type
                    from interfaceMember in iface.GetMembers()
                    let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
                    where symbol.Equals(impl, SymbolEqualityComparer.Default)
                    select interfaceMember;

                return query;
            }
        }
    }
}
