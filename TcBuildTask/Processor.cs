using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcBuildGenerator;
using Task = System.Threading.Tasks.Task;


namespace TcBuild {
    public class Processor {
        private readonly ILogger _log;
        public FileInfo AssemblyFile { get; set; }
        public FileInfo NativeAssemblyFile { get; set; }
        public DirectoryInfo IntermediateDirectory { get; set; }
        public List<FileInfo> ReferenceFiles { get; set; }


        public Processor(ILogger log)
        {
            _log = log;
        }


        public Task<string> ExecuteAsync(CancellationToken token = default)
        {
            // parse
            var parser = new AssemblyParser(AssemblyFile, _log);
            var plugin = parser.GetPluginDefinition();
            if (plugin == null) return null;

            token.ThrowIfCancellationRequested();


            var is64Bit = true;

            // files
            var settings = new FileInfo(Path.Combine(AssemblyFile.DirectoryName!, "settings.json"));
            var wrapperFile = new FileInfo(Path.Combine(NativeAssemblyFile.DirectoryName!, GetOutputFileName(plugin.Type, x64: is64Bit)));
            var zipFile = new FileInfo(Path.Combine(IntermediateDirectory.FullName, Path.ChangeExtension(AssemblyFile.Name, ".zip")));

            token.ThrowIfCancellationRequested();


            // info
            _log.LogMessage($"{plugin.Type}: {plugin.ClassFullName}");
            _log.LogMessage($"Output -> {zipFile.FullName}");

            token.ThrowIfCancellationRequested();


            // rename
            NativeAssemblyFile.MoveTo(wrapperFile.FullName);

            token.ThrowIfCancellationRequested();


            // zip
            if (plugin.Type != PluginType.QuickSearch) {
                using var zip = new ZipFile(zipFile)
                        .Add("pluginst.inf", PluginstFileContents(wrapperFile, plugin.Type))
                        .Add(wrapperFile)
                        .Add(settings)
                        .Add(AssemblyFile)
                        .Add(new FileInfo(Path.ChangeExtension(AssemblyFile.FullName, ".pdb")))
                        .AddRange(ReferenceFiles.Where(_ => _.Extension != ".xml" && _.Name != "TcPluginBase.dll"))
                    ;
            }


            // files for clean
            var generatedFiles = string.Join(";", new {
                wrapperFile,
                zipFile,
            });

            return Task.FromResult(generatedFiles);
        }


        private string PluginstFileContents(FileInfo wrapperFile, PluginType pluginType)
        {
            var version = FileVersionInfo.GetVersionInfo(AssemblyFile.FullName);

            var desc = version.Comments ?? version.FileDescription;
            if (string.IsNullOrEmpty(desc)) {
                // TC needs a description to show the install dialog!
                _log.LogWarning("[plugininstall] description is empty! Using default description. (to change description add <Description>..</Description> tag to .csproj)");
                desc = $"{Path.GetFileNameWithoutExtension(wrapperFile.Name)} is a File System Plugin for Total Commander";
            }

            // https://www.ghisler.ch/wiki/index.php/Plugins_Automated_Installation
            var plugininstall = new List<string> {
                $"[plugininstall]",
                $"type={TcInfos.PluginExtensions[pluginType]}",
                $"file={wrapperFile.Name}",
                $"description={desc}", // TODO desc max length 255
                $"defaultdir={Path.GetFileNameWithoutExtension(wrapperFile.Name)}",
                $"version={version.FileVersion}"
            };

            if (pluginType == PluginType.Packer) {
                plugininstall.Add($"defaultextension=???");
            }

            return string.Join(Environment.NewLine, plugininstall);
        }


        private string GetOutputFileName(TcBuildGenerator.PluginType pluginType, bool x64)
        {
            var name = pluginType == PluginType.QuickSearch ? "tcmatch" : Path.GetFileNameWithoutExtension(AssemblyFile.Name);
            var extension = "." + TcInfos.PluginExtensions[pluginType];

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
    }
}
