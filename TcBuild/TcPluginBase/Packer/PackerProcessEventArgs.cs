namespace TcPluginBase.Packer {
    public class PackerProcessEventArgs(string fileName, int size) : PluginEventArgs {
        public string FileName { get; private set; } = fileName;
        public int Size { get; private set; } = size;
    }
}
