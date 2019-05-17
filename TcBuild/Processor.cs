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
        private readonly Tools _tools;
        public FileInfo AssemblyFile { get; set; }
        public FileInfo TcPluginBase { get; set; }
        public DirectoryInfo IntermediateDirectory { get; set; }
        public List<FileInfo> ReferenceFiles { get; set; }
        public bool IsRelease { get; set; }
        public DirectoryInfo CacheDir { get; set; }


        public Processor(ILogger log, Tools tools)
        {
            _log = log;
            _tools = tools;
        }


        public Task<bool> ExecuteAsync(CancellationToken token = default)
        {
            // parse
            var (pluginType, excludedMethods, pluginClass, pluginAssemblyName) = AnalyzeAssembly(AssemblyFile);

            _log.LogMessage($"PluginAssembly: {pluginAssemblyName.FullName}");
            _log.LogMessage($"Type:           {pluginType}");
            _log.LogMessage($"Class:          {pluginClass}");
            _log.LogInfo($"ExcludedMethods:");
            foreach (var method in excludedMethods) {
                _log.LogInfo($"    {method}");
            }

            var outDir = IntermediateDirectory.CreateSubdirectory("out");
            var workDir = IntermediateDirectory.CreateSubdirectory(nameof(TcBuild));

            var outFile = new FileInfo(Path.Combine(outDir.FullName, GetOutputFileName(pluginType, x64: false)));
            var outFile64 = new FileInfo(Path.Combine(outDir.FullName, GetOutputFileName(pluginType, x64: true)));

            // .config
            var config = new FileInfo(AssemblyFile.FullName + ".config");

            try {
                token.ThrowIfCancellationRequested();

                // process
                var wrapperSource = ProcessSource(pluginType, excludedMethods, pluginClass, pluginAssemblyName);
                _log.LogInfo($"{wrapperSource.FullName}");

                token.ThrowIfCancellationRequested();

                // create: x86
                _tools.Assemble(wrapperSource, outFile, false, IsRelease);
                _log.LogInfo($"{outFile.FullName}");

                token.ThrowIfCancellationRequested();

                // create: x64
                _tools.Assemble(wrapperSource, outFile64, true, IsRelease);
                _log.LogInfo($"{outFile64.FullName}");

                token.ThrowIfCancellationRequested();

                // Zip
                if (pluginType != PluginType.QuickSearch) {
                    var zipFile = new FileInfo(Path.Combine(outDir.FullName, Path.ChangeExtension(AssemblyFile.Name, ".zip")));
                    var iniFile = new FileInfo(Path.Combine(workDir.FullName, "pluginst.inf"));

                    _log.LogInfo(zipFile.FullName);

                    CreatePluginstFile(iniFile, outFile, pluginType);

                    var success = _tools.CreateZip(zipFile,
                        new[] {
                            iniFile,
                            outFile,
                            outFile64,
                            config,
                            AssemblyFile,
                            new FileInfo(Path.ChangeExtension(AssemblyFile.FullName, ".pdb")),
                        }.Concat(ReferenceFiles.Where(_ => _.Extension != ".xml" && _.Name != "TcPluginBase.dll")
                            // Hotfix until my pull request gets merged: https://github.com/peters/ILRepack.MSBuild.Task/pull/42
                            //.Where(_ => _.Name != "Microsoft.Build.Framework.dll")
                            //.Where(_ => _.Name != "Microsoft.Build.Utilities.Core.dll")
                            //.Where(_ => _.Name != "System.Collections.Immutable.dll")
                        )
                    );
                    if (!success) {
                        _log.LogWarning("ZIP Archiver is not found - Installation Archive is not created.");
                    }
                }

                token.ThrowIfCancellationRequested();

                return Task.FromResult(true);
            }
            finally {
                // Cleanup
                //AssemblyFile.Delete();
                workDir.Delete(true);
            }
        }


        private void CreatePluginstFile(FileInfo iniFile, FileInfo outFile, PluginType pluginType)
        {
            var version = FileVersionInfo.GetVersionInfo(AssemblyFile.FullName);

            var desc = version.Comments ?? version.FileDescription;
            if (string.IsNullOrEmpty(desc)) {
                // TC needs a description to show the install dialog!
                _log.LogWarning("[plugininstall] description is empty! Using default description. (to change description add [assembly: AssemblyDescription(\"...\")] attribute)");
                desc = $"{Path.GetFileNameWithoutExtension(outFile.Name)} is a File System Plugin for Total Commander";
            }

            var plugininstall = new List<string> {
                $"[plugininstall]",
                $"type={TcUtils.PluginExtensions[pluginType]}",
                $"file={outFile.Name}",
                $"description={desc}",
                $"defaultdir={Path.GetFileNameWithoutExtension(outFile.Name)}",
                $"version={version.FileVersion}"
            };

            if (pluginType == PluginType.Packer) {
                plugininstall.Add($"defaultextension=???");
            }

            using (var sw = new StreamWriter(iniFile.FullName, false, Encoding.Default)) {
                foreach (var line in plugininstall) {
                    sw.WriteLine(line);
                }
            }
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


        internal (PluginType PluginType, string[] ExcludedMethods, string pluginClass, AssemblyName pluginAssemblyName) AnalyzeAssembly(FileInfo assemblyFile)
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

                    if (pluginTypes[0] == PluginType.FileSystem) {
                        // exclude ContentPlugin methods from FileSystem plugin
                        excludedMethods.AddRange(parser.GetExcludedMethods(null, PluginType.Content));
                    }

                    break;
                case 2 when pluginTypes.Contains(PluginType.FileSystem) && pluginTypes.Contains(PluginType.Content):
                    pluginTypes = new[] {PluginType.FileSystem};
                    pluginClass = implementations.FirstOrDefault(_ => _.Key == PluginType.FileSystem).Value.Single();
                    break;
                default:
                    throw new Exception("Too Many or invalid combination of Plugin implementations found!!");
            }

            var plgType = pluginTypes[0];

            return (plgType, excludedMethods.Distinct().ToArray(), pluginClass.FullName, pluginClass.Assembly.GetName());
        }


        private readonly Dictionary<PluginType, Type> Wrapper = new Dictionary<PluginType, Type> {
            {PluginType.Content, typeof(WdxWrapper.ContentWrapper)},
            {PluginType.FileSystem, typeof(WfxWrapper.FsWrapper)},
            {PluginType.Lister, typeof(WlxWrapper.ListerWrapper)},
            {PluginType.Packer, typeof(WcxWrapper.PackerWrapper)},
            {PluginType.QuickSearch, typeof(QSWrapper.QuickSearchWrapper)}
        };


        private FileInfo ProcessSource(PluginType pluginType, string[] methodsToRemoveFromWrapper, string pluginClass, AssemblyName pluginAssemblyName)
        {
            var assemblyName = pluginAssemblyName.Name;
            var v = pluginAssemblyName.Version;

            var wrapper = GetWrapperSource(pluginType, methodsToRemoveFromWrapper, assemblyName, pluginClass);

            var tcPluginBase = GetMsilFile(TcPluginBase.FullName, cache: false);
            tcPluginBase.AddClasses(wrapper.Classes);
            tcPluginBase.AddAssembly(new AssemblyBlock {
                Name = assemblyName,
                Header = $".assembly extern '{assemblyName}'",
                Lines = new object[] {
                    "{",
                    $"  .ver {v.Major}:{v.Minor}:{v.Build}:{v.Revision}",
                    "}"
                }
            });
            var wrapperSource = tcPluginBase.SaveTo("output.il");
            return wrapperSource;
        }


        private MsilFile GetWrapperSource(PluginType pluginType, string[] methodsToRemoveFromWrapper, string pluginAssemblyName, string pluginClass)
        {
            var wrapperType = Wrapper[pluginType];

            var exportedMethods = new AssemblyParser(wrapperType.Assembly).GetExportedMethods();
            exportedMethods = exportedMethods.Where(_ => !methodsToRemoveFromWrapper.Contains(_.ExportName)).ToArray();

            var wrapper = GetMsilFile(wrapperType.Assembly.Location, cache: false, il => il
                .Replace("[TcPluginBase]TcPluginBase.PluginClassPlaceholder", $"[{pluginAssemblyName}]{pluginClass}")
                .Replace("[TcPluginBase]", "")
            );

            var dllExportAttribute = $".custom instance void {typeof(DllExportAttribute).FullName}";

            var count = 1;
            foreach (var method in wrapper.Classes.SelectMany(_ => _.Methods).Where(_ => _.Public && _.Static)) {
                var index = method.Lines.ToList().FindIndex(_ => _.ToString().Contains(dllExportAttribute));

                if (index != -1) {
                    var exportName = exportedMethods.FirstOrDefault(x => x.Method == method.Name).ExportName;

                    if (!string.IsNullOrEmpty(exportName)) {
                        method.Lines[index] = $".export [{count++}] as '{exportName}'";
                    }
                }
            }

            return wrapper;
        }


        private MsilFile GetMsilFile(string assemblyLocation, bool cache = false, Func<string, string> modifySource = null)
        {
            var cacheFile = new FileInfo(Path.Combine(CacheDir.FullName, Path.GetFileName(assemblyLocation) + ".il"));
            if (cache && cacheFile.Exists) {
                return new MsilFile(cacheFile);
            }

            var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            try {
                dir.Create();

                var source = new FileInfo(Path.Combine(dir.FullName, "input.il"));
                _tools.Disassemble(new FileInfo(assemblyLocation), source, emitDebugSymbols: !IsRelease);

                if (modifySource != null) {
                    var src = File.ReadAllText(source.FullName);
                    src = modifySource(src);
                    File.WriteAllText(source.FullName, src);
                }

                if (cache) {
                    source.CopyTo(cacheFile.FullName);
                }

                var msilFile = new MsilFile(source, dir);

                return msilFile;
            }
            finally {
                //dir.Delete(true);
            }
        }
    }
}
