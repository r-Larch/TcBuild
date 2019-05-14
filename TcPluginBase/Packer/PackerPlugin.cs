using System;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace TcPluginBase.Packer {
    public class PackerPlugin : TcPlugin, IPackerPlugin {
        #region Properties

        public PackerCapabilities Capabilities { get; set; }

        public PackBackgroundFlags BackgroundFlags { get; set; }

        #endregion Properties

        #region Constructors

        public PackerPlugin(StringDictionary pluginSettings) : base(pluginSettings)
        {
            BackgroundFlags = PackBackgroundFlags.None;
            Capabilities = PackerCapabilities.None;
        }

        #endregion Constructors

        #region IPackerPlugin Members

        #region Mandatory Methods

        public virtual object OpenArchive(ref OpenArchiveData archiveData)
        {
            throw new MethodNotSupportedException(nameof(OpenArchive));
        }

        [CLSCompliant(false)]
        public virtual PackerResult ReadHeader(ref object arcData, out HeaderData headerData)
        {
            throw new MethodNotSupportedException(nameof(ReadHeader));
        }

        public virtual PackerResult ProcessFile(object arcData, ProcessFileOperation operation, string destFile)
        {
            throw new MethodNotSupportedException(nameof(ProcessFile));
        }

        public virtual PackerResult CloseArchive(object arcData)
        {
            throw new MethodNotSupportedException(nameof(CloseArchive));
        }

        #endregion Mandatory Methods

        #region Optional Methods

        public virtual PackerResult PackFiles(string packedFile, string subPath, string srcPath, List<string> addList, PackFilesFlags flags)
        {
            return PackerResult.NotSupported;
        }

        public virtual PackerResult DeleteFiles(string packedFile, List<string> deleteList)
        {
            return PackerResult.NotSupported;
        }

        public virtual void ConfigurePacker(TcWindow parentWin)
        {
        }

        public virtual object StartMemPack(MemPackOptions options, string fileName)
        {
            return null;
        }

        public virtual PackerResult PackToMem(ref object memData, byte[] bufIn, ref int taken, byte[] bufOut, ref int written, int seekBy)
        {
            return PackerResult.NotSupported;
        }

        public virtual PackerResult DoneMemPack(object memData)
        {
            return PackerResult.NotSupported;
        }

        public virtual bool CanYouHandleThisFile(string fileName)
        {
            return false;
        }

        #endregion Optional Methods

        #endregion IPackerPlugin Members

        #region Callback Procedures

        protected int ProcessDataProc(string fileName, int size)
        {
            return OnTcPluginEvent(new PackerProcessEventArgs(fileName, size));
        }

        protected int ChangeVolProc(string arcName, ChangeValueProcMode mode)
        {
            return OnTcPluginEvent(new PackerChangeVolEventArgs(arcName, (int) mode));
        }

        #endregion Callback Procedures

        public override void CreatePassword(int cryptoNumber, int flags)
        {
            if (Password == null) {
                Password = new PackerPassword(this, cryptoNumber, flags);
            }
        }
    }
}
