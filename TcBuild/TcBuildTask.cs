using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;


namespace TcBuild {
    public class TcBuildTask : Task, ICancelableTask, ITask {
        private readonly CancellationTokenSource _token;
        private ILogger _log;

        [Required]
        public string AssemblyFile { get; set; }
        [Required]
        public string TcPluginBase { get; set; }
        [Required]
        public string ProjectDirectory { get; set; }
        [Required]
        public string IntermediateDirectory { get; set; }
        [Required]
        public ITaskItem[] ReferenceCopyLocalFiles { get; set; }
        [Required]
        public string MSBuildFrameworkToolsPath { get; set; }
        [Required]
        public string FrameworkSDKRoot { get; set; }
        [Required]
        public string Configuration { get; set; }
        [Required]
        public string CacheDir { get; set; }

        //[Output]
        //public string TargetExt { get; private set; }

        public TcBuildTask() : base()
        {
            _token = new CancellationTokenSource();
        }

        public override bool Execute()
        {
            _log = new BuildLogger {BuildEngine = BuildEngine};

            ValidateAssemblyPath();

            var tools = new Tools(
                ilasmPath: Path.Combine(MSBuildFrameworkToolsPath, "ilasm.exe"),
                ildasmPath: new DirectoryInfo(FrameworkSDKRoot).GetFiles("ildasm.exe", SearchOption.AllDirectories).OrderByDescending(_ => _.DirectoryName).FirstOrDefault()?.FullName,
                _log
            );

            var cacheDir = new DirectoryInfo(CacheDir);
            if (!cacheDir.Exists) {
                cacheDir.Create();
            }

            var processor = new Processor(_log, tools) {
                AssemblyFile = new FileInfo(AssemblyFile),
                TcPluginBase = new FileInfo(TcPluginBase),
                IntermediateDirectory = new DirectoryInfo(IntermediateDirectory),
                CacheDir = cacheDir,
                ReferenceFiles = ReferenceCopyLocalFiles.Select(_ => new FileInfo(_.ItemSpec)).ToList(),
                IsRelease = Configuration == "Release",
            };

            return System.Threading.Tasks.Task.Run(async () => await processor.ExecuteAsync(_token.Token), _token.Token).Result;
        }


        public void Cancel()
        {
            _token.Cancel(throwOnFirstException: true);
        }


        private void ValidateAssemblyPath()
        {
            AssemblyFile = Path.Combine(ProjectDirectory, AssemblyFile);
            if (!File.Exists(AssemblyFile)) {
                throw new Exception("AssemblyFile '" + AssemblyFile + "' does not exists. If you have not done a build you can ignore this error.");
            }

            _log.LogDebug("AssemblyFile: '" + AssemblyFile + "'");
        }
    }
}
