using Microsoft.Extensions.Configuration;


namespace TcPluginBase.Lister {
    public abstract class WpfListerPlugin<TControl>(IConfiguration pluginSettings)
        : ListerPlugin<WpfLister<TControl>>(pluginSettings)
        where TControl : System.Windows.Controls.UserControl;


    public abstract class FormsListerPlugin<TControl>(IConfiguration pluginSettings)
        : ListerPlugin<FormsLister<TControl>>(pluginSettings)
        where TControl : System.Windows.Forms.UserControl;
}
