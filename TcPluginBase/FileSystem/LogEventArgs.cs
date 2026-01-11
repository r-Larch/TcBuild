namespace TcPluginBase.FileSystem {
    public class LogEventArgs(int pluginNumber, int messageType, string logText) : PluginEventArgs {
        public int PluginNumber { get; private set; } = pluginNumber;
        public int MessageType { get; private set; } = messageType;
        public string LogText { get; private set; } = logText;
    }
}
