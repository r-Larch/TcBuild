using System;


namespace OY.TotalCommander.TcPluginBase {
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
