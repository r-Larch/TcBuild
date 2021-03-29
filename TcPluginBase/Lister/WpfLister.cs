using System;
using System.Collections.Generic;
using System.Windows;
// Integration with WinForms
using System.Windows.Forms;
using System.Windows.Forms.Integration;
// WPF
using System.Windows.Input;


namespace TcPluginBase.Lister {
    public class WpfLister<TControl> : ILister where TControl : System.Windows.Controls.UserControl {
        public TControl Control { get; }
        private ElementHost? _elementHost;

        public ParentWindow Parent { get; }

        private IntPtr? _handle;
        public IntPtr Handle => _handle ??= GetHandle();

        public WpfLister(ParentWindow parent, TControl control)
        {
            Control = control;
            Parent = parent;
        }


        private IntPtr GetHandle()
        {
            _elementHost = new ElementHost {
                Dock = DockStyle.Fill,
                Child = Control
            };
            Control.KeyDown += wpfControl_KeyDown;
            _elementHost.Focus();
            Control.Focus();
            return _elementHost.Handle;
        }


        private static readonly List<Key> SentToParentKeys = new() {
            Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, // Options.Mode
            Key.N, Key.P,
            Key.A, Key.S, Key.V, Key.W, // Options.Text
            Key.F, Key.L, Key.C, // Options.Images
            Key.F2, Key.F5, Key.F7
        };

        private static readonly List<Key> SentToParentCtrlKeys = new() {
            Key.P //, Key.A, Key.C,
        };


        private void wpfControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                Parent.CloseParentWindow();
                e.Handled = true;
            }
            else if (SentToParentCtrlKeys.Contains(e.Key)
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) == 0) {
                var code = System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key) | (int) Keys.Control;
                Parent.SendKeyToParentWindow(code);
            }
            else if (SentToParentKeys.Contains(e.Key)
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) == 0
                     && (e.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) == 0) {
                Parent.SendKeyToParentWindow(System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key));
            }
        }
    }
}
