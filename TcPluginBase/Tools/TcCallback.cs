using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TcPluginBase.Content;
using TcPluginBase.FileSystem;
using TcPluginBase.Packer;


namespace TcPluginBase.Tools {
    public static class TcCallback {
        #region Constants

        //public const string PluginCallbackDataName = "PluginCallbackData";
        public const int CryptPasswordMaxLen = NativeMethods.MAX_PATH_UNI;

#if TRACE
        // Trace Messages
        private const string Callback = "Callback";
#endif

        #endregion Constants

        #region Variables

#if TRACE
        // to trace Progress callback
        const int ProgressTraceChunk = 25;
        private static int prevPercDone = -ProgressTraceChunk - 1;
#endif

        #endregion Variables


        #region Main Handler

        public static void HandleTcPluginEvent(object sender, PluginEventArgs e)
        {
            switch (e) {
                case CryptEventArgs args:
                    CryptCallback(args);
                    break;
                case ProgressEventArgs args:
                    FsProgressCallback(args);
                    break;
                case LogEventArgs args:
                    FsLogCallback(args);
                    break;
                case RequestEventArgs args:
                    FsRequestCallback(args);
                    break;
                case ContentProgressEventArgs args:
                    ContentProgressCallback(args);
                    break;
                case PackerProcessEventArgs args:
                    PackerProcessCallback(args);
                    break;
                case PackerChangeVolEventArgs args:
                    PackerChangeVolCallback(args);
                    break;
            }
        }

        #endregion Main Handler


        #region FS Callbacks

        private static ProgressCallback progressCallback;
        private static ProgressCallbackW progressCallbackW;
        private static LogCallback logCallback;
        private static LogCallbackW logCallbackW;
        private static RequestCallback requestCallback;
        private static RequestCallbackW requestCallbackW;
        private static FsCryptCallback fsCryptCallback;
        private static FsCryptCallbackW fsCryptCallbackW;

        public static void SetFsPluginCallbacks(ProgressCallback progress, ProgressCallbackW progressW, LogCallback log, LogCallbackW logW, RequestCallback request, RequestCallbackW requestW, FsCryptCallback crypt, FsCryptCallbackW cryptW)
        {
            if (progressCallback == null)
                progressCallback = progress;
            if (progressCallbackW == null)
                progressCallbackW = progressW;
            if (logCallback == null)
                logCallback = log;
            if (logCallbackW == null)
                logCallbackW = logW;
            if (requestCallback == null)
                requestCallback = request;
            if (requestCallbackW == null)
                requestCallbackW = requestW;
            if (fsCryptCallback == null)
                fsCryptCallback = crypt;
            if (fsCryptCallbackW == null)
                fsCryptCallbackW = cryptW;
        }

        public static void FsProgressCallback(ProgressEventArgs e)
        {
            if (progressCallbackW != null || progressCallback != null) {
                var pluginNumber = e.PluginNumber;
                var sourceName = e.SourceName;
                var targetName = e.TargetName;
                var percentDone = e.PercentDone;

                if (progressCallbackW != null) {
                    e.Result = progressCallbackW(pluginNumber, sourceName, targetName, percentDone);
                }
                else if (progressCallback != null) {
                    e.Result = progressCallback(pluginNumber, sourceName, targetName, percentDone);
                }

#if TRACE
                if (percentDone - prevPercDone >= ProgressTraceChunk || percentDone == 100) {
                    TraceOut(TraceLevel.Verbose, $"OnProgress ({pluginNumber}, {percentDone}): {sourceName} => {targetName} - {e.Result}.", Callback);
                    if (percentDone == 100) {
                        prevPercDone = -ProgressTraceChunk - 1;
                    }
                    else {
                        prevPercDone = percentDone;
                    }
                }
#endif
            }
        }

