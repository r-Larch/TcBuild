using System;


namespace TcPluginBase.FileSystem {
    /// <summary>
    /// A RemotePath always starts with a backslash, then the names returned by FsFindFirst/FsFindNext separated by backslashes.
    /// </summary>
    public struct RemotePath {
        // Path = '\just\some\path\starting\with\backslash\file.txt'
        public string Path { get; set; }

        public bool HasValue => Path.Length > 0;

        /// <summary>
        /// '\'                 level 0
        /// '\segment'          level 1
        /// '\segment\segment'  level 2
        /// </summary>
        public int Level => Segments.Length;

        /// <summary>true if this RemotePath has a trailing slash</summary>
        public bool TrailingSlash => (Path.Length > 1 ? Path[Path.Length - 1] : (char) 0) == '\\';

        /// <summary>Returns the directory information for this RemotePath.</summary>
        public RemotePath Directory => System.IO.Path.GetDirectoryName(Path);

        /// <summary>Returns the file name and extension of this RemotePath.</summary>
        public string FileName => System.IO.Path.GetFileName(Path);

        /// <summary>
        /// Create a new Path with a different fileName
        /// </summary>
        public RemotePath SetFileName(string fileName)
        {
            var newPath = Path.Substring(0, Path.IndexOf(FileName, StringComparison.Ordinal)) + fileName;
            return newPath;
        }

        /// <summary>Returns the file name of this RemotePath without the extension.</summary>
        public string FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(Path);

        /// <summary>Returns the extension of this RemotePath.</summary>
        public string Extension => System.IO.Path.GetExtension(Path);

        /// <summary>Returns a new RemotePath consisting of the current Path without trailing slash.</summary>
        public RemotePath PathWithoutTrailingSlash => TrailingSlash
            ? Path.Substring(0, Path.Length - 1)
            : Path;

        public RemotePath(string path)
        {
            if (string.IsNullOrEmpty(path)) {
                Path = string.Empty;
                return;
            }

            path = path.Replace('/', '\\');

            if (path[0] != '\\') {
                throw new NotSupportedException($"relative paths are not supported! path: '{path}'");
            }

            Path = path;
        }


        public string GetSegment(int level)
        {
            var index = level - 1;
            if (index < 0) {
                return null;
            }

            return index < Segments.Length ? Segments[index] : null;
        }


        public string[] Segments {
            get {
                var path = Path;
                path = TrailingSlash
                    ? path.Substring(1, path.Length - 1)
                    : path.Substring(1);

                return string.IsNullOrEmpty(path)
                    ? new string[0]
                    : path.Split('\\');
            }
        }


        public static RemotePath operator +(RemotePath parentPath, RemotePath subPath)
        {
            return new RemotePath(parentPath.PathWithoutTrailingSlash + subPath.Path);
        }

        public static RemotePath operator +(RemotePath path, string segment)
        {
            return new RemotePath(path.Path + segment);
        }

        public static RemotePath operator +(string parentPath, RemotePath subPath)
        {
            return new RemotePath(parentPath + subPath.Path);
        }


        public static implicit operator string(RemotePath remotePath)
        {
            return remotePath.ToString();
        }

        public static implicit operator RemotePath(string path)
        {
            return new RemotePath(path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
