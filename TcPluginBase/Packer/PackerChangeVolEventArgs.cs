using System;


namespace TcPluginBase.Packer {
    [Serializable]
    public class PackerChangeVolEventArgs : PluginEventArgs {
        public string ArcName { get; }
        public int Mode { get; }


        public PackerChangeVolEventArgs(string arcName, int mode)
        {
            ArcName = arcName;
            Mode = mode;
            this.Result = 0;
        }
    }
}
