namespace TcPluginBase.Packer {
    public class PackerPassword(TcPlugin plugin, int cryptoNumber, int flags) : PluginPassword(plugin, cryptoNumber, flags) {
        protected override CryptResult GetCryptResult(int tcCryptResult)
        {
            return (PackerResult) tcCryptResult switch {
                PackerResult.OK => CryptResult.Ok,
                PackerResult.ErrorCreate => CryptResult.Failed,
                PackerResult.NoFiles => CryptResult.NoMasterPassword,
                PackerResult.ErrorRead => CryptResult.PasswordNotFound,
                PackerResult.ErrorWrite => CryptResult.WriteError,
                _ => CryptResult.PasswordNotFound
            };
        }
    }
}
