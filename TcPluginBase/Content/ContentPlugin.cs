using System.Collections.Specialized;


namespace TcPluginBase.Content {
    public class ContentPlugin : TcPlugin, IContentPlugin {
        public virtual string DetectString { get; set; }
        public override string TraceTitle => Title;


        public ContentPlugin(StringDictionary pluginSettings) : base(pluginSettings)
        {
        }


        #region IContentPlugin Members

        #region Mandatory Methods

        public virtual ContentFieldType GetSupportedField(int fieldIndex, out string fieldName, out string units, int maxLen)
        {
            throw new MethodNotSupportedException(nameof(GetSupportedField));
        }

        public virtual GetValueResult GetValue(string fileName, int fieldIndex, int unitIndex, int maxLen, GetValueFlags flags, out string fieldValue, out ContentFieldType fieldType)
        {
            throw new MethodNotSupportedException(nameof(GetValue));
        }

        #endregion Mandatory Methods

        #region Optional Methods

        // methods used in both WDX and WFX plugins
        public virtual void StopGetValue(string fileName)
        {
        }

        public virtual DefaultSortOrder GetDefaultSortOrder(int fieldIndex)
        {
            return DefaultSortOrder.Asc;
        }

        public virtual void PluginUnloading()
        {
        }

        public virtual SupportedFieldOptions GetSupportedFieldFlags(int fieldIndex)
        {
            return SupportedFieldOptions.None;
        }

        public virtual SetValueResult SetValue(string fileName, int fieldIndex, int unitIndex, ContentFieldType fieldType, string fieldValue, SetValueFlags flags)
        {
            return SetValueResult.NoSuchField;
        }

        // method used in WFX plugins only
        public virtual bool GetDefaultView(out string viewContents, out string viewHeaders, out string viewWidths, out string viewOptions, int maxLen)
        {
            viewContents = null;
            viewHeaders = null;
            viewWidths = null;
            viewOptions = null;
            return false;
        }

        // methods used in WDX plugins only
        public virtual SetValueResult EditValue(TcWindow parentWin, int fieldIndex, int unitIndex, ContentFieldType fieldType, ref string fieldValue, int maxLen, EditValueFlags flags, string langIdentifier)
        {
            return SetValueResult.NoSuchField;
        }

        public virtual void SendStateInformation(StateChangeInfo state, string path)
        {
        }

        public virtual ContentCompareResult CompareFiles(int compareIndex, string fileName1, string fileName2, ContentFileDetails contentFileDetails, out int iconResourceId)
        {
            iconResourceId = 0;
            return ContentCompareResult.CannotCompare;
        }

        #endregion Optional Methods

        #endregion IContentPlugin Members

        #region Callback Procedures

        protected int ProgressProc(int nextBlockData)
        {
            return OnTcPluginEvent(new ContentProgressEventArgs(nextBlockData));
        }

        #endregion Callback Procedures
    }
}
