using Microsoft.Extensions.Configuration;


namespace TcPluginBase.QuickSearch {
    public abstract class QuickSearchPlugin : TcPlugin, IQuickSearchPlugin {
        protected QuickSearchPlugin(IConfiguration pluginSettings) : base(pluginSettings)
        {
        }


        public virtual bool MatchFile(string filter, string fileName)
        {
            return false;
        }

        public virtual MatchOptions MatchGetSetOptions(ExactNameMatch status)
        {
            return MatchOptions.None;
        }
    }
}
