namespace TcPluginBase.Content {
    public interface IContentPlugin {
        #region Mandatory Methods

        //[TcMethod("ContentGetSupportedField", "FsContentGetSupportedField")]
        ContentFieldType GetSupportedField(int fieldIndex, out string fieldName, out string units, int maxLen);

        //[TcMethod("ContentGetValueW", "ContentGetValue", "FsContentGetValue", "FsContentGetValueW")]
        GetValueResult GetValue(string fileName, int fieldIndex, int unitIndex, int maxLen, GetValueFlags flags, out string fieldValue, out ContentFieldType fieldType);

        #endregion Mandatory Methods

        #region Optional Methods

        // functions used in both WDX and WFX plugins

        [TcMethod("ContentStopGetValue", "ContentStopGetValueW", "FsContentStopGetValue", "FsContentStopGetValueW")]
        void StopGetValue(string fileName);

        [TcMethod("ContentGetDefaultSortOrder", "FsContentGetDefaultSortOrder")]
        DefaultSortOrder GetDefaultSortOrder(int fieldIndex);

        [TcMethod("ContentPluginUnloading", "FsContentPluginUnloading")]
        void PluginUnloading();

        [TcMethod("ContentGetSupportedFieldFlags", "FsContentGetSupportedFieldFlags")]
        SupportedFieldOptions GetSupportedFieldFlags(int fieldIndex);

        [TcMethod("ContentSetValue", "ContentSetValueW", "FsContentSetValueW", "FsContentSetValue")]
        SetValueResult SetValue(string fileName, int fieldIndex, int unitIndex, ContentFieldType fieldType, string fieldValue, SetValueFlags flags);


        // functions used in WDX plugins only

        [TcMethod("ContentEditValue")]
        SetValueResult EditValue(TcWindow parentWin, int fieldIndex, int unitIndex, ContentFieldType fieldType, ref string fieldValue, int maxLen, EditValueFlags flags, string langIdentifier);

        [TcMethod("ContentSendStateInformation", "ContentSendStateInformationW")]
        void SendStateInformation(StateChangeInfo state, string path);

        [TcMethod("ContentCompareFiles", "ContentCompareFilesW")]
        ContentCompareResult CompareFiles(int compareIndex, string fileName1, string fileName2, ContentFileDetails contentFileDetails, out int iconResourceId);


        // function used in WFX plugins only

        [TcMethod("FsContentGetDefaultView", "FsContentGetDefaultViewW")]
        bool GetDefaultView(out string viewContents, out string viewHeaders, out string viewWidths, out string viewOptions, int maxLen);

        #endregion Optional Methods
    }
}
