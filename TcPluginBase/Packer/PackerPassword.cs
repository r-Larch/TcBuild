namespace TcPluginBase.Packer {
    public class PackerPassword : PluginPassword {
        public PackerPassword(TcPlugin plugin, int cryptoNumber, int flags) : base(plugin, cryptoNumber, flags)
        {
        }

        protected override CryptResult GetCryptResult(int tcCryptResult)
        {
            switch (tcCryptResult) {
                case (int) PackerResult.OK:
                    return CryptResult.OK;
                case (int) PackerResult.ErrorCreate:
                    return CryptResult.Failed;
                case (int) PackerResult.NoFiles:
                    return CryptResult.NoMasterPassword;
                case (int) PackerResult.ErrorRead:
                    return CryptResult.PasswordNotFound;
                case (int) PackerResult.ErrorWrite:
                    return CryptResult.WriteError;
                default:
                    return CryptResult.PasswordNotFound;
            }
        }
    }
}
