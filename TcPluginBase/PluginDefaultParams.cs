using System;
using System.Runtime.InteropServices;


namespace TcPluginBase {
    // This structure is used in SetDefaultParams method (all TC plugins)
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PluginDefaultParams {
        public int Size;
        public Int32 PluginInterfaceVersionLow;
        public Int32 PluginInterfaceVersionHi;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_ANSI)]
        public string DefaultIniName;
    }
}
