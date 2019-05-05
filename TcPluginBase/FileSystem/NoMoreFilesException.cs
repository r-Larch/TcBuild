using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;


namespace OY.TotalCommander.TcPluginBase.FileSystem {
    [Serializable]
    [ComVisible(true)]
    public class NoMoreFilesException : Exception {
        public NoMoreFilesException()
        {
        }

        public NoMoreFilesException(string message) : base(message)
        {
        }

        public NoMoreFilesException(string message, Exception ex) : base(message, ex)
        {
        }

        protected NoMoreFilesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
