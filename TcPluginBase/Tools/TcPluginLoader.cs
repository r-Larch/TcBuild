using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;


namespace TcPluginBase.Tools {
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TcPluginLoader {
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

            var ctor = pluginClass.GetConstructor(new[] {typeof(IConfiguration)});
            var tcPlugin = (TPlugin) ctor.Invoke(new object[] {settings});
            return tcPlugin;
        }


        private static IConfiguration GetSettings(Assembly assembly)
        {
            var root = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource {
                    FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(assembly.Location)),
                    Path = $"settings.json",
                })
                .Build();

            return root;
        }


        public static void ProcessException(TcPlugin plugin, string callSignature, Exception ex)
        {
#if TRACE
            var pluginTitle = plugin == null ? "NULL" : plugin.TraceTitle;
            new Logger(pluginTitle).Error($"{callSignature}: {ex.Message}", ex);
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
