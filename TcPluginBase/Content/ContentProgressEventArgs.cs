using System;


namespace TcPluginBase.Content {
    [Serializable]
    public class ContentProgressEventArgs : PluginEventArgs {
        public int NextBlockData { get; private set; }


        public ContentProgressEventArgs(int nextBlockData)
        {
            NextBlockData = nextBlockData;
        }
    }
}
