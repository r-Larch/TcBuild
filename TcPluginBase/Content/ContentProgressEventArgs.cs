using System;


namespace OY.TotalCommander.TcPluginBase.Content {
    [Serializable]
    public class ContentProgressEventArgs : PluginEventArgs {
        #region Properties

        public int NextBlockData { get; private set; }

        #endregion Properties

        public ContentProgressEventArgs(int nextBlockData)
        {
            NextBlockData = nextBlockData;
        }
    }
}
