using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcPluginBase;
using TcPluginBase.Tools;
using Task = System.Threading.Tasks.Task;


namespace TcBuild {
    public class Processor {
        private readonly ILogger _log;
        public FileInfo AssemblyFile { get; set; }
        public DirectoryInfo IntermediateDirectory { get; set; }
        public List<FileInfo> ReferenceFiles { get; set; }
        public bool IsRelease { get; set; }


        public Processor(ILogger log)
        {
            _log = log;
        }


        public Task<bool> ExecuteAsync(CancellationToken token = default)
        {
            // parse
            var (pluginType, pluginClass, pluginAssemblyName) = AnalyzeAssembly(AssemblyFile);

            _log.LogMessage($"PluginAssembly: {pluginAssemblyName.FullName}");
            _log.LogMessage($"Type:           {pluginType}");
            _log.LogMessage($"Class:          {pluginClass}");

            var outDir = IntermediateDirectory.CreateSubdirectory("out");
            var workDir = IntermediateDirectory.CreateSubdirectory(nameof(TcBuild));

            var outFile = new FileInfo(Path.Combine(outDir.FullName, GetOutputFileName(pluginType, x64: false)));
            var outFile64 = new FileInfo(Path.Combine(outDir.FullName, GetOutputFileName(pluginType, x64: true)));

            // .config
            var config = new FileInfo(AssemblyFile.FullName + ".config");

            try {
                // Zip
                if (pluginType != PluginType.QuickSearch) {
                    var zipFile = new FileInfo(Path.Combine(outDir.FullName, Path.ChangeExtension(AssemblyFile.Name, ".zip")));

                    _log.LogInfo(zipFile.FullName);

                    using var zip = new ZipFile(zipFile)
                            .Add("pluginst.inf", PluginstFileContents(outFile, pluginType))
                            .Add(outFile)
                            .Add(outFile64)
                            .Add(config)
                            .Add(AssemblyFile)
                            .Add(new FileInfo(Path.ChangeExtension(AssemblyFile.FullName, ".pdb")))
                            .AddRange(ReferenceFiles.Where(_ => _.Extension != ".xml" && _.Name != "TcPluginBase.dll"))
                        ;
                }

                token.ThrowIfCancellationRequested();

                var PluginWrapperFile = "";
                var PluginstFile = "";

                return Task.FromResult(true);
            }
            finally {
                // Cleanup
                workDir.Delete(true);
            }
        }


        private string PluginstFileContents(FileInfo outFile, PluginType pluginType)
        {
            var version = FileVersionInfo.GetVersionInfo(AssemblyFile.FullName);

            var desc = version.Comments ?? version.FileDescription;
            if (string.IsNullOrEmpty(desc)) {
                // TC needs a description to show the install dialog!
                _log.LogWarning("[plugininstall] description is empty! Using default description. (to change description add [assembly: AssemblyDescription(\"...\")] attribute)");
                desc = $"{Path.GetFileNameWithoutExtension(outFile.Name)} is a File System Plugin for Total Commander";
            }

            // https://www.ghisler.ch/wiki/index.php/Plugins_Automated_Installation
            var plugininstall = new List<string> {
                $"[plugininstall]",
                $"type={TcUtils.PluginExtensions[pluginType]}",
                $"file={outFile.Name}",
                $"description={desc}", // TODO desc max length 255
                $"defaultdir={Path.GetFileNameWithoutExtension(outFile.Name)}",
                $"version={version.FileVersion}"
            };

            if (pluginType == PluginType.Packer) {
                plugininstall.Add($"defaultextension=???");
            }

            return string.Join(Environment.NewLine, plugininstall);
        }


        private string GetOutputFileName(PluginType pluginType, bool x64)
        {
            var name = pluginType == PluginType.QuickSearch ? "tcmatch" : Path.GetFileNameWithoutExtension(AssemblyFile.Name);
            var extension = "." + TcUtils.PluginExtensions[pluginType];

            if (x64) {
                if (extension == ".dll") {
                    name += "64";
                }
                else {
                    extension += "64";
                }
            }

            return name + extension;
        }


        internal (PluginType PluginType, string pluginClass, AssemblyName pluginAssemblyName) AnalyzeAssembly(FileInfo assemblyFile)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new RelativeAssemblyResolver(assemblyFile.FullName).AssemblyResolve;
            var assembly = Assembly.LoadFile(assemblyFile.FullName);

            var parser = new AssemblyParser(assembly);
            var implementations = parser.GetImplementations();

            // get excluded
            var excludedMethods = new List<string>();
            foreach (var implementation in implementations) {
                var pluginType = implementation.Key;
                var pluginClasses = implementation.Value;

                if (pluginClasses.Length > 1) {
                    throw new Exception($"Too many {pluginType} Plugin implementations in one project!! Found implementations in: {string.Join(", ", pluginClasses.Select(_ => _.FullName))}");
                }

                excludedMethods.AddRange(parser.GetExcludedMethods(pluginClasses[0], pluginType));
            }

            // check pluginType
            Type pluginClass;
            var pluginTypes = implementations.Select(_ => _.Key).ToArray();
            switch (pluginTypes.Length) {
                case 0:
                    throw new Exception("No Plugin implementation found!!");
                case 1:
                    // this is valid
                    pluginClass = implementations.FirstOrDefault().Value.Single();
                    break;
                case 2 when pluginTypes.Contains(PluginType.FileSystem) && pluginTypes.Contains(PluginType.Content):
                    pluginTypes = new[] {PluginType.FileSystem};
                    pluginClass = implementations.FirstOrDefault(_ => _.Key == PluginType.FileSystem).Value.Single();
                    break;
                default:
                    throw new Exception("Too Many or invalid combination of Plugin implementations found!!");
            }

            var plgType = pluginTypes[0];

            return (plgType, pluginClass.FullName, pluginClass.Assembly.GetName());
        }
    }
}
