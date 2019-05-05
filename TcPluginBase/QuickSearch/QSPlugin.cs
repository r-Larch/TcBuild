using System.Collections.Specialized;


namespace OY.TotalCommander.TcPluginBase.QuickSearch {
    public class QuickSearchPlugin : TcPlugin, IQuickSearchPlugin {
        #region Constructors

        public QuickSearchPlugin(StringDictionary pluginSettings) : base(pluginSettings)
        {
        }

        #endregion Constructors

        #region IQSPlugin Members

        public virtual bool MatchFile(string filter, string fileName)
        {
            return false;
        }

        public virtual MatchOptions MatchGetSetOptions(ExactNameMatch status)
        {
            return MatchOptions.None;
        }

        #endregion IQSPlugin Members
    }
}
