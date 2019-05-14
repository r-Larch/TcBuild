using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TcPluginBase;
using TcPluginBase.QuickSearch;
using TcPluginBase.Tools;


namespace QSWrapper {
    public class QuickSearchWrapper {
        static QuickSearchWrapper()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new RelativeAssemblyResolver(typeof(QuickSearchWrapper).Assembly.Location).AssemblyResolve;
        }

        private static string _callSignature;
        private static QuickSearchPlugin _plugin;
        private static QuickSearchPlugin Plugin => _plugin ?? (_plugin = (QuickSearchPlugin) TcPluginLoader.GetTcPlugin(typeof(PluginClassPlaceholder), PluginType.QuickSearch));


        private QuickSearchWrapper()
        {
        }

        #region QuickSearch Exported Functions

        #region Mandatory Methods

        [DllExport(EntryPoint = "MatchFileW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool MatchFile(IntPtr wcFilter, IntPtr wcFileName)
        {
            var filter = Marshal.PtrToStringUni(wcFilter);
            var fileName = Marshal.PtrToStringUni(wcFileName);

            var result = false;
            _callSignature = $"MatchFileW(\"{fileName}\",\"{filter}\")";
            try {
                result = Plugin.MatchFile(filter, fileName);

                // !!! may produce much trace info !!!
                TraceCall(TraceLevel.Verbose, result ? "Yes" : "No");
            }
            catch (Exception ex) {
                ProcessException(ex);
            }

            return result;
        }

        [DllExport]
        public static int MatchGetSetOptions(int status)
        {
            MatchOptions result;
            _callSignature = $"MatchGetSetOptions(\"{status}\")";
            try {
                result = Plugin.MatchGetSetOptions((ExactNameMatch) status);

                TraceCall(TraceLevel.Info, result.ToString());
            }
            catch (Exception ex) {
                ProcessException(ex);
                result = MatchOptions.None;
            }

            return (int) result;
        }

        #endregion Mandatory Methods

        #endregion QuickSearch Exported Functions


        #region Tracing & Exceptions

        public static void ProcessException(Exception ex)
        {
            TcPluginLoader.ProcessException(_plugin, _callSignature, ex);
        }

        public static void TraceCall(TraceLevel level, string result)
        {
            TcTrace.TraceCall(_plugin, level, _callSignature, result);
            _callSignature = null;
        }

        #endregion Tracing & Exceptions
    }
}
