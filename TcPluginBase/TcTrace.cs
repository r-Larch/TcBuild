using System;
using System.Diagnostics;
using System.Threading;


namespace TcPluginBase {
    public static class TcTrace {
#if TRACE
        public const string TraceDateTimeFormat = "MM/dd/yy HH:mm:ss.fff ";

        public static readonly TraceSwitch TcPluginTraceSwitch = new TraceSwitch("DotNetPlugins", "All .NET plugins", "Warning");

        public static void TraceError(string text, string pluginTitle)
        {
            TraceOut(TraceLevel.Error, text, $"ERROR ({pluginTitle})");
        }

        public static void TraceOut(TraceLevel level, string text, string category)
        {
            TraceOut(level, text, category, 0);
        }

        public static void TraceOut(TraceLevel level, string text, string category, int indent)
        {
            if (
                level.Equals(TraceLevel.Error) && TcPluginTraceSwitch.TraceError ||
                level.Equals(TraceLevel.Warning) && TcPluginTraceSwitch.TraceWarning ||
                level.Equals(TraceLevel.Info) && TcPluginTraceSwitch.TraceInfo ||
                level.Equals(TraceLevel.Verbose) && TcPluginTraceSwitch.TraceVerbose
            ) {
                var timeStr = GetTraceTimeString();
                if (indent < 0 && Trace.IndentLevel > 0) {
                    Trace.IndentLevel--;
                }

                Trace.WriteLine($"[T{Thread.CurrentThread.ManagedThreadId}] {text}", timeStr + " - " + category);

                if (indent > 0) {
                    Trace.IndentLevel++;
                }
            }
        }

        public static string GetTraceTimeString()
        {
            return DateTime.Now.ToString(TraceDateTimeFormat);
        }

        public static void TraceDelimiter()
        {
            if (TcPluginTraceSwitch.TraceWarning)
                Trace.WriteLine("- - - - - - - - - -");
        }

#endif

        public static void TraceCall(TcPlugin plugin, TraceLevel level, string callSignature, string result)
        {
#if TRACE
            var text = callSignature + (string.IsNullOrEmpty(result) ? null : ": " + result);
            if (plugin != null) {
                plugin.OnTcTrace(level, text);
                if (plugin.WriteTrace || level == TraceLevel.Error)
                    TraceOut(level, text, plugin.TraceTitle);
            }
            else
                TraceOut(level, text, null);
#endif
        }
    }
}
