using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcPluginBase.Content;


namespace TcPluginBase.FileSystem {
    public class FsPlugin : TcPlugin, IFsPlugin {
        public ContentPlugin ContentPlugin { get; set; }

        public virtual string RootName { get; set; }
        public override string TraceTitle => Title;
        public FsBackgroundFlags BackgroundFlags { get; set; } = FsBackgroundFlags.Download | FsBackgroundFlags.Upload;
        public bool IsTempFilePanel { get; set; }
        public bool WriteStatusInfo { get; set; }


        /// <summary>
        /// Gets set by FsSetCryptCallback
        /// You can use it to save and load passwords
        /// </summary>
        public FsPassword Password { get; set; }
        public FsPrompt Prompt { get; set; }


        public FsPlugin(Settings pluginSettings) : base(pluginSettings)
        {
            WriteStatusInfo = Convert.ToBoolean(pluginSettings["writeStatusInfo"]);
            Prompt = new FsPrompt(this);
        }


        #region Mandatory Methods

        // TODO use IAsyncEnumerable when C# 8
        // TODO return new []{ new FindData("..", FileAttributes.Directory) } when path == empty directory
        [CLSCompliant(false)]
        public virtual IEnumerable<FindData> GetFiles(RemotePath path)
        {
            return new FindData[0];
        }

        [CLSCompliant(false)]
        public virtual object FindFirst(RemotePath path, out FindData findData)
        {
            var enumerable = GetFiles(path);
            if (enumerable != null) {
                var enumerator = enumerable.GetEnumerator();
                if (enumerator.MoveNext()) {
                    findData = enumerator.Current;
                    return enumerator;
                }
            }

            // empty list
            findData = null;
            return null;
        }

        [CLSCompliant(false)]
        public virtual bool FindNext(ref object o, out FindData findData)
        {
            if (o is IEnumerator<FindData> fsEnum) {
                if (fsEnum.MoveNext()) {
                    var current = fsEnum.Current;
                    if (current != null) {
                        findData = current;
                        return true;
                    }
                }
            }

            // end of sequence
            findData = null;
            return false;
        }


        public virtual int FindClose(object o)
        {
            return 0;
        }

        #endregion Mandatory Methods

        #region Optional Methods

        [CLSCompliant(false)]
        public virtual FileSystemExitCode GetFile(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo)
        {
            try {
                // My ThreadKeeper class is needed here because calls to ProgressProc must be made from this thread and not from some random async one.
                using (var exec = new ThreadKeeper()) {
                    void Progress(int percentDone)
                    {
                        exec.RunInMainThread(() => {
                            if (ProgressProc(remoteName, localName, percentDone) == 1) {
                                exec.Cancel();
                            }
                        });
                    }

                    var ret = exec.ExecAsync(asyncFunc: (token) => GetFileAsync(remoteName, localName, copyFlags, remoteInfo, Progress, token));

                    return ret;
                }
            }
            catch (TaskCanceledException) {
                return FileSystemExitCode.UserAbort;
            }
            catch (OperationCanceledException) {
                return FileSystemExitCode.UserAbort;
            }
            catch (AggregateException e) {
                if (HasCanceledException(e)) {
                    return FileSystemExitCode.UserAbort;
                }

                throw;
            }
        }


        public virtual FileSystemExitCode PutFile(string localName, RemotePath remoteName, CopyFlags copyFlags)
        {
            try {
                // My ThreadKeeper class is needed here because calls to ProgressProc must be made from this thread and not from some random async one.
                using (var exec = new ThreadKeeper()) {
                    void Progress(int percentDone)
                    {
                        exec.RunInMainThread(() => {
                            if (ProgressProc(localName, remoteName, percentDone) == 1) {
                                exec.Cancel();
                            }
                        });
                    }

                    var ret = exec.ExecAsync(asyncFunc: (token) => PutFileAsync(localName, remoteName, copyFlags, Progress, token));

                    return ret;
                }
            }
            catch (TaskCanceledException) {
                return FileSystemExitCode.UserAbort;
            }
            catch (OperationCanceledException) {
                return FileSystemExitCode.UserAbort;
            }
            catch (AggregateException e) {
                if (HasCanceledException(e)) {
                    return FileSystemExitCode.UserAbort;
                }

                throw;
            }
        }


