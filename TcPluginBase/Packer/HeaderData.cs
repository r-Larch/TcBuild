using System;
using System.IO;
using System.Runtime.InteropServices;


namespace TcPluginBase.Packer {
    // Used as parameter type for ReadHeader method
    [Serializable]
    public class HeaderData {
        private const int AllowedPackerAttributes = 0x3F;

        #region Properties

        public string ArchiveName { get; set; }
        public string FileName { get; set; }
        public FileAttributes FileAttributes { get; set; }
        public ulong PackedSize { get; set; }
        public ulong UnpackedSize { get; set; }
        public int FileCRC { get; set; }
        public DateTime FileTime { get; set; }

        //public int UnpVer { get; set; }
        //public int Method { get; set; }

        //   may be used in the future
        //public string CmtBuf { get; set; }
        //public int CmtBufSize { get; set; }
        //public int CmtSize { get; set; }
        //public int CmtState { get; set; }
        //public string Reserved { get; set; }

        #endregion Properties

        public void CopyTo(IntPtr ptr, HeaderDataMode mode)
        {
            if (ptr != IntPtr.Zero) {
                if (mode == HeaderDataMode.Ansi) {
                    var data = new TcHeaderData {
                        ArchiveName = ArchiveName,
                        FileName = FileName,
                        FileAttr = ((int) FileAttributes) & AllowedPackerAttributes,
                        FileCRC = FileCRC,
                        FileTime = TcUtils.GetArchiveHeaderTime(FileTime),
                        PackSize = (int) PackedSize,
                        UnpSize = (int) UnpackedSize
                    };
                    Marshal.StructureToPtr(data, ptr, false);
                }
                else if (mode == HeaderDataMode.ExAnsi) {
                    var data = new TcHeaderDataEx {
                        ArchiveName = ArchiveName,
                        FileName = FileName,
                        FileAttr = ((int) FileAttributes) & AllowedPackerAttributes,
                        FileCRC = FileCRC,
                        FileTime = TcUtils.GetArchiveHeaderTime(FileTime),
                        PackSizeHigh = TcUtils.GetUHigh(PackedSize),
                        PackSizeLow = TcUtils.GetULow(PackedSize),
                        UnpSizeHigh = TcUtils.GetUHigh(UnpackedSize),
                        UnpSizeLow = TcUtils.GetULow(UnpackedSize)
                    };
                    Marshal.StructureToPtr(data, ptr, false);
                }
                else if (mode == HeaderDataMode.ExUnicode) {
                    var data = new TcHeaderDataExW {
                        ArchiveName = ArchiveName,
                        FileName = FileName,
                        FileAttr = ((int) FileAttributes) & AllowedPackerAttributes,
                        FileCRC = FileCRC,
                        FileTime = TcUtils.GetArchiveHeaderTime(FileTime),
                        PackSizeHigh = TcUtils.GetUHigh(PackedSize),
                        PackSizeLow = TcUtils.GetULow(PackedSize),
                        UnpSizeHigh = TcUtils.GetUHigh(UnpackedSize),
                        UnpSizeLow = TcUtils.GetULow(UnpackedSize)
                    };
                    Marshal.StructureToPtr(data, ptr, false);
                }
            }
        }

        #region TC Structures

        // Used in TC ReadHeaderEx function (Unicode version)
        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TcHeaderDataExW {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_UNI)]
            public string ArchiveName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_UNI)]
            public string FileName;
            public int Flags;
            public uint PackSizeLow;
            public uint PackSizeHigh;
            public uint UnpSizeLow;
            public uint UnpSizeHigh;
            public int HostOS;
            public int FileCRC;
            public int FileTime;
            public int UnpVer;
            public int Method;
            public int FileAttr;
            [MarshalAs(UnmanagedType.LPWStr)] public string CmtBuf;
            public int CmtBufSize;
            public int CmtSize;
            public int CmtState;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NativeMethods.MAX_PATH_UNI)]
            public byte[] Reserved;
        }

        // Used in TC ReadHeaderEx function (non-Unicode version)
        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct TcHeaderDataEx {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_UNI)]
            public string ArchiveName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_UNI)]
            public string FileName;
            public int Flags;
            public uint PackSizeLow;
            public uint PackSizeHigh;
            public uint UnpSizeLow;
            public uint UnpSizeHigh;
            public int HostOS;
            public int FileCRC;
            public int FileTime;
            public int UnpVer;
            public int Method;
            public int FileAttr;
            [MarshalAs(UnmanagedType.LPStr)] public string CmtBuf;
            public int CmtBufSize;
            public int CmtSize;
            public int CmtState;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NativeMethods.MAX_PATH_UNI)]
            public byte[] Reserved;
        }

        // Used in TC ReadHeader function
        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct TcHeaderData {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_ANSI)]
            public string ArchiveName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH_ANSI)]
            public string FileName;
            public int Flags;
            public int PackSize;
            public int UnpSize;
            public int HostOS;
            public int FileCRC;
            public int FileTime;
            public int UnpVer;
            public int Method;
            public int FileAttr;
            [MarshalAs(UnmanagedType.LPStr)] public string CmtBuf;
            public int CmtBufSize;
            public int CmtSize;
            public int CmtState;
        }

        #endregion TC Structures
    }
}
