using System;
using System.Windows.Forms;


namespace TcPluginBase {
    // Total Commander window, can be used as parent for any plugin child windows.
    [Serializable]
    public class TcWindow : IWin32Window {
        private const int TcMessageId = 1024 + 51;

        public IntPtr Handle { get; }

        public TcWindow(IntPtr handle)
        {
            Handle = handle;
        }

        // Forces the window to redraw itself and any child controls.
        public void Refresh()
        {
            NativeMethods.PostMessage(Handle, TcMessageId, (IntPtr) 540, IntPtr.Zero);
        }

        public static void SendMessage(IntPtr handle, int wParam)
        {
            NativeMethods.PostMessage(handle, TcMessageId, (IntPtr) wParam, IntPtr.Zero);
        }
    }
}
