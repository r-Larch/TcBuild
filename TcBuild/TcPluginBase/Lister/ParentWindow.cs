using System;
using System.Diagnostics;


namespace TcPluginBase.Lister {
    public class ParentWindow {
        public IntPtr Handle { get; }
        public bool IsQuickView { get; }

        public bool WriteTrace { get; set; }
        public IntPtr ListerHandle { get; private set; }

        public ParentWindow(IntPtr handle)
        {
            Handle = handle;

            long windowState = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);
            IsQuickView = (windowState & NativeMethods.WS_CHILD) != 0;
        }


        public void SetLister(IntPtr hWndLister)
        {
            ListerHandle = hWndLister;
            NativeMethods.SetParent(hWndLister, hWndNewParent: Handle);
        }


        // Use following methods to send WM_COMMAND message to the parent window
        // to set a new percentage value in Lister's title bar,
        // or to check some menu items like fonts or word wrap mode.
        // (See WM_COMMAND in "Lister Plugin Interface" help file)

        /// <summary>
        /// Set the percent value in the menu bar of the main Lister window.
        /// </summary>
        public void Scroll(int percent)
        {
            ListerPluginEvent(ListerMessage.Percent, percent);
        }

        /// <summary>
        /// Set the font style to ANSI, ASCII, or Variable.
        /// </summary>
        public void FontStyle(ShowFlags fontFlag)
        {
            if ((fontFlag & ShowFlags.Ansi).Equals(ShowFlags.Ansi)
                || (fontFlag & ShowFlags.Ascii).Equals(ShowFlags.Ascii)
                || (fontFlag & ShowFlags.Variable).Equals(ShowFlags.Variable)) {
                ListerPluginEvent(ListerMessage.FontStyle, (int) fontFlag);
            }
        }

        /// <summary>
        /// Set word wrap mode ON or OFF
        /// </summary>
        public void WordWrap(bool wordWrap)
        {
            ListerPluginEvent(ListerMessage.WordWrap, wordWrap ? 1 : 0);
        }

        /// <summary>
        /// Fit image to lister window ON, OFF, or ON for larger images only
        /// </summary>
        public void ImageFit(ShowFlags imgFlag)
        {
            var value = 0;
            if ((imgFlag & ShowFlags.FitToWindow).Equals(ShowFlags.FitToWindow))
                value = 2;
            if ((imgFlag & ShowFlags.FitLargerOnly).Equals(ShowFlags.FitLargerOnly))
                value += 1;
            if (value > 0)
                ListerPluginEvent(ListerMessage.ImageFit, value);
        }

        /// <summary>
        /// Center image on screen ON or OFF
        /// </summary>
        public void ImageCenter(bool centerImage)
        {
            ListerPluginEvent(ListerMessage.ImageCenter, centerImage ? 1 : 0);
        }

        /// <summary>
        /// Switch to next file if multiple opened
        /// </summary>
        public void NextFile()
        {
            ListerPluginEvent(ListerMessage.NextFile, 0);
        }

        private void ListerPluginEvent(ListerMessage message, int value)
        {
            if (ListerHandle != IntPtr.Zero && Handle != IntPtr.Zero) {
                var wParam = ((int) message * 0x10000) + value;
                NativeMethods.PostMessage(Handle, NativeMethods.WM_COMMAND, new IntPtr(wParam), ListerHandle);
#if TRACE
                if (WriteTrace) {
                    TcTrace.TraceOut(TraceLevel.Info, $"  << Callback: ({ListerHandle}) {message.ToString()} = {value}", null);
                }
#endif
            }
        }

        public void CloseParentWindow()
        {
            if (ListerHandle != IntPtr.Zero && Handle != IntPtr.Zero && !IsQuickView) {
                NativeMethods.PostMessage(Handle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public void SendKeyToParentWindow(int keyCode)
        {
            if (ListerHandle != IntPtr.Zero && Handle != IntPtr.Zero) {
                NativeMethods.PostMessage(Handle, NativeMethods.WM_KEYDOWN, new IntPtr(keyCode), IntPtr.Zero);
            }
        }
    }
}
