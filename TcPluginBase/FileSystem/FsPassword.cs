using System;


namespace TcPluginBase.FileSystem {
    [Serializable]
    public class FsPassword(TcPlugin plugin, int cryptoNumber, int flags) : PluginPassword(plugin, cryptoNumber, flags) {
        protected override CryptResult GetCryptResult(int tcCryptResult)
        {
            return (FileSystemExitCode) tcCryptResult switch {
                FileSystemExitCode.OK => CryptResult.Ok,
                FileSystemExitCode.NotSupported => CryptResult.Failed,
                FileSystemExitCode.FileNotFound => CryptResult.NoMasterPassword,
                FileSystemExitCode.ReadError => CryptResult.PasswordNotFound,
                FileSystemExitCode.WriteError => CryptResult.WriteError,
                _ => CryptResult.PasswordNotFound
            };
        }
    }
}
