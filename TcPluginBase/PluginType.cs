using System;


namespace TcPluginBase {
    [Serializable]
    public enum PluginType {
        Content,
        FileSystem,
        Lister,
        Packer,
        QuickSearch,
        Unknown
    }
}
