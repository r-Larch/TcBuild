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
        private readonly string _title;

        public Logger(string title)
        {
            _title = title;
        }

        public void Error(string message)
        {
            TcTrace.TraceOut(TraceLevel.Error, message, _title);
        }

        public void Error(string message, Exception e)
        {
            var errors = new List<Exception>();
            do {
                errors.Add(e);
                e = e.InnerException;
            } while (e != null);

            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine(string.Join("=========== InnerException ===========", errors));

            TcTrace.TraceOut(TraceLevel.Error, sb.ToString(), _title);
        }

        public void Warning(string message)
        {
            TcTrace.TraceOut(TraceLevel.Warning, message, _title);
        }

        public void Info(string message)
        {
            TcTrace.TraceOut(TraceLevel.Info, message, _title);
        }

        public void Debug(string message)
        {
            TcTrace.TraceOut(TraceLevel.Verbose, message, _title);
        }
    }
}
