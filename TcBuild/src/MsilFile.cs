using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace TcBuild {
    internal class MsilFile : IDisposable {
        private readonly FileInfo _sourceFile;
        private readonly DirectoryInfo _dir;

        private List<CodeBlock> Blocks { get; set; }
        public IEnumerable<ClassBlock> Classes => Blocks.OfType<ClassBlock>();
        public IEnumerable<AssemblyBlock> Assemblies => Blocks.OfType<AssemblyBlock>();

        public MsilFile(FileInfo sourceFile, DirectoryInfo dir = null)
        {
            _sourceFile = sourceFile;
            _dir = dir;
            Parse();
        }


        private void Parse()
        {
            var blocks = new List<CodeBlock>();
            foreach (var block in GetCodeBlocks()) {
                switch (block.Type) {
                    case ".assembly":
                        var assembly = AssemblyBlock.Parse(block);
                        blocks.Add(assembly);
                        break;
                    case ".class":
                        var clazz = ClassBlock.Parse(block);
                        blocks.Add(clazz);
                        break;
                    default:
                        blocks.Add(block);
                        break;
                }
            }

            Blocks = blocks;
        }


        public void AddClasses(IEnumerable<ClassBlock> classes)
        {
            var index = Blocks.FindLastIndex(block => block is ClassBlock);
            Blocks.InsertRange(index + 1, classes);
        }

        public void AddAssembly(AssemblyBlock assembly)
        {
            var index = Blocks.FindLastIndex(block => block is AssemblyBlock);
            Blocks.Insert(index + 1, assembly);
        }


        private CodeBlock GetMethod(IEnumerator<string> codeLines)
        {
            var line = codeLines.Current.TrimStart(' ');
            if (line.StartsWith(".method ")) {
                return MethodBlock.Parse(ReadBlock(".method", codeLines));
            }

            return null;
        }

        private IEnumerable<CodeBlock> GetCodeBlocks()
        {
            var codeLines = GetCodeLines().GetEnumerator();
            while (codeLines.MoveNext()) {
                var lineOrig = codeLines.Current;
                var line = lineOrig.TrimStart(' ');

                if (line.StartsWith(".assembly ")) {
                    yield return ReadBlock(".assembly", codeLines);
                }
                else if (line.StartsWith(".class ")) {
                    yield return ReadBlock(".class", codeLines, GetMethod);
                }
                else if (line.StartsWith(".data ")) {
                    yield return new CodeBlock {Type = ".data", Header = line, Lines = new object[0]};
                }
                else {
                    yield return new CodeBlock {
                        Type = null, // unknown
                        Header = lineOrig,
                        Lines = new object[0]
                    };
                }
            }
        }

        private CodeBlock ReadBlock(string type, IEnumerator<string> codeLines, Func<IEnumerator<string>, CodeBlock> readBody = null)
        {
            var header = codeLines.Current;

            var openBraces = 0;
            var blockEntered = false;
            var lines = new List<object>();
            while (codeLines.MoveNext()) {
                var line = codeLines.Current;

                foreach (var ch in line) {
                    if (ch == '{') {
                        openBraces++;
                        blockEntered = true;
                    }
                    else if (ch == '}') openBraces--;
                }

                if (blockEntered) {
                    var block = readBody?.Invoke(codeLines);
                    if (block != null) {
                        lines.Add(block);
                    }
                    else {
                        lines.Add(line);
                    }
                }
                else {
                    header += (string.IsNullOrEmpty(header) ? "" : Environment.NewLine) + line;
                }

                if (blockEntered && openBraces == 0) {
                    break;
                }
            }

            return new CodeBlock {
                Type = type,
                Header = header,
                Lines = lines.ToArray(),
            };
        }


        private IEnumerable<string> GetCodeLines()
        {
            var openBraces = 0;
            var codeLine = "";
            foreach (var line in GetFileLines()) {
                foreach (var ch in line) {
                    if (ch == '(') openBraces++;
                    else if (ch == ')') openBraces--;
                }

                codeLine += (codeLine.Length == 0 ? string.Empty : Environment.NewLine) + line;

                if (openBraces == 0) {
                    yield return codeLine;
                    codeLine = "";
                }
            }

            if (codeLine.Length > 0) {
                throw new Exception("There was code at the end");
            }
        }


        private IEnumerable<string> GetFileLines()
        {
            using (var fs = _sourceFile.OpenRead())
            using (var sr = new StreamReader(fs)) {
                while (!sr.EndOfStream) {
                    yield return sr.ReadLine();
                }
            }
        }


        public FileInfo SaveTo(string fileName)
        {
            var file = new FileInfo(Path.Combine(_sourceFile.Directory.FullName, fileName));
            using (var fs = file.OpenWrite())
            using (var sw = new StreamWriter(fs)) {
                foreach (var codeBlock in Blocks) {
                    sw.WriteLine(codeBlock.ToString());
                }
            }

            return file;
        }

        public void Dispose()
        {
            _dir?.Delete(true);
        }
    }


    internal class MethodBlock : CodeBlock {
        public string Signature;
        public string Name;
        public bool Public;
        public bool Static;
        public bool Instance;

        public static CodeBlock Parse(CodeBlock block)
        {
            var tokens = Tokenize(block.Header);

            var signature = tokens.First(_ => _.IndexOf('(') != -1 && !_.StartsWith("marshal(") && !_.StartsWith("pinvokeimpl("));
            var nameIndex = signature.IndexOf('(');
            var name = signature.Substring(0, nameIndex);

            return new MethodBlock {
                Signature = signature,
                Name = name,
                Public = tokens.Contains("public"),
                Static = tokens.Contains("static"),
                Instance = tokens.Contains("instance"),

                Type = block.Type,
                Header = block.Header,
                Lines = block.Lines
            };
        }

        private static List<string> Tokenize(string methodHeader)
        {
            var tokens = methodHeader.Split(new[] {' ', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>();
            var openBraces = 0;
            var codeLine = "";
            foreach (var line in tokens) {
                foreach (var ch in line) {
                    if (ch == '(') openBraces++;
                    else if (ch == ')') openBraces--;
                }

                codeLine += (codeLine.Length == 0 ? string.Empty : " ") + line;

                if (openBraces == 0) {
                    list.Add(codeLine);
                    codeLine = "";
                }
            }

            if (codeLine.Length > 0) {
                throw new Exception("There was code at the end");
            }

            return list;
        }
    }

    internal class ClassBlock : CodeBlock {
        public string Name;
        public MethodBlock[] Methods;

        public static ClassBlock Parse(CodeBlock block)
        {
            var tokens = block.Header.Split(new[] {' ', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();

            var nameIndex = tokens.IndexOf("extends") - 1;
            if (nameIndex < 0) {
                if (tokens.IndexOf("interface") != -1) {
                    nameIndex = tokens.Count - 1;
                }
                else {
                    throw new Exception($"Could not find class name in: '{string.Join(" ", tokens)}'");
                }
            }

            return new ClassBlock {
                Name = tokens[nameIndex],
                Methods = block.Lines.OfType<MethodBlock>().ToArray(),

                Type = block.Type,
                Header = block.Header,
                Lines = block.Lines
            };
        }
    }


    internal class AssemblyBlock : CodeBlock {
        public string Name;
        public bool Extern;

        public static AssemblyBlock Parse(CodeBlock block)
        {
            var tokens = block.Header.Split(new[] {' ', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            return new AssemblyBlock {
                Name = tokens.Last(),
                Extern = tokens.Contains("extern"),

                Type = block.Type,
                Header = block.Header,
                Lines = block.Lines
            };
        }
    }

    internal class CodeBlock {
        public string Type { get; set; }
        public string Header { get; set; }
        public object[] Lines { get; set; }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, new[] {Header}.Concat(Lines).Select(_ => _.ToString()));
        }
    }
}
