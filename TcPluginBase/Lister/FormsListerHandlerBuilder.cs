using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace TcPluginBase.Lister {
    [Serializable]
    public class FormsListerHandlerBuilder : IListerHandlerBuilder {
        private readonly ListerPlugin _plugin;

        public FormsListerHandlerBuilder(ListerPlugin plugin)
        {
            _plugin = plugin;
        }

        #region Keyboard Handler

        private static readonly List<Keys> SentToParentKeys = new() {
            Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, // Options.Mode
            Keys.N, Keys.P,
            Keys.A, Keys.S, Keys.V, Keys.W, // Options.Text
            Keys.F, Keys.L, Keys.C, // Options.Images
            Keys.F2, Keys.F5, Keys.F7
        };

        private static readonly List<Keys> SentToParentCtrlKeys = new() {
            Keys.A, Keys.C, Keys.P
        };

        private void wfControl_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) {
                _plugin.CloseParentWindow();
                e.Handled = true;
            }
            else if (SentToParentCtrlKeys.Contains(e.KeyCode) && e.Control && !e.Alt) {
                int code = e.KeyValue | (int) Keys.Control;
                _plugin.SendKeyToParentWindow(code);
            }
            else if (SentToParentKeys.Contains(e.KeyCode) && !e.Control && !e.Alt) {
                _plugin.SendKeyToParentWindow(e.KeyValue);
            }
        }

        #endregion Keyboard Handler

        public IntPtr GetHandle(object listerControl, IntPtr parentHandle)
        {
            if (listerControl != null) {
                if (listerControl is UserControl userControl) {
                    if (_plugin.FocusedControl is Control control) {
                        control.KeyDown += wfControl_KeyDown;
                    }

                    return userControl.Handle;
                }

                throw new Exception("Unexpected WinForms control type: " + listerControl.GetType());
            }

            return IntPtr.Zero;
        }
    }
}
