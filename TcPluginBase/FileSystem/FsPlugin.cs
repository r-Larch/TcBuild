using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TcPluginBase.Content;


namespace TcPluginBase.FileSystem {
    public abstract class FsPlugin : TcPlugin, IFsPlugin {
        public ContentPlugin? ContentPlugin { get; set; }

        public virtual string? RootName { get; set; }
        public override string TraceTitle => Title;
        public FsBackgroundFlags BackgroundFlags { get; set; } = FsBackgroundFlags.Download | FsBackgroundFlags.Upload;


        public bool WriteStatusInfo { get; set; }


        /// <summary>
        /// Gets set by FsSetCryptCallback
        /// You can use it to save and load passwords
        /// </summary>
        public FsPassword? Password { get; set; }
        public FsPrompt Prompt { get; set; }


        protected FsPlugin(IConfiguration pluginSettings) : base(pluginSettings)
        {
            WriteStatusInfo = Convert.ToBoolean(pluginSettings["writeStatusInfo"]);
            Prompt = new FsPrompt(this);
        }


        #region Mandatory Methods

        // TODO return new []{ new FindData("..", FileAttributes.Directory) } when path == empty directory
        public virtual IEnumerable<FindData> GetFiles(RemotePath path)
        {
            return null!;
        }

        public virtual IAsyncEnumerable<FindData> GetFilesAsync(RemotePath path)
        {
            return null!;
        }


        /// <exception cref="NoMoreFilesException"></exception>
        public virtual object? FindFirst(RemotePath path, out FindData? findData)
        {
            var syncFiles = GetFiles(path);
            if (syncFiles != null!) {
                var enumerator = syncFiles.GetEnumerator();
                if (enumerator.MoveNext()) {
                    findData = enumerator.Current;
                    return enumerator;
                }

                // empty list
                findData = null;
                return null;
            }
            else {
                using var exec = new ThreadKeeper();
                var (result, enumerator) = exec.ExecAsync(Run);

                async Task<(FindData? result, object? enumerator)> Run(CancellationToken token)
                {
                    var enumerable = GetFilesAsync(path);
                    if (enumerable != null!) {
                        var enumerator = enumerable.GetAsyncEnumerator(token);
                        if (await enumerator.MoveNextAsync()) {
                            var result = enumerator.Current;
                            return (result, enumerator);
                        }
                    }

                    // empty list
                    return (null, null);
                }

                findData = result;
                return enumerator;
            }
        }

