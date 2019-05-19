using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace TcPluginBase.FileSystem {
    [CLSCompliant(false)]
    public interface IFsPlugin {
        #region Optional Async

        [TcMethod("FsFindFirst", "FsFindFirstW", "FsFindNext", "FsFindNext", "FsFindClose")]
        IEnumerable<FindData> GetFiles(RemotePath path);

        [TcMethod("FsGetFile", "FsGetFileW")]
        Task<FileSystemExitCode> GetFileAsync(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token);

        [TcMethod("FsPutFile", "FsPutFileW")]
        Task<FileSystemExitCode> PutFileAsync(string localName, RemotePath remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token);

        #endregion

        #region Mandatory Methods

        [TcMethod("FsFindFirst", "FsFindFirstW", Mandatory = true)]
        object FindFirst(RemotePath path, out FindData findData);

        [TcMethod("FsFindNext", "FsFindNextW", Mandatory = true)]
        bool FindNext(ref object o, out FindData findData);

        [TcMethod("FsFindClose", Mandatory = true)]
        int FindClose(object o);

        #endregion Mandatory Methods

        #region Optional Methods

        string RootName { get; }
        FsBackgroundFlags BackgroundFlags { get; }
        bool IsTempFilePanel { get; }

        [TcMethod("FsGetFile", "FsGetFileW")]
        FileSystemExitCode GetFile(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo);

        [TcMethod("FsPutFile", "FsPutFileW")]
        FileSystemExitCode PutFile(string localName, RemotePath remoteName, CopyFlags copyFlags);

        [TcMethod("FsRenMovFile", "FsRenMovFileW")]
        FileSystemExitCode RenMovFile(RemotePath oldName, RemotePath newName, bool move, bool overwrite, RemoteInfo remoteInfo);

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

        [TcMethod("FsDisconnect", "FsDisconnectW", BaseImplemented = true)]
        bool Disconnect(RemotePath disconnectRoot);

        [TcMethod("FsStatusInfo", "FsStatusInfoW")]
        void StatusInfo(string remoteDir, InfoStartEnd startEnd, InfoOperation infoOperation);

        [TcMethod("FsExtractCustomIcon", "FsExtractCustomIconW")]
        ExtractIconResult ExtractCustomIcon(RemotePath remoteName, ExtractIconFlags extractFlags);

        [TcMethod("FsGetPreviewBitmap", "FsGetPreviewBitmapW")]
        PreviewBitmapResult GetPreviewBitmap(RemotePath remoteName, int width, int height);

        [TcMethod("FsGetLocalName", "FsGetLocalNameW")]
        string GetLocalName(RemotePath remoteName, int maxLen);

        // FsContent... methods - are determined in IContentPlugin interface

        #endregion Optional Methods
    }
}
