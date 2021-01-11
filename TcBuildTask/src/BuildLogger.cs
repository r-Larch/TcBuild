using System;
using Microsoft.Build.Framework;


namespace TcBuild {
    public class BuildLogger : MarshalByRefObject, ILogger {
        public IBuildEngine BuildEngine { get; set; }

        public virtual void SetOperationName(string weaverName)
        {
            _currentOperationName = weaverName;
        }

        public virtual void ClearOperationName()
        {
            _currentOperationName = null;
        }

        public virtual void LogMessage(string message)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(GetIndent() + PrependMessage(message), "", SenderName, MessageImportance.High));
        }

        public virtual void LogDebug(string message)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(GetIndent() + PrependMessage(message), "", SenderName, MessageImportance.Low));
        }

        public virtual void LogInfo(string message)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(GetIndent() + PrependMessage(message), "", SenderName, MessageImportance.Normal));
        }

        public virtual void LogWarning(string message)
        {
            LogWarning(message, null, 0, 0, 0, 0);
        }

        public virtual void LogWarning(string message, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber)
        {
            BuildEngine.LogWarningEvent(new BuildWarningEventArgs("", "", file, lineNumber, columnNumber, endLineNumber, endColumnNumber, this.PrependMessage(message), "", SenderName));
        }

        public virtual void LogError(string message)
        {
            LogError(message, null, 0, 0, 0, 0);
        }

        public virtual void LogError(string message, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber)
        {
            ErrorOccurred = true;
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", file, lineNumber, columnNumber, endLineNumber, endColumnNumber, this.PrependMessage(message), "", SenderName));
        }

        private string GetIndent()
        {
            if (_currentOperationName != null) {
                return "    ";
            }

            return "  ";
        }

        private string PrependMessage(string message)
        {
            if (_currentOperationName == null) {
                return SenderName + ": " + message;
            }

            return SenderName + "/" + _currentOperationName + ": " + message;
        }

        public bool ErrorOccurred;

        private const string SenderName = nameof(TcBuild);
        private string _currentOperationName;
    }


    public interface ILogger {
        void SetOperationName(string weaverName);
        void ClearOperationName();
        void LogDebug(string message);
        void LogInfo(string message);
        void LogMessage(string message);
        void LogWarning(string message);
        void LogWarning(string message, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber);
        void LogError(string message, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber);
        void LogError(string message);
    }
}
