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

#nullable disable

        public FileInfo AssemblyFile { get; set; }
        public FileInfo NativeAssemblyFile { get; set; }
        public DirectoryInfo OutputDirectory { get; set; }
        public List<FileInfo> ReferenceFiles { get; set; }
        public bool Is64Bit { get; set; }

#nullable enable

        public Processor(ILogger log)
        {
            _log = log;
        }


        public Task<string> ExecuteAsync(CancellationToken token = default)
        {
            // parse
            var plugin = GetPluginDefinition(AssemblyFile);
            if (plugin == null) return Task.FromResult(string.Empty);

            token.ThrowIfCancellationRequested();


            // files
            var wrapperFile = new FileInfo(Path.Combine(NativeAssemblyFile.DirectoryName!, GetOutputFileName(plugin.Type, x64: Is64Bit)));
            var zipFile = new FileInfo(Path.Combine(OutputDirectory.FullName, Path.ChangeExtension(AssemblyFile.Name, ".zip")));

            token.ThrowIfCancellationRequested();


            // info
            _log.LogMessage($"{plugin.Type}: {plugin.ClassFullName}");
            _log.LogMessage($"Output -> {zipFile.FullName}");

            token.ThrowIfCancellationRequested();


            // rename
            if (wrapperFile.Exists) wrapperFile.Delete();
            NativeAssemblyFile.MoveTo(wrapperFile.FullName);


            // get all references
            var outputFiles = OutputDirectory.GetFiles("*", SearchOption.AllDirectories).ToList();
            var references = outputFiles
                .Where(_ => !_.Name.StartsWith($"{Path.GetFileNameWithoutExtension(AssemblyFile.Name)}NE."))
                .Where(_ => _.Name != "dnne.h");


            token.ThrowIfCancellationRequested();


            // zip
            if (plugin.Type != PluginType.QuickSearch) {
                if (zipFile.Exists) zipFile.Delete();
                using var zip = new ZipFile(zipFile, OutputDirectory)
                        .Add("pluginst.inf", PluginstFileContents(wrapperFile, plugin.Type))
                        .AddRange(references)
                    ;
            }


            // delete all files and only keep the zip
            //foreach (var file in outputFiles) {
            //    try {
            //        file.Delete();
            //    }
            //    catch {
            //        // Ignore - file is locked
            //    }
            //}


            // files for clean
            var generatedFiles = string.Join(";",
                wrapperFile.FullName,
                zipFile.FullName
            );

            return Task.FromResult(generatedFiles);
        }


        private PluginDefinition? GetPluginDefinition(FileInfo assemblyFile)
        {
            using var parser = new AssemblyParser(assemblyFile, _log);
            var plugin = parser.GetPluginDefinition();
            return plugin;
        }


        private string PluginstFileContents(FileInfo wrapperFile, PluginType pluginType)
        {
            var version = FileVersionInfo.GetVersionInfo(AssemblyFile.FullName);

            var desc = version.Comments ?? version.FileDescription;
            if (string.IsNullOrEmpty(desc)) {
                // TC needs a description to show the install dialog!
                _log.LogWarning("[plugininstall] description is empty! Using default description. (to change description add <Description>..</Description> tag to .csproj)");
                desc = $"{Path.GetFileNameWithoutExtension(wrapperFile.Name)} is a {pluginType} Plugin for Total Commander";
            }

            // desc has a max length of 255!
            if (desc.Length > 255) {
                _log.LogWarning(
                    "[plugininstall] plugin description may not exceed 255 characters. It got trimmed to fit! " +
                    "To remove this warning adjust the text inside of <Description>..</Description> tag in .csproj)"
                );
                desc = desc.Substring(0, 255 - 3) + "...";
            }

            // https://www.ghisler.ch/wiki/index.php/Plugins_Automated_Installation
            var plugininstall = new List<string> {
                $"[plugininstall]",
                $"type={TcInfos.PluginExtensions[pluginType]}",
                $"file={wrapperFile.Name}",
                $"description={desc}",
                $"defaultdir={Path.GetFileNameWithoutExtension(wrapperFile.Name)}",
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
