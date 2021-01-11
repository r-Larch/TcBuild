using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using FileTime = System.Runtime.InteropServices.ComTypes.FILETIME;


namespace TcPluginBase {
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TcUtils {
        private const uint EmptyDateTimeHi = 0xFFFFFFFF;
        private const uint EmptyDateTimeLo = 0xFFFFFFFE;

        #region Long Conversion Methods

        internal static int GetHigh(long value)
        {
            return (int) (value >> 32);
        }

        internal static int GetLow(long value)
        {
            return (int) (value & uint.MaxValue);
        }

        //internal static long GetLong(int high, int low)
        //{
        //    return ((long) high << 32) + low;
        //}

        internal static uint GetUHigh(ulong value)
        {
            return (uint) (value >> 32);
        }

        internal static uint GetULow(ulong value)
        {
            return (uint) (value & uint.MaxValue);
        }

        internal static ulong GetULong(uint high, uint low)
        {
            return ((ulong) high << 32) + low;
        }

        #endregion Long Conversion Methods

        #region DateTime Conversion Methods

        internal static FileTime GetFileTime(DateTime? dateTime)
        {
            var longTime =
                (dateTime.HasValue && dateTime.Value != DateTime.MinValue)
                    ? dateTime.Value.ToFileTime()
                    : long.MaxValue << 1;
            return new FileTime {
                dwHighDateTime = GetHigh(longTime),
                dwLowDateTime = GetLow(longTime),
            };
        }

        internal static ulong GetULong(DateTime? dateTime)
        {
            if (dateTime.HasValue && dateTime.Value != DateTime.MinValue) {
                ulong ulongTime = Convert.ToUInt64(dateTime.Value.ToFileTime());
                return ulongTime;
            }

            return GetULong(EmptyDateTimeHi, EmptyDateTimeLo);
        }

        internal static DateTime? FromFileTime(FileTime fileTime)
        {
            try {
                long longTime = Convert.ToInt64(fileTime);
                return DateTime.FromFileTime(longTime);
            }
            catch (Exception) {
                return null;
            }
        }

        //internal static DateTime? FromULong(ulong fileTime) {
        //    long longTime = Convert.ToInt64(fileTime);
        //    return longTime != 0
        //        ? DateTime.FromFileTime(longTime) : (DateTime?)null;
        //}

        public static DateTime? ReadDateTime(IntPtr addr)
        {
            return addr == IntPtr.Zero
                ? (DateTime?) null
                : DateTime.FromFileTime(Marshal.ReadInt64(addr));
        }

        internal static int GetArchiveHeaderTime(DateTime dt)
        {
            if (dt.Year < 1980 || dt.Year > 2100)
                return 0;
            return
                (dt.Year - 1980) << 25
                | dt.Month << 21
                | dt.Day << 16
                | dt.Hour << 11
                | dt.Minute << 5
                | dt.Second / 2;
        }

        #endregion DateTime Conversion Methods

        #region Unmanaged String Methods

        internal static string ReadStringAnsi(IntPtr addr)
        {
            return (addr == IntPtr.Zero)
                ? string.Empty
                : Marshal.PtrToStringAnsi(addr);
        }

        internal static List<string> ReadStringListAnsi(IntPtr addr)
        {
            List<string> result = new List<string>();
            if (addr != IntPtr.Zero) {
                while (true) {
                    string s = ReadStringAnsi(addr);
                    if (String.IsNullOrEmpty(s))
                        break;
                    result.Add(s);
                    addr = new IntPtr(addr.ToInt64() + s.Length + 1);
                }
            }

            return result;
        }

        internal static string ReadStringUni(IntPtr addr)
        {
            return (addr == IntPtr.Zero)
                ? string.Empty
                : Marshal.PtrToStringUni(addr);
        }

        internal static List<string> ReadStringListUni(IntPtr addr)
        {
            List<string> result = new List<string>();
            if (addr != IntPtr.Zero) {
                while (true) {
                    string s = ReadStringUni(addr);
                    if (String.IsNullOrEmpty(s))
                        break;
                    result.Add(s);
                    addr = new IntPtr(addr.ToInt64() + (s.Length + 1) * 2);
                }
            }

            return result;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void WriteStringAnsi(string str, IntPtr addr, int length)
        {
            if (String.IsNullOrEmpty(str))
                Marshal.WriteIntPtr(addr, IntPtr.Zero);
            else {
                int strLen = str.Length;
                if (length > 0 && strLen >= length)
                    strLen = length - 1;
                int i = 0;
                Byte[] bytes = new Byte[strLen + 1];
                foreach (char ch in str.Substring(0, strLen)) {
                    bytes[i++] = Convert.ToByte(ch);
                }

                bytes[strLen] = 0;
                Marshal.Copy(bytes, 0, addr, strLen + 1);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void WriteStringUni(string str, IntPtr addr, int length)
        {
            if (String.IsNullOrEmpty(str))
                Marshal.WriteIntPtr(addr, IntPtr.Zero);
            else {
                int strLen = str.Length;
                if (length > 0 && strLen >= length)
                    strLen = length - 1;
                Marshal.Copy((str + (char) 0).ToCharArray(0, strLen + 1), 0, addr, strLen + 1);
            }
        }

        #endregion Unmanaged String Methods
    }
}
