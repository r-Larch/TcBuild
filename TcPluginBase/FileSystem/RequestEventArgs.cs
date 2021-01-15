using System;


namespace TcPluginBase.FileSystem {
    [Serializable]
    public class RequestEventArgs : PluginEventArgs {
        #region Properties

        public int PluginNumber { get; private set; }
        public int RequestType { get; private set; }
        public string? CustomTitle { get; private set; }
        public string? CustomText { get; private set; }
        public string? ReturnedText { get; set; }
        public int MaxLen { get; private set; }

        #endregion Properties

        public RequestEventArgs(int pluginNumber, int requestType, string? customTitle, string? customText, string? returnedText, int maxLen)
        {
            PluginNumber = pluginNumber;
            RequestType = requestType;
            CustomTitle = customTitle;
            CustomText = customText;
            ReturnedText = returnedText;
            MaxLen = maxLen;
        }
    }
}
