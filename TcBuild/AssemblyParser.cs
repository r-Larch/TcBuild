using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using TcBuildGenerator;


namespace TcBuild {
    internal class AssemblyParser {
        private readonly FileInfo _assembly;
        private readonly ILogger _log;


        internal AssemblyParser(FileInfo assembly, ILogger logger)
        {
            _assembly = assembly;
            _log = logger;
        }


        internal PluginDefinition GetPluginDefinition()
        {
            //AppDomain.ReflectionOnlyAssemblyResolve += ...
            //var assembly = Assembly.ReflectionOnlyLoadFrom(_assembly.FullName);

            // Get the array of runtime assemblies. This will allow us to at least inspect types depending only on BCL.
            var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
            var paths = new List<string>(runtimeAssemblies) {_assembly.FullName};

            var resolver = new PathAssemblyResolver(paths);
            using var context = new MetadataLoadContext(resolver);
            var assembly = context.LoadFromAssemblyPath(_assembly.FullName);


            var definitions = assembly.GetCustomAttributesData()
                .Where(_ => _.AttributeType.Name == TcInfos.TcPluginDefinitionAttribute)
                .Select(ParseAttribute)
                .ToList();

            if (definitions.Count == 1) {
                return definitions.Single();
            }

            else {
                if (definitions.Count == 0) {
                    _log.LogMessage($"No Plugin found in: '{_assembly.FullName}'");
                }
                else {
                    _log.LogError($"To many plugins found in: '{_assembly.FullName}'. Found {definitions.Count} plugin implementations: '{string.Join(", ", definitions.Select(_ => $"{_.Type}: {_.ClassFullName}"))}'");
                }

                return null;
            }
        }


        private PluginDefinition ParseAttribute(CustomAttributeData attribute)
        {
            var arguments = attribute.ConstructorArguments;

            var plugin = arguments switch {
                { Count: 2 } args when (
                    args[0].Value is string name &&
                    args[1].Value is Type type
                ) => new PluginDefinition {
                    Type = Enum.Parse<PluginType>(name),
                    ClassFullName = type.FullName,
                },
                _ => null
            };

            if (plugin == null) {
                _log.LogWarning($"Found invalid [{TcInfos.TcPluginDefinitionAttribute}] on '{_assembly.Name}'. Parameters: ({string.Join(", ", arguments.Select(_ => $"{_.ArgumentType.FullName} '{_.Value}'"))})");
            }

            return plugin;
        }
    }

    internal class PluginDefinition {
        public PluginType Type { get; set; }
        public string ClassFullName { get; set; }
    }
}
