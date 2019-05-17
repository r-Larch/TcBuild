using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TcPluginBase;
using TcPluginBase.FileSystem;


namespace TcBuild.Test.PluginImplementation {
    public class FsPluginTest : FsPlugin {
        protected FsPluginTest(Settings pluginSettings) : base(pluginSettings)
        {
        }

        public override IEnumerable<FindData> GetFiles(string path)
        {
            return base.GetFiles(path);
        }

        public override Task<FileSystemExitCode> PutFileAsync(string localName, string remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token)
        {
            return base.PutFileAsync(localName, remoteName, copyFlags, setProgress, token);
        }

        public override Task<FileSystemExitCode> GetFileAsync(string remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token)
        {
            return base.GetFileAsync(remoteName, localName, copyFlags, remoteInfo, setProgress, token);
        }


        public override bool MkDir(string dir)
        {
            return false;
        }


        public override bool RemoveDir(string dirName)
        {
            return false;
        }

        public override bool DeleteFile(string fileName)
        {
            return false;
        }


        public override FileSystemExitCode RenMovFile(string oldName, string newName, bool move, bool overwrite, RemoteInfo remoteInfo)
        {
            return FileSystemExitCode.NotSupported;
        }


        public override ExtractIconResult ExtractCustomIcon(ref string remoteName, ExtractIconFlags extractFlags, out Icon icon)
        {
            icon = null;
            return ExtractIconResult.UseDefault;
        }


        public override void StatusInfo(string remoteDir, InfoStartEnd startEnd, InfoOperation infoOperation)
        {
            base.StatusInfo(remoteDir, startEnd, infoOperation);
        }


        public override bool Disconnect(string disconnectRoot)
        {
            return false;
        }

        public override ExecResult ExecuteCommand(TcWindow mainWin, ref string remoteName, string command)
        {
            return ExecResult.Yourself;
        }

        public override ExecResult ExecuteProperties(TcWindow mainWin, string remoteName)
        {
            return ExecResult.Yourself;
        }

        public override ExecResult ExecuteOpen(TcWindow mainWin, ref string remoteName)
        {
            return ExecResult.Yourself;
        }

        public override PreviewBitmapResult GetPreviewBitmap(ref string remoteName, int width, int height, out Bitmap returnedBitmap)
        {
            returnedBitmap = null;
            return PreviewBitmapResult.None;
        }

        public override bool SetAttr(string remoteName, FileAttributes attr)
        {
            return false;
        }

        public override bool SetTime(string remoteName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            return false;
        }


        public override int OnTcPluginEvent(PluginEventArgs e)
        {
            return base.OnTcPluginEvent(e);
        }
    }
}
