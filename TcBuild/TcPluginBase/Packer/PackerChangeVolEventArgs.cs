namespace TcPluginBase.Packer {
    public class PackerChangeVolEventArgs(string arcName, int mode) : PluginEventArgs {
        public string ArcName { get; } = arcName;
        public int Mode { get; } = mode;
    }
}
