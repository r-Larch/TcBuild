using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using TcPluginBase;
using TcPluginBase.Lister;
using TcPluginBase.Tools;


namespace WlxWrapper {
    public class ListerWrapper {
        private static string? _callSignature;
        private static ListerPlugin? _plugin;
        private static ListerPlugin Plugin => _plugin ??= TcPluginLoader.GetTcPlugin<ListerPlugin>(typeof(PluginClassPlaceholder));


        private ListerWrapper()
        {
        }


        #region ListLoad

        [UnmanagedCallersOnly(EntryPoint = "ListLoad")]
        public static IntPtr Load(IntPtr parentWin, IntPtr fileToLoadPtr, int flags)
        {
            var fileToLoad = Marshal.PtrToStringAnsi(fileToLoadPtr)!;
            return LoadInternal(parentWin, fileToLoad, flags);
        }

        [UnmanagedCallersOnly(EntryPoint = "ListLoadW")]
        public static IntPtr LoadW(IntPtr parentWin, IntPtr fileToLoadPtr, int flags)
        {
            var fileToLoad = Marshal.PtrToStringUni(fileToLoadPtr)!;
            return LoadInternal(parentWin, fileToLoad, flags);
        }

        private static IntPtr LoadInternal(IntPtr parentWin, string fileToLoad, int flags)
        {
            var handle = IntPtr.Zero;
            var showFlags = (ShowFlags) flags;
            _callSignature = $"Load ({fileToLoad}, {showFlags.ToString()})";
            try {
                var parent = new ParentWindow(parentWin) {WriteTrace = Plugin.WriteTrace};
                var lister = Plugin.Load(parent, fileToLoad, showFlags);
                if (lister != null) {
                    handle = lister.Handle;
                    if (handle != IntPtr.Zero) {
                        parent.SetLister(handle);
                        TcHandles.AddHandle(handle, lister);
                    }
                }

                TraceCall(TraceLevel.Warning, handle.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return handle;
        }

        #endregion ListLoad

        #region ListLoadNext

        [UnmanagedCallersOnly(EntryPoint = "ListLoadNext")]
        public static int LoadNext(IntPtr parentWin, IntPtr listWin, IntPtr fileToLoadPtr, int flags)
        {
            var fileToLoad = Marshal.PtrToStringAnsi(fileToLoadPtr)!;
            return LoadNextInternal(parentWin, listWin, fileToLoad, flags);
        }

        [UnmanagedCallersOnly(EntryPoint = "ListLoadNextW")]
        public static int LoadNextW(IntPtr parentWin, IntPtr listWin, IntPtr fileToLoadPtr, int flags)
        {
            var fileToLoad = Marshal.PtrToStringUni(fileToLoadPtr)!;
            return LoadNextInternal(parentWin, listWin, fileToLoad, flags);
        }

        private static int LoadNextInternal(IntPtr parentWin, IntPtr listWin, string fileToLoad, int flags)
        {
            var result = ListerResult.Error;
            var showFlags = (ShowFlags) flags;
            _callSignature = $"LoadNext ({listWin.ToString()}, {fileToLoad}, {showFlags.ToString()})";
            try {
                var listerControl = (ILister) TcHandles.GetObject(listWin)!;
                result = Plugin.LoadNext(listerControl, fileToLoad, showFlags);
                TcHandles.UpdateHandle(listWin, listerControl);
                TraceCall(TraceLevel.Warning, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ListLoadNext

        #region ListCloseWindow

        [UnmanagedCallersOnly(EntryPoint = "ListCloseWindow")]
        public static void CloseWindow(IntPtr listWin)
        {
            _callSignature = $"CloseWindow ({listWin.ToString()})";
            try {
                var listerControl = (ILister) TcHandles.GetObject(listWin)!;
                Plugin.CloseWindow(listerControl);
                var count = TcHandles.RemoveHandle(listWin);
                NativeMethods.DestroyWindow(listWin);
                TraceCall(TraceLevel.Warning, $"{count} calls.");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion ListCloseWindow

        #region ListGetDetectString

        // ListGetDetectString functionality is implemented here, not included to Lister Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "ListGetDetectString")]
        public static void GetDetectString(IntPtr detectString, int maxLen)
        {
            _callSignature = "GetDetectString";
            try {
                TcUtils.WriteStringAnsi(Plugin.CanHandle?.Value, detectString, maxLen);
                TraceCall(TraceLevel.Warning, Plugin.CanHandle?.Value);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion ListGetDetectString

        #region ListSearchText

        [UnmanagedCallersOnly(EntryPoint = "ListSearchText")]
        public static int SearchText(IntPtr listWin, IntPtr searchStringPtr, int searchParameter)
        {
            var searchString = Marshal.PtrToStringAnsi(searchStringPtr)!;
            return SearchTextInternal(listWin, searchString, searchParameter);
        }

        [UnmanagedCallersOnly(EntryPoint = "ListSearchTextW")]
        public static int SearchTextW(IntPtr listWin, IntPtr searchStringPtr, int searchParameter)
        {
            var searchString = Marshal.PtrToStringUni(searchStringPtr)!;
            return SearchTextInternal(listWin, searchString, searchParameter);
        }

        private static int SearchTextInternal(IntPtr listWin, string searchString, int searchParameter)
        {
            var result = ListerResult.Error;
            var sp = (SearchParameter) searchParameter;
            _callSignature = $"SearchText ({listWin.ToString()}, {searchString}, {sp.ToString()})";
            try {
                var listerControl = (ILister) TcHandles.GetObject(listWin)!;
                result = Plugin.SearchText(listerControl, searchString, sp);
                TcHandles.UpdateHandle(listWin, listerControl);
                TraceCall(TraceLevel.Warning, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ListSearchText

        #region ListSendCommand

        [UnmanagedCallersOnly(EntryPoint = "ListSendCommand")]
        public static int SendCommand(IntPtr listWin, int command, int parameter)
        {
            var result = ListerResult.Error;
            var cmd = (ListerCommand) command;
            var par = (ShowFlags) parameter;
            _callSignature = $"SendCommand ({listWin.ToString()}, {cmd.ToString()}, {par.ToString()})";
            try {
                var listerControl = (ILister) TcHandles.GetObject(listWin)!;
                result = Plugin.SendCommand(listerControl, cmd, par);
                TcHandles.UpdateHandle(listWin, listerControl);
                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ListSendCommand

        #region ListPrint

        [UnmanagedCallersOnly(EntryPoint = "ListPrint")]
        public static int Print(IntPtr listWin, IntPtr fileToPrintPtr, IntPtr defPrinterPtr, int flags, PrintMargins margins)
        {
            var fileToPrint = Marshal.PtrToStringAnsi(fileToPrintPtr)!;
            var defPrinter = Marshal.PtrToStringAnsi(defPrinterPtr)!;
            return PrintInternal(listWin, fileToPrint, defPrinter, flags, margins);
        }

        [UnmanagedCallersOnly(EntryPoint = "ListPrintW")]
        public static int PrintW(IntPtr listWin, IntPtr fileToPrintPtr, IntPtr defPrinterPtr, int flags, PrintMargins margins)
        {
            var fileToPrint = Marshal.PtrToStringUni(fileToPrintPtr)!;
            var defPrinter = Marshal.PtrToStringUni(defPrinterPtr)!;
            return PrintInternal(listWin, fileToPrint, defPrinter, flags, margins);
        }

        private static int PrintInternal(IntPtr listWin, string fileToPrint, string defPrinter, int flags, PrintMargins margins)
        {
            var result = ListerResult.Error;
            var printFlags = (PrintFlags) flags;
            _callSignature = $"Print ({listWin.ToString()}, {fileToPrint}, {defPrinter}, {printFlags.ToString()})";
            try {
                var listerControl = (ILister) TcHandles.GetObject(listWin)!;
                result = Plugin.Print(listerControl, fileToPrint, defPrinter, printFlags, margins);
                TcHandles.UpdateHandle(listWin, listerControl);
                TraceCall(TraceLevel.Warning, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ListPrint

        #region ListNotificationReceived

        [UnmanagedCallersOnly(EntryPoint = "ListNotificationReceived")]
        public static int NotificationReceived(IntPtr listWin, int message, int wParam, int lParam) // 32, 64 ???
        {
            var result = 0;
            _callSignature = $"NotificationReceived ({listWin.ToString()}, {message}, {wParam}, {lParam})";
            try {
                var listerControl = (ILister) TcHandles.GetObject(listWin)!;
                result = Plugin.NotificationReceived(listerControl, message, wParam, lParam);
                TcHandles.UpdateHandle(listWin, listerControl);
                TraceCall(TraceLevel.Info, result.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion ListNotificationReceived

        #region ListSetDefaultParams

        // ListSetDefaultParams functionality is implemented here, not included to Lister Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "ListSetDefaultParams")]
        public static void SetDefaultParams(IntPtr defParamsPtr)
        {
            var defParams = Marshal.PtrToStructure<PluginDefaultParams>(defParamsPtr);

            _callSignature = "SetDefaultParams";
            try {
                Plugin.DefaultParams = defParams;
                TraceCall(TraceLevel.Info, null);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion ListSetDefaultParams

        #region ListGetPreviewBitmap

        [UnmanagedCallersOnly(EntryPoint = "ListGetPreviewBitmap")]
        public static IntPtr GetPreviewBitmap(IntPtr fileToLoadPtr, int width, int height, IntPtr contentBufPtr, int contentBufLen)
        {
            var fileToLoad = Marshal.PtrToStringAnsi(fileToLoadPtr)!;
            var contentBuf = TcUtils.ReadByteArray(contentBufPtr, contentBufLen);
            return GetPreviewBitmapInternal(fileToLoad, width, height, contentBuf);
        }

        [UnmanagedCallersOnly(EntryPoint = "ListGetPreviewBitmapW")]
        public static IntPtr GetPreviewBitmapW(IntPtr fileToLoadPtr, int width, int height, IntPtr contentBufPtr, int contentBufLen)
        {
            var fileToLoad = Marshal.PtrToStringUni(fileToLoadPtr)!;
            var contentBuf = TcUtils.ReadByteArray(contentBufPtr, contentBufLen);
            return GetPreviewBitmapInternal(fileToLoad, width, height, contentBuf);
        }

        public static IntPtr GetPreviewBitmapInternal(string fileToLoad, int width, int height, byte[] contentBuf)
        {
            IntPtr result;
            _callSignature = $"GetPreviewBitmap '{fileToLoad}' ({width} x {height})";
            try {
                var bitmap = Plugin.GetPreviewBitmap(fileToLoad, width, height, contentBuf);
                if (bitmap != null) {
                    result = bitmap.GetHbitmap(Plugin.BitmapBackgroundColor);
                }
                else {
                    result = IntPtr.Zero;
                }

                TraceCall(TraceLevel.Info, result.Equals(IntPtr.Zero) ? "OK" : "None");
            }
            catch (Exception ex) {
#if TRACE
                TcTrace.TraceOut(TraceLevel.Error, $"{_callSignature}: {ex.Message}", $"ERROR ({Plugin.TraceTitle})");
#endif
                result = IntPtr.Zero;
            }

            return result;
        }

        #endregion ListGetPreviewBitmap

        #region ListSearchDialog

        [UnmanagedCallersOnly(EntryPoint = "ListSearchDialog")]
        public static int SearchDialog(IntPtr listWin, int findNext)
        {
            var result = ListerResult.Error;
            _callSignature = $"SearchDialog ({listWin.ToString()}, {findNext})";
            try {
                var listerControl = (ILister) TcHandles.GetObject(listWin)!;
                result = Plugin.SearchDialog(listerControl, findNext != 0);
                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ListSearchDialog


        public static void ProcessException(Exception ex)
        {
            TcPluginLoader.ProcessException(_plugin, _callSignature, ex);
        }

        public static void TraceCall(TraceLevel level, string? result)
        {
            TcTrace.TraceCall(_plugin, level, _callSignature, result);
            _callSignature = null;
        }
    }
}
