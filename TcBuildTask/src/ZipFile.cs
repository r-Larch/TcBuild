using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;


namespace TcBuild {
    public class ZipFile : IDisposable {
        private readonly ZipArchive _zip;

        public ZipFile(FileInfo zipFile)
        {
            zipFile.Delete();
            _zip = new ZipArchive(zipFile.OpenWrite(), ZipArchiveMode.Create);
        }

        public ZipFile Add(FileInfo file)
        {
            using var fileContents = file.OpenRead();
            Add(file.Name, fileContents);

            return this;
        }

        public ZipFile AddRange(IEnumerable<FileInfo> files)
        {
            foreach (var file in files) {
                Add(file);
            }

            return this;
        }

        public ZipFile Add(string fileName, string fileContents, Encoding encoding = default)
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

        public void Dispose()
        {
            _zip.Dispose();
        }
    }
}
