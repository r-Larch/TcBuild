using System;


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

        public int PluginNumber { get; }
        public int CryptoNumber { get; }
        public int Mode { get; }
        public string StoreName { get; }
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
}
