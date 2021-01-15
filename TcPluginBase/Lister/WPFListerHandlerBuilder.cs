using System;
using System.Collections.Generic;
// Integration with WinForms
using System.Windows.Forms;
using System.Windows.Forms.Integration;
// WPF
using System.Windows.Input;
using WPFUserControl = System.Windows.Controls.UserControl;


namespace TcPluginBase.Lister {
    public class WpfListerHandlerBuilder : IListerHandlerBuilder {
        private readonly ListerPlugin _plugin;
        private ElementHost? _elementHost;

        public WpfListerHandlerBuilder(ListerPlugin plugin)
        {
            _plugin = plugin;
        }

        #region Keyboard Handler

        private static readonly List<Key> SentToParentKeys = new List<Key>() {
            Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, // Options.Mode
            Key.N, Key.P,
            Key.A, Key.S, Key.V, Key.W, // Options.Text
            Key.F, Key.L, Key.C, // Options.Images
            Key.F2, Key.F5, Key.F7
        };

        private static readonly List<Key> SentToParentCtrlKeys = new List<Key>() {
            Key.P //, Key.A, Key.C,
        };


        private void wpfControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                _plugin.CloseParentWindow();
                e.Handled = true;
            }
            else if (SentToParentCtrlKeys.Contains(e.Key)
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) == 0) {
                int code = System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key) | (int) Keys.Control;
                _plugin.SendKeyToParentWindow(code);
            }
            else if (SentToParentKeys.Contains(e.Key)
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) == 0
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) == 0) {
                _plugin.SendKeyToParentWindow(System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key));
            }
        }

        #endregion Keyboard Handler

        public IntPtr GetHandle(object listerControl, IntPtr parentHandle)
        {
            if (listerControl != null) {
                if (listerControl is WPFUserControl wpfControl) {
                    _elementHost = new ElementHost {
                        Dock = DockStyle.Fill,
                        Child = wpfControl
                    };
                    wpfControl.KeyDown += wpfControl_KeyDown;
                    _elementHost.Focus();
                    wpfControl.Focus();
                    return _elementHost.Handle;
                }

                throw new Exception("Unexpected WPF control type: " + listerControl.GetType());
            }

            return IntPtr.Zero;
        }
    }
}
