using System.Collections.Generic;
using Microsoft.CodeAnalysis;


namespace TcBuildGenerator {
    public enum PluginType {
        FsPlugin,
    }

    public class TcHelper {
        private static readonly Dictionary<string, PluginType> Plugins = new Dictionary<string, PluginType> {
            {"TcPluginBase.FileSystem.IFsPlugin", PluginType.FsPlugin}
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
