using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace TcBuild {
    public class Tools {
        private readonly string _ilasmPath;
        private readonly string _ildasmPath;
        private readonly ILogger _log;

        public Tools(string ilasmPath, string ildasmPath, ILogger log)
        {
            _log = log;
            _ilasmPath = ilasmPath;
            _ildasmPath = ildasmPath;

            // MSBuildFrameworkToolsPath + ilasm.exe
            // FrameworkSDKRoot + ildasm.exe

            //_assemblerPath = Microsoft.Build.Utilities.ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("ildasm.exe", TargetDotNetFrameworkVersion.Latest, DotNetFrameworkArchitecture.Current);
            //_disassemblerPath = Microsoft.Build.Utilities.ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("ildasm.exe", TargetDotNetFrameworkVersion.Latest, DotNetFrameworkArchitecture.Current);

            if (!File.Exists(_ilasmPath)) {
                _log.LogError(ilasmPath);
                throw new Exception("Cannot locate IL Assembler ilasm.exe!");
            }

            if (!File.Exists(_ildasmPath)) {
                _log.LogError(ildasmPath);
                throw new Exception("Cannot locate IL Disassembler ildasm.exe!");
            }

            _log.LogInfo($"IL Disassembler: '{_ildasmPath}'");
            _log.LogInfo($"IL Assembler   : '{_ilasmPath}'");
        }


        public void Disassemble(FileInfo assemblyFile, FileInfo sourcePath, bool emitDebugSymbols = true)
        {
            var list = new List<string>();

            list.Add(Quote(assemblyFile.FullName));
            //list.Add("/quoteallnames");
            list.Add("/unicode");
            list.Add("/nobar");
            if (emitDebugSymbols) {
                list.Add("/linenum");
            }

            list.Add($"/out:{Quote(sourcePath.FullName)}");

            var args = string.Join(" ", list);

            if (!TryRun(_ildasmPath, args)) {
                throw new Exception($"ildasm.exe has failed disassembling {assemblyFile.FullName}!");
            }
        }


        public void Assemble(FileInfo inFile, FileInfo outFile, bool x64, bool release = false)
        {
            outFile.Delete();

            var args = new List<string>();

            args.Add($"{Quote(inFile.FullName)}");
            args.Add($"/out:{Quote(outFile.FullName)}");
            args.Add($"/dll");

            //if (resFile.Exists()) {
            //    args.Add($"/res:{Quote(resFile.FullName)}");
            //}

            if (x64) {
                args.Add($"/x64");
                args.Add($"/PE64");
            }

            if (release) {
                args.Add($"/optimize");
            }

            var arguments = string.Join(" ", args.ToArray());
            if (!TryRun(_ilasmPath, arguments)) {
                throw new Exception($"ilasm.exe has failed assembling generated source!\r\n{_ilasmPath} {arguments}");
            }
        }


        public bool CreateZip(FileInfo zipFile, IEnumerable<FileInfo> files, IEnumerable<FileInfo> satelliteAssemblyFiles)
        {
            try {
                zipFile.Delete();
                using (var fs = zipFile.OpenWrite())
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Create)) {
                    foreach (var file in files.Where(_ => _.Exists)) {
                        using (var entry = zip.CreateEntry(file.Name).Open())
                        using (var fileContents = file.OpenRead()) {
                            fileContents.CopyTo(entry);
                        }
                    }
                    foreach (var file in satelliteAssemblyFiles.Where(_ => _.Exists)) {
                        using (var entry = zip.CreateEntry($"{file.DirectoryName}/{file.Name}").Open())
                        using (var fileContents = file.OpenRead()) {
                            fileContents.CopyTo(entry);
                        }
                    }
                }
                return true;
            }
            catch (Exception e) {
                _log.LogError(e.ToString());
                return false;
            }
        }


        private static string Quote(string arg)
        {
            return $"\"{arg}\"";
        }


        private bool TryRun(string exe, string args)
        {
            var process = new Process {
                StartInfo = new ProcessStartInfo(exe, args) {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
            };

            process.OutputDataReceived += (sender, eventArgs) => {
                _log.LogDebug($"    {eventArgs.Data}");
            };

            var error = new StringBuilder();
            process.ErrorDataReceived += (sender, eventArgs) => {
                error.AppendLine(eventArgs.Data);
            };

            if (process.Start()) {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                var log = error.ToString().Trim(' ', '\r', '\n', '\t');
                if (!string.IsNullOrEmpty(log)) {
                    _log.LogError(log);
                }
            }

            return process.ExitCode == 0;
        }
    }
}
