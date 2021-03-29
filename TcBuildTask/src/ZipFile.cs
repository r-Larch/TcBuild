using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;


namespace TcBuild {
    public class ZipFile : IDisposable {
        private readonly ZipArchive _zip;
        private readonly string _baseDirectory;

        public ZipFile(FileInfo zipFile, DirectoryInfo baseDirectory)
        {
            _baseDirectory = baseDirectory.FullName.TrimEnd('\\') + '\\';
            _zip = new ZipArchive(OpenWrite(zipFile), ZipArchiveMode.Create);
        }

        public ZipFile Add(FileInfo file)
        {
            if (file.Exists) {
                using var fileContents = OpenRead(file);
                var fileName = file.FullName.Replace(_baseDirectory, "");
                Add(fileName, fileContents);
            }

            return this;
        }

        public ZipFile AddRange(IEnumerable<FileInfo> files)
        {
            foreach (var file in files) {
                Add(file);
            }

            return this;
        }

        public ZipFile Add(string fileName, string fileContents, Encoding? encoding = default)
        {
            using var entry = _zip.CreateEntry(fileName).Open();
            using var sw = new StreamWriter(entry, encoding ?? Encoding.Default);
            sw.Write(fileContents);

            return this;
        }

        public ZipFile Add(string fileName, Stream fileContents)
        {
            using var entry = _zip.CreateEntry(fileName).Open();
            fileContents.CopyTo(entry);

            return this;
        }

        private static Stream OpenWrite(FileInfo zipFile)
        {
            var count = 0;
            while (true) {
                try {
                    return zipFile.OpenWrite();
                }
                catch (IOException) {
                    if (count++ < 10) {
                        Thread.Sleep(10 * count * count);
                        continue;
                    }
                    else {
                        throw;
                    }
                }
            }
        }

        private static Stream OpenRead(FileInfo zipFile)
        {
            var count = 0;
            while (true) {
                try {
                    return zipFile.OpenRead();
                }
                catch (IOException) {
                    if (count++ < 10) {
                        Thread.Sleep(10 * count * count);
                        continue;
                    }
                    else {
                        throw;
                    }
                }
            }
        }

        public void Dispose()
        {
            _zip.Dispose();
        }
    }
}
