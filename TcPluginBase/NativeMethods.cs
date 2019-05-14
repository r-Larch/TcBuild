using System;
using System.Runtime.InteropServices;


namespace TcPluginBase {
    // see help for CA1060
    [CLSCompliant(false)]
    public static class NativeMethods {
        // ReSharper disable InconsistentNaming
        public const int MAX_PATH_UNI = 1024;
        public const int MAX_PATH_ANSI = 260;
        public const uint ERROR_NO_MORE_FILES = 18;
        public const int IMAGE_ICON = 1;
        public const int LR_LOADFROMFILE = 0x10;
        public const int LR_SHARED = 0x8000;
        public const int WM_CHAR = 0x0102;
        public const int WM_CLOSE = 0x10;
        public const int WM_COMMAND = 0x0111;
        public const int WM_KEYDOWN = 0x100;
        public const int GWL_STYLE = -16;
        public const int WS_CHILD = 0x40000000;
        public static readonly IntPtr INVALID_HANDLE = new IntPtr(-1);

        [DllImport("user32.dll")]
        public static extern Int32 PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32.dll")]
        public static extern Int32 GetLastError();

        [DllImport("kernel32.dll")]
        public static extern void SetLastError(uint errCode);
    }
}
