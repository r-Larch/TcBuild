using System;
using System.Diagnostics;


namespace TcPluginBase {
    [Serializable]
    public class PluginEventArgs : EventArgs {
        public int Result { get; set; }

        public PluginEventArgs()
        {
            Result = 0;
        }
    }

    [Serializable]
    public class CryptEventArgs : PluginEventArgs {
        #region Properties

        public int PluginNumber { get; private set; }
        public int CryptoNumber { get; private set; }
        public int Mode { get; private set; }
        public string StoreName { get; private set; }
        public string Password { get; set; }

        #endregion Properties

        public CryptEventArgs(int pluginNumber, int cryptoNumber, int mode, string storeName, string password)
        {
            PluginNumber = pluginNumber;
            CryptoNumber = cryptoNumber;
            Mode = mode;
            StoreName = storeName;
            Password = password;
        }
    }

    [Serializable]
    public class TraceEventArgs : EventArgs {
        #region Properties

        public TraceLevel Level { get; private set; }
        public string Text { get; private set; }

        #endregion Properties

        public TraceEventArgs(TraceLevel level, string text)
        {
            Level = level;
            Text = text;
        }
    }
}
