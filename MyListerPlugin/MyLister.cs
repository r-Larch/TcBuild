using System;
using Microsoft.Extensions.Configuration;
using TcPluginBase.Lister;


namespace MyListerPlugin {
    public class MyLister : ListerPlugin {
        public MyLister(IConfiguration pluginSettings) : base(pluginSettings)
        {
        }

        public override SupportExpression CanHandle { get; } = SupportExpression.Create(x =>
            x.Multimedia && x.HasExt("svg")
        );

        public override ILister? Load(ParentWindow parent, string fileToLoad, ShowFlags showFlags)
        {
            var control = new ListerControl();

            control.Text.Text = fileToLoad;

            return new WpfLister(parent, control: control);
        }

        //public override void CloseWindow(ILister lister)
        //{
        //    base.CloseWindow(lister);
        //}
    }
}
