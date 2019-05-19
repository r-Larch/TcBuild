using System;
using System.Collections.Generic;
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

        public override IEnumerable<FindData> GetFiles(RemotePath path)
        {
            return base.GetFiles(path);
        }

        public override Task<FileSystemExitCode> PutFileAsync(string localName, RemotePath remoteName, CopyFlags copyFlags, Action<int> setProgress, CancellationToken token)
        {
            return base.PutFileAsync(localName, remoteName, copyFlags, setProgress, token);
        }

        public override Task<FileSystemExitCode> GetFileAsync(RemotePath remoteName, string localName, CopyFlags copyFlags, RemoteInfo remoteInfo, Action<int> setProgress, CancellationToken token)
        {
            return base.GetFileAsync(remoteName, localName, copyFlags, remoteInfo, setProgress, token);
        }


        public override bool MkDir(RemotePath dir)
        {
            return false;
        }


        public override bool RemoveDir(RemotePath dirName)
        {
            return false;
        }

        public override bool DeleteFile(RemotePath fileName)
        {
            return false;
        }


        public override FileSystemExitCode RenMovFile(RemotePath oldName, RemotePath newName, bool move, bool overwrite, RemoteInfo remoteInfo)
        {
            return FileSystemExitCode.NotSupported;
        }


        public override ExtractIconResult ExtractCustomIcon(RemotePath remoteName, ExtractIconFlags extractFlags)
        {
            return ExtractIconResult.UseDefault;
        }


        public override void StatusInfo(string remoteDir, InfoStartEnd startEnd, InfoOperation infoOperation)
        {
            base.StatusInfo(remoteDir, startEnd, infoOperation);
        }


        public override bool Disconnect(RemotePath disconnectRoot)
        {
            return false;
        }

        public override ExecResult ExecuteCommand(TcWindow mainWin, RemotePath remoteName, string command)
        {
            return ExecResult.Yourself;
        }

        public override ExecResult ExecuteProperties(TcWindow mainWin, RemotePath remoteName)
        {
            return ExecResult.Yourself;
        }

        public override ExecResult ExecuteOpen(TcWindow mainWin, RemotePath remoteName)
        {
            return ExecResult.Yourself;
        }

        public override PreviewBitmapResult GetPreviewBitmap(RemotePath remoteName, int width, int height)
        {
            return PreviewBitmapResult.None;
        }

        public override bool SetAttr(RemotePath remoteName, FileAttributes attr)
        {
            return false;
        }

        public override bool SetTime(RemotePath remoteName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime)
        {
            return false;
        }


        public override int OnTcPluginEvent(PluginEventArgs e)
        {
            return base.OnTcPluginEvent(e);
        }
    }
}