        private static bool HasCanceledException(AggregateException e)
        {
            foreach (var exception in e.InnerExceptions) {
                switch (exception) {
                    case AggregateException agg:
                        return HasCanceledException(agg);
                    case TaskCanceledException _:
                    case OperationCanceledException _:
                        return true;
                }
            }

            return false;
        }

        [CLSCompliant(false)]
        public virtual Task<FileSystemExitCode> GetFileAsync(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token)
        {
            return Task.FromResult(FileSystemExitCode.NotSupported);
        }

        public virtual Task<FileSystemExitCode> PutFileAsync(string localName, RemotePath remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token)
        {
            return Task.FromResult(FileSystemExitCode.NotSupported);
        }

        [CLSCompliant(false)]
        public virtual FileSystemExitCode RenMovFile(RemotePath oldName, RemotePath newName, bool move, bool overwrite, RemoteInfo remoteInfo)
        {
            return FileSystemExitCode.NotSupported;
        }

        public virtual bool DeleteFile(RemotePath fileName)
        {
            return false;
        }

        public virtual bool RemoveDir(RemotePath dirName)
        {
            return false;
        }

        public virtual bool MkDir(RemotePath dir)
        {
            return false;
        }

        public ExecResult ExecuteFile(TcWindow mainWin, RemotePath remoteName, string verb)
        {
            if (string.IsNullOrEmpty(verb)) {
                return ExecResult.Error;
            }

            var cmd = verb.Split(' ')[0].ToLower();
            switch (cmd) {
                case "open":
                    return ExecuteOpen(mainWin, remoteName);
                case "properties":
                    return ExecuteProperties(mainWin, remoteName);
                case "chmod":
                    return ExecuteCommand(mainWin, remoteName, verb.Trim());
                case "quote":
                    return ExecuteCommand(mainWin, remoteName, verb.Substring(6).Trim());
                default:
                    return ExecResult.Yourself;
            }
        }

        public virtual ExecResult ExecuteOpen(TcWindow mainWin, RemotePath remoteName)
        {
            return ExecResult.Yourself;
        }

        public virtual ExecResult ExecuteProperties(TcWindow mainWin, RemotePath remoteName)
        {
            return ExecResult.Yourself;
        }

        public virtual ExecResult ExecuteCommand(TcWindow mainWin, RemotePath remoteName, string command)
        {
            return ExecResult.Yourself;
        }

        public virtual bool SetAttr(RemotePath remoteName, FileAttributes attr)
        {
            return false;
        }

        public virtual bool SetTime(RemotePath remoteName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            return false;
        }

        public virtual bool Disconnect(RemotePath disconnectRoot)
        {
            if (Connections.TryRemove(disconnectRoot, out var connection)) {
                connection.Disconnect();
                return true;
            }

            return false;
        }

        protected InfoOperation CurrentTcOperation;

        public virtual void StatusInfo(string remoteDir, InfoStartEnd startEnd, InfoOperation infoOperation)
        {
            CurrentTcOperation = startEnd == InfoStartEnd.Start ? infoOperation : InfoOperation.None;
        }

        /// <summary>
        /// ExtractCustomIcon is called when a file/directory is displayed in the file list.
        /// It can be used to specify a custom icon for that file/directory.
        /// This function is new in version 1.1. It requires Total Commander >=5.51, but is ignored by older versions.
        /// </summary>
        /// <param name="remoteName">This is the full path to the file or directory whose icon is to be retrieved</param>
        /// <param name="extractFlags">Flags for the extract operation. A combination of <see cref="ExtractIconFlags"/></param>
        /// <returns><see cref="ExtractIconResult"/> with the extracted Icon, caching infos or the path to a local file where TC can extract the Icon on its own.</returns>
        public virtual ExtractIconResult ExtractCustomIcon(RemotePath remoteName, ExtractIconFlags extractFlags)
        {
            return ExtractIconResult.UseDefault;
        }

