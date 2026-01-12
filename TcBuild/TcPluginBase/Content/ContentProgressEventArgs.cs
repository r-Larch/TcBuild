namespace TcPluginBase.Content {
    public class ContentProgressEventArgs(int nextBlockData) : PluginEventArgs {
        public int NextBlockData { get; private set; } = nextBlockData;
    }
}
