using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;


// ReSharper disable LocalizableElement
// ReSharper disable InconsistentNaming
namespace TcBuild.Test {
    public class TcBuildTask_Test {
        private readonly ITestOutputHelper _output;

        public TcBuildTask_Test(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output, "StdOut: "));
            Console.SetError(new Converter(output, "StdError: "));
        }


        [Fact]
        public void Test_Run()
        {
            var cacheDir = GetNewTempDir();
            var outDir = GetNewTempDir();

            try {
                var task = new TcBuildTask {
                    Configuration = "Debug",
                    AssemblyFile = typeof(TcBuildTask_Test).Assembly.Location,
                    IntermediateDirectory = outDir.FullName,
                    ProjectDirectory = "/ignored",
                    ReferenceCopyLocalFiles = AppDomain.CurrentDomain.GetAssemblies().Select(_ => new TaskItem(_.Location)).Cast<ITaskItem>().ToArray(),
                    MSBuildFrameworkToolsPath = @"C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\",
                    FrameworkSDKRoot = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\",
                    CacheDir = cacheDir.FullName,
                    //--
                    BuildEngine = new FakeBuildEngine(_output)
                };

                var success = task.Execute();

                ValidateOutput(task, outDir, cacheDir);

                Assert.True(success);
            }
            finally {
                cacheDir.Delete(true);
                outDir.Delete(true);
            }
        }


        private void ValidateOutput(TcBuildTask task, DirectoryInfo outDir, DirectoryInfo cacheDir)
        {
            _output.WriteLine("-----  ValidateOutput  -----");

            foreach (var outputFile in outDir.GetFiles()) {
                _output.WriteLine($"  out: {outputFile.FullName}");
                if (outputFile.FullName.EndsWith(".zip")) {
                    using (var fs = outputFile.OpenRead()) {
                        foreach (var entry in new ZipArchive(fs, ZipArchiveMode.Read).Entries.OrderBy(_ => _.FullName)) {
                            _output.WriteLine($"    - {entry.FullName,-60} {entry.Length,10} B");
                        }
                    }
                }
            }

            foreach (var outputFile in new FileInfo(task.AssemblyFile).Directory.GetFiles()) {
                _output.WriteLine($"  bin: {outputFile.FullName}");
            }
        }


        private static DirectoryInfo GetNewTempDir()
        {
            return new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        }
    }
}
