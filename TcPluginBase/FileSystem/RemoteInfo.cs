using System;
using System.IO;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;


namespace OY.TotalCommander.TcPluginBase.FileSystem {
    // Used as parameter type for GetFile and RenMovFile methods
    [CLSCompliant(false)]
    [Serializable]
    public class RemoteInfo {
        #region Properties

        public ulong Size { get; private set; }
        public DateTime? LastWriteTime { get; private set; }
        public FileAttributes Attributes { get; private set; }

        #endregion Properties

        #region Constructors

        public RemoteInfo(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero) {
                TcRemoteInfo data = (TcRemoteInfo) Marshal.PtrToStructure(ptr, typeof(TcRemoteInfo));
                Size = TcUtils.GetULong(data.sizeHigh, data.sizeLow);
                LastWriteTime = TcUtils.FromFileTime(data.lastWriteTime);
                Attributes = (FileAttributes) data.attr;
            }
        }

        #endregion Constructors

        #region TC Structure

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct TcRemoteInfo {
            public uint sizeLow;
            public uint sizeHigh;
            public FILETIME lastWriteTime;
            public int attr;
        }

        #endregion TC Structure
    }
}
