using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;
using System.Threading;


namespace OY.TotalCommander.TcPluginBase {
    [Serializable]
    public class TcPlugin : MarshalByRefObject {
        private static readonly string TcFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        public string DataBufferName { get; private set; }

        public PluginDefaultParams DefaultParams { get; set; }

        private IntPtr _mainWindowHandle = IntPtr.Zero;
        public IntPtr MainWindowHandle {
            get => _mainWindowHandle;
            set {
                if (_mainWindowHandle == IntPtr.Zero) {
                    _mainWindowHandle = value;
                }
            }
        }

        public TcPlugin MasterPlugin { get; set; }
        //public string PluginId { get; private set; }
        public int PluginNumber { get; set; }

        public string Title { get; set; }
        public virtual string TraceTitle => MasterPlugin == null ? Title : MasterPlugin.TraceTitle;

        public StringDictionary Settings { get; private set; }
        public PluginPassword Password { get; protected set; }

        public bool WriteTrace { get; private set; }
        public bool ShowErrorDialog { get; set; }

        private readonly int _mainThreadId;
        protected bool IsBackgroundThread => Thread.CurrentThread.ManagedThreadId != _mainThreadId;
        protected string PluginFolder { get; private set; }


        public TcPlugin(StringDictionary pluginSettings = null)
        {
            //PluginId = Guid.NewGuid().ToString();
            //DataBufferName = Guid.NewGuid().ToString();
            PluginNumber = -1;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            if (pluginSettings != null) {
                Settings = pluginSettings;
                PluginFolder = pluginSettings["pluginFolder"];
                Title = pluginSettings["pluginTitle"];
                ShowErrorDialog = !Convert.ToBoolean(pluginSettings["hideErrorDialog"]);
                WriteTrace = Convert.ToBoolean(pluginSettings["writeTrace"]);
            }
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
            var handler = TcPluginEventHandler;
            handler?.Invoke(this, e);
            return e.Result;
        }

        #endregion Plugin Event Handler

        #region Trace Handler

        protected void TraceProc(TraceLevel level, string text)
        {
#if TRACE
            PluginDomainTraceHandler(this, new TraceEventArgs(level, text));
#endif
        }

#if TRACE
        protected static void PluginDomainTraceHandler(object sender, TraceEventArgs e)
        {
            if (!(sender is TcPlugin tp)) {
                return;
            }

            TcTrace.TraceOut(e.Level, e.Text, tp.TraceTitle);
        }
#endif

        #endregion Trace Handler

        #region Other Methods

        public virtual void OnTcTrace(TraceLevel level, string text)
        {
        }

        public virtual void CreatePassword(int cryptoNumber, int flags)
        {
            Password = null;
        }

        protected void SetPluginFolder(string folderKey, string defaultFolder)
        {
            var folderName = Settings.ContainsKey(folderKey) ? Settings[folderKey] : defaultFolder;
            if (!string.IsNullOrEmpty(folderName)) {
                folderName = folderName
                    .Replace("%TC%", TcFolder)
                    .Replace("%PLUGIN%", PluginFolder);
                Settings[folderKey] = folderName;
            }
        }

        #endregion Other Methods
    }
}
