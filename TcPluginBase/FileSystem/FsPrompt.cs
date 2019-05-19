namespace TcPluginBase.FileSystem {
    public class FsPrompt {
        private readonly FsPlugin _plugin;

        public FsPrompt(FsPlugin plugin)
        {
            _plugin = plugin;
        }

        public string AskUserName(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.UserName, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        public string AskPassword(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Password, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        public string AskUserNameFirewall(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.UserNameFirewall, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        public string AskPasswordFirewall(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.PasswordFirewall, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        public string AskAccount(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Account, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        public string AskTargetDir(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.TargetDir, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        public string AskUrl(string title = null, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Url, title, null, ref value, 2048)) {
                return value;
            }

            return null;
        }

        public string AskOther(string title, string text, string preValue = "")
        {
            var value = preValue;
            if (_plugin.RequestProc(RequestType.Other, title, text, ref value, 2048)) {
                return value;
            }

            return null;
        }


        public void MsgOk(string title, string text)
        {
            string _ = null;
            _plugin.RequestProc(RequestType.MsgOk, title, text, ref _, 45);
        }


        public bool MsgYesNo(string title, string text)
        {
            string _ = null;
            if (_plugin.RequestProc(RequestType.MsgYesNo, title, text, ref _, 45)) {
                return true;
            }

            return false;
        }


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
