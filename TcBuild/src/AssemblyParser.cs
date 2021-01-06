using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using TcPluginBase;


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
                var attr = method.GetCustomAttribute<UnmanagedCallersOnlyAttribute>();
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

            var methods = iPlugin.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(_ => {
                var attr = _.GetCustomAttribute<TcMethodAttribute>(false);
                return new {
                    PluginMethod = _.Name,
                    IsMandatory = attr?.Mandatory ?? false,
                    Exports = attr?.MethodNames ?? new string[0],
                    BaseImplemented = attr?.BaseImplemented == true,
                };
            }).ToList();

            // to remove ContentPlugin from FsWrapper
            if (pluginClass == null && pluginType == PluginType.Content) {
                return methods.SelectMany(_ => _.Exports).ToArray();
            }

            var typeMethods = pluginClass.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(method => method.Name).ToList();

            // exports to keep
            var mandatory = methods.Where(_ => _.IsMandatory).SelectMany(_ => _.Exports);
            var implemented = methods.Where(_ => typeMethods.Contains(_.PluginMethod) || _.BaseImplemented).SelectMany(_ => _.Exports);

            // all exports that have no corresponding implementation
            var exportsToRemove = methods
                .SelectMany(_ => _.Exports)
                .Where(_ => !mandatory.Contains(_))
                .Where(_ => !implemented.Contains(_));

            return exportsToRemove.ToArray();
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