        public virtual bool FindNext(ref object o, [NotNullWhen(true)] out FindData? findData)
        {
            if (o is IEnumerator<FindData?> fsEnum) {
                if (fsEnum.MoveNext()) {
                    var current = fsEnum.Current;
                    if (current != null) {
                        findData = current;
                        return true;
                    }
                }
            }
            else if (o is IAsyncEnumerator<FindData?> fsAsyncEnum) {
                using var exec = new ThreadKeeper();
                var result = exec.ExecAsync(async token => {
                    if (await fsAsyncEnum.MoveNextAsync()) {
                        var current = fsAsyncEnum.Current;
                        if (current != null) {
                            return current;
                        }
                    }

                    return null;
                });
                findData = result;
                return findData != null;
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

        public virtual GetFileResult GetFile(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo)
        {
            try {
                // My ThreadKeeper class is needed here because calls to ProgressProc must be made from this thread and not from some random async one.
                using (var exec = new ThreadKeeper()) {
                    void Progress(int percentDone)
                    {
                        exec.RunInMainThread(() => {
                            if (ProgressProc(remoteName, localName, percentDone)) {
                                exec.Cancel();
                            }
                        });
                    }

                    var ret = exec.ExecAsync(asyncFunc: (token) => GetFileAsync(remoteName, localName, copyFlags, remoteInfo, Progress, token));

                    return ret;
                }
            }
            catch (TaskCanceledException) {
                return GetFileResult.UserAbort;
            }
            catch (OperationCanceledException) {
                return GetFileResult.UserAbort;
            }
            catch (AggregateException e) {
                if (HasCanceledException(e)) {
                    return GetFileResult.UserAbort;
                }

                throw;
            }
        }


        public virtual PutFileResult PutFile(string localName, RemotePath remoteName, CopyFlags copyFlags)
        {
            try {
                // My ThreadKeeper class is needed here because calls to ProgressProc must be made from this thread and not from some random async one.
                using (var exec = new ThreadKeeper()) {
                    void Progress(int percentDone)
                    {
                        exec.RunInMainThread(() => {
                            if (ProgressProc(localName, remoteName, percentDone)) {
                                exec.Cancel();
                            }
                        });
                    }

                    var ret = exec.ExecAsync(asyncFunc: (token) => PutFileAsync(localName, remoteName, copyFlags, Progress, token));

                    return ret;
                }
            }
            catch (TaskCanceledException) {
                return PutFileResult.UserAbort;
            }
            catch (OperationCanceledException) {
                return PutFileResult.UserAbort;
            }
            catch (AggregateException e) {
                if (HasCanceledException(e)) {
                    return PutFileResult.UserAbort;
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

        public virtual Task<GetFileResult> GetFileAsync(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token)
        {
            return Task.FromResult(GetFileResult.NotSupported);
        }

        public virtual Task<PutFileResult> PutFileAsync(string localName, RemotePath remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token)
        {
            return Task.FromResult(PutFileResult.NotSupported);
        }

        public virtual RenMovFileResult RenMovFile(RemotePath oldName, RemotePath newName, bool move, bool overwrite, RemoteInfo remoteInfo)
        {
            return RenMovFileResult.NotSupported;
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
            return cmd switch {
                "open" => ExecuteOpen(mainWin, remoteName),
                "properties" => ExecuteProperties(mainWin, remoteName),
                "chmod" => ExecuteCommand(mainWin, remoteName, verb.Trim()),
                "quote" => ExecuteCommand(mainWin, remoteName, verb.Substring(6).Trim()),
                _ => ExecResult.Yourself
            };
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

        public virtual string? GetLocalName(RemotePath remoteName, int maxLen)
        {
            return null;
        }

        public virtual bool IsTempFilePanel()
        {
            return false;
        }

        #endregion Optional Methods


        #region Callback Procedures

        /// <summary>
        /// <see cref="ProgressProc"/> is a callback function, which the plugin can call to show copy progress.
        /// The address of this callback function is received through the FsInit() function when the plugin is loaded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You should call this function at least twice in the copy functions FsGetFile(), FsPutFile() and FsRenMovFile(), at the beginning and at the end.
        /// If you can't determine the progress, call it with 0% at the beginning and 100% at the end.
        /// </para>
        /// <para>
        /// New in 1.3: During the FsFindFirst/FsFindNext/FsFindClose loop, the plugin may now call the ProgressProc to make a progress dialog appear.
        /// This is useful for very slow connections. Don't call ProgressProc for fast connections!
        /// The progress dialog will only be shown for normal dir changes, not for compound operations like get/put.
        /// The calls to ProgressProc will also be ignored during the first 5 seconds,
        /// so the user isn't bothered with a progress dialog on every dir change.
        /// </para>
        /// </remarks>
        /// <param name="source">Name of the source file being copied. Depending on the direction of the operation (Get, Put), this may be a local file name of a name in the plugin file system.</param>
        /// <param name="destination">Name to which the file is copied.</param>
        /// <param name="percentDone">Percentage of THIS file being copied. Total Commander automatically shows a second percent bar if possible when multiple files are copied.</param>
        /// <returns>Total Commander returns <c>true</c> if the user wants to abort copying, and <c>false</c> if the operation can continue.</returns>
        protected virtual bool ProgressProc(string source, string destination, int percentDone)
        {
            return OnTcPluginEvent(new ProgressEventArgs(PluginNumber, source, destination, percentDone)) == 1;
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

        internal virtual bool RequestProc(RequestType requestType, string? customTitle, string? customText, ref string? returnedText, int maxLen)
        {
            var e = new RequestEventArgs(PluginNumber, (int) requestType, customTitle, customText, returnedText, maxLen);
            if (OnTcPluginEvent(e) != 0) {
                returnedText = e.ReturnedText;
                return true;
            }

            return false;
        }

        #endregion Callback Procedures

        protected ConcurrentDictionary<RemotePath, FsConnection> Connections = new();

        protected FsConnection GetConnection(RemotePath connectionRoot)
        {
            return Connections.GetOrAdd(connectionRoot, root => {
                var connection = new FsConnection(connectionRoot, this);
                return connection;
            });
        }
    }
}
