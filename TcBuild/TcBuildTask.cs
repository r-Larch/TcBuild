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
        public string AssemblyName { get; set; }
        [Required]
        public string AssemblyFile { get; set; }
        [Required]
        public string IntermediateDirectory { get; set; }
        [Required]
        public ITaskItem[] ReferenceCopyLocalFiles { get; set; }
        [Required]
        public string Configuration { get; set; }

        [Output]
        public string PluginWrapperFile { get; set; }
        [Output]
        public string PluginstFile { get; set; }

        public TcBuildTask() : base()
        {
            _token = new CancellationTokenSource();
        }

        public override bool Execute()
        {
            _log = new BuildLogger {BuildEngine = BuildEngine};

            ValidateAssemblyPath();

            var tools = new Tools(_log);

            var processor = new Processor(_log, tools) {
                AssemblyFile = new FileInfo(AssemblyFile),
                ReferenceFiles = ReferenceCopyLocalFiles.Select(_ => new FileInfo(_.ItemSpec)).ToList(),
                IsRelease = Configuration == "Release",
            };

            //var assemblyFile = new FileInfo(AssemblyFile);
            //PluginWrapperFile = Path.Combine(assemblyFile.DirectoryName!, GetOutputFileName(pluginType, x64: false));
            //PluginstFile = Path.Combine(workDir.FullName, "pluginst.inf");


            return System.Threading.Tasks.Task.Run(async () => await processor.ExecuteAsync(_token.Token), _token.Token).Result;
        }


        public void Cancel()
        {
            _token.Cancel(throwOnFirstException: true);
        }


        private void ValidateAssemblyPath()
        {
            if (!File.Exists(AssemblyFile)) {
                throw new Exception("AssemblyFile '" + AssemblyFile + "' does not exists. If you have not done a build you can ignore this error.");
            }

            _log.LogDebug("AssemblyFile: '" + AssemblyFile + "'");
        }
    }
}
