namespace OY.TotalCommander.TcPluginBase.QuickSearch {
    public interface IQuickSearchPlugin {
        #region Mandatory Methods

        //[TcMethod("MatchFileW")]
        bool MatchFile(string filter, string fileName);

        //[TcMethod("MatchGetSetOptions")]
        MatchOptions MatchGetSetOptions(ExactNameMatch status);

        #endregion Mandatory Methods
    }
}
