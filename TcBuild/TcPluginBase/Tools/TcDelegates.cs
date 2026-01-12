using System;
using System.Runtime.InteropServices;


namespace TcPluginBase.Tools {
    #region FS Plugin Delegates

    [Serializable]
    public delegate int ProgressCallback(int pluginNumber, [MarshalAs(UnmanagedType.LPStr)] string sourceName, [MarshalAs(UnmanagedType.LPStr)] string targetName, int percentDone);

    [Serializable]
    public delegate int ProgressCallbackW(int pluginNumber, [MarshalAs(UnmanagedType.LPWStr)] string sourceName, [MarshalAs(UnmanagedType.LPWStr)] string targetName, int percentDone);

    [Serializable]
    public delegate void LogCallback(int pluginNumber, int messageType, [MarshalAs(UnmanagedType.LPStr)] string logText);

    [Serializable]
    public delegate void LogCallbackW(int pluginNumber, int messageType, [MarshalAs(UnmanagedType.LPWStr)] string logText);

    [Serializable]
    [return: MarshalAs(UnmanagedType.Bool)]
    public delegate bool RequestCallback(int pluginNumber, int requestType, [MarshalAs(UnmanagedType.LPStr)] string? customTitle, [MarshalAs(UnmanagedType.LPStr)] string? customText, IntPtr returnedText, int maxLen);

    [Serializable]
    [return: MarshalAs(UnmanagedType.Bool)]
    public delegate bool RequestCallbackW(int pluginNumber, int requestType, [MarshalAs(UnmanagedType.LPWStr)] string? customTitle, [MarshalAs(UnmanagedType.LPWStr)] string? customText, IntPtr returnedText, int maxLen);

    [Serializable]
    public delegate int FsCryptCallback(int pluginNumber, int cryptoNumber, int mode, [MarshalAs(UnmanagedType.LPStr)] string connectionName, IntPtr password, int maxLen);

    [Serializable]
    public delegate int FsCryptCallbackW(int pluginNumber, int cryptoNumber, int mode, [MarshalAs(UnmanagedType.LPWStr)] string connectionName, IntPtr password, int maxLen);

    #endregion FS Plugin Delegates

    #region Content Delegates

    [Serializable]
    public delegate int ContentProgressCallback(int nextBlockData);

    #endregion Content Delegates

    #region Packer Plugin Delegates

    [Serializable]
    public delegate int ProcessDataCallback([MarshalAs(UnmanagedType.LPStr)] string fileName, int size);

    [Serializable]
    public delegate int ProcessDataCallbackW([MarshalAs(UnmanagedType.LPWStr)] string fileName, int size);

    [Serializable]
    public delegate int ChangeVolCallback([MarshalAs(UnmanagedType.LPStr)] string arcName, int mode);

    [Serializable]
    public delegate int ChangeVolCallbackW([MarshalAs(UnmanagedType.LPWStr)] string arcName, int mode);

    [Serializable]
    public delegate int PkCryptCallback(int cryptoNumber, int mode, [MarshalAs(UnmanagedType.LPStr)] string archiveName, IntPtr password, int maxLen);

    [Serializable]
    public delegate int PkCryptCallbackW(int cryptoNumber, int mode, [MarshalAs(UnmanagedType.LPWStr)] string archiveName, IntPtr password, int maxLen);

    #endregion Packer Plugin Delegates

    #region Lister Delegates

    // There are no delegates for Lister plugins on plugin level.

    #endregion Lister Delegates
}
