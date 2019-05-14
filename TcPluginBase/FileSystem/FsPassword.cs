using System;


namespace TcPluginBase.FileSystem {
    [Serializable]
    public class FsPassword : PluginPassword {
        public FsPassword(TcPlugin plugin, int cryptoNumber, int flags)
            : base(plugin, cryptoNumber, flags)
        {
        }

        protected override CryptResult GetCryptResult(int tcCryptResult)
        {
            switch (tcCryptResult) {
                case (int) FileSystemExitCode.OK:
                    return CryptResult.OK;
                case (int) FileSystemExitCode.NotSupported:
                    return CryptResult.Failed;
                case (int) FileSystemExitCode.FileNotFound:
                    return CryptResult.NoMasterPassword;
                case (int) FileSystemExitCode.ReadError:
                    return CryptResult.PasswordNotFound;
                case (int) FileSystemExitCode.WriteError:
                    return CryptResult.WriteError;
                default:
                    return CryptResult.PasswordNotFound;
            }
        }
    }
}
