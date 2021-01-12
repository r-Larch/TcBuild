using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using TcPluginBase;
using TcPluginBase.Content;
using TcPluginBase.FileSystem;
using TcPluginBase.Tools;


namespace WfxWrapper {
    public class FsWrapper {
        private static string _callSignature;
        private static FsPlugin _plugin;
        private static FsPlugin Plugin => _plugin ??= TcPluginLoader.GetTcPlugin<FsPlugin>(typeof(PluginClassPlaceholder));
        private static ContentPlugin ContentPlugin => Plugin.ContentPlugin;


        #region File System Plugin Exported Functions

        //Order of TC calls to FS Plugin methods (before first call to FsFindFirst(W)):
        // - FsGetDefRootName (Is called once, when user installs the plugin in Total Commander)
        // - FsContentGetSupportedField - can be called before FsInit if custom columns set is determined
        //                                and plugin panel is visible
        // - FsInit
        // - FsInitW
        // - FsSetDefaultParams
        // - FsSetCryptCallbackW
        // - FsSetCryptCallback
        // - FsExecuteFile(W) (with verb = "MODE I")
        // - FsContentGetDefaultView(W) - can be called here if custom column set is not determined
        //                                and plugin panel is visible
        // - first call to file list cycle:
        //     FsFindFirst - FsFindNext - FsFindClose
        // - FsLinksToLocalFiles

        #region Mandatory Methods

        #region FsInit

