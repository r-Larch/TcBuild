using System.IO;
using System.Text;
using Xunit.Abstractions;


namespace TcBuild.Test {
    public class Converter : TextWriter {
        readonly ITestOutputHelper _output;
        private readonly string _prefix;

        public Converter(ITestOutputHelper output, string prefix)
        {
            _output = output;
            _prefix = prefix;
        }

        public override Encoding Encoding => Encoding.Default;

        public override void WriteLine(string message)
        {
            _output.WriteLine(_prefix + message);
        }

        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(_prefix + format, args);
        }
    }
}
