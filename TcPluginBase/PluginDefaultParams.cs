using System;
using System.Runtime.InteropServices;


namespace TcPluginBase {
    // This structure is used in SetDefaultParams method (all TC plugins)
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PluginDefaultParams {
        /// <summary>
        /// The size of the structure, in bytes. Later revisions of the plugin interface may add more structure members, and will adjust this size field accordingly.
        /// </summary>
        public int Size;
        /// <summary>
        /// Low value of plugin interface version. This is the value after the comma, multiplied by 100! Example. For plugin interface version 1.3, the low DWORD is 30 and the high DWORD is 1.
        /// </summary>
        public Int32 PluginInterfaceVersionLow;
        /// <summary>
        /// High value of plugin interface version.
        /// </summary>
        public Int32 PluginInterfaceVersionHi;

        /// <summary>
        /// Suggested location+name of the ini file where the plugin could store its data.
        /// This is a fully qualified path+file name, and will be in the same directory as the wincmd.ini.
        /// It's recommended to store the plugin data in this file or at least in this directory,
        /// because the plugin directory or the Windows directory may not be writable!
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_ANSI)]
        public string DefaultIniName;
    }
}
