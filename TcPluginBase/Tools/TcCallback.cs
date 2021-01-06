using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TcPluginBase.Content;
using TcPluginBase.FileSystem;
using TcPluginBase.Packer;


namespace TcPluginBase.Tools {
    internal static class TcCallback {
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

        private static ProgressCallback _progressCallback;
        private static ProgressCallbackW _progressCallbackW;
        private static LogCallback _logCallback;
        private static LogCallbackW _logCallbackW;
        private static RequestCallback _requestCallback;
        private static RequestCallbackW _requestCallbackW;
        private static FsCryptCallback _fsCryptCallback;
        private static FsCryptCallbackW _fsCryptCallbackW;

        public static void SetFsPluginCallbacks(ProgressCallback progress, ProgressCallbackW progressW, LogCallback log, LogCallbackW logW, RequestCallback request, RequestCallbackW requestW, FsCryptCallback crypt, FsCryptCallbackW cryptW)
        {
            _progressCallback ??= progress;
            _progressCallbackW ??= progressW;
            _logCallback ??= log;
            _logCallbackW ??= logW;
            _requestCallback ??= request;
            _requestCallbackW ??= requestW;
            _fsCryptCallback ??= crypt;
            _fsCryptCallbackW ??= cryptW;
        }

        public static void FsProgressCallback(ProgressEventArgs e)
        {
            if (_progressCallbackW != null || _progressCallback != null) {
                var pluginNumber = e.PluginNumber;
                var sourceName = e.SourceName;
                var targetName = e.TargetName;
                var percentDone = e.PercentDone;

                if (_progressCallbackW != null) {
                    e.Result = _progressCallbackW(pluginNumber, sourceName, targetName, percentDone);
                }
                else if (_progressCallback != null) {
                    e.Result = _progressCallback(pluginNumber, sourceName, targetName, percentDone);
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
            if (_logCallbackW != null || _logCallback != null) {
                if (_logCallbackW != null)
                    _logCallbackW(e.PluginNumber, e.MessageType, e.LogText);
                else
                    _logCallback(e.PluginNumber, e.MessageType, e.LogText);
#if TRACE
                TraceOut(TraceLevel.Info, $"OnLog ({e.PluginNumber}, {((LogMsgType) e.MessageType).ToString()}): {e.LogText}.", Callback);
#endif
            }
        }

        public static void FsRequestCallback(RequestEventArgs e)
        {
            if (_requestCallbackW != null || _requestCallback != null) {
                var retText = IntPtr.Zero;
                if (e.RequestType < (int) RequestType.MsgOk) {
                    if (_requestCallbackW != null) {
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
                        if (_requestCallbackW != null)
                            Marshal.Copy(e.ReturnedText.ToCharArray(), 0, retText, e.ReturnedText.Length);
                        else
                            TcUtils.WriteStringAnsi(e.ReturnedText, retText, 0);
                    }

                    if (_requestCallbackW != null) {
                        e.Result = _requestCallbackW(e.PluginNumber, e.RequestType, e.CustomTitle, e.CustomText, retText, e.MaxLen) ? 1 : 0;
                    }
                    else {
                        e.Result = _requestCallback(e.PluginNumber, e.RequestType, e.CustomTitle, e.CustomText, retText, e.MaxLen) ? 1 : 0;
                    }
#if TRACE
                    var traceStr = $"OnRequest ({e.PluginNumber}, {(RequestType) e.RequestType}): {e.ReturnedText}";
#endif
                    if (e.Result != 0 && retText != IntPtr.Zero) {
                        e.ReturnedText = (_requestCallbackW != null) ? Marshal.PtrToStringUni(retText) : Marshal.PtrToStringAnsi(retText);
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

        private static ContentProgressCallback _contentProgressCallback;

        public static void SetContentPluginCallback(ContentProgressCallback contentProgress)
        {
            _contentProgressCallback = contentProgress;
        }

        public static void ContentProgressCallback(ContentProgressEventArgs e)
        {
            if (_contentProgressCallback != null) {
                e.Result = _contentProgressCallback(e.NextBlockData);
#if TRACE
                TraceOut(TraceLevel.Verbose, $"OnCompareProgress ({e.NextBlockData}) - {e.Result}.", Callback);
#endif
            }
        }

        #endregion Content Callbacks


        #region Packer Callbacks

        private static ChangeVolCallback _changeVolCallback;
        private static ChangeVolCallbackW _changeVolCallbackW;
        private static ProcessDataCallback _processDataCallback;
        private static ProcessDataCallbackW _processDataCallbackW;
        private static PkCryptCallback _pkCryptCallback;
        private static PkCryptCallbackW _pkCryptCallbackW;

        public static void SetPackerPluginCallbacks(ChangeVolCallback changeVol, ChangeVolCallbackW changeVolW, ProcessDataCallback processData, ProcessDataCallbackW processDataW, PkCryptCallback crypt, PkCryptCallbackW cryptW)
        {
            _changeVolCallback ??= changeVol;
            _changeVolCallbackW ??= changeVolW;
            _processDataCallback ??= processData;
            _processDataCallbackW ??= processDataW;
            _pkCryptCallback ??= crypt;
            _pkCryptCallbackW ??= cryptW;
        }

        public static void PackerProcessCallback(PackerProcessEventArgs e)
        {
            if (_processDataCallbackW != null || _processDataCallback != null) {
                string fileName = e.FileName;
                int size = e.Size;

                if (_processDataCallbackW != null)
                    e.Result = _processDataCallbackW(fileName, size);
                else if (_processDataCallback != null)
                    e.Result = _processDataCallback(fileName, size);
#if TRACE
                TraceOut(TraceLevel.Verbose, $"OnProcessData ({fileName}, {size}) - {e.Result}.", Callback);
#endif
            }
        }

        public static void PackerChangeVolCallback(PackerChangeVolEventArgs e)
        {
            if (_changeVolCallbackW != null || _changeVolCallback != null) {
                string arcName = e.ArcName;
                int mode = e.Mode;

                if (_changeVolCallbackW != null)
                    e.Result = _changeVolCallbackW(arcName, mode);
                else if (_changeVolCallback != null)
                    e.Result = _changeVolCallback(arcName, mode);
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
                if (_pkCryptCallbackW == null && _pkCryptCallback == null)
                    return;
                isUnicode = (_pkCryptCallbackW != null);
            }
            else {
                // File System plugin call
                if (_fsCryptCallbackW == null && _fsCryptCallback == null)
                    return;
                isUnicode = (_fsCryptCallbackW != null);
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
                        ? _pkCryptCallbackW(e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen)
                        : _fsCryptCallbackW(e.PluginNumber, e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen);
                }
                else {
                    if (loadPassword) {
                        pswText = Marshal.AllocHGlobal(CryptPasswordMaxLen);
                    }
                    else if (!string.IsNullOrEmpty(e.Password)) {
                        pswText = Marshal.StringToHGlobalAnsi(e.Password);
                    }

                    e.Result = (e.PluginNumber < 0)
                        ? _pkCryptCallback(e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen)
                        : _fsCryptCallback(e.PluginNumber, e.CryptoNumber, e.Mode, e.StoreName, pswText, CryptPasswordMaxLen);
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
