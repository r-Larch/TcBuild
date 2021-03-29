using System;
using Microsoft.Extensions.Configuration;
using TcPluginBase.Lister;


namespace MyListerPlugin {
    public class MyLister : WpfListerPlugin<ListerControl> {
        public MyLister(IConfiguration pluginSettings) : base(pluginSettings)
        {
        }

        public override SupportExpression CanHandle { get; } = SupportExpression.Create(x =>
            x.Multimedia && x.HasExt("svg")
        );

        public override WpfLister<ListerControl>? Load(ParentWindow parent, string fileToLoad, ShowFlags showFlags)
        {
            var control = new ListerControl {
                TextControl = {
                    Text = fileToLoad
                }
            };

            return new(parent, control);
        }

        public override ListerResult LoadNext(WpfLister<ListerControl> lister, string fileToLoad, ShowFlags showFlags)
        {
            lister.Control.TextControl.Text = fileToLoad;

            return ListerResult.Ok;
        }
    }
}