        /// <summary>
        /// GetPreviewBitmap is called when a file/directory is displayed in thumbnail view.
        /// It can be used to return a custom bitmap for that file/directory.
        /// This function is new in version 1.4. It requires Total Commander >=7.0, but is ignored by older versions.
        /// </summary>
        /// <param name="remoteName">This is the full path to the file or directory whose bitmap is to be retrieved.</param>
        /// <param name="width">The maximum dimensions of the preview bitmap. If your image is smaller, or has a different side ratio, then you need to return an image which is smaller than these dimensions! See notes below!</param>
        /// <param name="height">The maximum dimensions of the preview bitmap. If your image is smaller, or has a different side ratio, then you need to return an image which is smaller than these dimensions! See notes below!</param>
        /// <returns><see cref="PreviewBitmapResult"/> with the extracted Bitmap, caching infos or the path to a local file where TC can extract the bitmap on its own.</returns>
        public virtual PreviewBitmapResult GetPreviewBitmap(RemotePath remoteName, int width, int height)
        {
            return PreviewBitmapResult.None;
        }

        /// <summary>
        /// GetLocalName must not be implemented unless your plugin is a temporary file panel plugin! Temporary file panels just hold links to files on the local file system.
        /// </summary>
        /// <remarks>
        /// If your plugin is a temporary panel plugin, the following functions MUST be thread-safe (can be called from background transfer manager):
        /// - GetLocalName
        /// - FindFirst
        /// - FindNext
        /// - FindClose
        ///     This means that when uploading subdirectories from your plugin to FTP in the background, Total Commander will call these functions in a background thread.If the user continues to work in the foreground, calls to FsFindFirst and FsFindNext may be occuring at the same time! Therefore it's very important to use the search handle to keep temporary information about the search.
        ///     FsStatusInfo will NOT be called from the background thread!
        /// </remarks>
        /// <param name="remoteName">Full path to the file name in the plugin namespace, e.g. \somedir\file.ext</param>
        /// <param name="maxLen">Maximum number of characters you can return in RemoteName, including the final 0.</param>
        /// <returns>Return the path of the file on the local file system, e.g. c:\windows\file.ext or null if it does not point to a local file.</returns>
        public virtual string GetLocalName(RemotePath remoteName, int maxLen)
        {
            return null;
        }

        #endregion Optional Methods


        #region Callback Procedures

        protected virtual int ProgressProc(string source, string destination, int percentDone)
        {
            return OnTcPluginEvent(new ProgressEventArgs(PluginNumber, source, destination, percentDone));
        }


        /// <summary>
        /// param logText: String which should be logged.
        /// When MsgType==MSGTYPE_CONNECT, the string MUST have a specific format:
        /// "CONNECT" followed by a single whitespace, then the root of the file system which was connected, without trailing backslash. Example: CONNECT \Filesystem
        /// When MsgType==MSGTYPE_TRANSFERCOMPLETE, this parameter should contain both the source and target names, separated by an arrow " -> ", e.g.
        /// Download complete: \Filesystem\dir1\file1.txt -> c:\localdir\file1.txt
        /// </summary>
        internal virtual void LogProc(LogMsgType msgType, string logText)
        {
            OnTcPluginEvent(new LogEventArgs(PluginNumber, (int) msgType, logText));
        }

        internal virtual bool RequestProc(RequestType requestType, string customTitle, string customText, ref string returnedText, int maxLen)
        {
            var e = new RequestEventArgs(PluginNumber, (int) requestType, customTitle, customText, returnedText, maxLen);
            if (OnTcPluginEvent(e) != 0) {
                returnedText = e.ReturnedText;
                return true;
            }

            return false;
        }

        #endregion Callback Procedures

        protected ConcurrentDictionary<RemotePath, FsConnection> Connections = new ConcurrentDictionary<RemotePath, FsConnection>();

        protected FsConnection GetConnection(RemotePath connectionRoot)
        {
            return Connections.GetOrAdd(connectionRoot, root => {
                var connection = new FsConnection(connectionRoot, this);
                return connection;
            });
        }
    }
}
