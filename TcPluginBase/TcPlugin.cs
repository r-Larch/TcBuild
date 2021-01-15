using System;
using System.Threading;
using Microsoft.Extensions.Configuration;


namespace TcPluginBase {
    public class TcPlugin {
        public int PluginNumber { get; set; }
        public PluginDefaultParams DefaultParams { get; set; }

        public string Title { get; set; }
        public virtual string TraceTitle => Title;
        public ILogger Log { get; set; }
        public bool WriteTrace { get; set; }
        public bool ShowErrorDialog { get; set; }

        private readonly int _mainThreadId;
        protected bool IsBackgroundThread => Thread.CurrentThread.ManagedThreadId != _mainThreadId;


        public TcPlugin(IConfiguration? pluginSettings = null)
        {
            PluginNumber = -1;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

            if (pluginSettings != null) {
                Title = pluginSettings["pluginTitle"];
                ShowErrorDialog = !Convert.ToBoolean(pluginSettings["hideErrorDialog"]);
                WriteTrace = Convert.ToBoolean(pluginSettings["writeTrace"]);
            }
            else {
                Title = this.GetType().Name;
            }

            Log = new Logger(() => TraceTitle);
        }


        public event EventHandler<PluginEventArgs>? TcPluginEventHandler;

        public virtual int OnTcPluginEvent(PluginEventArgs e)
        {
            TcPluginEventHandler?.Invoke(this, e);
            return e.Result;
        }
    }
}
