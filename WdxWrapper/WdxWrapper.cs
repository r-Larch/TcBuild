using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TcPluginBase;
using TcPluginBase.Content;
using TcPluginBase.Packer;
using TcPluginBase.Tools;


namespace WdxWrapper {
    public class ContentWrapper {
        private static string? _callSignature;
        private static ContentPlugin? _plugin;
        private static ContentPlugin Plugin => _plugin ??= TcPluginLoader.GetTcPlugin<ContentPlugin>(typeof(PluginClassPlaceholder));


        private ContentWrapper()
        {
        }


        #region Content Plugin Exported Functions

        #region ContentGetSupportedField

        [UnmanagedCallersOnly(EntryPoint = "ContentGetSupportedField")]
        public static int GetSupportedField(int fieldIndex, IntPtr fieldName, IntPtr units, int maxLen)
        {
            var result = ContentFieldType.NoMoreFields;
            _callSignature = $"ContentGetSupportedField ({fieldIndex})";
            try {
                result = Plugin.GetSupportedField(fieldIndex, out var fieldNameStr, out var unitsStr, maxLen);
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
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ContentGetSupportedField

        #region ContentGetValue

        [UnmanagedCallersOnly(EntryPoint = "ContentGetValue")]
        public static int GetValue([MarshalAs(UnmanagedType.LPStr)] IntPtr fileName, int fieldIndex, int unitIndex, IntPtr fieldValue, int maxLen, int flags)
        {
            return GetValueInternal(Marshal.PtrToStringAnsi(fileName)!, fieldIndex, unitIndex, fieldValue, maxLen, flags);
        }

        [UnmanagedCallersOnly(EntryPoint = "ContentGetValueW")]
        public static int GetValueW([MarshalAs(UnmanagedType.LPWStr)] IntPtr fileName, int fieldIndex, int unitIndex, IntPtr fieldValue, int maxLen, int flags)
        {
            return GetValueInternal(Marshal.PtrToStringUni(fileName)!, fieldIndex, unitIndex, fieldValue, maxLen, flags);
        }

        public static int GetValueInternal(string fileName, int fieldIndex, int unitIndex, IntPtr fieldValue, int maxLen, int flags)
        {
            GetValueResult result;
            var fieldType = ContentFieldType.NoMoreFields;
            var gvFlags = (GetValueFlags) flags;
            _callSignature = $"ContentGetValue '{fileName}' ({fieldIndex}/{unitIndex}/{gvFlags.ToString()})";
            try {
                // TODO: add - comments where .NET plugin interface differs from TC
                result = Plugin.GetValue(fileName, fieldIndex, unitIndex, maxLen, gvFlags, out var fieldValueStr, out fieldType);
                if (result is
                    GetValueResult.Success or
                    GetValueResult.Delayed or
                    GetValueResult.OnDemand
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

        #endregion ContentGetValue

        #region ContentStopGetValue

        [UnmanagedCallersOnly(EntryPoint = "ContentStopGetValue")]
        public static void StopGetValue([MarshalAs(UnmanagedType.LPStr)] IntPtr fileName)
        {
            StopGetValueInternal(Marshal.PtrToStringAnsi(fileName)!);
        }

        [UnmanagedCallersOnly(EntryPoint = "ContentStopGetValueW")]
        public static void StopGetValueW([MarshalAs(UnmanagedType.LPWStr)] IntPtr fileName)
        {
            StopGetValueInternal(Marshal.PtrToStringUni(fileName)!);
        }

        public static void StopGetValueInternal(string fileName)
        {
            _callSignature = "ContentStopGetValue";
            try {
                Plugin.StopGetValue(fileName);

                TraceCall(TraceLevel.Info, null);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion ContentStopGetValue

        #region ContentGetDefaultSortOrder

        [UnmanagedCallersOnly(EntryPoint = "ContentGetDefaultSortOrder")]
        public static int GetDefaultSortOrder(int fieldIndex)
        {
            var result = DefaultSortOrder.Asc;
            _callSignature = $"ContentGetDefaultSortOrder ({fieldIndex})";
            try {
                result = Plugin.GetDefaultSortOrder(fieldIndex);

                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ContentGetDefaultSortOrder

        #region ContentPluginUnloading

        [UnmanagedCallersOnly(EntryPoint = "ContentPluginUnloading")]
        public static void PluginUnloading()
        {
            _callSignature = "ContentPluginUnloading";
            try {
                Plugin.PluginUnloading();

                TraceCall(TraceLevel.Info, null);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion ContentPluginUnloading

        #region ContentGetSupportedFieldFlags

        [UnmanagedCallersOnly(EntryPoint = "ContentGetSupportedFieldFlags")]
        public static int GetSupportedFieldFlags(int fieldIndex)
        {
            var result = SupportedFieldOptions.None;
            _callSignature = $"ContentGetSupportedFieldFlags ({fieldIndex})";
            try {
                result = Plugin.GetSupportedFieldFlags(fieldIndex);

                TraceCall(TraceLevel.Verbose, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return (int) result;
        }

        #endregion ContentGetSupportedFieldFlags

        #region ContentSetValue

        [UnmanagedCallersOnly(EntryPoint = "ContentSetValue")]
        public static int SetValue([MarshalAs(UnmanagedType.LPStr)] IntPtr fileName, int fieldIndex, int unitIndex, int fieldType, IntPtr fieldValue, int flags)
        {
            return SetValueInternal(Marshal.PtrToStringAnsi(fileName)!, fieldIndex, unitIndex, fieldType, fieldValue, flags);
        }

        [UnmanagedCallersOnly(EntryPoint = "ContentSetValueW")]
        public static int SetValueW([MarshalAs(UnmanagedType.LPWStr)] IntPtr fileName, int fieldIndex, int unitIndex, int fieldType, IntPtr fieldValue, int flags)
        {
            return SetValueInternal(Marshal.PtrToStringUni(fileName)!, fieldIndex, unitIndex, fieldType, fieldValue, flags);
        }

        public static int SetValueInternal(string fileName, int fieldIndex, int unitIndex, int fieldType, IntPtr fieldValue, int flags)
        {
            SetValueResult result;
            var fldType = (ContentFieldType) fieldType;
            var svFlags = (SetValueFlags) flags;
            _callSignature = $"ContentSetValue '{fileName}' ({fieldIndex}/{unitIndex}/{svFlags.ToString()})";
            try {
                var value = new ContentValue(fieldValue, fldType);
                result = Plugin.SetValue(fileName, fieldIndex, unitIndex, fldType, value.StrValue, svFlags);

                TraceCall(TraceLevel.Info, $"{result.ToString()} - {value.StrValue}");
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = SetValueResult.NoSuchField;
            }

            return (int) result;
        }

        #endregion ContentSetValue

        #region ContentGetDetectString

        // ContentGetDetectString functionality is implemented here, not included to Content Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "ContentGetDetectString")]
        public static int GetDetectString(IntPtr detectString, int maxLen)
        {
            _callSignature = "GetDetectString";
            try {
                TcUtils.WriteStringAnsi(Plugin.DetectString, detectString, maxLen);

                TraceCall(TraceLevel.Info, Plugin.DetectString);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return 0;
        }

        #endregion ContentGetDetectString

        #region ContentSetDefaultParams

        // ContentSetDefaultParams functionality is implemented here, not included to Content Plugin interface.
        [UnmanagedCallersOnly(EntryPoint = "ContentSetDefaultParams")]
        public static void SetDefaultParams(IntPtr defParams)
        {
            _callSignature = "SetDefaultParams";
            try {
                Plugin.DefaultParams = Marshal.PtrToStructure<PluginDefaultParams>(defParams);

                TraceCall(TraceLevel.Info, null);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion ContentSetDefaultParams

        #region ContentEditValue

        [UnmanagedCallersOnly(EntryPoint = "ContentEditValue")]
        public static int EditValue(IntPtr parentWin, int fieldIndex, int unitIndex, int fieldType, IntPtr fieldValue, int maxLen, int flags, [MarshalAs(UnmanagedType.LPStr)] IntPtr langIdentifier)
        {
            var langIdentifierString = Marshal.PtrToStringAnsi(langIdentifier)!;
            SetValueResult result;
            var fldType = (ContentFieldType) fieldType;
            var evFlags = (EditValueFlags) flags;
            _callSignature = $"ContentEditValue ({fieldIndex}/{unitIndex}/{evFlags.ToString()})";
            try {
                var tcWin = new TcWindow(parentWin);
                var value = new ContentValue(fieldValue, fldType);
                var strValue = value.StrValue;
                result = Plugin.EditValue(tcWin, fieldIndex, unitIndex, fldType, ref strValue, maxLen, evFlags, langIdentifierString);
                if (result == SetValueResult.Success) {
                    value.StrValue = strValue;
                    value.CopyTo(fieldValue);
                }

                TraceCall(TraceLevel.Info, $"{result.ToString()} - {value.StrValue}");
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = SetValueResult.NoSuchField;
            }

            return (int) result;
        }

        #endregion ContentEditValue

        #region ContentSendStateInformation

        [UnmanagedCallersOnly(EntryPoint = "ContentSendStateInformation")]
        public static void SendStateInformation(int state, [MarshalAs(UnmanagedType.LPStr)] IntPtr path)
        {
            SendStateInformationInternal(state, Marshal.PtrToStringAnsi(path)!);
        }

        [UnmanagedCallersOnly(EntryPoint = "ContentSendStateInformationW")]
        public static void SendStateInformationW(int state, [MarshalAs(UnmanagedType.LPWStr)] IntPtr path)
        {
            SendStateInformationInternal(state, Marshal.PtrToStringUni(path)!);
        }

        public static void SendStateInformationInternal(int state, [MarshalAs(UnmanagedType.LPWStr)] string path)
        {
            _callSignature = "ContentSendStateInformation";
            try {
                Plugin.SendStateInformation((StateChangeInfo) state, path);

                TraceCall(TraceLevel.Info, null);
            }
            catch (Exception ex) {
                ProcessException(ex);
            }
        }

        #endregion ContentSendStateInformation

        #region ContentCompareFiles

        [UnmanagedCallersOnly(EntryPoint = "ContentCompareFiles")]
        public static int CompareFiles(IntPtr progressCallback, int compareIndex,
            [MarshalAs(UnmanagedType.LPStr)] IntPtr fileName1,
            [MarshalAs(UnmanagedType.LPStr)] IntPtr fileName2,
            IntPtr contentFileDetails)
        {
            return CompareFilesInternal(
                Marshal.GetDelegateForFunctionPointer<ContentProgressCallback>(progressCallback),
                compareIndex,
                Marshal.PtrToStringAnsi(fileName1)!,
                Marshal.PtrToStringAnsi(fileName2)!,
                Marshal.PtrToStructure<ContentFileDetails>(contentFileDetails)
            );
        }

        [UnmanagedCallersOnly(EntryPoint = "ContentCompareFilesW")]
        public static int CompareFilesW(IntPtr progressCallback, int compareIndex,
            [MarshalAs(UnmanagedType.LPWStr)] IntPtr fileName1,
            [MarshalAs(UnmanagedType.LPWStr)] IntPtr fileName2,
            IntPtr contentFileDetails)
        {
            return CompareFilesInternal(
                Marshal.GetDelegateForFunctionPointer<ContentProgressCallback>(progressCallback),
                compareIndex,
                Marshal.PtrToStringUni(fileName1)!,
                Marshal.PtrToStringUni(fileName2)!,
                Marshal.PtrToStructure<ContentFileDetails>(contentFileDetails)
            );
        }

        public static int CompareFilesInternal(ContentProgressCallback progressCallback, int compareIndex, string fileName1, string fileName2, ContentFileDetails contentFileDetails)
        {
            _callSignature = $"ContentCompareFiles '{fileName1}' => '{fileName2}' ({compareIndex})";
            TcCallback.SetContentPluginCallback(progressCallback);
            try {
                var result = Plugin.CompareFiles(compareIndex, fileName1, fileName2, contentFileDetails, out var iconResourceId);

                TraceCall(TraceLevel.Info, result.ToString());

                if (result == ContentCompareResult.EqualWithIcon && iconResourceId >= 100)
                    return iconResourceId;
                else
                    return (int) result;
            }
            catch (Exception ex) {
                ProcessException(ex);
                return (int) ContentCompareResult.FileOpenError;
            }
            finally {
                TcCallback.SetContentPluginCallback(null);
            }
        }

        #endregion ContentCompareFiles

        #endregion Content Plugin Exported Functions


        #region Tracing & Exceptions

        public static void ProcessException(Exception ex)
        {
            TcPluginLoader.ProcessException(_plugin, _callSignature, ex);
        }

        public static void TraceCall(TraceLevel level, string? result)
        {
            TcTrace.TraceCall(_plugin, level, _callSignature, result);
            _callSignature = null;
        }

        #endregion Tracing & Exceptions
    }
}
