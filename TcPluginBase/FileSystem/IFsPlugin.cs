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
        IEnumerable<FindData> GetFiles(string path);

        [TcMethod("FsGetFile", "FsGetFileW")]
        Task<FileSystemExitCode> GetFileAsync(string remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token);

        [TcMethod("FsPutFile", "FsPutFileW")]
        Task<FileSystemExitCode> PutFileAsync(string localName, string remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token);

        #endregion

        #region Mandatory Methods

        [TcMethod("FsFindFirst", "FsFindFirstW", Mandatory = true)]
        object FindFirst(string path, out FindData findData);

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
        FileSystemExitCode GetFile(string remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo);

        [TcMethod("FsPutFile", "FsPutFileW")]
        FileSystemExitCode PutFile(string localName, string remoteName, CopyFlags copyFlags);

        [TcMethod("FsRenMovFile", "FsRenMovFileW")]
        FileSystemExitCode RenMovFile(string oldName, string newName, bool move, bool overwrite, RemoteInfo remoteInfo);

        [TcMethod("FsDeleteFile", "FsDeleteFileW")]
        bool DeleteFile(string fileName);

        [TcMethod("FsRemoveDir", "FsRemoveDirW")]
        bool RemoveDir(string dirName);

        [TcMethod("FsMkDir", "FsMkDirW")]
        bool MkDir(string dir);

        [TcMethod("FsExecuteFile", "FsExecuteFileW")]
        ExecResult ExecuteOpen(TcWindow mainWin, ref string remoteName);

        [TcMethod("FsExecuteFile", "FsExecuteFileW")]
        ExecResult ExecuteProperties(TcWindow mainWin, string remoteName);

        [TcMethod("FsExecuteFile", "FsExecuteFileW")]
        ExecResult ExecuteCommand(TcWindow mainWin, ref string remoteName, string command);

        [TcMethod("FsSetAttr", "FsSetAttrW")]
        bool SetAttr(string remoteName, FileAttributes attr);

        [TcMethod("FsSetTime", "FsSetTimeW")]
        bool SetTime(string remoteName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime);

        [TcMethod("FsDisconnect", "FsDisconnectW")]
        bool Disconnect(string disconnectRoot);

        [TcMethod("FsStatusInfo", "FsStatusInfoW")]
        void StatusInfo(string remoteDir, InfoStartEnd startEnd, InfoOperation infoOperation);

        [TcMethod("FsExtractCustomIcon", "FsExtractCustomIconW")]
        ExtractIconResult ExtractCustomIcon(ref string remoteName, ExtractIconFlags extractFlags, out Icon icon);

        [TcMethod("FsGetPreviewBitmap", "FsGetPreviewBitmapW")]
        PreviewBitmapResult GetPreviewBitmap(ref string remoteName, int width, int height, out Bitmap returnedBitmap);

        [TcMethod("FsGetLocalName", "FsGetLocalNameW")]
        bool GetLocalName(ref string remoteName, int maxLen);

        // FsContent... methods - are determined in IContentPlugin interface

        #endregion Optional Methods
    }
}
