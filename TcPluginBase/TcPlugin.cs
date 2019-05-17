using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;
using System.Threading;


namespace TcPluginBase {
    [Serializable]
    public class TcPlugin : MarshalByRefObject {
        public int PluginNumber { get; internal set; }
        public PluginDefaultParams DefaultParams { get; internal set; }

        public string Title { get; set; }
        public virtual string TraceTitle => Title;
        public ILogger Log { get; set; }
        public bool WriteTrace { get; set; }
        public bool ShowErrorDialog { get; set; }

        private readonly int _mainThreadId;
        protected bool IsBackgroundThread => Thread.CurrentThread.ManagedThreadId != _mainThreadId;
        protected static readonly string TcFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        protected string PluginFolder { get; }

        public TcPlugin(Settings pluginSettings = null)
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


        #region MarshalByRefObject - Lifetime initialization

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            var lease = (ILease) base.InitializeLifetimeService();
            if (lease != null && lease.CurrentState == LeaseState.Initial) {
                // By default we set infinite lifetime for each created plugin (initialLifeTime = 0)
                lease.InitialLeaseTime = TimeSpan.Zero;
            }

            return lease;
        }

        #endregion


        #region Plugin Event Handler

        public event EventHandler<PluginEventArgs> TcPluginEventHandler;

        public virtual int OnTcPluginEvent(PluginEventArgs e)
        {
            TcPluginEventHandler?.Invoke(this, e);
            return e.Result;
        }

        #endregion Plugin Event Handler


        #region Other Methods

        //protected void SetPluginFolder(string folderKey, string defaultFolder)
        //{
        //        folder = folder
        //            .Replace("%TC%", TcFolder)
        //            .Replace("%PLUGIN%", PluginFolder);
        //}

        #endregion Other Methods
    }
}
