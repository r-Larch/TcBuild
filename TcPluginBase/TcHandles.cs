using System;
using System.Collections.Generic;
using System.Threading;


namespace TcPluginBase {
    internal class RefCountObject(object o) {
        public object Obj { get; private set; } = o;
        public int RefCount { get; private set; } = 1;

        public void Update(object o)
        {
            Obj = o;
            RefCount++;
        }
    }

    // Some TC plugin methods return handles as pointers to internal plugin structures.
    // This class contains methods for TC plugin Handle management.
    public static class TcHandles {
        private static readonly Dictionary<IntPtr, RefCountObject> HandleDictionary = new();
        private static int _lastHandle;
        private static readonly Lock HandleSyncObj = new();

        public static IntPtr AddHandle(object obj)
        {
            lock (HandleSyncObj) {
                _lastHandle++;
                var handle = new IntPtr(_lastHandle);
                HandleDictionary.Add(handle, new RefCountObject(obj));
                return handle;
            }
        }

        public static void AddHandle(IntPtr handle, object obj)
        {
            lock (HandleSyncObj) {
                HandleDictionary.Add(handle, new RefCountObject(obj));
            }
        }

        public static object? GetObject(IntPtr handle)
        {
            lock (HandleSyncObj) {
                return HandleDictionary.TryGetValue(handle, out var value) ? value.Obj : null;
            }
        }

        public static void UpdateHandle(IntPtr handle, object obj)
        {
            lock (HandleSyncObj) {
                HandleDictionary[handle].Update(obj);
            }
        }

        public static int RemoveHandle(IntPtr handle)
        {
            lock (HandleSyncObj) {
                if (HandleDictionary.ContainsKey(handle)) {
                    var result = HandleDictionary[handle].RefCount;
                    HandleDictionary.Remove(handle);
                    return result;
                }

                return -1;
            }
        }
    }
}
