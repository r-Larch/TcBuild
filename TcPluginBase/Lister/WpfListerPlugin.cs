using System.Drawing;
using Microsoft.Extensions.Configuration;


namespace TcPluginBase.Lister {
    public abstract class WpfListerPlugin<TControl> : ListerPlugin<WpfLister<TControl>> where TControl : System.Windows.Controls.UserControl {
        protected WpfListerPlugin(IConfiguration pluginSettings) : base(pluginSettings)
        {
        }
    }


    public abstract class FormsListerPlugin<TControl> : ListerPlugin<FormsLister<TControl>> where TControl : System.Windows.Forms.UserControl {
        protected FormsListerPlugin(IConfiguration pluginSettings) : base(pluginSettings)
        {
        }
    }
}
