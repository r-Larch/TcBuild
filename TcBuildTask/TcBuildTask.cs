using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using System.Threading.Tasks;


namespace TcBuild {
    public class TcBuildTask : Microsoft.Build.Utilities.Task, ICancelableTask, ITask {
#nullable disable
        private readonly CancellationTokenSource _token;
        private ILogger _log;

        [Required]
        public string AssemblyFile { get; set; }
        [Required]
        public string NativeAssemblyFile { get; set; }
        [Required]
        public string OutputDirectory { get; set; }
        [Required]
        public ITaskItem[] ReferenceCopyLocalFiles { get; set; }

        [Required]
        public string RuntimeId { get; set; }
        [Required]
        public string Architecture { get; set; }

        [Output]
        public string GeneratedFiles { get; set; }

#nullable enable

        public TcBuildTask()
        {
            _token = new CancellationTokenSource();
        }

        public override bool Execute()
        {
            _log = new BuildLogger(BuildEngine);

            if (!File.Exists(AssemblyFile)) {
                _log.LogWarning("AssemblyFile '" + AssemblyFile + "' does not exists. If you have not done a build you can ignore this error.");
                return false;
            }

            var processor = new Processor(_log) {
                AssemblyFile = new FileInfo(AssemblyFile),
                NativeAssemblyFile = new FileInfo(NativeAssemblyFile),
                OutputDirectory = new DirectoryInfo(OutputDirectory),
                ReferenceFiles = ReferenceCopyLocalFiles.Select(_ => new FileInfo(_.ItemSpec)).ToList(),
                Is64Bit = Is64Bit
            };

            GeneratedFiles = Task.Run(async () => await processor.ExecuteAsync(_token.Token), _token.Token).Result;

            return true;
        }


        public bool Is64Bit {
            get {
                // same detections as DNNE uses:
                // https://github.com/AaronRobinsonMSFT/DNNE/blob/bf86d7d4fc575fb3e2a57d197a73896159e0c90b/src/msbuild/DNNE.BuildTasks/Windows.cs#L104
                return Architecture.ToLower() switch {
                    "x64" => true,
                    "amd64" => true,
                    "x86" => false,
                    "msil" => RuntimeId.Contains("x64"), // e.g. win-x86, win-x64, etc
                    _ => IntPtr.Size == 8, // Fallback is the process bitness
                };
            }
        }


        public void Cancel()
        {
            _token.Cancel(throwOnFirstException: true);
        }
    }
}
