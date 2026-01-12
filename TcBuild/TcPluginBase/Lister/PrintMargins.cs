using System;
using System.Runtime.InteropServices;


namespace TcPluginBase.Lister {
    /// <summary>
    /// Used as parameter type for Print method
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PrintMargins {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
