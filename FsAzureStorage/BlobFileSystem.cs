using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;
using TcPluginBase.FileSystem;


// ReSharper disable LocalizableElement

namespace FsAzureStorage {
    internal delegate void FileProgress(string source, string destination, int percentDone);

    internal class BlobFileSystem {
        internal readonly PathCache _pathCache;
        private readonly BlobStorage _blobStorage;

        public BlobFileSystem()
        {
            _blobStorage = new BlobStorage();
            _pathCache = new PathCache();
        }

        public IAsyncEnumerable<FindData> ListDirectory(CloudPath path)
        {
            switch (path.Level) {
                case 0:
                    return GetAccounts();
                case 1 when path.AccountName == "settings":
                    return GetSettings();
                case 1:
                    return GetContainers(path.AccountName);
                default:
                    return GetBlobSegments(path);
            }

            async IAsyncEnumerable<FindData> GetSettings()
            {
                await Task.CompletedTask;
                yield return new FindData("Connect to Azure");
            }

            async IAsyncEnumerable<FindData> GetAccounts()
            {
                yield return new FindData("settings", FileAttributes.Directory);

                await foreach (var account in _blobStorage.GetAccounts()) {
                    yield return new FindData(account.Name, FileAttributes.Directory);
                }
            }

            async IAsyncEnumerable<FindData> GetContainers(string accountName)
            {
                var client = await _blobStorage.GetAccount(accountName);
                if (client == null) {
                    yield break;
                }

                await foreach (var _ in client.Client.GetBlobContainersAsync(prefix: null, traits: BlobContainerTraits.Metadata)) {
                    yield return new FindData(
                        fileName: _.Name,
                        fileSize: 0,
                        attributes: FileAttributes.Directory,
                        lastWriteTime: _.Properties.LastModified.LocalDateTime
                    );
                }
            }

            async IAsyncEnumerable<FindData> GetBlobSegments(CloudPath path)
            {
                var prefix = path.Prefix;

                if (prefix.Length > 0) {
                    prefix += "/";
                }

                var container = await _blobStorage.GetContainerReference(path.AccountName, path.ContainerName);
                if (container == null) {
                    yield break;
                }

                var list = new List<FindData>();
                await foreach (var x in container.GetBlobsByHierarchyAsync(traits: BlobTraits.Metadata, prefix: prefix, delimiter: "/")) {
                    var t = x switch {
                        {IsBlob: true, Blob: { } block} => new FindData(
                            fileName: block.Name[prefix.Length..],
                            fileSize: (ulong) (block.Properties.ContentLength ?? 0),
                            attributes: FileAttributes.Normal,
                            lastWriteTime: block.Properties.LastModified?.LocalDateTime,
                            creationTime: block.Properties.CreatedOn?.LocalDateTime,
                            lastAccessTime: block.Properties.LastModified?.LocalDateTime
                        ),
                        {IsPrefix: true, Prefix: { } dir} => new FindData(
                            fileName: dir[prefix.Length..].TrimEnd('/'),
                            fileSize: 0,
                            attributes: FileAttributes.Directory
                        ),
                        _ => throw new NotSupportedException($"Type: {x.GetType().FullName} not supported!"),
                    };
                    list.Add(t);
                }

                var result = _pathCache
                    .WithCached(path, list)
                    .DefaultIfEmpty(new FindData("..", FileAttributes.Directory));

                foreach (var findData in result) {
                    yield return findData;
                }
            }
        }


        public async Task<GetFileResult> DownloadFile(CloudPath srcFileName, FileInfo dstFileName, bool overwrite, FileProgress fileProgress, bool deleteAfter = false, CancellationToken token = default)
        {
            var blob = await _blobStorage.GetBlockBlobReference(srcFileName.AccountName, srcFileName.ContainerName, srcFileName.BlobName, token);
            if (blob is null || !await blob.ExistsAsync(token)) {
                return GetFileResult.FileNotFound;
            }

            if (!overwrite && dstFileName.Exists) {
                return GetFileResult.FileExists;
            }

            var properties = await blob.GetPropertiesAsync(cancellationToken: token);
            var fileSize = properties.Value.ContentLength;

            Progress(0);

            void Progress(long bytesTransferred)
            {
                var percent = fileSize == 0
                    ? 0
                    : decimal.ToInt32((bytesTransferred * 100) / (decimal) fileSize);

                fileProgress(srcFileName, dstFileName.FullName, percent);
            }

            var mode = overwrite ? FileMode.Create : FileMode.CreateNew;

            try {
                _ = await blob.DownloadToAsync(dstFileName.FullName, mode, new Progress<long>(Progress), cancellationToken: token);

                if (deleteAfter) {
                    await blob.DeleteAsync(cancellationToken: token);
                }

                Progress(fileSize);

                return GetFileResult.Ok;
            }
            catch (IOException) {
                return GetFileResult.WriteError;
            }
            catch (TaskCanceledException) {
                return GetFileResult.UserAbort;
            }
        }


