using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


namespace TcPluginBase.Tools {
    public class RelativeAssemblyResolver {
        private readonly IEnumerable<FileInfo> _dlls;


        public RelativeAssemblyResolver(string pluginAssemblyPath)
        {
            _dlls = new FileInfo(pluginAssemblyPath).Directory?.GetFiles("*.dll") ?? Enumerable.Empty<FileInfo>();
        }


        public Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(_ => _.FullName == args.Name);
            if (assembly != null) {
                return assembly;
            }

            var assemblyName = new AssemblyName(args.Name);
            var fileName = assemblyName.Name.ToLower();

            if (!string.Equals(Path.GetExtension(fileName), ".dll", StringComparison.InvariantCultureIgnoreCase)) {
                fileName += ".dll";
            }

            var dll = _dlls.FirstOrDefault(fi => fi.Name.ToLower() == fileName);
            if (dll == null) {
                return null;
            }

            try {
                return Assembly.LoadFrom(dll.FullName);
            }
            catch (Exception) {
                Console.Error.WriteLine("# failed to load " + dll.FullName);
                throw;
            }
        }
    }
}
