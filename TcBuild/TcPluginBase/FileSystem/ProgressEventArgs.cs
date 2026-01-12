namespace TcPluginBase.FileSystem {
    public class ProgressEventArgs(int pluginNumber, string sourceName, string targetName, int percentDone) : PluginEventArgs {
        public int PluginNumber { get; private set; } = pluginNumber;
        public string SourceName { get; private set; } = sourceName;
        public string TargetName { get; private set; } = targetName;
        public int PercentDone { get; private set; } = percentDone;
    }
}
