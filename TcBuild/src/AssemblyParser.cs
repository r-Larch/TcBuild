using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OY.TotalCommander.TcPluginBase;


namespace TcBuild {
    internal class AssemblyParser {
        private readonly Assembly _assembly;

        public AssemblyParser(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Dictionary<PluginType, Type[]> GetImplementations()
        {
            var ret = new Dictionary<PluginType, Type[]>();
            foreach (var pluginInterface in TcUtils.PluginInterfaceTypes) {
                var implementations = ClassesWhereImplements(pluginInterface.Value)
                    .Where(x => !TcUtils.BaseTypes.Contains(x.FullName)) // exclude base classes
                    .ToArray();

                if (implementations.Length > 0) {
                    ret.Add(pluginInterface.Key, implementations);
                }
            }

            return ret;
        }


        // call on wrapper !!
        public (string Method, string ExportName)[] GetExportedMethods()
        {
            var ret = new List<(string, string)>();

            var methods = Classes().SelectMany(clazz => clazz.GetMethods(BindingFlags.Static | BindingFlags.Public));
            foreach (var method in methods) {
                var attr = method.GetCustomAttribute<DllExportAttribute>();
                if (attr != null) {
                    var exportName = method.Name;

                    if (!string.IsNullOrEmpty(attr.EntryPoint)) {
                        exportName = attr.EntryPoint;
                    }

                    ret.Add((method.Name, exportName));
                }
            }

            return ret.ToArray();
        }


        public string[] GetExcludedMethods(Type pluginClass, PluginType pluginType)
        {
            var iPlugin = TcUtils.PluginInterfaceTypes[pluginType];

            var optionalMethods = iPlugin.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(_ => new {
                PluginMethod = _.Name,
                Exports = _.GetCustomAttribute<TcMethodAttribute>(false)?.MethodNames ?? new string[0],
            }).ToList();

            var typeMethods = pluginClass.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(method => method.Name).ToList();

            var exports = new List<string>();
            foreach (var optionalMethod in optionalMethods) {
                if (typeMethods.Contains(optionalMethod.PluginMethod)) {
                    // implemented
                    foreach (var export in optionalMethod.Exports) {
                        exports.Remove(export);
                    }
                }
                else {
                    // not implemented
                    exports.AddRange(optionalMethod.Exports);
                }
            }

            // all exports that have no corresponding implementation
            return exports.ToArray();
        }


        private IEnumerable<Type> Classes()
        {
            return _assembly
                .GetExportedTypes()
                .Where(t => t.IsPublic && t.IsClass && t.IsAbstract is false);
        }

        private IEnumerable<Type> ClassesWhereImplements(Type interfaceType)
        {
            return Classes().Where(_ => _.GetInterface(interfaceType.FullName) != null);
            //return Classes().Where(interfaceType.IsAssignableFrom);
        }
    }
}