        // FsInit, FsInitW functionality is implemented here, not included to FS Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "FsInit")]
        public static int Init(int pluginNumber, IntPtr progressProcPtr, IntPtr logProcPtr, IntPtr requestProcPtr)
        {
            var progressProc = Marshal.GetDelegateForFunctionPointer<ProgressCallback>(progressProcPtr);
            var logProc = Marshal.GetDelegateForFunctionPointer<LogCallback>(logProcPtr);
            var requestProc = Marshal.GetDelegateForFunctionPointer<RequestCallback>(requestProcPtr);

            try {
                _callSignature = "FsInit";
                Plugin.PluginNumber = pluginNumber;
                TcCallback.SetFsPluginCallbacks(progressProc, null, logProc, null, requestProc, null, null, null);

                TraceCall(TraceLevel.Warning, $"PluginNumber={pluginNumber}, {progressProc.Method.MethodHandle.GetFunctionPointer().ToString("X")}, {logProc.Method.MethodHandle.GetFunctionPointer().ToString("X")}, {requestProc.Method.MethodHandle.GetFunctionPointer().ToString("X")}");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsInitW")]
        public static int InitW(int pluginNumber, IntPtr progressProcWPtr, IntPtr logProcWPtr, IntPtr requestProcWPtr)
        {
            var progressProcW = Marshal.GetDelegateForFunctionPointer<ProgressCallbackW>(progressProcWPtr);
            var logProcW = Marshal.GetDelegateForFunctionPointer<LogCallbackW>(logProcWPtr);
            var requestProcW = Marshal.GetDelegateForFunctionPointer<RequestCallbackW>(requestProcWPtr);

            try {
                _callSignature = "FsInitW";
                Plugin.PluginNumber = pluginNumber;
                TcCallback.SetFsPluginCallbacks(null, progressProcW, null, logProcW, null, requestProcW, null, null);

                TraceCall(TraceLevel.Warning, $"PluginNumber={pluginNumber}, {progressProcW.Method.MethodHandle.GetFunctionPointer().ToString("X")}, {logProcW.Method.MethodHandle.GetFunctionPointer().ToString("X")}, {requestProcW.Method.MethodHandle.GetFunctionPointer().ToString("X")}");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return 0;
        }

        #endregion FsInit


        #region FsFindFirst

        [UnmanagedCallersOnly(EntryPoint = "FsFindFirst")]
        public static IntPtr FindFirst(IntPtr pathPtr, IntPtr findFileData)
        {
            var path = Marshal.PtrToStringAnsi(pathPtr);
            return FindFirstInternal(path, findFileData, false);
        }

        [UnmanagedCallersOnly(EntryPoint = "FsFindFirstW")]
        public static IntPtr FindFirstW(IntPtr pathPtr, IntPtr findFileData)
        {
            var path = Marshal.PtrToStringUni(pathPtr);
            return FindFirstInternal(path, findFileData, true);
        }

        private static IntPtr FindFirstInternal(string path, IntPtr findFileData, bool isUnicode)
        {
            var result = NativeMethods.INVALID_HANDLE;
            _callSignature = $"FindFirst ({path})";
            try {
                var o = Plugin.FindFirst(path, out var findData);
                if (o == null)
                    TraceCall(TraceLevel.Info, "<None>");
                else {
                    findData.CopyTo(findFileData, isUnicode);
                    result = TcHandles.AddHandle(o);
                    TraceCall(TraceLevel.Info, findData.FileName);
                }
            }
            catch (NoMoreFilesException) {
                TraceCall(TraceLevel.Info, "<Nothing>");
                NativeMethods.SetLastError(NativeMethods.ERROR_NO_MORE_FILES);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsFindFirst


        #region FsFindNext

        [UnmanagedCallersOnly(EntryPoint = "FsFindNext")]
        public static int FindNext(IntPtr hdl, IntPtr findFileData)
        {
            return FindNextInternal(hdl, findFileData, false) ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsFindNextW")]
        public static int FindNextW(IntPtr hdl, IntPtr findFileData)
        {
            return FindNextInternal(hdl, findFileData, true) ? 1 : 0;
        }

        private static bool FindNextInternal(IntPtr hdl, IntPtr findFileData, bool isUnicode)
        {
            var result = false;
            _callSignature = "FindNext";
            try {
                FindData findData = null;
                var o = TcHandles.GetObject(hdl);
                if (o != null) {
                    result = Plugin.FindNext(ref o, out findData);
                    if (result) {
                        findData.CopyTo(findFileData, isUnicode);
                        TcHandles.UpdateHandle(hdl, o);
                    }
                }

                // !!! may produce much trace info !!!
                TraceCall(TraceLevel.Verbose, result ? findData.FileName : "<None>");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsFindNext


        #region FsFindClose

        [UnmanagedCallersOnly(EntryPoint = "FsFindClose")]
        public static int FindClose(IntPtr hdl)
        {
            _callSignature = "FindClose";
            try {
                var count = 0;

                var o = TcHandles.GetObject(hdl);
                if (o != null) {
                    Plugin.FindClose(o);
                    (o as IDisposable)?.Dispose();
                    count = TcHandles.RemoveHandle(hdl);
                }

                TraceCall(TraceLevel.Info, $"{count} item(s)");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return 0;
        }

        #endregion FsFindClose

        #endregion Mandatory Methods


        #region Optional Methods

        #region FsSetCryptCallback

        // FsSetCryptCallback & FsSetCryptCallbackW functionality is implemented here, not included to FS Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "FsSetCryptCallback")]
        public static void SetCryptCallback(IntPtr cryptProcPtr, int cryptNumber, int flags)
        {
            _callSignature = "SetCryptCallback";
            var cryptProc = Marshal.GetDelegateForFunctionPointer<FsCryptCallback>(cryptProcPtr);
            try {
                TcCallback.SetFsPluginCallbacks(null, null, null, null, null, null, cryptProc, null);
                Plugin.Password ??= new FsPassword(Plugin, cryptNumber, flags);

                TraceCall(TraceLevel.Warning, $"CryptoNumber={cryptNumber}, Flags={flags}, {cryptProc.Method.MethodHandle.GetFunctionPointer().ToString("X")}");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        [UnmanagedCallersOnly(EntryPoint = "FsSetCryptCallbackW")]
        public static void SetCryptCallbackW(IntPtr cryptProcWPtr, int cryptNumber, int flags)
        {
            _callSignature = "SetCryptCallbackW";
            var cryptProcW = Marshal.GetDelegateForFunctionPointer<FsCryptCallbackW>(cryptProcWPtr);
            try {
                TcCallback.SetFsPluginCallbacks(null, null, null, null, null, null, null, cryptProcW);
                Plugin.Password ??= new FsPassword(Plugin, cryptNumber, flags);

                TraceCall(TraceLevel.Warning, $"CryptoNumber={cryptNumber}, Flags={flags}, {cryptProcW.Method.MethodHandle.GetFunctionPointer().ToString("X")}");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion FsSetCryptCallback


        #region FsGetDefRootName

        // FsGetDefRootName functionality is implemented here, not included to FS Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "FsGetDefRootName")]
        public static void GetDefRootName(IntPtr rootName, int maxLen)
        {
            _callSignature = "GetDefRootName";
            try {
                var name = string.IsNullOrEmpty(Plugin.RootName)
                    ? Plugin.Title
                    : Plugin.RootName;

                TcUtils.WriteStringAnsi(name, rootName, maxLen);

                TraceCall(TraceLevel.Warning, name);
            }
            catch (Exception ex) {
                TcUtils.WriteStringAnsi(ex.Message, rootName, maxLen);
                ProcessException(ex);
            }
        }

        #endregion FsGetDefRootName


        #region FsGetFile

        [UnmanagedCallersOnly(EntryPoint = "FsGetFile")]
        public static int GetFile(IntPtr remoteNamePtr, IntPtr localNamePtr, int copyFlags, IntPtr remoteInfo)
        {
            var remoteName = Marshal.PtrToStringAnsi(remoteNamePtr);
            var localName = Marshal.PtrToStringAnsi(localNamePtr);
            var result = GetFileInternal(remoteName, localName, (CopyFlags) copyFlags, remoteInfo);
            if (result.Code == FileSystemExitCode.OK && !string.IsNullOrEmpty(result.FileName)) {
                var newPath = new RemotePath(localName).SetFileName(result.FileName);
                TcUtils.WriteStringAnsi(newPath, localNamePtr, 0);
            }

            return (int) result.Code;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsGetFileW")]
        public static int GetFileW(IntPtr remoteNamePtr, IntPtr localNamePtr, int copyFlags, IntPtr remoteInfo)
        {
            var remoteName = Marshal.PtrToStringUni(remoteNamePtr);
            var localName = Marshal.PtrToStringUni(localNamePtr);
            var result = GetFileInternal(remoteName, localName, (CopyFlags) copyFlags, remoteInfo);
            if (result.Code == FileSystemExitCode.OK && !string.IsNullOrEmpty(result.FileName)) {
                var newPath = new RemotePath(localName).SetFileName(result.FileName);
                TcUtils.WriteStringUni(newPath, localNamePtr, 0);
            }

            return (int) result.Code;
        }

        private static GetFileResult GetFileInternal(string remoteName, in string localName, CopyFlags copyFlags, IntPtr rmtInfo)
        {
            GetFileResult result;
            _callSignature = $"GetFile '{remoteName}' => '{localName}' ({copyFlags.ToString()})";
            var remoteInfo = new RemoteInfo(rmtInfo);
            try {
                result = Plugin.GetFile(remoteName, localName, copyFlags, remoteInfo);

                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = GetFileResult.ReadError;
            }

            return result;
        }

        #endregion FsGetFile


        #region FsPutFile

        [UnmanagedCallersOnly(EntryPoint = "FsPutFile")]
        public static int PutFile(IntPtr localNamePtr, IntPtr remoteNamePtr, int copyFlags)
        {
            var localName = Marshal.PtrToStringAnsi(localNamePtr);
            var remoteName = Marshal.PtrToStringAnsi(remoteNamePtr);
            var result = PutFileInternal(localName, remoteName, (CopyFlags) copyFlags);
            if (result.Code == FileSystemExitCode.OK && !string.IsNullOrEmpty(result.FileName)) {
                var newPath = new RemotePath(remoteName).SetFileName(result.FileName);
                TcUtils.WriteStringAnsi(newPath, remoteNamePtr, 0);
            }

            return (int) result.Code;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsPutFileW")]
        public static int PutFileW(IntPtr localNamePtr, IntPtr remoteNamePtr, int copyFlags)
        {
            var localName = Marshal.PtrToStringUni(localNamePtr);
            var remoteName = Marshal.PtrToStringUni(remoteNamePtr);
            var result = PutFileInternal(localName, remoteName, (CopyFlags) copyFlags);
            if (result.Code == FileSystemExitCode.OK && !string.IsNullOrEmpty(result.FileName)) {
                var newPath = new RemotePath(remoteName).SetFileName(result.FileName);
                TcUtils.WriteStringUni(newPath, remoteNamePtr, 0);
            }

            return (int) result.Code;
        }

        private static PutFileResult PutFileInternal(string localName, string remoteName, CopyFlags copyFlags)
        {
            PutFileResult result;
            _callSignature = $"PutFile '{localName}' => '{remoteName}' ({copyFlags.ToString()})";
            try {
                result = Plugin.PutFile(localName, remoteName, copyFlags);

                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = PutFileResult.ReadError;
            }

            return result;
        }

        #endregion FsPutFile


        #region FsRenMovFile

        [UnmanagedCallersOnly(EntryPoint = "FsRenMovFile")]
        public static int RenMovFile(IntPtr oldNamePtr, IntPtr newNamePtr, int move, int overwrite, IntPtr remoteInfo)
        {
            var oldName = Marshal.PtrToStringAnsi(oldNamePtr);
            var newName = Marshal.PtrToStringAnsi(newNamePtr);
            return RenMovFileInternal(oldName, newName, move != 0, overwrite != 0, remoteInfo);
        }

        [UnmanagedCallersOnly(EntryPoint = "FsRenMovFileW")]
        public static int RenMovFileW(IntPtr oldNamePtr, IntPtr newNamePtr, int move, int overwrite, IntPtr remoteInfo)
        {
            var oldName = Marshal.PtrToStringUni(oldNamePtr);
            var newName = Marshal.PtrToStringUni(newNamePtr);
            return RenMovFileInternal(oldName, newName, move != 0, overwrite != 0, remoteInfo);
        }

        private static int RenMovFileInternal(string oldName, string newName, bool move, bool overwrite, IntPtr rmtInfo)
        {
            var result = RenMovFileResult.NotSupported;
            if (oldName == null || newName == null) {
                return (int) result.Code;
            }

            _callSignature = $"RenMovFile '{oldName}' => '{newName}' ({(move ? "M" : " ") + (overwrite ? "O" : " ")})";
            var remoteInfo = new RemoteInfo(rmtInfo);
            try {
                result = Plugin.RenMovFile(oldName, newName, move, overwrite, remoteInfo);

                TraceCall(TraceLevel.Warning, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = RenMovFileResult.ReadError;
            }

            return (int) result.Code;
        }

        #endregion FsRenMovFile


        #region FsDeleteFile

        [UnmanagedCallersOnly(EntryPoint = "FsDeleteFile")]
        public static int DeleteFile(IntPtr fileNamePtr)
        {
            var fileName = Marshal.PtrToStringAnsi(fileNamePtr);
            return DeleteFileInternal(fileName) ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsDeleteFileW")]
        public static int DeleteFileW(IntPtr fileNamePtr)
        {
            var fileName = Marshal.PtrToStringUni(fileNamePtr);
            return DeleteFileInternal(fileName) ? 1 : 0;
        }

        private static bool DeleteFileInternal(string fileName)
        {
            var result = false;
            _callSignature = $"DeleteFile '{fileName}'";
            try {
                result = Plugin.DeleteFile(fileName);

                TraceCall(TraceLevel.Warning, result ? "OK" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsDeleteFile


        #region FsRemoveDir

        [UnmanagedCallersOnly(EntryPoint = "FsRemoveDir")]
        public static int RemoveDir(IntPtr dirNamePtr)
        {
            var dirName = Marshal.PtrToStringAnsi(dirNamePtr);
            return RemoveDirInternal(dirName) ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsRemoveDirW")]
        public static int RemoveDirW(IntPtr dirNamePtr)
        {
            var dirName = Marshal.PtrToStringUni(dirNamePtr);
            return RemoveDirInternal(dirName) ? 1 : 0;
        }

        private static bool RemoveDirInternal(string dirName)
        {
            var result = false;
            _callSignature = $"RemoveDir '{dirName}'";
            try {
                result = Plugin.RemoveDir(dirName);

                TraceCall(TraceLevel.Warning, result ? "OK" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsRemoveDir


        #region FsMkDir

        [UnmanagedCallersOnly(EntryPoint = "FsMkDir")]
        public static int MkDir(IntPtr dirNamePtr)
        {
            var dirName = Marshal.PtrToStringAnsi(dirNamePtr);
            return MkDirInternal(dirName) ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsMkDirW")]
        public static int MkDirW(IntPtr dirNamePtr)
        {
            var dirName = Marshal.PtrToStringUni(dirNamePtr);
            return MkDirInternal(dirName) ? 1 : 0;
        }

        private static bool MkDirInternal(string dirName)
        {
            var result = false;
            _callSignature = $"MkDir '{dirName}'";
            try {
                result = Plugin.MkDir(dirName);
                TraceCall(TraceLevel.Warning, result ? "OK" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsMkDir


        #region FsExecuteFile

        [UnmanagedCallersOnly(EntryPoint = "FsExecuteFile")]
        public static int ExecuteFile(IntPtr mainWin, IntPtr remoteName, IntPtr verbPtr)
        {
            var rmtName = Marshal.PtrToStringAnsi(remoteName);
            var verb = Marshal.PtrToStringAnsi(verbPtr);
            var result = ExecuteFileInternal(mainWin, rmtName, verb);

            if (result.Type == ExecResult.ExecEnum.SymLink && !string.IsNullOrEmpty(result.SymlinkTarget)) {
                TcUtils.WriteStringAnsi(result.SymlinkTarget, remoteName, 0);
            }

            return (int) result.Type;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsExecuteFileW")]
        public static int ExecuteFileW(IntPtr mainWin, IntPtr remoteName, IntPtr verbPtr)
        {
            var rmtName = Marshal.PtrToStringUni(remoteName);
            var verb = Marshal.PtrToStringUni(verbPtr);
            var result = ExecuteFileInternal(mainWin, rmtName, verb);

            if (result.Type == ExecResult.ExecEnum.SymLink && !string.IsNullOrEmpty(result.SymlinkTarget)) {
                TcUtils.WriteStringUni(result.SymlinkTarget, remoteName, 0);
            }

            return (int) result.Type;
        }

        private static ExecResult ExecuteFileInternal(IntPtr mainWin, RemotePath remoteName, string verb)
        {
            var result = ExecResult.Error;
            _callSignature = $"ExecuteFile '{remoteName}' - {verb}";
            try {
                result = Plugin.ExecuteFile(new TcWindow(mainWin), remoteName, verb);

                var resStr = result.Type.ToString();
                if (result.Type == ExecResult.ExecEnum.SymLink && !string.IsNullOrEmpty(result.SymlinkTarget)) {
                    resStr += " (" + result.SymlinkTarget + ")";
                }

                TraceCall(TraceLevel.Warning, resStr);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsExecuteFile


        #region FsSetAttr

        [UnmanagedCallersOnly(EntryPoint = "FsSetAttr")]
        public static int SetAttr(IntPtr remoteNamePtr, int newAttr)
        {
            var remoteName = Marshal.PtrToStringAnsi(remoteNamePtr);
            return SetAttrInternal(remoteName, newAttr) ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsSetAttrW")]
        public static int SetAttrW(IntPtr remoteNamePtr, int newAttr)
        {
            var remoteName = Marshal.PtrToStringUni(remoteNamePtr);
            return SetAttrInternal(remoteName, newAttr) ? 1 : 0;
        }

        private static bool SetAttrInternal(string remoteName, int newAttr)
        {
            var result = false;
            var attr = (FileAttributes) newAttr;
            _callSignature = $"SetAttr '{remoteName}' ({attr})";
            try {
                result = Plugin.SetAttr(remoteName, attr);

                TraceCall(TraceLevel.Info, result ? "OK" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsSetAttr


        #region FsSetTime

        [UnmanagedCallersOnly(EntryPoint = "FsSetTime")]
        public static int SetTime(IntPtr remoteNamePtr, IntPtr creationTime, IntPtr lastAccessTime, IntPtr lastWriteTime)
        {
            var remoteName = Marshal.PtrToStringAnsi(remoteNamePtr);
            return SetTimeInternal(remoteName, creationTime, lastAccessTime, lastWriteTime) ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsSetTimeW")]
        public static int SetTimeW(IntPtr remoteNamePtr, IntPtr creationTime, IntPtr lastAccessTime, IntPtr lastWriteTime)
        {
            var remoteName = Marshal.PtrToStringUni(remoteNamePtr);
            return SetTimeInternal(remoteName, creationTime, lastAccessTime, lastWriteTime) ? 1 : 0;
        }

        private static bool SetTimeInternal(string remoteName, IntPtr creationTime, IntPtr lastAccessTime, IntPtr lastWriteTime)
        {
            var crTime = TcUtils.ReadDateTime(creationTime);
            var laTime = TcUtils.ReadDateTime(lastAccessTime);
            var lwTime = TcUtils.ReadDateTime(lastWriteTime);

            _callSignature = $"SetTime '{remoteName}' (" +
                             (crTime.HasValue ? $" {crTime.Value:g} #" : " NULL #") +
                             (laTime.HasValue ? $" {laTime.Value:g} #" : " NULL #") +
                             (lwTime.HasValue ? $" {lwTime.Value:g} #" : " NULL #") +
                             ")";

            var result = false;
            try {
                result = Plugin.SetTime(remoteName, crTime, laTime, lwTime);

                TraceCall(TraceLevel.Info, result ? "OK" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsSetTime


        #region FsDisconnect

        [UnmanagedCallersOnly(EntryPoint = "FsDisconnect")]
        public static int Disconnect(IntPtr disconnectRootPtr)
        {
            var disconnectRoot = Marshal.PtrToStringAnsi(disconnectRootPtr);
            return DisconnectInternal(disconnectRoot) ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsDisconnectW")]
        public static int DisconnectW(IntPtr disconnectRootPtr)
        {
            var disconnectRoot = Marshal.PtrToStringUni(disconnectRootPtr);
            return DisconnectInternal(disconnectRoot) ? 1 : 0;
        }

        private static bool DisconnectInternal(string disconnectRoot)
        {
            var result = false;
            _callSignature = $"Disconnect '{disconnectRoot}'";
            try {
                result = Plugin.Disconnect(disconnectRoot);

                TraceCall(TraceLevel.Warning, result ? "OK" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsDisconnect


        #region FsStatusInfo

        [UnmanagedCallersOnly(EntryPoint = "FsStatusInfo")]
        public static void StatusInfo(IntPtr remoteDirPtr, int startEnd, int operation)
        {
            var remoteDir = Marshal.PtrToStringAnsi(remoteDirPtr);
            StatusInfoInternal(remoteDir, startEnd, operation);
        }

        [UnmanagedCallersOnly(EntryPoint = "FsStatusInfoW")]
        public static void StatusInfoW(IntPtr remoteDirPtr, int startEnd, int operation)
        {
            var remoteDir = Marshal.PtrToStringUni(remoteDirPtr);
            StatusInfoInternal(remoteDir, startEnd, operation);
        }

        private static void StatusInfoInternal(string remoteDir, int startEnd, int operation)
        {
            try {
#if TRACE
                _callSignature = $"{((InfoOperation) operation).ToString()} - '{remoteDir}': {((InfoStartEnd) startEnd).ToString()}";
                if (Plugin.WriteStatusInfo) {
                    TcTrace.TraceOut(TraceLevel.Warning, _callSignature, Plugin.TraceTitle, startEnd == (int) InfoStartEnd.End ? -1 : startEnd == (int) InfoStartEnd.Start ? 1 : 0);
                }
#endif
                Plugin.StatusInfo(remoteDir, (InfoStartEnd) startEnd, (InfoOperation) operation);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion FsStatusInfo

        #region FsExtractCustomIcon

        [UnmanagedCallersOnly(EntryPoint = "FsExtractCustomIcon")]
        public static int ExtractCustomIcon(IntPtr remoteName, int extractFlags, IntPtr theIcon)
        {
            var rmtName = Marshal.PtrToStringAnsi(remoteName);
            var inRmtName = rmtName;
            var result = ExtractIconInternal(ref rmtName, extractFlags, theIcon);
            if (result != ExtractIconResult.ExtractIconEnum.UseDefault && !rmtName.Equals(inRmtName, StringComparison.CurrentCultureIgnoreCase)) {
                TcUtils.WriteStringAnsi(rmtName, remoteName, 0);
            }

            return (int) result;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsExtractCustomIconW")]
        public static int ExtractCustomIconW(IntPtr remoteName, int extractFlags, IntPtr theIcon)
        {
            var rmtName = Marshal.PtrToStringUni(remoteName);
            var inRmtName = rmtName;
            var result = ExtractIconInternal(ref rmtName, extractFlags, theIcon);
            if (result != ExtractIconResult.ExtractIconEnum.UseDefault && !rmtName.Equals(inRmtName, StringComparison.CurrentCultureIgnoreCase)) {
                TcUtils.WriteStringUni(rmtName, remoteName, 0);
            }

            return (int) result;
        }

        private static ExtractIconResult.ExtractIconEnum ExtractIconInternal(ref string remoteName, int extractFlags, IntPtr theIcon)
        {
            var flags = (ExtractIconFlags) extractFlags;
            _callSignature = $"ExtractCustomIcon '{remoteName}' ({flags.ToString()})";

            var ret = ExtractIconResult.ExtractIconEnum.UseDefault;
            try {
                var result = Plugin.ExtractCustomIcon(remoteName, flags);
                var resultStr = result.ToString();

                if (result.IconName != null) {
                    remoteName = result.IconName;
                }

                if (result.Icon != null) {
                    Marshal.WriteIntPtr(theIcon, result.Icon.Handle);
                    ret = result.Value;
                }

                // !!! may produce much trace info !!!
                TraceCall(TraceLevel.Verbose, resultStr);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return ret;
        }

        #endregion FsExtractCustomIcon


        #region FsSetDefaultParams

        // FsSetDefaultParams functionality is implemented here, not included to FS Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "FsSetDefaultParams")]
        public static void SetDefaultParams(IntPtr defParamsPtr)
        {
            _callSignature = "SetDefaultParams";
            var defParams = Marshal.PtrToStructure<PluginDefaultParams>(defParamsPtr);
            try {
                Plugin.DefaultParams = defParams;

                TraceCall(TraceLevel.Info, null);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion FsSetDefaultParams


        #region FsGetPreviewBitmap

        [UnmanagedCallersOnly(EntryPoint = "FsGetPreviewBitmap")]
        public static int GetPreviewBitmap(IntPtr remoteName, int width, int height, IntPtr returnedBitmap)
        {
            var rmtName = Marshal.PtrToStringAnsi(remoteName);
            var inRmtName = rmtName;
            var result = GetPreviewBitmapInternal(ref rmtName, width, height, returnedBitmap);
            if (result != PreviewBitmapResult.PreviewBitmapEnum.None && !string.IsNullOrEmpty(rmtName) && !rmtName.Equals(inRmtName, StringComparison.CurrentCultureIgnoreCase)) {
                TcUtils.WriteStringAnsi(rmtName, remoteName, 0);
            }

            return (int) result;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsGetPreviewBitmapW")]
        public static int GetPreviewBitmapW(IntPtr remoteName, int width, int height, IntPtr returnedBitmap)
        {
            var rmtName = Marshal.PtrToStringUni(remoteName);
            var inRmtName = rmtName;
            var result = GetPreviewBitmapInternal(ref rmtName, width, height, returnedBitmap);
            if (result != PreviewBitmapResult.PreviewBitmapEnum.None && !string.IsNullOrEmpty(rmtName) && !rmtName.Equals(inRmtName, StringComparison.CurrentCultureIgnoreCase)) {
                TcUtils.WriteStringUni(rmtName, remoteName, 0);
            }

            return (int) result;
        }

        private static PreviewBitmapResult.PreviewBitmapEnum GetPreviewBitmapInternal(ref string remoteName, int width, int height, IntPtr returnedBitmap)
        {
            _callSignature = $"GetPreviewBitmap '{remoteName}' ({width} x {height})";

            var ret = PreviewBitmapResult.PreviewBitmapEnum.None;
            try {
                var result = Plugin.GetPreviewBitmap(remoteName, width, height);

                if (result.Bitmap != null) {
                    var extrBitmap = result.Bitmap.GetHbitmap();
                    Marshal.WriteIntPtr(returnedBitmap, extrBitmap);
                }

                if (result.BitmapName != null) {
                    remoteName = result.BitmapName;
                }

                ret = result.Value;
                if (result.Cache) {
                    ret |= PreviewBitmapResult.PreviewBitmapEnum.Cache;
                }

                // !!! may produce much trace info !!!
                TraceCall(TraceLevel.Verbose, $"{ret} ({result.BitmapName})");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return ret;
        }

        #endregion FsGetPreviewBitmap


        #region FsLinksToLocalFiles

        // FsLinksToLocalFiles functionality is implemented here, not included to FS Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "FsLinksToLocalFiles")]
        public static int LinksToLocalFiles()
        {
            var result = false;
            _callSignature = "LinksToLocalFiles";
            try {
                result = Plugin.IsTempFilePanel();

                TraceCall(TraceLevel.Info, result ? "Yes" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result ? 1 : 0;
        }

        #endregion FsLinksToLocalFiles


        #region FsGetLocalName

        [UnmanagedCallersOnly(EntryPoint = "FsGetLocalName")]
        public static int GetLocalName(IntPtr remoteName, int maxLen)
        {
            var rmtName = Marshal.PtrToStringAnsi(remoteName);
            var result = GetLocalNameInternal(ref rmtName, maxLen);
            if (result) {
                TcUtils.WriteStringAnsi(rmtName, remoteName, 0);
            }

            return result ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsGetLocalNameW")]
        public static int GetLocalNameW(IntPtr remoteName, int maxLen)
        {
            var rmtName = Marshal.PtrToStringUni(remoteName);
            var result = GetLocalNameInternal(ref rmtName, maxLen);
            if (result) {
                TcUtils.WriteStringUni(rmtName, remoteName, 0);
            }

            return result ? 1 : 0;
        }

        private static bool GetLocalNameInternal(ref string remoteName, int maxLen)
        {
            var result = false;
            _callSignature = $"GetLocalName '{remoteName}'";
            try {
                var localName = Plugin.GetLocalName(remoteName, maxLen);
                if (localName != null) {
                    remoteName = localName;
                    result = true;
                }

                // !!! may produce much trace info !!!
                TraceCall(TraceLevel.Verbose, result ? remoteName : "<N/A>");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsGetLocalName


        // FsGetBackgroundFlags functionality is implemented here, not included to FS Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "FsGetBackgroundFlags")]
        public static int GetBackgroundFlags()
        {
            var result = FsBackgroundFlags.None;
            _callSignature = "GetBackgroundFlags";
            try {
                result = Plugin.BackgroundFlags;

                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion Optional Methods

        #endregion File System Plugin Exported Functions


        #region Content Plugin Exported Functions

        #region FsContentGetSupportedField

        [UnmanagedCallersOnly(EntryPoint = "FsContentGetSupportedField")]
        public static int GetSupportedField(int fieldIndex, IntPtr fieldName, IntPtr units, int maxLen)
        {
            var result = ContentFieldType.NoMoreFields;
            _callSignature = $"ContentGetSupportedField ({fieldIndex})";
            try {
                if (ContentPlugin != null) {
                    result = ContentPlugin.GetSupportedField(fieldIndex, out var fieldNameStr, out var unitsStr, maxLen);
                    if (result != ContentFieldType.NoMoreFields) {
                        if (string.IsNullOrEmpty(fieldNameStr))
                            result = ContentFieldType.NoMoreFields;
                        else {
                            TcUtils.WriteStringAnsi(fieldNameStr, fieldName, maxLen);
                            if (string.IsNullOrEmpty(unitsStr))
                                units = IntPtr.Zero;
                            else
                                TcUtils.WriteStringAnsi(unitsStr, units, maxLen);
                        }
                    }

                    // !!! may produce much trace info !!!
                    TraceCall(TraceLevel.Verbose, $"{result.ToString()} - {fieldNameStr} - {unitsStr}");
                }
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion FsContentGetSupportedField

        #region FsContentGetValue

        [UnmanagedCallersOnly(EntryPoint = "FsContentGetValue")]
        public static int GetValue(IntPtr fileNamePtr, int fieldIndex, int unitIndex, IntPtr fieldValue, int maxLen, int flags)
        {
            var fileName = Marshal.PtrToStringAnsi(fileNamePtr);
            return GetValueInternal(fileName, fieldIndex, unitIndex, fieldValue, maxLen, flags);
        }

        [UnmanagedCallersOnly(EntryPoint = "FsContentGetValueW")]
        public static int GetValueW(IntPtr fileNamePtr, int fieldIndex, int unitIndex, IntPtr fieldValue, int maxLen, int flags)
        {
            var fileName = Marshal.PtrToStringUni(fileNamePtr);
            return GetValueInternal(fileName, fieldIndex, unitIndex, fieldValue, maxLen, flags);
        }

        private static int GetValueInternal(string fileName, int fieldIndex, int unitIndex, IntPtr fieldValue, int maxLen, int flags)
        {
            GetValueResult result;
            var fieldType = ContentFieldType.NoMoreFields;
            var gvFlags = (GetValueFlags) flags;
            fileName = fileName.Substring(1);
            _callSignature = $"ContentGetValue '{fileName}' ({fieldIndex}/{unitIndex}/{gvFlags.ToString()})";
            try {
                result = ContentPlugin.GetValue(fileName, fieldIndex, unitIndex, maxLen, gvFlags, out var fieldValueStr, out fieldType);
                if (
                    result == GetValueResult.Success ||
                    result == GetValueResult.Delayed ||
                    result == GetValueResult.OnDemand
                ) {
                    var resultType = result == GetValueResult.Success ? fieldType : ContentFieldType.WideString;
                    new ContentValue(fieldValueStr, resultType).CopyTo(fieldValue);
                }

                // !!! may produce much trace info !!!
                TraceCall(TraceLevel.Verbose, $"{result.ToString()} - {fieldValueStr}");
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = GetValueResult.NoSuchField;
            }

            return result == GetValueResult.Success ? (int) fieldType : (int) result;
        }

        #endregion FsContentGetValue

        #region FsContentStopGetValue

        [UnmanagedCallersOnly(EntryPoint = "FsContentStopGetValue")]
        public static void StopGetValue(IntPtr fileNamePtr)
        {
            var fileName = Marshal.PtrToStringAnsi(fileNamePtr);
            StopGetValueInternal(fileName);
        }

        [UnmanagedCallersOnly(EntryPoint = "FsContentStopGetValueW")]
        public static void StopGetValueW(IntPtr fileNamePtr)
        {
            var fileName = Marshal.PtrToStringUni(fileNamePtr);
            StopGetValueInternal(fileName);
        }

        private static void StopGetValueInternal(string fileName)
        {
            _callSignature = "ContentStopGetValue";
            try {
                fileName = fileName.Substring(1);
                ContentPlugin.StopGetValue(fileName);

                TraceCall(TraceLevel.Info, null);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion FsContentStopGetValue

        #region FsContentGetDefaultSortOrder

        [UnmanagedCallersOnly(EntryPoint = "FsContentGetDefaultSortOrder")]
        public static int GetDefaultSortOrder(int fieldIndex)
        {
            var result = DefaultSortOrder.Asc;
            _callSignature = $"ContentGetDefaultSortOrder ({fieldIndex})";
            try {
                result = ContentPlugin.GetDefaultSortOrder(fieldIndex);

                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion FsContentStopGetValue

        #region FsContentPluginUnloading

        [UnmanagedCallersOnly(EntryPoint = "FsContentPluginUnloading")]
        public static void PluginUnloading()
        {
            if (ContentPlugin != null) {
                _callSignature = "ContentPluginUnloading";
                try {
                    ContentPlugin.PluginUnloading();

                    TraceCall(TraceLevel.Info, null);
                }
                catch (Exception ex) {
                    ProcessException(ex);
                }
            }
        }

        #endregion FsContentPluginUnloading

        #region FsContentGetSupportedFieldFlags

        [UnmanagedCallersOnly(EntryPoint = "FsContentGetSupportedFieldFlags")]
        public static int GetSupportedFieldFlags(int fieldIndex)
        {
            var result = SupportedFieldOptions.None;
            _callSignature = $"ContentGetSupportedFieldFlags ({fieldIndex})";
            try {
                result = ContentPlugin.GetSupportedFieldFlags(fieldIndex);

                TraceCall(TraceLevel.Verbose, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion FsContentGetSupportedFieldFlags

        #region FsContentSetValue

        [UnmanagedCallersOnly(EntryPoint = "FsContentSetValue")]
        public static int SetValue(IntPtr fileNamePtr, int fieldIndex, int unitIndex, int fieldType, IntPtr fieldValue, int flags)
        {
            var fileName = Marshal.PtrToStringAnsi(fileNamePtr);
            return SetValueInternal(fileName, fieldIndex, unitIndex, fieldType, fieldValue, flags);
        }

        [UnmanagedCallersOnly(EntryPoint = "FsContentSetValueW")]
        public static int SetValueW(IntPtr fileNamePtr, int fieldIndex, int unitIndex, int fieldType, IntPtr fieldValue, int flags)
        {
            var fileName = Marshal.PtrToStringUni(fileNamePtr);
            return SetValueInternal(fileName, fieldIndex, unitIndex, fieldType, fieldValue, flags);
        }

        private static int SetValueInternal(string fileName, int fieldIndex, int unitIndex, int fieldType, IntPtr fieldValue, int flags)
        {
            SetValueResult result;
            var fldType = (ContentFieldType) fieldType;
            var svFlags = (SetValueFlags) flags;
            fileName = fileName.Substring(1);
            _callSignature = $"ContentSetValue '{fileName}' ({fieldIndex}/{unitIndex}/{svFlags.ToString()})";
            try {
                var value = new ContentValue(fieldValue, fldType);
                result = ContentPlugin.SetValue(fileName, fieldIndex, unitIndex, fldType, value.StrValue, svFlags);

                TraceCall(TraceLevel.Info, $"{result.ToString()} - {value.StrValue}");
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = SetValueResult.NoSuchField;
            }

            return (int) result;
        }

        #endregion FsContentSetValue

        #region FsContentGetDefaultView

        [UnmanagedCallersOnly(EntryPoint = "FsContentGetDefaultView")]
        public static int GetDefaultView(IntPtr viewContents, IntPtr viewHeaders, IntPtr viewWidths, IntPtr viewOptions, int maxLen)
        {
            var result = GetDefaultViewFs(out var contents, out var headers, out var widths, out var options, maxLen);
            if (result) {
                TcUtils.WriteStringAnsi(contents, viewContents, maxLen);
                TcUtils.WriteStringAnsi(headers, viewHeaders, maxLen);
                TcUtils.WriteStringAnsi(widths, viewWidths, maxLen);
                TcUtils.WriteStringAnsi(options, viewOptions, maxLen);

                return true ? 1 : 0;
            }

            return false ? 1 : 0;
        }

        [UnmanagedCallersOnly(EntryPoint = "FsContentGetDefaultViewW")]
        public static int GetDefaultViewW(IntPtr viewContents, IntPtr viewHeaders, IntPtr viewWidths, IntPtr viewOptions, int maxLen)
        {
            var result = GetDefaultViewFs(out var contents, out var headers, out var widths, out var options, maxLen);
            if (result) {
                TcUtils.WriteStringUni(contents, viewContents, maxLen);
                TcUtils.WriteStringUni(headers, viewHeaders, maxLen);
                TcUtils.WriteStringUni(widths, viewWidths, maxLen);
                TcUtils.WriteStringUni(options, viewOptions, maxLen);

                return true ? 1 : 0;
            }

            return false ? 1 : 0;
        }

        public static bool GetDefaultViewFs(out string viewContents, out string viewHeaders, out string viewWidths, out string viewOptions, int maxLen)
        {
            var result = false;
            viewContents = null;
            viewHeaders = null;
            viewWidths = null;
            viewOptions = null;
            _callSignature = "ContentGetDefaultView";
            try {
                if (ContentPlugin != null) {
                    result = ContentPlugin.GetDefaultView(out viewContents, out viewHeaders, out viewWidths, out viewOptions, maxLen);

                    TraceCall(TraceLevel.Info, $"\n  {viewContents}\n  {viewHeaders}\n  {viewWidths}\n  {viewOptions}");
                }
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        #endregion FsContentGetDefaultView

        #endregion Content Plugin Exported Functions


        #region Tracing & Exceptions

        private static void ProcessException(Exception ex)
        {
            TcPluginLoader.ProcessException(_plugin, _callSignature, ex);
        }

        private static void TraceCall(TraceLevel level, string result)
        {
#if TRACE
            TcTrace.TraceCall(_plugin, level, _callSignature, result);
            _callSignature = null;
#endif
        }

        #endregion Tracing & Exceptions
    }
}
