using System;
using System.Collections.Specialized;
using System.Diagnostics;


namespace TcPluginBase.Tools {
    [Serializable]
    internal static class TcPluginLoader {
        public static TcPlugin GetTcPlugin(Type pluginClass, PluginType pluginType)
        {
            var tcPlugin = CreatePluginInstance(pluginClass, pluginType);
            tcPlugin.TcPluginEventHandler += TcCallback.HandleTcPluginEvent;

            return tcPlugin;
        }


        private static TcPlugin CreatePluginInstance(Type pluginClass, PluginType pluginType)
        {
            TcTrace.TraceDelimiter();
            var name = pluginClass.Assembly.GetName().Name;
            TcTrace.TraceOut(TraceLevel.Warning, $"[{name}]{pluginClass.FullName}", $"{TcUtils.PluginNames[pluginType]} plugin load");

            var ctor = pluginClass.GetConstructor(new[] {typeof(StringDictionary)});
            var tcPlugin = (TcPlugin) ctor.Invoke(new object[] {new StringDictionary()});
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
