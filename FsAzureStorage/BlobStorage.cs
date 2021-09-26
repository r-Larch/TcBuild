using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


namespace FsAzureStorage {
    public class BlobStorage {
        private readonly List<BlobStorageAccount> _accounts = new();

        public async IAsyncEnumerable<BlobStorageAccount> GetAccounts([EnumeratorCancellation] CancellationToken token = default)
        {
            if (_accounts.Count > 0) {
                foreach (var account in _accounts) {
                    yield return account;
                }
            }

            else {
                var credential = new DefaultAzureCredential(includeInteractiveCredentials: true);
                var armClient = new ArmClient(credential);

                await foreach (var subscription in armClient.GetSubscriptions().GetAllAsync()) {
                    await foreach (var storage in subscription.GetStorageAccountsAsync()) {
                        var account = new BlobStorageAccount(
                            Name: storage.Data.Name,
                            Subscription: subscription.Data.DisplayName,
                            Client: new BlobServiceClient(new Uri($"https://{storage.Data.Name}.blob.core.windows.net"), credential)
                        );
                        _accounts.Add(account);
                        yield return account;
                    }
                }
            }
        }

        public async Task<BlobStorageAccount?> GetAccount(string accountName, CancellationToken token = default)
        {
            await foreach (var account in GetAccounts(token)) {
                if (account.Name == accountName) {
                    return account;
                }
            }

            return null;
        }

        public async Task<BlobContainerClient?> GetContainerReference(string accountName, string containerName, CancellationToken token = default)
        {
            try {
                var account = await GetAccount(accountName, token);
                if (account is null) {
                    return null;
                }

                var container = account.Client.GetBlobContainerClient(containerName);
                if (!await container.ExistsAsync(token)) {
                    return null;
                }

                return container;
            }
            catch {
                return null;
            }
        }

        public async Task<BlobClient?> GetBlockBlobReference(string accountName, string containerName, string blobName, CancellationToken token = default)
        {
            try {
                var container = await GetContainerReference(accountName, containerName, token);
                if (container is null) {
                    return null;
                }

                var blob = container.GetBlobClient(blobName);
                return blob;
            }
            catch {
                return null;
            }
        }
    }


    public record BlobStorageAccount(string Name, string Subscription, BlobServiceClient Client);


    internal static class Extensions {
        public static async Task<Response> DownloadToAsync(this BlobClient blob, string path, FileMode mode, IProgress<long> progressHandler, int sleepMs = 100, CancellationToken cancellationToken = default)
        {
            await using var fs = new FileStream(path, mode);

            var download = blob.DownloadToAsync(fs, cancellationToken: cancellationToken);

            await Task.Run(() => {
                while (!download.IsCompleted) {
                    Thread.Sleep(sleepMs);
                    progressHandler.Report(fs.Length);
                }
            }, cancellationToken);

            return download.Result;
        }


        public static async Task<CopyFromUriOperation> CopyAndOverwrite(this BlobClient src, BlobClient dst, IProgress<long> progressHandler, CancellationToken cancellationToken = default)
        {
            var properties = await src.GetPropertiesAsync(cancellationToken: cancellationToken);

            var sw = new Stopwatch();
            sw.Start();

            var operation = await dst.StartCopyFromUriAsync(src.Uri, metadata: properties.Value.Metadata, cancellationToken: cancellationToken);
            //await operation.WaitForCompletionAsync(token);

            // TODO improve this hack!

            const double transferRate = (1024 * 1024) / 1000d; // 1MB/s

            var fileSize = properties.Value.ContentLength;
            var timeRemaining = (fileSize / transferRate) - sw.Elapsed.TotalMilliseconds;
            var sleep = (int) Math.Max(100, timeRemaining / 20);

            while (!operation.HasCompleted) {
                await Task.Delay(sleep, cancellationToken).ConfigureAwait(false);
                await operation.UpdateStatusAsync(cancellationToken).ConfigureAwait(false);

                var bytesTransferred = transferRate * sw.Elapsed.TotalMilliseconds;
                var report = (int) Math.Min(bytesTransferred, fileSize * 0.9d);

                progressHandler.Report(report);

                cancellationToken.ThrowIfCancellationRequested();
            }

            progressHandler.Report(fileSize);

            sw.Stop();

            return operation;
        }
    }
}
