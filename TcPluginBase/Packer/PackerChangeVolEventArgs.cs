using System;


namespace OY.TotalCommander.TcPluginBase.Packer {
    [Serializable]
    public class PackerChangeVolEventArgs : PluginEventArgs {
        #region Properties

        public string ArcName { get; private set; }
        public int Mode { get; private set; }

        #endregion Properties

        public PackerChangeVolEventArgs(string arcName, int mode)
        {
            ArcName = arcName;
            Mode = mode;
            this.Result = 0;
        }
    }
}
