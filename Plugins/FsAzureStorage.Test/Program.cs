using System.Threading;
using FsAzureStorage;
using FsAzureStorage.Windows;
using TcPluginBase;


CloudPath remoteName = @"\my-container\demo\test-folder\hoi.txt";

var blobStorage = new BlobStorage();
var blob = await blobStorage.GetBlockBlobReference(remoteName.AccountName, remoteName.ContainerName, remoteName.BlobName, CancellationToken.None);
if (blob == null) {
    return;
}

using var dispatcher = new WpfDispatcher();
dispatcher.Invoke(() => new PropertiesWindow(blob).ShowDialog());
