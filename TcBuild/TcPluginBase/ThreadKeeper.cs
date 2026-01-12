using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


namespace TcPluginBase {
    public class ThreadKeeper : IDisposable {
        public int MainThreadId { get; }

        private readonly AutoResetEvent _resetEvent1;
        private readonly ConcurrentQueue<object> _queue;
        private readonly CancellationTokenSource _token;
        public CancellationToken Token => _token.Token;

        private struct MyFunc {
            public Func<object> Func { get; set; }
            public object Result { get; set; }
            public Exception Exception { get; set; }
            public AutoResetEvent Reset { get; set; }
            public bool Done { get; set; }
        }

        private struct MyAction {
            public Action Action { get; set; }
        }

        public void Cancel()
        {
            _token.Cancel();
        }

        public ThreadKeeper()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;
            _resetEvent1 = new AutoResetEvent(false);
            _queue = new ConcurrentQueue<object>();
            _token = new CancellationTokenSource();
        }


        public void RunInMainThread(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == MainThreadId) {
                action();
                return;
            }

            var item = new MyAction {
                Action = action
            };

            _queue.Enqueue(item);

            _resetEvent1.Set();
        }


        //public T RunInMainThread<T>(Func<T> func)
        //{
        //    if (Thread.CurrentThread.ManagedThreadId == MainThreadId) {
        //        return func();
        //    }

        //    var item = new MyFunc {
        //        Func = () => (object) func(),
        //        Reset = new AutoResetEvent(false),
        //    };

        //    _queue.Enqueue(item);

        //    _resetEvent1.Set();
        //    item.Reset.WaitOne();
        //    //WaitOne(item.Reset, Token); // TODO this throws OperationCanceledException which might kill the AppDomain!!

        //    item.Reset.Dispose();

        //    if (!item.Done) {
        //        throw new Exception("RunInMainThread: func was not executed!");
        //    }

        //    if (item.Exception != null) {
        //        throw item.Exception;
        //    }

        //    return (T) item.Result;
        //}


        public T ExecAsync<T>(Func<CancellationToken, Task<T>> asyncFunc)
        {
            var task = asyncFunc(Token);
            task.ContinueWith(_ => _resetEvent1.Set(), Token);

            while (!task.IsCompleted) {
                if (!WaitOne(_resetEvent1, Token)) {
                    break;
                }

                while (_queue.TryDequeue(out var item)) {
                    switch (item) {
                        //case MyFunc func:
                        //    ExecFunc(func);
                        //    continue;
                        case MyAction action:
                            ExecAction(action);
                            continue;
                    }
                }
            }

            var ret = task.Result;
            return ret;
        }


        private static void ExecAction(MyAction action)
        {
            try {
                action.Action();
            }
            catch (Exception) {
                // action.Exception = e;
            }
        }


        private static void ExecFunc(MyFunc item)
        {
            try {
                item.Result = item.Func();
                item.Done = true;
            }
            catch (Exception e) {
                item.Exception = e;
                item.Done = true;
            }

            try {
                item.Reset.Set();
            }
            catch (ObjectDisposedException) {
                // ignored
            }
        }


        private static bool WaitOne(WaitHandle handle, CancellationToken token)
        {
            var n = WaitHandle.WaitAny(new[] {handle, token.WaitHandle}, Timeout.Infinite);
            switch (n) {
                case WaitHandle.WaitTimeout:
                    return false;
                case 0:
                    return true;
                default:
                    token.ThrowIfCancellationRequested();
                    return false; // never reached
            }
        }


        public void Dispose()
        {
            _token.Dispose();
            _resetEvent1.Dispose();
        }
    }
}
