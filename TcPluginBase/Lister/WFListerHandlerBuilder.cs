using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace OY.TotalCommander.TcPluginBase.Lister {
    [Serializable]
    public class WFListerHandlerBuilder : IListerHandlerBuilder {
        public ListerPlugin Plugin { get; set; }

        #region Keyboard Handler

        private static readonly List<Keys> SentToParentKeys = new List<Keys>() {
            Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, // Options.Mode
            Keys.N, Keys.P,
            Keys.A, Keys.S, Keys.V, Keys.W, // Options.Text
            Keys.F, Keys.L, Keys.C, // Options.Images
            Keys.F2, Keys.F5, Keys.F7
        };

        private static readonly List<Keys> SentToParentCtrlKeys = new List<Keys>() {
            Keys.A, Keys.C, Keys.P
        };

        private void wfControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) {
                Plugin.CloseParentWindow();
                e.Handled = true;
            }
            else if (SentToParentCtrlKeys.Contains(e.KeyCode) && e.Control && !e.Alt) {
                int code = e.KeyValue | (int) Keys.Control;
                Plugin.SendKeyToParentWindow(code);
            }
            else if (SentToParentKeys.Contains(e.KeyCode) && !e.Control && !e.Alt) {
                Plugin.SendKeyToParentWindow(e.KeyValue);
            }
        }

        #endregion Keyboard Handler

        public IntPtr GetHandle(object listerControl, IntPtr parentHandle)
        {
            if (listerControl != null) {
                if (listerControl is UserControl userControl) {
                    if (Plugin.FocusedControl is Control control) {
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
