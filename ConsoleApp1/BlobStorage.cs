using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Storage;
using Azure.Storage.Blobs;


namespace MyPlugin {
    internal class BlobStorage {
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


    internal record BlobStorageAccount(string Name, string Subscription, BlobServiceClient Client);
}
