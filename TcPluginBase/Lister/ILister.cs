using System;


namespace TcPluginBase.Lister {
    public interface ILister {
        public ParentWindow Parent { get; }
        public IntPtr Handle { get; }
    }
}