        public async Task<PutFileResult> UploadFile(FileInfo srcFileName, CloudPath dstFileName, bool overwrite, FileProgress fileProgress, CancellationToken token = default)
        {
            var blob = await _blobStorage.GetBlockBlobReference(dstFileName.AccountName, dstFileName.ContainerName, dstFileName.BlobName, token);
            if (blob == null) {
                return PutFileResult.NotSupported;
            }

            if (!overwrite && await blob.ExistsAsync(token)) {
                return PutFileResult.FileExists;
            }

            var fileSize = srcFileName.Length;

            Progress(0);

            void Progress(long bytesTransferred)
            {
                var percent = fileSize == 0
                    ? 0
                    : decimal.ToInt32((bytesTransferred * 100) / (decimal) fileSize);

                fileProgress(dstFileName, srcFileName.FullName, percent);
            }

            try {
                var httpHeaders = new BlobHttpHeaders {
                    ContentType = MimeUtility.GetMimeMapping(srcFileName.Name),
                };
                var metadata = new Dictionary<string, string>();
                _ = await blob.UploadAsync(srcFileName.FullName, httpHeaders, metadata, progressHandler: new Progress<long>(Progress), cancellationToken: token);

                Progress(fileSize);

                return PutFileResult.Ok;
            }
            catch (TaskCanceledException) {
                return PutFileResult.UserAbort;
            }
        }


        public async Task<RenMovFileResult> Move(CloudPath sourceFileName, CloudPath destFileName, bool overwrite, CancellationToken token)
        {
            var source = await _blobStorage.GetBlockBlobReference(sourceFileName.AccountName, sourceFileName.ContainerName, sourceFileName.BlobName, token);
            var target = await _blobStorage.GetBlockBlobReference(destFileName.AccountName, destFileName.ContainerName, destFileName.BlobName, token);

            if (source is null || target is null) {
                return RenMovFileResult.NotSupported;
            }

            if (!overwrite && await target.ExistsAsync(token)) {
                return RenMovFileResult.FileExists;
            }

            var res = await CopyAndOverwrite(source, target, token);
            if (res != RenMovFileResult.Ok) {
                return res;
            }

            if (!await target.ExistsAsync(token)) {
                throw new Exception("Move failed because the target file wasn't created.");
            }

            await source.DeleteIfExistsAsync(cancellationToken: token);
            return RenMovFileResult.Ok;
        }


        public async Task<RenMovFileResult> Copy(CloudPath sourceFileName, CloudPath destFileName, bool overwrite, CancellationToken token)
        {
            var source = await _blobStorage.GetBlockBlobReference(sourceFileName.AccountName, sourceFileName.ContainerName, sourceFileName.BlobName, token);
            var target = await _blobStorage.GetBlockBlobReference(destFileName.AccountName, destFileName.ContainerName, destFileName.BlobName, token);

            if (source is null || target is null) {
                return RenMovFileResult.NotSupported;
            }

            if (!overwrite && await target.ExistsAsync(token)) {
                return RenMovFileResult.FileExists;
            }

            var res = await CopyAndOverwrite(source, target, token);
            if (res != RenMovFileResult.Ok) {
                return res;
            }

            if (!await target.ExistsAsync(token)) {
                throw new Exception("Move failed because the target file wasn't created.");
            }

            return RenMovFileResult.Ok;
        }


        public bool RemoveVirtualDir(CloudPath directory)
        {
            return _pathCache.Remove(directory); // allow removing virtual dirs
        }

        public async Task<bool> DeleteFile(CloudPath fileName)
        {
            var blob = await _blobStorage.GetBlockBlobReference(fileName.AccountName, fileName.ContainerName, fileName.BlobName);
            if (blob is null) {
                return false;
            }

            var success = RemoveVirtualDir(fileName);

            if (await blob.DeleteIfExistsAsync()) {
                // cache the directory to allow adding some files
                _pathCache.Add(fileName.Directory);
                return true;
            }

            return success;
        }


        //public bool RemoveDir(CloudPath dirName)
        //{
        //    // TODO implement (maybe where level > 2)

        //    //MessageBox.Show($"Delete {dirName} ?", "Delete directory?");

        //    //if (!dirName.IsBlobPath) {
        //    //    // don't let TC delete hole accounts and container
        //    //    return true; // return "yes it is deleted" so TC doesn't try to delete all one by one!!
        //    //}

        //    return false;
        //}


        private static async Task<RenMovFileResult> CopyAndOverwrite(BlobClient src, BlobClient dst, CancellationToken token)
        {
            if (!await src.ExistsAsync(token)) {
                return RenMovFileResult.FileNotFound;
            }

            var operation = await dst.StartCopyFromUriAsync(src.Uri, cancellationToken: token);
            await operation.WaitForCompletionAsync(token);

            return RenMovFileResult.Ok;
        }


        public bool CacheDirectory(CloudPath dir)
        {
            if (dir.IsBlobPath) {
                _pathCache.Add(dir);
                return true;
            }

            // can not create accounts and container
            return false;
        }
    }
}
