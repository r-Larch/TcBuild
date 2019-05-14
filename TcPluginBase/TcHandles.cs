using System;
using System.Collections.Generic;
using System.Threading;


namespace TcPluginBase {
    internal class RefCountObject {
        public object Obj { get; private set; }
        public int RefCount { get; private set; }

        public RefCountObject(object o)
        {
            Obj = o;
            RefCount = 1;
        }

        public void Update(object o)
        {
            Obj = o;
            RefCount++;
        }
    }

    // Some TC plugin methods return handles as pointers to internal plugin structures.
    // This class contains methods for TC plugin Handle management.
    public static class TcHandles {
        #region Handle Management

        private static readonly Dictionary<IntPtr, RefCountObject> HandleDictionary = new Dictionary<IntPtr, RefCountObject>();
        private static int lastHandle;
        private static readonly object HandleSyncObj = new object();

        public static IntPtr AddHandle(object obj)
        {
            Monitor.Enter(HandleSyncObj);
            try {
                lastHandle++;
                var handle = new IntPtr(lastHandle);
                HandleDictionary.Add(handle, new RefCountObject(obj));
                return handle;
            }
            finally {
                Monitor.Exit(HandleSyncObj);
            }
        }

        public static void AddHandle(IntPtr handle, object obj)
        {
            Monitor.Enter(HandleSyncObj);
            try {
                HandleDictionary.Add(handle, new RefCountObject(obj));
            }
            finally {
                Monitor.Exit(HandleSyncObj);
            }
        }

        public static object GetObject(IntPtr handle)
        {
            Monitor.Enter(HandleSyncObj);
            try {
                return HandleDictionary.ContainsKey(handle) ? HandleDictionary[handle].Obj : null;
            }
            finally {
                Monitor.Exit(HandleSyncObj);
            }
        }

        public static int GetRefCount(IntPtr handle)
        {
            Monitor.Enter(HandleSyncObj);
            try {
                if (HandleDictionary.ContainsKey(handle))
                    return HandleDictionary[handle].RefCount;
                else
                    return -1;
            }
            finally {
                Monitor.Exit(HandleSyncObj);
            }
        }

        public static void UpdateHandle(IntPtr handle, object obj)
        {
            Monitor.Enter(HandleSyncObj);
            try {
                HandleDictionary[handle].Update(obj);
            }
            finally {
                Monitor.Exit(HandleSyncObj);
            }
        }

        public static int RemoveHandle(IntPtr handle)
        {
            Monitor.Enter(HandleSyncObj);
            try {
                if (HandleDictionary.ContainsKey(handle)) {
                    int result = HandleDictionary[handle].RefCount;
                    HandleDictionary.Remove(handle);
                    return result;
                }
                else
                    return -1;
            }
            finally {
                Monitor.Exit(HandleSyncObj);
            }
        }

        #endregion Handle Management
    }
}
