using System;
using System.Collections.Generic;


namespace OY.TotalCommander.TcPluginBase.Packer {
    [CLSCompliant(false)]
    public interface IPackerPlugin {
        #region Mandatory Methods

        //[TcMethod("OpenArchive", "OpenArchiveW")]
        object OpenArchive(ref OpenArchiveData archiveData);

        //[TcMethod("ReadHeaderEx", "ReadHeaderExW")]
        PackerResult ReadHeader(ref object arcData, out HeaderData headerData);

        //[TcMethod("ProcessFile", "ProcessFileW")]
        PackerResult ProcessFile(object arcData, ProcessFileOperation operation, string destFile);

        //[TcMethod("CloseArchive")]
        PackerResult CloseArchive(object arcData);

        #endregion Mandatory Methods

        #region Optional Methods

        [TcMethod("PackFiles", "PackFilesW")]
        PackerResult PackFiles(string packedFile, string subPath, string srcPath, List<string> addList, PackFilesFlags flags);

        [TcMethod("DeleteFilesW", "DeleteFilesW")]
        PackerResult DeleteFiles(string packedFile, List<string> deleteList);

        [TcMethod("ConfigurePacker")]
        void ConfigurePacker(TcWindow parentWin);

        [TcMethod("StartMemPack", "StartMemPackW")]
        object StartMemPack(MemPackOptions options, string fileName);

        [TcMethod("PackToMem")]
        PackerResult PackToMem(ref object memData, byte[] bufIn, ref int taken, byte[] bufOut, ref int written, int seekBy);

        [TcMethod("DoneMemPack")]
        PackerResult DoneMemPack(object memData);

        [TcMethod("CanYouHandleThisFile", "CanYouHandleThisFileW")]
        bool CanYouHandleThisFile(string fileName);

        #endregion Optional Methods
    }
}
