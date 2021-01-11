using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using System.Threading.Tasks;


namespace TcBuild {
    public class TcBuildTask : Microsoft.Build.Utilities.Task, ICancelableTask, ITask {
        private readonly CancellationTokenSource _token;
        private ILogger _log;

        [Required]
        public string AssemblyFile { get; set; }
        [Required]
        public string NativeAssemblyFile { get; set; }
        [Required]
        public string IntermediateDirectory { get; set; }
        [Required]
        public ITaskItem[] ReferenceCopyLocalFiles { get; set; }

        [Output]
        public string GeneratedFiles { get; set; }

        public TcBuildTask()
        {
            _token = new CancellationTokenSource();
        }

        public override bool Execute()
        {
            _log = new BuildLogger {BuildEngine = BuildEngine};

            if (!File.Exists(AssemblyFile)) {
                _log.LogWarning("AssemblyFile '" + AssemblyFile + "' does not exists. If you have not done a build you can ignore this error.");
                return false;
            }

            var processor = new Processor(_log) {
                AssemblyFile = new FileInfo(AssemblyFile),
                NativeAssemblyFile = new FileInfo(NativeAssemblyFile),
                IntermediateDirectory = new DirectoryInfo(IntermediateDirectory),
                ReferenceFiles = ReferenceCopyLocalFiles.Select(_ => new FileInfo(_.ItemSpec)).ToList(),
            };

            GeneratedFiles = Task.Run(async () => await processor.ExecuteAsync(_token.Token), _token.Token).Result;

            return true;
        }


        public void Cancel()
        {
            _token.Cancel(throwOnFirstException: true);
        }
    }
}
