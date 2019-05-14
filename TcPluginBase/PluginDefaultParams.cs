using System;
using System.Runtime.InteropServices;


namespace TcPluginBase {
    // This structure is used in SetDefaultParams method (all TC plugins)
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PluginDefaultParams {
        public int size;
        public Int32 pluginInterfaceVersionLow;
        public Int32 pluginInterfaceVersionHi;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_ANSI)]
        public string defaultIniName;
    }
}
