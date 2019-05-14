using System;


namespace TcPluginBase.Packer {
    [Serializable]
    public class PackerProcessEventArgs : PluginEventArgs {
        #region Properties

        public string FileName { get; private set; }
        public int Size { get; private set; }

        #endregion Properties

        public PackerProcessEventArgs(string fileName, int size)
        {
            FileName = fileName;
            Size = size;
            Result = 0;
        }
    }
}
