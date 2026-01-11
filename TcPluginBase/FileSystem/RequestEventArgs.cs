namespace TcPluginBase.FileSystem {
    public class RequestEventArgs(int pluginNumber, int requestType, string? customTitle, string? customText, string? returnedText, int maxLen) : PluginEventArgs {
        public int PluginNumber { get; private set; } = pluginNumber;
        public int RequestType { get; private set; } = requestType;
        public string? CustomTitle { get; private set; } = customTitle;
        public string? CustomText { get; private set; } = customText;
        public string? ReturnedText { get; set; } = returnedText;
        public int MaxLen { get; private set; } = maxLen;
    }
}
