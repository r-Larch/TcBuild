using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;


namespace TcPluginBase {
    public class TcPlugin {
        public int PluginNumber { get; set; }
        public PluginDefaultParams DefaultParams { get; set; } // for unit tests

        public string Title { get; set; }
        public virtual string TraceTitle => Title;
        public ILogger Log { get; set; }
        public bool WriteTrace { get; set; }
        public bool ShowErrorDialog { get; set; }

        private readonly int _mainThreadId;
        protected bool IsBackgroundThread => Thread.CurrentThread.ManagedThreadId != _mainThreadId;
        protected static readonly string TcFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        protected string PluginFolder { get; }


        public TcPlugin(IConfiguration pluginSettings = null)
        {
            PluginNumber = -1;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

            PluginFolder = new FileInfo(GetType().Assembly.Location).DirectoryName;

            if (pluginSettings != null) {
                Title = pluginSettings["pluginTitle"];
                ShowErrorDialog = !Convert.ToBoolean(pluginSettings["hideErrorDialog"]);
                WriteTrace = Convert.ToBoolean(pluginSettings["writeTrace"]);
            }

            Log = new Logger(() => TraceTitle);
        }


        public event EventHandler<PluginEventArgs> TcPluginEventHandler;

        public virtual int OnTcPluginEvent(PluginEventArgs e)
        {
            TcPluginEventHandler?.Invoke(this, e);
            return e.Result;
        }
    }
}
