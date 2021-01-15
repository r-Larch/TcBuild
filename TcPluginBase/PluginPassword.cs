using System;


namespace TcPluginBase {
    // Class, used to store passwords in the TC secure password store,
    // retrieve them back, or copy them to a new store.
    // It's a parent class for FsPassword and PackerPassword classes.
    [Serializable]
    public abstract class PluginPassword {
        private readonly TcPlugin _plugin;
        private readonly int _cryptoNumber;
        private readonly CryptFlags _flags;

        protected PluginPassword(TcPlugin plugin, int cryptoNumber, int flags)
        {
            _plugin = plugin;
            _cryptoNumber = cryptoNumber;
            _flags = (CryptFlags) flags;
        }

        public bool TcMasterPasswordDefined => _flags.HasFlag(CryptFlags.MasterPassSet);

        // Convert result returned by TC to CryptResult. Must be overridden in derived classes.
        protected abstract CryptResult GetCryptResult(int tcCryptResult);

        #region Public Methods

        /// <summary>
        /// Save password to password store.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public CryptResult Save(string store, string password)
        {
            return Crypt(CryptMode.SavePassword, store, ref password);
        }

        /// <summary>
        /// Load password from password store.
        /// and automatically prompt the user for MasterPassword.
        /// </summary>
        public CryptResult Load(string store, ref string password)
        {
            password = string.Empty;
            return Crypt(CryptMode.LoadPassword, store, ref password);
        }

        /// <summary>
        /// Load password from password store only if master password has already been entered.
        /// Use <see cref="Load"/> to automatically prompt the user for MasterPassword.
        /// </summary>
        public CryptResult LoadNoUI(string store, ref string password)
        {
            password = string.Empty;
            return Crypt(CryptMode.LoadPasswordNoUI, store, ref password);
        }

        /// <summary>
        /// Copy password to new store.
        /// </summary>
        public CryptResult Copy(string sourceStore, string targetStore)
        {
            return Crypt(CryptMode.CopyPassword, sourceStore, ref targetStore);
        }

        /// <summary>
        /// Copy password to new store and delete the source password.
        /// </summary>
        public CryptResult Move(string sourceStore, string targetStore)
        {
            return Crypt(CryptMode.MovePassword, sourceStore, ref targetStore);
        }

        /// <summary>
        /// Delete the password of the given store.
        /// </summary>
        public CryptResult Delete(string store)
        {
            var password = string.Empty;
            return Crypt(CryptMode.DeletePassword, store, ref password);
        }

        public int GetCryptoNumber()
        {
            return _cryptoNumber;
        }

        public int GetFlags()
        {
            return (int) _flags;
        }

        #endregion Public Methods

        private CryptResult Crypt(CryptMode mode, string storeName, ref string password)
        {
            var e = new CryptEventArgs(_plugin.PluginNumber, _cryptoNumber, (int) mode, storeName, password);
            var result = GetCryptResult(_plugin.OnTcPluginEvent(e));
            if (result == CryptResult.Ok) {
                password = e.Password;
            }

            return result;
        }


        [Flags]
        private enum CryptFlags {
            None = 0,
            MasterPassSet = 1 // The user already has a master password defined.
        }

        private enum CryptMode {
            SavePassword = 1,
            LoadPassword,
            LoadPasswordNoUI,
            CopyPassword,
            MovePassword,
            DeletePassword
        }
    }
}
