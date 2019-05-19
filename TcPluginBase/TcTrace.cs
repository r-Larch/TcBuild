using System;
using System.Diagnostics;
using System.Threading;


namespace TcPluginBase {
    internal static class TcTrace {
#if TRACE

        private static readonly TraceSwitch TcPluginTraceSwitch = new TraceSwitch("DotNetPlugins", "All .NET plugins", "Warning");

        internal static void TraceOut(TraceLevel level, string text, string category)
        {
            TraceOut(level, text, category, 0);
        }

        internal static void TraceOut(TraceLevel level, string text, string category, int indent)
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

                var levelStr = level.ToString()
                    .Replace("Verbose", "Debug")
                    .Replace("Warning", "Warn ")
                    .Replace("Info", "Info ");

                Trace.WriteLine($"[A{AppDomain.CurrentDomain.Id}|T{Thread.CurrentThread.ManagedThreadId}] {text}", timeStr + " - " + levelStr + " - " + category);

                if (indent > 0) {
                    Trace.IndentLevel++;
                }
            }
        }

        private const string TraceDateTimeFormat = "MM/dd/yy HH:mm:ss.fff ";

        private static string GetTraceTimeString()
        {
            return DateTime.Now.ToString(TraceDateTimeFormat);
        }

        internal static void TraceDelimiter()
        {
            if (TcPluginTraceSwitch.TraceWarning)
                Trace.WriteLine("- - - - - - - - - -");
        }

#endif

        internal static void TraceCall(TcPlugin plugin, TraceLevel level, string callSignature, string result)
        {
#if TRACE
            var text = callSignature + (string.IsNullOrEmpty(result) ? null : ": " + result);
            if (plugin != null) {
                if (plugin.WriteTrace || level == TraceLevel.Error) {
                    TraceOut(level, text, plugin.TraceTitle);
                }
            }
            else {
                TraceOut(level, text, null);
            }
#endif
        }
    }
}
