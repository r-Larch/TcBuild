using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;


namespace TcPluginBase {
    [Serializable]
    [ComVisible(true)]
    internal class MethodNotSupportedException : Exception {
        private const string Msg = "Mandatory method '{0}' is not supported";

        public MethodNotSupportedException()
        {
        }

        public MethodNotSupportedException(string message, Exception ex) : base(message, ex)
        {
        }

        public MethodNotSupportedException(string methodName) : base(string.Format(Msg, methodName))
        {
        }

        protected MethodNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
