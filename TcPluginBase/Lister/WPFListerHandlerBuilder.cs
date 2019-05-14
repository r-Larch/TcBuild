using System;
using System.Collections.Generic;
// Integration with WinForms
using System.Windows.Forms;
using System.Windows.Forms.Integration;
// WPF
using System.Windows.Input;
using WPFUserControl = System.Windows.Controls.UserControl;


namespace TcPluginBase.Lister {
    public class WPFListerHandlerBuilder : IListerHandlerBuilder {
        public ListerPlugin Plugin { get; set; }

        private ElementHost elementHost = null;

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
                Plugin.CloseParentWindow();
                e.Handled = true;
            }
            else if (SentToParentCtrlKeys.Contains(e.Key)
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) == 0) {
                int code = System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key) | (int) Keys.Control;
                Plugin.SendKeyToParentWindow(code);
            }
            else if (SentToParentKeys.Contains(e.Key)
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) == 0
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) == 0) {
                Plugin.SendKeyToParentWindow(System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key));
            }
        }

        #endregion Keyboard Handler

        public IntPtr GetHandle(object listerControl, IntPtr parentHandle)
        {
            if (listerControl != null) {
                if (listerControl is WPFUserControl wpfControl) {
                    elementHost = new ElementHost {
                        Dock = DockStyle.Fill,
                        Child = wpfControl
                    };
                    wpfControl.KeyDown += wpfControl_KeyDown;
                    elementHost.Focus();
                    wpfControl.Focus();
                    return elementHost.Handle;
                }

                throw new Exception("Unexpected WPF control type: " + listerControl.GetType());
            }

            return IntPtr.Zero;
        }
    }
}
