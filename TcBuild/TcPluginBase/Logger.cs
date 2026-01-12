using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace TcPluginBase {
    public interface ILogger {
        void Error(string message);
        void Error(string message, Exception e);
        void Warning(string message);
        void Info(string message);
        void Debug(string message);
    }

    public class Logger : ILogger {
        private readonly string? _title;
        private readonly Func<string>? _getTitle;
        private string? Title => _title ?? _getTitle?.Invoke();

        public Logger(string title)
        {
            _title = title;
        }

        public Logger(Func<string> title)
        {
            _getTitle = title;
        }

        public void Error(string message)
        {
            TcTrace.TraceOut(TraceLevel.Error, message, Title);
        }

        public void Error(string message, Exception e)
        {
            var errors = new List<Exception>();
            do {
                errors.Add(e);
                e = e.InnerException!;
            } while (e != null!);

            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine(string.Join("=========== InnerException ===========", errors));

            TcTrace.TraceOut(TraceLevel.Error, sb.ToString(), Title);
        }

        public void Warning(string message)
        {
            TcTrace.TraceOut(TraceLevel.Warning, message, Title);
        }

        public void Info(string message)
        {
            TcTrace.TraceOut(TraceLevel.Info, message, Title);
        }

        public void Debug(string message)
        {
            TcTrace.TraceOut(TraceLevel.Verbose, message, Title);
        }
    }
}
