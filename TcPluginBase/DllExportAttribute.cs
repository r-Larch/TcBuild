﻿using System;


namespace TcPluginBase {
    // Indicates that the attributed method will be exposed to unmanaged code as a static entry point.
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    internal sealed class DllExportAttribute : Attribute {
        // Gets or sets the name of the DLL entry point. If not set, attributed method name will be used as entry point name.
        public string EntryPoint { get; set; }
    }


    /// <summary>
    /// A placeholder which will be replaced by the actual Plugin implementation
    /// </summary>
    internal class PluginClassPlaceholder {
    }


    /// <summary>
    /// Used to mark methods that can be omitted by the TcBuilder
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal class TcMethodAttribute : Attribute {
        public string[] MethodNames { get; }
        public bool Mandatory { get; set; }
        public bool BaseImplemented { get; set; }

        public TcMethodAttribute(string method)
        {
            MethodNames = new[] {method};
        }

        public TcMethodAttribute(string method1, string method2)
        {
            MethodNames = new[] {method1, method2};
        }

        public TcMethodAttribute(string method1, string method2, string method3)
        {
            MethodNames = new[] {method1, method2, method3};
        }

        public TcMethodAttribute(string method1, string method2, string method3, string method4)
        {
            MethodNames = new[] {method1, method2, method3, method4};
        }

        public TcMethodAttribute(string method1, string method2, string method3, string method4, string method5)
        {
            MethodNames = new[] {method1, method2, method3, method4, method5};
        }
    }
}
