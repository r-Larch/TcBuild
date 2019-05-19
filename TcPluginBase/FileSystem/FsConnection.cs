using System;
using System.Collections.Concurrent;


namespace TcPluginBase.FileSystem {
    public class FsConnection {
        private readonly FsPlugin _plugin;
        private readonly ConcurrentDictionary<string, object> _data;

        public RemotePath ConnectionRoot { get; }
        public bool IsConnected;

        internal FsConnection(RemotePath connectionRoot, FsPlugin plugin)
        {
            _plugin = plugin;
            ConnectionRoot = connectionRoot;
            _data = new ConcurrentDictionary<string, object>();
        }

        public void Connect()
        {
            _plugin.LogProc(LogMsgType.Connect, $"CONNECT {ConnectionRoot.PathWithoutTrailingSlash}");
            IsConnected = true;
        }

        public void WriteStatus(string msg)
        {
            _plugin.LogProc(LogMsgType.Details, msg);
        }

        public void LogError(string error)
        {
            _plugin.Log.Error(error);
            _plugin.LogProc(LogMsgType.ImportantError, error);
        }

        public void SetData(string key, object data)
        {
            _data.AddOrUpdate(key, data, (k, v) => data);
        }

        public T GetData<T>(string key)
        {
            if (_data.TryGetValue(key, out var data)) {
                return (T) data;
            }

            return default;
        }

        public void Disconnect()
        {
            _plugin.LogProc(LogMsgType.Details, $"disconnect {ConnectionRoot}");
            _plugin.LogProc(LogMsgType.Disconnect, null);

            foreach (var keyValuePair in _data) {
                if (keyValuePair.Value is IDisposable disposable) {
                    disposable.Dispose();
                }
            }

            _data.Clear();
            IsConnected = false;
        }
    }
}
