using System;


namespace TcPluginBase {
    public class PluginEventArgs : EventArgs {
        public int Result { get; set; } = 0;
    }

    public class CryptEventArgs(int pluginNumber, int cryptoNumber, int mode, string storeName, string password) : PluginEventArgs {
        public int PluginNumber { get; } = pluginNumber;
        public int CryptoNumber { get; } = cryptoNumber;
        public int Mode { get; } = mode;
        public string StoreName { get; } = storeName;
        public string Password { get; set; } = password;
    }
}
