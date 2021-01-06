using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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

            var assemblyFile = new FileInfo(typeof(TcBuildTask_Test).Assembly.Location);
            // copy files to controlled outputDir
            CopyDirectory(assemblyFile.Directory, outDir);
            assemblyFile = new FileInfo(Path.Combine(outDir.FullName, assemblyFile.Name));


            try {
                var task = new TcBuildTask {
                    Configuration = "Debug",
                    AssemblyFile = assemblyFile.FullName,
                    //TargetFile = assemblyFile.FullName,
                    TcPluginBase = typeof(UnmanagedCallersOnlyAttribute).Assembly.Location,
                    IntermediateDirectory = outDir.FullName,
                    ProjectDirectory = "/ignored",
                    ReferenceCopyLocalFiles = AppDomain.CurrentDomain.GetAssemblies().Select(_ => new TaskItem(_.Location)).Cast<ITaskItem>().ToArray(),
                    MSBuildFrameworkToolsPath = @"C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\",
                    FrameworkSDKRoot = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\",
                    CacheDir = cacheDir.FullName,
                    //--
                    BuildEngine = new FakeBuildEngine(_output)
                };

                //var pdbFile = new FileInfo(assemblyFile.FullName.Replace(".dll", ".pdb"));
                //Assert.Contains(outDir.GetFiles(), _ => _.Name == pdbFile.Name);
                var hash = GetFileHash(task.AssemblyFile);
                //var pdbHash = GetFileHash(pdbFile.FullName);

                var success = task.Execute();

                var hashAfter = GetFileHash(task.AssemblyFile);
                //Assert.Contains(outDir.GetFiles(), _ => _.Name == pdbFile.Name);
                //var pdbHashAfter = GetFileHash(pdbFile.FullName);

                Assert.True(hash.SequenceEqual(hashAfter));
                //Assert.True(pdbHash.SequenceEqual(pdbHashAfter));

                ValidateOutput(task, outDir, cacheDir);

                Assert.True(success);
            }
            finally {
                try {
                    cacheDir.Delete(true);
                    //outDir.Delete(true);
                }
                catch {
                    // ignore
                }
            }
        }

        private void CopyDirectory(DirectoryInfo dir, DirectoryInfo outDir)
        {
            var prefix = dir.FullName.TrimEnd('\\') + '\\';
            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories)) {
                file.CopyTo(Path.Combine(outDir.FullName, file.FullName.Replace(prefix, "")));
            }
        }


        private void ValidateOutput(TcBuildTask task, DirectoryInfo outDir, DirectoryInfo cacheDir)
        {
            _output.WriteLine("-----  ValidateOutput  -----");

            foreach (var outputFile in outDir.GetFiles("*", SearchOption.AllDirectories)) {
                _output.WriteLine($"  out: {outputFile.FullName}");

                // print .zip contents
                if (outputFile.FullName.EndsWith(".zip")) {
                    using (var fs = outputFile.OpenRead()) {
                        foreach (var entry in new ZipArchive(fs, ZipArchiveMode.Read).Entries.OrderBy(_ => _.FullName)) {
                            _output.WriteLine($"    - {entry.FullName,-60} {entry.Length,10} B");
                        }
                    }
                }
            }
        }


        private static DirectoryInfo GetNewTempDir()
        {
            return new DirectoryInfo(Path.GetTempPath()).CreateSubdirectory(Guid.NewGuid().ToString());
        }


        private static byte[] GetFileHash(string file)
        {
            using (var fs = new FileInfo(file).OpenRead())
            using (var algorithm = HashAlgorithm.Create("md5")) {
                return algorithm.ComputeHash(fs);
            }
        }
    }
}
