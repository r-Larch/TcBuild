namespace TcPluginBase.QuickSearch {
    public interface IQuickSearchPlugin {
        [TcMethod("MatchFileW", Mandatory = true)]
        bool MatchFile(string filter, string fileName);

        [TcMethod("MatchGetSetOptions", Mandatory = true)]
        MatchOptions MatchGetSetOptions(ExactNameMatch status);
    }
}
