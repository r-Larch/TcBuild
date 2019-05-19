namespace TcPluginBase.FileSystem {
    /// <summary>
    /// FsPrompt wraps calls to RequestProc, which is a callback function that can be called to request input from the user.
    /// When using one of the standard methods, the request will be in the selected language.
    /// </summary>
    public class FsPrompt {
        private readonly FsPlugin _plugin;

        public FsPrompt(FsPlugin plugin)
        {
            _plugin = plugin;
        }

        /// <summary>
        /// Ask for the user name, e.g. for a connection
        /// </summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskUserName(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.UserName, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        /// <summary>Ask for a password, e.g. for a connection (shows ***)</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskPassword(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Password, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        /// <summary>User name for a firewall</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskUserNameFirewall(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.UserNameFirewall, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        /// <summary>Password for a firewall</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskPasswordFirewall(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.PasswordFirewall, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        /// <summary>Ask for an account (needed for some FTP servers)</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskAccount(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Account, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        /// <summary>Asks for a local directory (with browse button)</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskTargetDir(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.TargetDir, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        /// <summary>Asks for an URL</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskUrl(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Url, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        /// <summary>
        /// The requested string is none of the default types
        /// </summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="text">Override the text default text. Set this to <c>null</c> or an empty string to use the default text. The default text will be translated to the language set in the calling program.</param>
        /// <param name="preValue">This string contains the default text presented to the user, set this to <c>string.Empty</c> to have no default text.</param>
        /// <returns>The string which the user enters</returns>
        public string AskOther(string title, string text, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Other, title, text, ref value, 2048)) {
                return value;
            }

            return null;
        }


        /// <summary>Shows MessageBox with OK button</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="text">Override the text default text. Set this to <c>null</c> or an empty string to use the default text. The default text will be translated to the language set in the calling program.</param>
        public void MsgOk(string title, string text)
        {
            string _ = null;
            _plugin.RequestProc(RequestType.MsgOk, title, text, ref _, 45);
        }


        /// <summary>Shows MessageBox with Yes/No buttons</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="text">Override the text default text. Set this to <c>null</c> or an empty string to use the default text. The default text will be translated to the language set in the calling program.</param>
        /// <returns><c>true</c> if the user clicked Yes, <c>false</c> otherwise.</returns>
        public bool MsgYesNo(string title, string text)
        {
            string _ = null;
            if (_plugin.RequestProc(RequestType.MsgYesNo, title, text, ref _, 45)) {
                return true;
            }

            return false;
        }


        /// <summary>Shows MessageBox with OK/Cancel buttons</summary>
        /// <param name="title">Custom title for the dialog box. If NULL or empty, it will be "Total Commander"</param>
        /// <param name="text">Override the text default text. Set this to <c>null</c> or an empty string to use the default text. The default text will be translated to the language set in the calling program.</param>
        /// <returns><c>true</c> if the user clicked OK, <c>false</c> otherwise.</returns>
        public bool MsgOkCancel(string title, string text)
        {
            string _ = null;
            if (_plugin.RequestProc(RequestType.MsgOkCancel, title, text, ref _, 45)) {
                return true;
            }

            return false;
        }
    }
}
