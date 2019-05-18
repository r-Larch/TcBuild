using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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


        public FsPlugin(Settings pluginSettings) : base(pluginSettings)
        {
            WriteStatusInfo = Convert.ToBoolean(pluginSettings["writeStatusInfo"]);
        }


        #region Mandatory Methods

        // TODO use IAsyncEnumerable when C# 8
        // TODO return new []{ new FindData("..", FileAttributes.Directory) } when path == empty directory
        [CLSCompliant(false)]
        public virtual IEnumerable<FindData> GetFiles(string path)
        {
            return new FindData[0];
        }

        [CLSCompliant(false)]
        public virtual object FindFirst(string path, out FindData findData)
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
        public virtual FileSystemExitCode GetFile(string remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo)
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


        public virtual FileSystemExitCode PutFile(string localName, string remoteName, CopyFlags copyFlags)
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
        public virtual Task<FileSystemExitCode> GetFileAsync(string remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token)
        {
            return Task.FromResult(FileSystemExitCode.NotSupported);
        }

        public virtual Task<FileSystemExitCode> PutFileAsync(string localName, string remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token)
        {
            return Task.FromResult(FileSystemExitCode.NotSupported);
        }

        [CLSCompliant(false)]
        public virtual FileSystemExitCode RenMovFile(string oldName, string newName, bool move, bool overwrite, RemoteInfo remoteInfo)
        {
            return FileSystemExitCode.NotSupported;
        }

        public virtual bool DeleteFile(string fileName)
        {
            return false;
        }

        public virtual bool RemoveDir(string dirName)
        {
            return false;
        }

        public virtual bool MkDir(string dir)
        {
            return false;
        }

        public ExecResult ExecuteFile(TcWindow mainWin, ref string remoteName, string verb)
        {
            if (string.IsNullOrEmpty(verb))
                return ExecResult.Error;
            if (verb.Equals("open", StringComparison.CurrentCultureIgnoreCase))
                return ExecuteOpen(mainWin, ref remoteName);
            if (verb.Equals("properties", StringComparison.CurrentCultureIgnoreCase))
                return ExecuteProperties(mainWin, remoteName);
            if (verb.StartsWith("chmod ", StringComparison.CurrentCultureIgnoreCase))
                return ExecuteCommand(mainWin, ref remoteName, verb.Trim());
            if (verb.StartsWith("quote ", StringComparison.CurrentCultureIgnoreCase))
                return ExecuteCommand(mainWin, ref remoteName, verb.Substring(6).Trim());
            return ExecResult.Yourself;
        }

        public virtual ExecResult ExecuteOpen(TcWindow mainWin, ref string remoteName)
        {
            return ExecResult.Yourself;
        }

        public virtual ExecResult ExecuteProperties(TcWindow mainWin, string remoteName)
        {
            return ExecResult.Yourself;
        }

        public virtual ExecResult ExecuteCommand(TcWindow mainWin, ref string remoteName, string command)
        {
            return ExecResult.Yourself;
        }

        public virtual bool SetAttr(string remoteName, FileAttributes attr)
        {
            return false;
        }

        public virtual bool SetTime(string remoteName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            return false;
        }

        public virtual bool Disconnect(string disconnectRoot)
        {
            return false;
        }

        protected InfoOperation CurrentTcOperation;

        public virtual void StatusInfo(string remoteDir, InfoStartEnd startEnd, InfoOperation infoOperation)
        {
            CurrentTcOperation = startEnd == InfoStartEnd.Start ? infoOperation : InfoOperation.None;
        }

        public virtual ExtractIconResult ExtractCustomIcon(ref string remoteName, ExtractIconFlags extractFlags, out Icon icon)
        {
            icon = null;
            return ExtractIconResult.UseDefault;
        }

        public virtual PreviewBitmapResult GetPreviewBitmap(ref string remoteName, int width, int height, out Bitmap returnedBitmap)
        {
            returnedBitmap = null;
            return PreviewBitmapResult.None;
        }

        public virtual bool GetLocalName(ref string remoteName, int maxLen)
        {
            return false;
        }

        #endregion Optional Methods


        #region Callback Procedures

        protected virtual int ProgressProc(string source, string destination, int percentDone)
        {
            return OnTcPluginEvent(new ProgressEventArgs(PluginNumber, source, destination, percentDone));
        }

        protected virtual void LogProc(LogMsgType msgType, string logText)
        {
            OnTcPluginEvent(new LogEventArgs(PluginNumber, (int) msgType, logText));
        }

        protected virtual bool RequestProc(RequestType requestType, string customTitle, string customText, ref string returnedText, int maxLen)
        {
            var e = new RequestEventArgs(PluginNumber, (int) requestType, customTitle, customText, returnedText, maxLen);
            if (OnTcPluginEvent(e) != 0) {
                returnedText = e.ReturnedText;
                return true;
            }

            return false;
        }

        #endregion Callback Procedures

        public FsPassword Password { get; protected set; }

        public virtual void CreatePassword(int cryptoNumber, int flags)
        {
            if (Password == null) {
                Password = new FsPassword(this, cryptoNumber, flags);
            }
        }


        private IntPtr _mainWindowHandle = IntPtr.Zero;
        public IntPtr MainWindowHandle {
            get => _mainWindowHandle;
            set {
                if (_mainWindowHandle == IntPtr.Zero) {
                    _mainWindowHandle = value;
                }
            }
        }

        // TODO do something with it
        protected void OpenTcPluginHome()
        {
            if (MainWindowHandle != IntPtr.Zero) {
                const int cmOpenNetwork = 2125;
                TcWindow.SendMessage(MainWindowHandle, cmOpenNetwork);
                Thread.Sleep(500);
            }
        }
    }
}
