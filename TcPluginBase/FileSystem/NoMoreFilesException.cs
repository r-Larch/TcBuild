using System;


namespace TcPluginBase.FileSystem {
    public class NoMoreFilesException(string message, Exception? ex = null) : Exception(message, ex);
}
