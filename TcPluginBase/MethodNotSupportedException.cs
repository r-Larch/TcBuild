using System;


namespace TcPluginBase {
    internal class MethodNotSupportedException(string methodName)
        : Exception($"Mandatory method '{methodName}' is not supported");
}
