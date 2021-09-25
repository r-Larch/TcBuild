using System;
using System.Linq;
using TcPluginBase.FileSystem;


namespace FsAzureStorage {
    public struct CloudPath {
        public string[] Segments { get; set; }

        // Path = '/some/segments/without/trailing/slash'
        public string Path => $"/{string.Join("/", Segments)}";

        public string AccountName => GetSegment(1) ?? string.Empty;
        public string ContainerName => GetSegment(2) ?? string.Empty;
        public string Prefix => string.Join("/", Segments.Skip(2));
        public string BlobName => Prefix;

        /// <summary>
        /// '\'                 level 0<br/>
        /// '\segment'          level 1<br/>
        /// '\segment\segment'  level 2
        /// </summary>
        public int Level => Segments.Length;

        //public bool IsAccountPath => !string.IsNullOrEmpty(AccountName);
        //public bool IsContainerPath => !string.IsNullOrEmpty(ContainerName);
        public bool IsBlobPath => Level > 2;
        public CloudPath Directory => System.IO.Path.GetDirectoryName(Path);

        /// <param name="path"> can contain backslashes and forward slashes </param>
        public CloudPath(string? path)
        {
            if (string.IsNullOrEmpty(path)) {
                Segments = new string[0];
                return;
            }

            var separators = new[] {'\\', '/'};

            path = path.Trim();

            if (!separators.Contains(path[0])) {
                throw new NotSupportedException($"relative paths are not supported! path: '{path}'");
            }

            path = path.Length > 1 && separators.Contains(path[^1]) ? path[..^1] : path;

            var substring = path[1..];
            Segments = string.IsNullOrEmpty(substring)
                ? new string[0]
                : substring.Split(separators);
        }

        public string? GetSegment(int level)
        {
            var index = level - 1;
            if (index < 0) {
                return null;
            }

            return index < Segments.Length ? Segments[index] : null;
        }

        public static implicit operator string(CloudPath path)
        {
            return path.ToString();
        }

        public static implicit operator CloudPath(string? path)
        {
            return new CloudPath(path);
        }

        public static implicit operator CloudPath(RemotePath path)
        {
            return new CloudPath(path);
        }

        public static implicit operator RemotePath(CloudPath path)
        {
            return new RemotePath(path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
