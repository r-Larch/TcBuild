using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;


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

        /// <summary>
        /// Calling <see cref="Connect"/> show the FTP connections toolbar in Total Commander,
        /// and lets TC listen for log messages. Total Commander can show these messages in the log window (ftp toolbar) and write them to a log file.
        /// <para> Call this if your file system requires explicit disconnection. </para>
        /// </summary>
        public void Connect()
        {
            _plugin.LogProc(LogMsgType.Connect, $"CONNECT {ConnectionRoot}");
            IsConnected = true;
        }

        /// <summary>
        /// Can be used to log messages to Total Commander.
        /// </summary>
        /// <remarks>You must <see cref="Connect"/> to a <see cref="ConnectionRoot"/> for this to work!</remarks>
        public void WriteStatus(string msg)
        {
            _plugin.LogProc(LogMsgType.Details, msg);
        }

        /// <summary>
        /// Can be used to log errors to Total Commander.
        /// </summary>
        /// <remarks>You must <see cref="Connect"/> to a <see cref="ConnectionRoot"/> for this to work!</remarks>
        public void LogError(string error)
        {
            _plugin.Log.Error(error);
            _plugin.LogProc(LogMsgType.ImportantError, error);
        }

        public void SetData(string key, object data)
        {
            _data.AddOrUpdate(key, data, (k, v) => data);
        }

        [return: MaybeNull]
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
            _plugin.LogProc(LogMsgType.Disconnect, string.Empty);

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
