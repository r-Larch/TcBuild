using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Utilities;
using TcBuild;
using TcBuild.Test;
using Xunit;
using Xunit.Abstractions;


namespace BuildTask.Tests {
    public class Tests {
        private readonly ITestOutputHelper _output;

        public Tests(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output, "StdOut: "));
            Console.SetError(new Converter(output, "StdError: "));
        }


        [Fact]
        public void TcBuildTask_Test()
        {
            var assemblyDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Files"));
            var outDir = GetNewTempDir();

            // copy files to controlled outputDir
            CopyDirectory(assemblyDir, outDir);

            // fake reference file
            var fakeReference = new FileInfo(Path.Combine(outDir.FullName, "FakeReference.dll"));
            File.WriteAllText(fakeReference.FullName, "foobar");

            try {
                var task = new TcBuildTask {
                    // input:
                    AssemblyFile = Path.Combine(outDir.FullName, "FsPlugin2.dll"),
                    NativeAssemblyFile = Path.Combine(outDir.FullName, "FsPlugin2NE.dll"),
                    OutputDirectory = outDir.FullName,
                    ReferenceCopyLocalFiles = new[] {new TaskItem(fakeReference.FullName)},

                    // msbuild:
                    BuildEngine = new FakeBuildEngine(_output),

                    // output:
                    // GeneratedFiles
                };

                var success = task.Execute();
                Assert.True(success);

                ValidateOutput(task, outDir);
            }
            finally {
                try {
                    outDir.Delete(true);
                }
                catch {
                    // ignore
                }
            }
        }


        private static void CopyDirectory(DirectoryInfo dir, DirectoryInfo outDir)
        {
            var prefix = dir.FullName.TrimEnd('\\') + '\\';
            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories)) {
                file.CopyTo(Path.Combine(outDir.FullName, file.FullName.Replace(prefix, "")));
            }
        }


        private void ValidateOutput(TcBuildTask task, DirectoryInfo outDir)
        {
            _output.WriteLine("-----  ValidateOutput  -----");

            foreach (var outputFile in outDir.GetFiles("*", SearchOption.AllDirectories)) {
                _output.WriteLine($"  out: {outputFile.FullName}");

                // print .zip contents
                if (outputFile.FullName.EndsWith(".zip")) {
                    using var zip = new ZipArchive(outputFile.OpenRead(), ZipArchiveMode.Read);
                    foreach (var entry in zip.Entries.OrderBy(_ => _.FullName)) {
                        _output.WriteLine($"    - {entry.FullName,-60} {entry.Length,10} B");
                    }
                }
            }

            _output.WriteLine("");
            _output.WriteLine($"task.GeneratedFiles: '{task.GeneratedFiles}'");
            _output.WriteLine("");
        }


        private static DirectoryInfo GetNewTempDir()
        {
            return new DirectoryInfo(Path.GetTempPath()).CreateSubdirectory(Guid.NewGuid().ToString());
        }
    }
}