        public static void FsLogCallback(LogEventArgs e)
        {
            if (logCallbackW != null || logCallback != null) {
                if (logCallbackW != null)
                    logCallbackW(e.PluginNumber, e.MessageType, e.LogText);
                else
                    logCallback(e.PluginNumber, e.MessageType, e.LogText);
#if TRACE
                TraceOut(TraceLevel.Info, $"OnLog ({e.PluginNumber}, {((LogMsgType) e.MessageType).ToString()}): {e.LogText}.", Callback);
#endif
            }
        }

        public static void FsRequestCallback(RequestEventArgs e)
        {
            if (requestCallbackW != null || requestCallback != null) {
                var retText = IntPtr.Zero;
                if (e.RequestType < (int) RequestType.MsgOk) {
                    if (requestCallbackW != null) {
                        retText = Marshal.AllocHGlobal(e.MaxLen * 2);
                        Marshal.Copy(new char[e.MaxLen], 0, retText, e.MaxLen);
                    }
                    else {
                        retText = Marshal.AllocHGlobal(e.MaxLen);
                        Marshal.Copy(new byte[e.MaxLen], 0, retText, e.MaxLen);
                    }
                }

                try {
                    if (retText != IntPtr.Zero && !string.IsNullOrEmpty(e.ReturnedText)) {
                        if (requestCallbackW != null)
                            Marshal.Copy(e.ReturnedText.ToCharArray(), 0, retText, e.ReturnedText.Length);
                        else
                            TcUtils.WriteStringAnsi(e.ReturnedText, retText, 0);
                    }

                    if (requestCallbackW != null) {
                        e.Result = requestCallbackW(e.PluginNumber, e.RequestType, e.CustomTitle, e.CustomText, retText, e.MaxLen) ? 1 : 0;
                    }
                    else {
                        e.Result = requestCallback(e.PluginNumber, e.RequestType, e.CustomTitle, e.CustomText, retText, e.MaxLen) ? 1 : 0;
                    }
#if TRACE
                    var traceStr = $"OnRequest ({e.PluginNumber}, {((RequestType) e.RequestType).ToString()}): {e.ReturnedText}";
#endif
                    if (e.Result != 0 && retText != IntPtr.Zero) {
                        e.ReturnedText = (requestCallbackW != null) ? Marshal.PtrToStringUni(retText) : Marshal.PtrToStringAnsi(retText);
#if TRACE
                        traceStr += " => " + e.ReturnedText;
#endif
                    }
#if TRACE
                    TraceOut(TraceLevel.Verbose, $"{traceStr} - {e.Result}.", Callback);
#endif
                }
                finally {
                    if (retText != IntPtr.Zero) {
                        Marshal.FreeHGlobal(retText);
                    }
                }
            }
        }

        #endregion FS Callbacks


        #region Content Callbacks

        private static ContentProgressCallback contentProgressCallback;

        public static void SetContentPluginCallback(ContentProgressCallback contentProgress)
        {
            contentProgressCallback = contentProgress;
        }

        public static void ContentProgressCallback(ContentProgressEventArgs e)
        {
            if (contentProgressCallback != null) {
                e.Result = contentProgressCallback(e.NextBlockData);
#if TRACE
                TraceOut(TraceLevel.Verbose, $"OnCompareProgress ({e.NextBlockData}) - {e.Result}.", Callback);
#endif
            }
        }

        #endregion Content Callbacks


        #region Packer Callbacks

        private static ChangeVolCallback changeVolCallback;
        private static ChangeVolCallbackW changeVolCallbackW;
        private static ProcessDataCallback processDataCallback;
        private static ProcessDataCallbackW processDataCallbackW;
        private static PkCryptCallback pkCryptCallback;
        private static PkCryptCallbackW pkCryptCallbackW;

        public static void SetPackerPluginCallbacks(ChangeVolCallback changeVol, ChangeVolCallbackW changeVolW, ProcessDataCallback processData, ProcessDataCallbackW processDataW, PkCryptCallback crypt, PkCryptCallbackW cryptW)
        {
            if (changeVolCallback == null)
                changeVolCallback = changeVol;
            if (changeVolCallbackW == null)
                changeVolCallbackW = changeVolW;
            if (processDataCallback == null)
                processDataCallback = processData;
            if (processDataCallbackW == null)
                processDataCallbackW = processDataW;
            if (pkCryptCallback == null)
                pkCryptCallback = crypt;
            if (pkCryptCallbackW == null)
                pkCryptCallbackW = cryptW;
        }

