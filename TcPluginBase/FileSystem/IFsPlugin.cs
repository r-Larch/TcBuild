using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace TcPluginBase.FileSystem {
    public interface IFsPlugin {
        #region Optional Async

        [TcMethod("FsFindFirst", "FsFindFirstW", "FsFindNext", "FsFindNext", "FsFindClose")]
        IEnumerable<FindData> GetFiles(RemotePath path);

        [TcMethod("FsGetFile", "FsGetFileW")]
        Task<GetFileResult> GetFileAsync(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token);

        [TcMethod("FsPutFile", "FsPutFileW")]
        Task<PutFileResult> PutFileAsync(string localName, RemotePath remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token);

        #endregion

        #region Mandatory Methods

        [TcMethod("FsFindFirst", "FsFindFirstW", Mandatory = true)]
        object? FindFirst(RemotePath path, out FindData? findData);

        [TcMethod("FsFindNext", "FsFindNextW", Mandatory = true)]
        bool FindNext(ref object o, [NotNullWhen(true)] out FindData? findData);

        [TcMethod("FsFindClose", Mandatory = true)]
        int FindClose(object o);

        #endregion Mandatory Methods

        #region Optional Methods

        /// <summary>
        /// FsGetDefRootName is called only when the plugin is installed. It asks the plugin for the default root name which should appear in the Network Neighborhood. This root name is NOT part of the path passed to the plugin when Wincmd accesses the plugin file system! The root will always be "\", and all subpaths will be built from the directory names returned by the plugin.
        /// Example: The root name may be "Linux file system" for a plugin which accesses Linux drives.If this function isn't implemented, Wincmd will suggest the name of the DLL (without extension .DLL) as the plugin root. This function is called directly after loading the plugin (when the user installs it), FsInit() is NOT called when installing the plugin.
        /// </summary>
        string? RootName { get; }


        FsBackgroundFlags BackgroundFlags { get; }


        /// <summary>
        /// FsGetFile is called to transfer a file from the plugin's file system to the normal file system (drive letters or UNC).
        /// </summary>
        /// <remarks>
        /// Total Commander usually calls this function twice:
        /// <list type="bullet">
        ///   <item> once with CopyFlags==0 or CopyFlags==<see cref="CopyFlags.Move"/>. If the local file exists and resume is supported, return <see cref="FileSystemExitCode.ExistsResumeAllowed"/>. If resume isn't allowed, return <see cref="FileSystemExitCode.FileExists"/> </item>
        ///   <item> a second time with <see cref="CopyFlags.Resume"/> or <see cref="CopyFlags.Overwrite"/>, depending on the user's choice. </item>
        ///   <item> The resume option is only offered to the user if <see cref="FileSystemExitCode.ExistsResumeAllowed"/> was returned by the first call. </item>
        ///   <item> <see cref="CopyFlags.ExistsSameCase"/> and <see cref="CopyFlags.ExistsDifferentCase"/> are NEVER passed to this function, because the plugin can easily determine whether a local file exists or not. </item>
        ///   <item> <see cref="CopyFlags.Move"/> is set, the plugin needs to delete the remote file after a successful download. </item>
        /// </list>
        /// While copying the file, but at least at the beginning and the end, call ProgressProc to show the copy progress and allow the user to abort the operation.
        /// </remarks>
        /// <param name="remoteName">Name of the file to be retrieved, with full path. The name always starts with a backslash, then the names returned by FsFindFirst/FsFindNext separated by backslashes.</param>
        /// <param name="localName">Local file name with full path, either with a drive letter or UNC path (\\Server\Share\filename). The plugin may change the NAME/EXTENSION of the file (e.g. when file conversion is done), but not the path! use <see cref="GetFileResult.OkNameChanged"/> to do that. </param>
        /// <param name="copyFlags">
        /// Can be a combination of the following three flags:
        /// <see cref="CopyFlags.Overwrite"/>: If set, overwrite any existing file without asking. If not set, simply fail copying.
        /// <see cref="CopyFlags.Resume"/>: Resume an aborted or failed transfer.
        /// <see cref="CopyFlags.Move"/>: The plugin needs to delete the remote file after uploading.
        /// See above for important notes!
        /// </param>
        /// <param name="remoteInfo">This parameter contains information about the remote file which was previously retrieved via FsFindFirst/FsFindNext: The size, date/time, and attributes of the remote file. May be useful to copy the attributes with the file, and for displaying a progress dialog.</param>
        [TcMethod("FsGetFile", "FsGetFileW")]
        GetFileResult GetFile(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo);

        /// <summary>
        /// FsPutFile is called to transfer a file from the normal file system (drive letters or UNC) to the plugin's file system.
        /// </summary>
        /// <remarks>
        /// Total Commander usually calls this function twice, with the following parameters in CopyFlags:
        /// <list type="bullet">
        ///   <item> once with neither <see cref="CopyFlags.Resume"/> nor <see cref="CopyFlags.Overwrite"/> set. If the remote file exists and resume is supported, return <see cref="PutFileResult.ExistsResumeAllowed"/>. If resume isn't allowed, return <see cref="PutFileResult.FileExists"/> </item>
        ///   <item> a second time with <see cref="CopyFlags.Resume"/> or <see cref="CopyFlags.Overwrite"/>, depending on the user's choice. The resume option is only offered to the user if <see cref="PutFileResult.ExistsResumeAllowed"/> was returned by the first call. </item>
        ///   <item> The flags <see cref="CopyFlags.ExistsSameCase"/> or <see cref="CopyFlags.ExistsDifferentCase"/> are added to CopyFlags when the remote file exists and needs to be overwritten. This is a hint to the plugin to allow optimizations: Depending on the plugin type, it may be very slow to check the server for every single file when uploading. </item>
        ///   <item> If the flag <see cref="CopyFlags.Move"/> is set, the plugin needs to delete the local file after a successful upload. </item>
        /// </list>
        /// While copying the file, but at least at the beginning and the end, call ProgressProc to show the copy progress and allow the user to abort the operation.
        /// </remarks>
        /// <param name="localName">Local file name with full path, either with a drive letter or UNC path (\\Server\Share\filename). This file needs to be uploaded to the plugin's file system.</param>
        /// <param name="remoteName">Name of the remote file, with full path. The name always starts with a backslash, then the names returned by FsFindFirst/FsFindNext separated by backslashes. The plugin may change the NAME/EXTENSION of the file (e.g. when file conversion is done), but not the path! Return OkNameChanged(..) to do that. </param>
        /// <param name="copyFlags">Can be a combination of <see cref="CopyFlags"/></param>
        /// <returns></returns>
        [TcMethod("FsPutFile", "FsPutFileW")]
        PutFileResult PutFile(string localName, RemotePath remoteName, CopyFlags copyFlags);

        /// <summary>
        /// FsRenMovFile is called to transfer (copy or move) a file within the plugin's file system.
        /// </summary>
        /// <remarks>
        /// Total Commander usually calls this function twice:
        /// <list type="bullet">
        ///   <item> once with OverWrite==false. If the remote file exists, return FS_FILE_EXISTS. If it doesn't exist, try to copy the file, and return an appropriate error code. </item>
        ///   <item> a second time with OverWrite==true, if the user chose to overwrite the file. While copying the file, but at least at the beginning and the end, call ProgressProc to show the copy progress and allow the user to abort the operation. </item>
        /// </list>
        /// </remarks>
        /// <param name="oldName">Name of the remote source file, with full path. The name always starts with a backslash, then the names returned by FsFindFirst/FsFindNext separated by backslashes.</param>
        /// <param name="newName">Name of the remote destination file, with full path. The name always starts with a backslash, then the names returned by FsFindFirst/FsFindNext separated by backslashes.</param>
        /// <param name="move">If true, the file needs to be moved to the new location and name. Many file systems allow to rename/move a file without actually moving any of its data, only the pointer to it.</param>
        /// <param name="overwrite">Tells the function whether it should overwrite the target file or not. See notes below on how this parameter is used.</param>
        /// <param name="remoteInfo">An instance of class RemoteInfo which contains the parameters of the file being renamed/moved (not of the target file!). In TC 5.51, the fields are set as follows for directories: SizeLow=0, SizeHigh=0xFFFFFFFF.</param>
        [TcMethod("FsRenMovFile", "FsRenMovFileW")]
        RenMovFileResult RenMovFile(RemotePath oldName, RemotePath newName, bool move, bool overwrite, RemoteInfo remoteInfo);

        [TcMethod("FsDeleteFile", "FsDeleteFileW")]
        bool DeleteFile(RemotePath fileName);

        [TcMethod("FsRemoveDir", "FsRemoveDirW")]
        bool RemoveDir(RemotePath dirName);

        [TcMethod("FsMkDir", "FsMkDirW")]
        bool MkDir(RemotePath dir);

        [TcMethod("FsExecuteFile", "FsExecuteFileW")]
        ExecResult ExecuteOpen(TcWindow mainWin, RemotePath remoteName);

        [TcMethod("FsExecuteFile", "FsExecuteFileW")]
        ExecResult ExecuteProperties(TcWindow mainWin, RemotePath remoteName);

        [TcMethod("FsExecuteFile", "FsExecuteFileW")]
        ExecResult ExecuteCommand(TcWindow mainWin, RemotePath remoteName, string command);

        [TcMethod("FsSetAttr", "FsSetAttrW")]
        bool SetAttr(RemotePath remoteName, FileAttributes attr);

        [TcMethod("FsSetTime", "FsSetTimeW")]
        bool SetTime(RemotePath remoteName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime);

        /// <summary>
        /// FsDisconnect is called when the user presses the Disconnect button in the FTP connections toolbar. This toolbar is only shown if MSGTYPE_CONNECT is passed to LogProc().
        /// </summary>
        /// <remarks>
        /// To get calls to this function,
        /// the plugin MUST call <see cref="FsPlugin.GetConnection"/> and <see cref="FsConnection.Connect"/> with the root of the file system which has been connected. This file system root will be passed to <see cref="FsPlugin.Disconnect"/> when the user presses the Disconnect button, so the plugin knows which connection to close.
        /// Do NOT call <see cref="FsPlugin.GetConnection"/> if your plugin does not require connect/disconnect!
        /// Examples:
        /// <list type="bullet">
        ///   <item> FTP requires connect/disconnect.Connect can be done automatically when the user enters a subdir, disconnect when the user clicks the Disconnect button. </item>
        ///   <item> Access to local file systems (e.g.Linux EXT2) does not require connect/disconnect, so don't call LogProc with the parameter MSGTYPE_CONNECT. </item>
        /// </list>
        /// </remarks>
        /// <param name="disconnectRoot"></param>
        /// <returns></returns>
        [TcMethod("FsDisconnect", "FsDisconnectW", BaseImplemented = true)]
        bool Disconnect(RemotePath disconnectRoot);

        [TcMethod("FsStatusInfo", "FsStatusInfoW")]
        void StatusInfo(string remoteDir, InfoStartEnd startEnd, InfoOperation infoOperation);

        [TcMethod("FsExtractCustomIcon", "FsExtractCustomIconW")]
        ExtractIconResult ExtractCustomIcon(RemotePath remoteName, ExtractIconFlags extractFlags);

        [TcMethod("FsGetPreviewBitmap", "FsGetPreviewBitmapW")]
        PreviewBitmapResult GetPreviewBitmap(RemotePath remoteName, int width, int height);

        /// <summary>
        /// FsLinksToLocalFiles must not be implemented unless your plugin is a temporary file panel plugin! Temporary file panels just hold links to files on the local file system.
        /// </summary>
        /// <remarks>
        /// If your plugin is a temporary panel plugin, the following functions MUST be thread-safe (can be called from background transfer manager):
        /// <list type="bullet">
        ///   <item> FsLinksToLocalFiles </item>
        ///   <item> FsFindFirst </item>
        ///   <item> FsFindNext </item>
        ///   <item> FsFindClose </item>
        ///   <item> FsGetLocalName </item>
        /// </list>
        /// This means that when uploading subdirectories from your plugin to FTP in the background, Total Commander will call these functions in a background thread. If the user continues to work in the foreground, calls to FsFindFirst and FsFindNext may be occuring at the same time! Therefore it's very important to use the search handle to keep temporary information about the search.
        /// FsStatusInfo will NOT be called from the background thread!
        /// </remarks>
        [TcMethod("FsLinksToLocalFiles")]
        bool IsTempFilePanel();

        /// <summary>
        /// GetLocalName must not be implemented unless your plugin is a temporary file panel plugin! Temporary file panels just hold links to files on the local file system.
        /// </summary>
        /// <remarks>
        /// If your plugin is a temporary panel plugin, the following functions MUST be thread-safe (can be called from background transfer manager):
        /// <list type="bollet">
        /// <item> GetLocalName </item>
        /// <item> FindFirst </item>
        /// <item> FindNext </item>
        /// <item> FindClose </item>
        /// </list>
        /// This means that when uploading subdirectories from your plugin to FTP in the background, Total Commander will call these functions in a background thread.If the user continues to work in the foreground, calls to FsFindFirst and FsFindNext may be occuring at the same time! Therefore it's very important to use the search handle to keep temporary information about the search.
        /// FsStatusInfo will NOT be called from the background thread!
        /// </remarks>
        /// <param name="remoteName">Full path to the file name in the plugin namespace, e.g. \somedir\file.ext</param>
        /// <param name="maxLen">Maximum number of characters you can return in RemoteName, including the final 0.</param>
        /// <returns>Return the path of the file on the local file system, e.g. c:\windows\file.ext or null if it does not point to a local file.</returns>
        [TcMethod("FsGetLocalName", "FsGetLocalNameW")]
        string? GetLocalName(RemotePath remoteName, int maxLen);

        // FsContent... methods - are determined in IContentPlugin interface

        #endregion Optional Methods
    }
}
