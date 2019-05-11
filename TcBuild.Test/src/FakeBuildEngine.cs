using System;
using System.Collections;
using Microsoft.Build.Framework;
using Xunit.Abstractions;


namespace TcBuild.Test {
    public class FakeBuildEngine : IBuildEngine {
        private readonly ITestOutputHelper _output;

        public FakeBuildEngine(ITestOutputHelper output)
        {
            _output = output;
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Write("error", e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Write("warn", e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            if (e.Importance == MessageImportance.Low) {
                return;
            }

            Write(e.Importance.ToString(), e.Message);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            Write("custom", e.Message);
        }

        private void Write(string type, string msg)
        {
            //Console.WriteLine($"{type} - {msg}");
            _output.WriteLine($"{type} - {msg}");
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            return true;
        }

        public bool ContinueOnError { get; set; }
        public int LineNumberOfTaskNode { get; set; }
        public int ColumnNumberOfTaskNode { get; set; }
        public string ProjectFileOfTaskNode { get; set; }
    }
}
