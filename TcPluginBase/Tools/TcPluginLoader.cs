using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;


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

            var settings = GetSettings(pluginClass.Assembly);

            var ctor = pluginClass.GetConstructor(new[] {typeof(Settings)});
            var tcPlugin = (TPlugin) ctor.Invoke(new object[] {settings});
            return tcPlugin;
        }


        private static Settings GetSettings(Assembly assembly)
        {
            var settings = new Settings();

            // load global settings
            var appSettings = ConfigurationManager.AppSettings;
            foreach (var key in appSettings.AllKeys) {
                settings.Add(key, appSettings[key]);
            }

            // and add plugin settings
            var sett = ConfigurationManager.OpenExeConfiguration(assembly.Location).AppSettings.Settings;
            foreach (var key in appSettings.AllKeys) {
                if (settings.ContainsKey(key)) {
                    settings[key] = sett[key].Value;
                }
                else {
                    settings.Add(key, appSettings[key]);
                }
            }

            return settings;
        }


        internal static void ProcessException(TcPlugin plugin, string callSignature, Exception ex)
        {
#if TRACE
            var pluginTitle = plugin == null ? "NULL" : plugin.TraceTitle;
            TcTrace.TraceError($"{callSignature}: {ex.Message}", pluginTitle);
#endif
            if (plugin == null || plugin.ShowErrorDialog) {
                ErrorDialog.Show(callSignature, ex);
            }

            // TODO: add - unload plugin AppDomain on critical error (configurable)
        }
    }
}
