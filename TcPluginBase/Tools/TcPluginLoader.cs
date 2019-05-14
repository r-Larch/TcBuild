using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LarchSys.Core;
using OY.TotalCommander.TcPluginBase;


namespace OY.TotalCommander.TcPluginTools {
    [Serializable]
    public static class TcPluginLoader {
        public static TcPlugin GetTcPlugin(AssemblyName pluginAssembly, PluginType pluginType)
        {
            var tcPlugin = CreatePluginInstance(pluginAssembly, pluginType);
            if (tcPlugin == null) {
                throw new InvalidOperationException("Could not find TC Plugin interface.");
            }

            tcPlugin.TcPluginEventHandler += TcCallback.HandleTcPluginEvent;

            return tcPlugin;
        }


        private static TcPlugin CreatePluginInstance(AssemblyName pluginAssemblyName, PluginType pluginType)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(_ => _.FullName == pluginAssemblyName.FullName);
            if (assembly == null) {
                assembly = Assembly.Load(pluginAssemblyName);
            }

            var assemblyFile = assembly.Location;
            TraceOut(assemblyFile, "Plugin assembly");

            var pluginSettings = GetPluginSettings(assemblyFile);
            pluginSettings["pluginFolder"] = Path.GetDirectoryName(assemblyFile);
            TraceOut($"Plugin: {assembly.FullName}.", "Start");

            //AppDomain.CurrentDomain.AssemblyResolve += new RelativeAssemblyResolver(assemblyFile).AssemblyResolve;

            var className = pluginSettings["pluginClass"]
                            ?? GetClassName(assembly, pluginType)
                            ?? throw new Exception("pluginClass info is missing in <appSettings>");

            if (string.IsNullOrEmpty(className)) {
                return null;
            }

            TcPlugin tcPlugin = null;
            try {
                tcPlugin = (TcPlugin) assembly.CreateInstance(className, ignoreCase: false, BindingFlags.Default, null, new[] {pluginSettings}, null, null);
                return tcPlugin;
            }
            finally {
                TraceOut($"\"{tcPlugin?.Title}\" [Type={className}].", $"{TcUtils.PluginNames[pluginType]} plugin loaded");
            }
        }

        private static string GetClassName(Assembly assembly, PluginType pluginType)
        {
            var iface = TcUtils.PluginInterfaceTypes[pluginType];
            var pluginClasses = assembly.GetTypes()
                .Where(t => iface.IsAssignableFrom(t) && !TcUtils.BaseTypes.Contains(t.FullName))
                .Where(t => t.IsClass && t.IsPublic && t.IsAbstract is false)
                .ToList();

            if (pluginClasses.Count > 1) {
                throw new Exception($"Multiple classes found with {iface.FullName} implemented!");
            }

            if (pluginClasses.Count != 1) {
                throw new Exception($"No classes found with {iface.FullName} implemented!");
            }

            return pluginClasses.Single().FullName;
        }


        private static StringDictionary GetPluginSettings(string wrapperAssembly)
        {
            var settings = new StringDictionary();

            var config = ConfigurationManager.OpenExeConfiguration(wrapperAssembly);
            var appSettings = config.AppSettings;
            if (appSettings != null) {
                foreach (var key in appSettings.Settings.AllKeys) {
                    settings.Add(key, appSettings.Settings[key].Value);
                }
            }

            return settings;
        }


        public static void ProcessException(TcPlugin plugin, string callSignature, Exception ex)
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

#if TRACE
        private static void TraceOut(string text, string category)
        {
            if (category == "Start") {
                TcTrace.TraceDelimiter();
            }

            TcTrace.TraceOut(TraceLevel.Warning, text, category);
        }
#endif
    }
}
