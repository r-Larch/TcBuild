using System;


namespace OY.TotalCommander.TcPluginBase.FileSystem {
    [Serializable]
    public class LogEventArgs : PluginEventArgs {
        #region Properties

        public int PluginNumber { get; private set; }
        public int MessageType { get; private set; }
        public string LogText { get; private set; }

        #endregion Properties

        public LogEventArgs(int pluginNumber, int messageType, string logText)
        {
            PluginNumber = pluginNumber;
            MessageType = messageType;
            LogText = logText;
        }
    }
}
