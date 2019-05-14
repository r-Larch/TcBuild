using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using TcPluginBase;
using TcPluginBase.FileSystem;


namespace TcBuild.Test.PluginImplementation {
    public class FsPluginTest : FsPlugin {
        public FsPluginTest(StringDictionary pluginSettings) : base(pluginSettings)
        {
        }

        public IEnumerable<FindData> GetFiles(string path)
        {
            return new FindData[0];
        }

        public override object FindFirst(string path, out FindData findData)
        {
            var enumerator = GetFiles(path).GetEnumerator();

            if (enumerator.MoveNext()) {
                findData = enumerator.Current;
                return enumerator;
            }

            // empty list
            findData = null;
            return null;
        }

        public override bool FindNext(ref object o, out FindData findData)
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


        public override FileSystemExitCode GetFile(string remoteName, ref string localName, CopyFlags copyFlags, RemoteInfo remoteInfo)
        {
            return FileSystemExitCode.NotSupported;
        }


        public override FileSystemExitCode PutFile(string localName, ref string remoteName, CopyFlags copyFlags)
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
    }
}
