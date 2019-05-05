using System;
using System.Runtime.InteropServices;


namespace OY.TotalCommander.TcPluginBase.Lister {
    // Used as parameter type for Print method
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PrintMargins {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
