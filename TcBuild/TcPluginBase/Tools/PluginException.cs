using System;


namespace TcPluginBase.Tools {
    internal class PluginException(string? message) : Exception(message);
}
