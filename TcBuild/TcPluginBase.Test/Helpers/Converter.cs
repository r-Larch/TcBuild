using System.IO;
using System.Text;
using Xunit.Abstractions;


namespace TcPluginBase.Test.Helpers {
    public class Converter(ITestOutputHelper output, string prefix) : TextWriter {
        public override Encoding Encoding => Encoding.Default;

        public override void WriteLine(string? message)
        {
            output.WriteLine(prefix + message);
        }

        public override void WriteLine(string format, params object?[] args)
        {
            output.WriteLine(prefix + format, args);
        }
    }
}
