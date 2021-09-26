using FsAzureStorage.Windows;
using TcPluginBase;


namespace FsAzureStorage.Test {
    class Program {
        static void Main(string[] args)
        {
            CloudPath remoteName = @"\egonstorage\demo\test-folder\hoi.txt";
            var _blobStorage = new BlobStorage();
            var blob = _blobStorage.GetBlockBlobReference(remoteName.AccountName, remoteName.ContainerName, remoteName.BlobName, default).GetAwaiter().GetResult();
            if (blob == null) {
                return;
            }

            using var dispatcher = new WpfDispatcher();
            dispatcher.Invoke(() => new PropertiesWindow(blob).ShowDialog());
        }
    }
}
