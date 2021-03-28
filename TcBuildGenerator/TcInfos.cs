using System.Collections.Generic;


namespace TcBuildGenerator {
    public static class TcInfos {
        public const string TcPluginDefinitionAttribute = "TcPluginDefinitionAttribute";

        public static readonly Dictionary<PluginType, string> PluginExtensions = new() {
            {PluginType.Content, "wdx"},
            {PluginType.FileSystem, "wfx"},
            {PluginType.Lister, "wlx"},
            {PluginType.Packer, "wcx"},
            {PluginType.QuickSearch, "dll"}
        };
    }

    public enum PluginType {
        FileSystem,
        Content,
        Lister,
        Packer,
        QuickSearch,
    }
}
