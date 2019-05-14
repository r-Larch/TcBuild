using System;
using System.Drawing;
using System.IO;
using System.Threading;
using TcPluginBase.Content;


namespace TcPluginBase.FileSystem {
    public class FsPlugin : TcPlugin, IFsPlugin {
        public ContentPlugin ContentPlugin { get; set; }

        public virtual string RootName { get; set; }
        public override string TraceTitle => Title;
        public virtual FsBackgroundFlags BackgroundFlags { get; set; } = FsBackgroundFlags.None;
        public virtual bool IsTempFilePanel { get; set; } = false;

        public bool WriteStatusInfo { get; }


        public FsPlugin(Settings pluginSettings) : base(pluginSettings)
        {
            PluginNumber = -1;
            //SetPluginFolder("iconFolder", Path.Combine(PluginFolder, "img"));
            WriteStatusInfo = Convert.ToBoolean(pluginSettings["writeStatusInfo"]);
        }

        #region IFsPlugin Members

        #region Mandatory Methods

        [CLSCompliant(false)]
        public virtual object FindFirst(string path, out FindData findData)
        {
            findData = null;
            return false;
        }

        [CLSCompliant(false)]
        public virtual bool FindNext(ref object o, out FindData findData)
        {
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
        public virtual FileSystemExitCode GetFile(string remoteName, ref string localName, CopyFlags copyFlags, RemoteInfo remoteInfo)
        {
            return FileSystemExitCode.NotSupported;
        }

        public virtual FileSystemExitCode PutFile(string localName, ref string remoteName, CopyFlags copyFlags)
        {
            return FileSystemExitCode.NotSupported;
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

        #endregion IFsPlugin Members

        #region Callback Procedures

        // TODO overwrite with ThreadKeeper
        protected virtual int ProgressProc(string source, string destination, int percentDone)
        {
            return OnTcPluginEvent(new ProgressEventArgs(PluginNumber, source, destination, percentDone));
        }

        // TODO overwrite with ThreadKeeper
        protected virtual void LogProc(LogMsgType msgType, string logText)
        {
            OnTcPluginEvent(new LogEventArgs(PluginNumber, (int) msgType, logText));
        }

        // TODO overwrite with ThreadKeeper
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
