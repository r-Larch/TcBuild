using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;


namespace TcPluginBase.Tools {
    [Serializable]
    internal static class TcPluginLoader {
        public static TPlugin GetTcPlugin<TPlugin>(Type pluginClass) where TPlugin : TcPlugin
        {
            var tcPlugin = CreatePluginInstance<TPlugin>(pluginClass);
            tcPlugin.TcPluginEventHandler += TcCallback.HandleTcPluginEvent;

            return tcPlugin;
        }


        private static TPlugin CreatePluginInstance<TPlugin>(Type pluginClass)
        {
            TcTrace.TraceDelimiter();
            var name = pluginClass.Assembly.GetName().Name;
            var types = typeof(TPlugin).GetInterfaces().Select(_ => _.Name);
            TcTrace.TraceOut(TraceLevel.Warning, $"[{name}]{pluginClass.FullName}", $"{string.Join(",", types)} plugin load");

            var ctor = pluginClass.GetConstructor(new[] {typeof(StringDictionary)});
            var tcPlugin = (TPlugin) ctor.Invoke(new object[] {new StringDictionary()});
            return tcPlugin;
        }


        internal static void ProcessException(TcPlugin plugin, string callSignature, Exception ex)
        {
#if TRACE
            var pluginTitle = plugin == null ? "NULL" : plugin.TraceTitle;
            TcTrace.TraceError($"{callSignature}: {ex.Message}", pluginTitle);
#endif
            if (plugin == null || plugin.ShowErrorDialog) {
                ErrorDialog.Show(callSignature, ex /*, plugin?.MainWindowHandle ?? default*/);
            }

            // TODO: add - unload plugin AppDomain on critical error (configurable)
        }
    }
}
