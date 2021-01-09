using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace TcBuild
{
    internal static class IconExtractor
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private delegate bool ENUMRESNAMEPROC(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [SuppressUnmanagedCodeSecurity]
        private static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, ENUMRESNAMEPROC lpEnumFunc, IntPtr lParam);


        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        private readonly static IntPtr RT_ICON = (IntPtr)3;
        private readonly static IntPtr RT_GROUP_ICON = (IntPtr)14;

        public static bool ExtractIconFromExecutable(FileInfo sourceFile, FileInfo targetFile)
        {
            IntPtr hModule = LoadLibraryEx(sourceFile.FullName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);

            ENUMRESNAMEPROC callback = (h, t, name, l) =>
            {
                var dir = GetDataFromResource(hModule, RT_GROUP_ICON, name);

                // Calculate the size of an entire .icon file.

                int count = BitConverter.ToUInt16(dir, 4);  // GRPICONDIR.idCount
                int len = 6 + 16 * count;                   // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count
                for (int i = 0; i < count; ++i)
                    len += BitConverter.ToInt32(dir, 6 + 14 * i + 8);   // GRPICONDIRENTRY.dwBytesInRes

                using (FileStream targetStream = targetFile.Create())
                using (var dst = new BinaryWriter(targetStream))
                {
                    // Copy GRPICONDIR to ICONDIR.

                    dst.Write(dir, 0, 6);

                    int picOffset = 6 + 16 * count; // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count

                    for (int i = 0; i < count; ++i)
                    {
                        // Load the picture.

                        ushort id = BitConverter.ToUInt16(dir, 6 + 14 * i + 12);    // GRPICONDIRENTRY.nID
                        var pic = GetDataFromResource(hModule, RT_ICON, (IntPtr)id);

                        // Copy GRPICONDIRENTRY to ICONDIRENTRY.

                        dst.Seek(6 + 16 * i, 0);

                        dst.Write(dir, 6 + 14 * i, 8);  // First 8bytes are identical.
                        dst.Write(pic.Length);          // ICONDIRENTRY.dwBytesInRes
                        dst.Write(picOffset);           // ICONDIRENTRY.dwImageOffset

                        // Copy a picture.

                        dst.Seek(picOffset, 0);
                        dst.Write(pic, 0, pic.Length);

                        picOffset += pic.Length;
                    }
                }
                return true;
            };
            return EnumResourceNames(hModule, RT_GROUP_ICON, callback, IntPtr.Zero);
        }
        private static byte[] GetDataFromResource(IntPtr hModule, IntPtr type, IntPtr name)
        {
            // Load the binary data from the specified resource.

            IntPtr hResInfo = FindResource(hModule, name, type);

            IntPtr hResData = LoadResource(hModule, hResInfo);

            IntPtr pResData = LockResource(hResData);

            uint size = SizeofResource(hModule, hResInfo);

            byte[] buf = new byte[size];
            Marshal.Copy(pResData, buf, 0, buf.Length);

            return buf;
        }

    }
}
