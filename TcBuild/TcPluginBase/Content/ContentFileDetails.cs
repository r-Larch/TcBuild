using System;
using System.Runtime.InteropServices;


namespace TcPluginBase.Content {
    // Used as parameter type for CompareFiles method
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct ContentFileDetails {
        public long fileSize1;
        public long fileSize2;
        public long fileTime1;
        public long fileTime2;
        public int attr1;
        public int attr2;
    }
}
