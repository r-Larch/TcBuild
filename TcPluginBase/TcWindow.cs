using System;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using IWin32Window = System.Windows.Forms.IWin32Window;


namespace TcPluginBase {
    // Total Commander window, can be used as parent for any plugin child windows.
    [Serializable]
    public class TcWindow : IWin32Window {
        private const int TcMessageId = 1024 + 51;

        public IntPtr Handle { get; }

        public TcWindow(IntPtr handle)
        {
            Handle = handle;
        }

        // Forces the window to redraw itself and any child controls.
        public void Refresh()
        {
            NativeMethods.PostMessage(Handle, TcMessageId, (IntPtr) 540, IntPtr.Zero);
        }

        public static void SendMessage(IntPtr handle, int wParam)
        {
            NativeMethods.PostMessage(handle, TcMessageId, (IntPtr) wParam, IntPtr.Zero);
        }

        public void OpenTcPluginHome()
        {
            const int cmOpenNetwork = 2125;
            SendMessage(Handle, cmOpenNetwork);
        }

        /// <summary>
        /// Creates a STA Thread and runs the windows inside it.
        /// </summary>
        /// <param name="windowFactory"></param>
        public void ShowDialog(Func<Window> windowFactory)
        {
            using var dispatcher = new WpfDispatcher();
            dispatcher.Invoke(() => {
                var window = windowFactory();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                new WindowInteropHelper(window).Owner = Handle;
                window.ShowDialog();
            });
        }
    }


    public class WpfDispatcher : IDisposable {
        private static readonly object Lock = new();
        private static volatile int _count;
        private static Dispatcher? _dispatcher;
        private static Dispatcher Dispatcher {
            get {
                if (_dispatcher == null) {
                    lock (Lock) {
                        _dispatcher ??= SetupDispatcher();
                    }
                }

                return _dispatcher;
            }
        }

        private static Dispatcher SetupDispatcher()
        {
            Dispatcher? dispatcher = null;
            var resetEvent = new AutoResetEvent(false);
            var thread = new Thread(() => {
                dispatcher = Dispatcher.CurrentDispatcher;
                resetEvent.Set();
                Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            resetEvent.WaitOne();
            return dispatcher!;
        }

        public void Invoke(Action action)
        {
            Dispatcher.Invoke(Run);

            void Run()
            {
                action();
            }
        }

        public WpfDispatcher()
        {
            Interlocked.Increment(ref _count);
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref _count);
            if (_count <= 0) {
                lock (Lock) {
                    if (_dispatcher != null) {
                        _dispatcher.InvokeShutdown();
                        _dispatcher = null;
                    }
                }
            }
        }
    }
}
