using System;
using System.IO;
using System.Runtime.InteropServices;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;


namespace TcPluginBase.FileSystem {
    /// <summary><see cref="RemoteInfo"/> is passed to FsGetFile and FsRenMovFile. It contains details about the remote file being copied.</summary>
    /// <remarks>This struct is passed to <see cref="FsPlugin.GetFile"/> and <see cref="FsPlugin.RenMovFile"/> to make it easier for the plugin to copy the file. You can of course also ignore this parameter.</remarks>
    [CLSCompliant(false)]
    [Serializable]
    public class RemoteInfo {
        #region Properties

        /// <summary>
        /// The remote file size. Useful for a progress indicator.
        /// </summary>
        public ulong Size { get; private set; }
        /// <summary>
        /// Time stamp of the remote file - should be copied with the file.
        /// </summary>
        public DateTime? LastWriteTime { get; private set; }
        /// <summary>
        /// Attributes of the remote file - should be copied with the file.
        /// </summary>
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
