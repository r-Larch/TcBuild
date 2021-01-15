using System;


namespace TcPluginBase.Lister {
    public interface IListerHandlerBuilder {
        IntPtr GetHandle(object listerControl, IntPtr parentHandle);
    }
}
