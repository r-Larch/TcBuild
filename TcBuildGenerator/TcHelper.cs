using System.Collections.Generic;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    public class TcHelper {
        private static readonly Dictionary<string, PluginType> Plugins = new() {
            {"TcPluginBase.FileSystem.IFsPlugin", PluginType.FileSystem},
            {"TcPluginBase.Lister.IListerPlugin", PluginType.Lister},
            {"TcPluginBase.Packer.IPackerPlugin", PluginType.Packer},
            {"TcPluginBase.QuickSearch.IQuickSearchPlugin", PluginType.QuickSearch},
        };

        public static bool IsPluginClass(INamedTypeSymbol clazz, out PluginType pluginType, out INamedTypeSymbol pluginInterface)
        {
            if (
                clazz.TypeKind == TypeKind.Class &&
                !clazz.IsAbstract && clazz.ContainingAssembly.Name != "TcPluginBase"
            ) {
                foreach (var iface in clazz.AllInterfaces) {
                    if (Plugins.TryGetValue(iface.ToString(), out pluginType)) {
                        pluginInterface = iface;
                        return true;
                    }
                }
            }

            pluginType = default;
            pluginInterface = default!;
            return false;
        }

        public static bool IsPluginInterface(INamedTypeSymbol iface, out PluginType pluginType)
        {
            pluginType = default;
            return (
                iface.TypeKind == TypeKind.Interface &&
                Plugins.TryGetValue(iface.ToString(), out pluginType)
            );
        }
    }
}
