using System;
using System.IO;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;


namespace OY.TotalCommander.TcPluginBase.FileSystem {
    // Used as parameter type for FindFirst and FindNext methods
    [CLSCompliant(false)]
    [Serializable]
    public class FindData {
        #region Properties

        public string FileName { get; private set; }
        //public string AltFileName { get; private set; }
        public FileAttributes Attributes { get; private set; }
        public ulong FileSize { get; private set; }
        public DateTime? CreationTime { get; private set; }
        public DateTime? LastAccessTime { get; private set; }
        public DateTime? LastWriteTime { get; private set; }
        public uint Reserved0 { get; private set; }
        public uint Reserved1 { get; private set; }

        #endregion Properties

        #region Constructors

        public FindData(string fileName)
        {
            FileName = fileName;
        }

        public FindData(string fileName, ulong fileSize) : this(fileName)
        {
            FileSize = fileSize;
        }

        public FindData(string fileName, ulong fileSize, FileAttributes attributes) : this(fileName, fileSize)
        {
            Attributes = attributes;
        }

        public FindData(string fileName, ulong fileSize, DateTime? lastWriteTime) : this(fileName, fileSize)
        {
            LastWriteTime = lastWriteTime;
        }

        public FindData(string fileName, FileAttributes attributes) : this(fileName)
        {
            Attributes = attributes;
        }

        public FindData(string fileName, ulong fileSize, FileAttributes attributes, DateTime? lastWriteTime) : this(fileName, fileSize, lastWriteTime)
        {
            Attributes = attributes;
        }

        public FindData(string fileName, ulong fileSize, FileAttributes attributes, DateTime? lastWriteTime, DateTime? creationTime, DateTime? lastAccessTime) : this(fileName, fileSize, attributes, lastWriteTime)
        {
            CreationTime = creationTime;
            LastAccessTime = lastAccessTime;
        }

        public FindData(string fileName, ulong fileSize, FileAttributes attributes, DateTime? lastWriteTime, DateTime? creationTime, DateTime? lastAccessTime, uint reserved0, uint reserved1) : this(fileName, fileSize, attributes, lastWriteTime, creationTime, lastAccessTime)
        {
            Reserved0 = reserved0;
            Reserved1 = reserved1;
        }

        #endregion Constructors

        public void ChangeFileName(string newName)
        {
            FileName = newName;
        }

        public void CopyTo(IntPtr ptr, bool isUnicode)
        {
            if (ptr != IntPtr.Zero) {
                if (isUnicode) {
                    var data = new TcFindDataW {
                        FileAttributes = (int) Attributes,
                        CreationTime = TcUtils.GetFileTime(CreationTime),
                        LastAccessTime = TcUtils.GetFileTime(LastAccessTime),
                        LastWriteTime = TcUtils.GetFileTime(LastWriteTime),
                        FileSizeHigh = TcUtils.GetUHigh(FileSize),
                        FileSizeLow = TcUtils.GetULow(FileSize),
                        Reserved0 = Reserved0,
                        Reserved1 = Reserved1,
                        FileName = FileName,
                        AlternateFileName = string.Empty
                    };
                    Marshal.StructureToPtr(data, ptr, false);
                }
                else {
                    var data = new TcFindData {
                        FileAttributes = (int) Attributes,
                        CreationTime = TcUtils.GetFileTime(CreationTime),
                        LastAccessTime = TcUtils.GetFileTime(LastAccessTime),
                        LastWriteTime = TcUtils.GetFileTime(LastWriteTime),
                        FileSizeHigh = TcUtils.GetUHigh(FileSize),
                        FileSizeLow = TcUtils.GetULow(FileSize),
                        Reserved0 = Reserved0,
                        Reserved1 = Reserved1,
                        FileName = FileName,
                        AlternateFileName = string.Empty
                    };
                    Marshal.StructureToPtr(data, ptr, false);
                }
            }
        }

        #region TC Structures

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TcFindDataW {
            public int FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint Reserved0;
            public uint Reserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_UNI)]
            public string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string AlternateFileName;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct TcFindData {
            public int FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint Reserved0;
            public uint Reserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_ANSI)]
            public string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string AlternateFileName;
        }

        #endregion TC Structures
    }
}
