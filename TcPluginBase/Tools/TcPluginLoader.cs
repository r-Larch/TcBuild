using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;


namespace TcPluginBase.Tools {
    [Serializable]
    internal static class TcPluginLoader {
        /// <summary>
        /// This is the TcPluginBase EntryPoint!
        /// </summary>
        public static TPlugin GetTcPlugin<TPlugin>(Type pluginClass) where TPlugin : TcPlugin
        {
            RegisterUnhandledExceptionHandler();

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
            foreach (var key in ConfigurationManager.AppSettings.AllKeys) {
                settings.Add(key, ConfigurationManager.AppSettings[key]);
            }

            // and add plugin settings
            var appSettings = ConfigurationManager.OpenExeConfiguration(assembly.Location).AppSettings.Settings;
            foreach (var key in appSettings.AllKeys) {
                if (settings.ContainsKey(key)) {
                    settings[key] = appSettings[key].Value;
                }
                else {
                    settings.Add(key, appSettings[key].Value);
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
        }


        private static bool _handlerRegistered;

        private static void RegisterUnhandledExceptionHandler()
        {
            if (!_handlerRegistered) {
                AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                    Trace.WriteLine($"[T{Thread.CurrentThread.ManagedThreadId}] AppDomain.UnhandledException: " + args.ExceptionObject.ToString());
                    Trace.Flush();
                };
                _handlerRegistered = true;
            }
        }
    }
}
