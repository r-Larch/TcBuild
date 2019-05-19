namespace TcPluginBase {
    public enum CryptResult {
        /// <summary>Success.</summary>
        OK = 0,

        /// <summary>Password not found in password store.</summary>
        PasswordNotFound,

        /// <summary>No master password entered yet.</summary>
        NoMasterPassword,

        /// <summary>Encrypt/Decrypt failed.</summary>
        Failed,

        /// <summary>Could not write password to password store.</summary>
        WriteError
    }
}
