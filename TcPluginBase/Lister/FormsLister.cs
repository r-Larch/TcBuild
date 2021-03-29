using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace TcPluginBase.Lister {
    public class FormsLister<TControl> : ILister where TControl : System.Windows.Forms.UserControl {
        public TControl Control { get; }
        public ParentWindow Parent { get; }
        public IntPtr Handle => Control.Handle;

        public FormsLister(ParentWindow parent, TControl control)
        {
            Control = control;
            Parent = parent;
        }

        public void SetActiveControl(Control control)
        {
            control.KeyDown += wfControl_KeyDown;
        }

        private static readonly List<Keys> SentToParentKeys = new() {
            Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, // Options.Mode
            Keys.N, Keys.P, Keys.A, Keys.S, Keys.V, Keys.W, // Options.Text
            Keys.F, Keys.L, Keys.C, // Options.Images
            Keys.F2, Keys.F5, Keys.F7
        };

        private static readonly List<Keys> SentToParentCtrlKeys = new() {
            Keys.A, Keys.C, Keys.P
        };

        private void wfControl_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) {
                Parent.CloseParentWindow();
                e.Handled = true;
            }
            else if (SentToParentCtrlKeys.Contains(e.KeyCode) && e.Control && !e.Alt) {
                var code = e.KeyValue | (int) Keys.Control;
                Parent.SendKeyToParentWindow(code);
            }
            else if (SentToParentKeys.Contains(e.KeyCode) && !e.Control && !e.Alt) {
                Parent.SendKeyToParentWindow(e.KeyValue);
            }
        }
    }
}
