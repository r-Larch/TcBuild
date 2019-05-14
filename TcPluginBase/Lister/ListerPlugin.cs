using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;


namespace TcPluginBase.Lister {
    public class ListerPlugin : TcPlugin, IListerPlugin {
        #region Constants

        // Lister handler builder load
        public const string WFListerHandlerBuilderName = "WinForms";
        public const string WPFListerHandlerBuilderName = "WPF";

        #endregion Constants

        #region Properties

        public Color BitmapBackgroundColor { get; set; }

        public string DetectString { get; set; }

        public IntPtr ListerHandle { private get; set; }

        public IntPtr ParentHandle { private get; set; }

        public bool IsQuickView { get; set; }

        public object FocusedControl { get; protected set; }

        #endregion Properties

        #region Constructors

        public ListerPlugin(StringDictionary pluginSettings) : base(pluginSettings)
        {
            BitmapBackgroundColor = Color.White;
            ListerHandle = IntPtr.Zero;
        }

        #endregion Constructors

        #region IListerPlugin Members

        #region Mandatory Methods

        public virtual object Load(string fileToLoad, ShowFlags showFlags)
        {
            throw new MethodNotSupportedException(nameof(Load));
        }

        #endregion Mandatory Methods

        #region Optional Methods

        public virtual ListerResult LoadNext(object control, string fileToLoad, ShowFlags showFlags)
        {
            return ListerResult.Error;
        }

        public virtual void CloseWindow(object control)
        {
        }

        public virtual ListerResult SearchText(object control, string searchString, SearchParameter searchParameter)
        {
            return ListerResult.Error;
        }

        public virtual ListerResult SendCommand(object control, ListerCommand command, ShowFlags parameter)
        {
            return ListerResult.Error;
        }

        public virtual ListerResult Print(object control, string fileToPrint, string defPrinter, PrintFlags printFlags,
            PrintMargins margins)
        {
            return ListerResult.Error;
        }

        public virtual int NotificationReceived(object control, int message, int wParam, int lParam)
        {
            return 0;
        }

        public virtual Bitmap GetPreviewBitmap(string fileToLoad, int width, int height, byte[] contentBuf)
        {
            return null;
        }

        public virtual ListerResult SearchDialog(object control, bool findNext)
        {
            return ListerResult.Error;
        }

        #endregion Optional Methods

        #endregion IListerPlugin Members

        #region Callback Procedures

        // Use following methods to send WM_COMMAND message to the parent window
        // to set a new percentage value in Lister's title bar,
        // or to check some menu items like fonts or word wrap mode.
        // (See WM_COMMAND in "Lister Plugin Interface" help file)

        // Set the percent value in the menu bar of the main Lister window.
        protected void ScrollProc(int percent)
        {
            ListerPluginEvent(ListerMessage.Percent, percent);
        }

        // Set the font style to ANSI, ASCII, or Variable.
        protected void FontStyleProc(ShowFlags fontFlag)
        {
            if ((fontFlag & ShowFlags.Ansi).Equals(ShowFlags.Ansi)
                || (fontFlag & ShowFlags.Ascii).Equals(ShowFlags.Ascii)
                || (fontFlag & ShowFlags.Variable).Equals(ShowFlags.Variable)) {
                ListerPluginEvent(ListerMessage.FontStyle, (int) fontFlag);
            }
        }

        // Set word wrap mode ON or OFF
        protected void WordWrapProc(bool wordWrap)
        {
            ListerPluginEvent(ListerMessage.WordWrap, wordWrap ? 1 : 0);
        }

        // Fit image to lister window ON, OFF, or ON for larger images only
        protected void ImageFitProc(ShowFlags imgFlag)
        {
            int value = 0;
            if ((imgFlag & ShowFlags.FitToWindow).Equals(ShowFlags.FitToWindow))
                value = 2;
            if ((imgFlag & ShowFlags.FitLargerOnly).Equals(ShowFlags.FitLargerOnly))
                value += 1;
            if (value > 0)
                ListerPluginEvent(ListerMessage.ImageFit, value);
        }

        // Center image on screen ON or OFF
        protected void ImageCenterProc(bool centerImage)
        {
            ListerPluginEvent(ListerMessage.ImageCenter, centerImage ? 1 : 0);
        }

        // Switch to next file if multiple opened
        protected void NextFileProc()
        {
            ListerPluginEvent(ListerMessage.NextFile, 0);
        }

        private void ListerPluginEvent(ListerMessage message, int value)
        {
            if (ListerHandle != IntPtr.Zero && ParentHandle != IntPtr.Zero) {
                var wParam = ((int) message * 0x10000) + value;
                NativeMethods.PostMessage(ParentHandle, NativeMethods.WM_COMMAND, new IntPtr(wParam), ListerHandle);
#if TRACE
                if (WriteTrace) {
                    TcTrace.TraceOut(TraceLevel.Info, $"  << Callback: ({ListerHandle}) {message.ToString()} = {value}", null);

                }
#endif
            }
        }

        public void CloseParentWindow()
        {
            if (ListerHandle != IntPtr.Zero && ParentHandle != IntPtr.Zero && !IsQuickView) {
                NativeMethods.PostMessage(ParentHandle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public void SendKeyToParentWindow(int keyCode)
        {
            if (ListerHandle != IntPtr.Zero && ParentHandle != IntPtr.Zero) {
                NativeMethods.PostMessage(ParentHandle, NativeMethods.WM_KEYDOWN, new IntPtr(keyCode), IntPtr.Zero);
            }
        }

        #endregion Callback Procedures
    }
}
