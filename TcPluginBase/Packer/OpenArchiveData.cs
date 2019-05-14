using System;
using System.Runtime.InteropServices;


namespace TcPluginBase.Packer {
    // Used as parameter type for OpenArchive method
    [Serializable]
    public class OpenArchiveData {
        private readonly IntPtr ptr;
        private TcOpenArchiveData data;
        private TcOpenArchiveDataW dataW;
        private bool isUnicode;

        #region Properties

        public string ArchiveName { get; private set; }
        public ArcOpenMode Mode { get; private set; }
        public PackerResult Result { get; set; }

        //   may be used in the future
        //public string CmtBuf { get; set; }
        //public int CmtBufSize { get; set; }
        //public int CmtSize { get; set; }
        //public int CmtState { get; set; }

        #endregion Properties

        #region Constructors

        public OpenArchiveData(IntPtr ptr, bool isUnicode)
        {
            this.ptr = ptr;
            this.isUnicode = isUnicode;
            if (ptr != IntPtr.Zero) {
                if (isUnicode) {
                    dataW = (TcOpenArchiveDataW) Marshal.PtrToStructure(ptr, typeof(TcOpenArchiveDataW));
                    ArchiveName = dataW.ArchiveName;
                    Mode = (ArcOpenMode) dataW.Mode;
                }
                else {
                    data = (TcOpenArchiveData) Marshal.PtrToStructure(ptr, typeof(TcOpenArchiveData));
                    ArchiveName = data.ArchiveName;
                    Mode = (ArcOpenMode) data.Mode;
                }
            }
        }

        #endregion Constructors

        public void Update()
        {
            if (ptr != IntPtr.Zero) {
                if (isUnicode) {
                    dataW.Result = (int) Result;
                    Marshal.StructureToPtr(dataW, ptr, false);
                }
                else {
                    data.Result = (int) Result;
                    Marshal.StructureToPtr(data, ptr, false);
                }
            }
        }

        #region TC Structures

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct TcOpenArchiveDataW {
            [MarshalAs(UnmanagedType.LPWStr)] public string ArchiveName;
            public int Mode;
            public int Result;
            [MarshalAs(UnmanagedType.LPWStr)] public string CommentBuffer;
            public int CommentBufferSize;
            public int CommentSize;
            public int CommentState;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct TcOpenArchiveData {
            [MarshalAs(UnmanagedType.LPStr)] public string ArchiveName;
            public int Mode;
            public int Result;
            [MarshalAs(UnmanagedType.LPStr)] public string CommentBuffer;
            public int CommentBufferSize;
            public int CommentSize;
            public int CommentState;
        }

        #endregion TC Structures
    }
}