        public static void PackerProcessCallback(PackerProcessEventArgs e)
        {
            if (processDataCallbackW != null || processDataCallback != null) {
                string fileName = e.FileName;
                int size = e.Size;

                if (processDataCallbackW != null)
                    e.Result = processDataCallbackW(fileName, size);
                else if (processDataCallback != null)
                    e.Result = processDataCallback(fileName, size);
#if TRACE
                TraceOut(TraceLevel.Verbose, $"OnProcessData ({fileName}, {size}) - {e.Result}.", Callback);
#endif
            }
        }

        public static void PackerChangeVolCallback(PackerChangeVolEventArgs e)
        {
            if (changeVolCallbackW != null || changeVolCallback != null) {
                string arcName = e.ArcName;
                int mode = e.Mode;

                if (changeVolCallbackW != null)
                    e.Result = changeVolCallbackW(arcName, mode);
                else if (changeVolCallback != null)
                    e.Result = changeVolCallback(arcName, mode);
#if TRACE
                TraceOut(TraceLevel.Verbose, $"OnChangeVol ({arcName}, {mode}) - {e.Result}.", Callback);
#endif
            }
        }

        public static void CryptCallback(CryptEventArgs e)
        {
            bool isUnicode;
            var loadPassword = e.Mode == 2 || e.Mode == 3; // LoadPassword or LoadPasswordNoUI
            if (e.PluginNumber < 0) {
                // Packer plugin call
                if (pkCryptCallbackW == null && pkCryptCallback == null)
                    return;
                isUnicode = (pkCryptCallbackW != null);
            }
            else {
                // File System plugin call
                if (fsCryptCallbackW == null && fsCryptCallback == null)
                    return;
                isUnicode = (fsCryptCallbackW != null);
            }

            var pswText = IntPtr.Zero;
            try {
                if (isUnicode) {
                    if (loadPassword) {
                        pswText = Marshal.AllocHGlobal(CryptPasswordMaxLen * 2);
                    }
                    else if (!string.IsNullOrEmpty(e.Password)) {
                        pswText = Marshal.StringToHGlobalUni(e.Password);
                    }

                    e.Result = (e.PluginNumber < 0)
                        ? pkCryptCallbackW(e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen)
                        : fsCryptCallbackW(e.PluginNumber, e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen);
                }
                else {
                    if (loadPassword) {
                        pswText = Marshal.AllocHGlobal(CryptPasswordMaxLen);
                    }
                    else if (!string.IsNullOrEmpty(e.Password)) {
                        pswText = Marshal.StringToHGlobalAnsi(e.Password);
                    }

                    e.Result = (e.PluginNumber < 0)
                        ? pkCryptCallback(e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen)
                        : fsCryptCallback(e.PluginNumber, e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen);
                }

                // tracing
#if TRACE
                var traceStr = $"OnCrypt ({e.PluginNumber}, {e.CryptoNumber}, {e.Mode}): {e.StoreName}";
#endif

                if (loadPassword && e.Result == 0) {
                    e.Password = isUnicode ? Marshal.PtrToStringUni(pswText) : Marshal.PtrToStringAnsi(pswText);
#if TRACE
                    traceStr += " => (PASSWORD)"; //+ e.Password;
#endif
                }
                else
                    e.Password = string.Empty;

                // tracing
#if TRACE
                TraceOut(TraceLevel.Info, $"{traceStr} - {((CryptResult) e.Result).ToString()}.", Callback);
#endif
            }
            finally {
                if (pswText != IntPtr.Zero)
                    Marshal.FreeHGlobal(pswText);
            }
        }

        #endregion Packer Callbacks


#if TRACE
        private static void TraceOut(TraceLevel level, string text, string category)
        {
            TcTrace.TraceOut(level, text, category);
        }
#endif
    }
}
