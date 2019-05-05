using System;


namespace OY.TotalCommander.TcPluginBase.Lister {
    public interface IListerHandlerBuilder {
        ListerPlugin Plugin { get; set; }
        IntPtr GetHandle(object listerControl, IntPtr parentHandle);
    }
}
