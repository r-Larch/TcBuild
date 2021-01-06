using System;
using System.ComponentModel;


namespace TcPluginBase {
    /// <summary>
    /// A placeholder which will be replaced by the actual Plugin implementation
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PluginClassPlaceholder {
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
