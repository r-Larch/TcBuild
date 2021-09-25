using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TcPluginBase.FileSystem;


namespace FsAzureStorage {
    internal class PathCache {
        public List<CloudPath> Paths { get; }

        public PathCache()
        {
            Paths = new List<CloudPath>();
        }

        public void Add(CloudPath path)
        {
            Paths.RemoveAll(_ => _.Path == path);
            Paths.Add(path);
        }

        public bool Remove(CloudPath fileName)
        {
            return Paths.RemoveAll(path =>
                    path.Path == fileName.Path ||
                    path.Path.StartsWith($"{fileName.Path}/") // remove all subfolders
            ) > 0;
        }

        public IEnumerable<FindData> WithCached(CloudPath parent, IEnumerable<FindData> list)
        {
            // get a distinct list of all cached folders in "parent" directory
            var folders = Paths
                .Where(x => x.Path.StartsWith(parent))
                .Select(x => x.GetSegment(parent.Level + 1))
                .Where(x => x != null).Distinct().OrderBy(_ => _)
                .Select(x => new FindData(x!, FileAttributes.Directory))
                .ToList();

            // yield all cached folders in correct order together with all real folders and files
            var comp = (IComparer<string>) StringComparer.Ordinal;
            foreach (var data in list) {
                var next = data;

                for (var i = 0; i < folders.Count; i++) {
                    var cached = folders[i];

                    if (cached.FileName == next.FileName && cached.Attributes == next.Attributes) {
                        folders.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (comp.Compare(cached.FileName, next.FileName) < 0) {
                        yield return cached;
                        folders.RemoveAt(i);
                        i--;
                    }
                }

                yield return data;
            }

            foreach (var data in folders) {
                yield return data;
            }
        }
    }
}
