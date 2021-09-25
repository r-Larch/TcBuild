using System;
using System.Linq;
using Azure.Storage.Blobs.Models;
using MyPlugin;


Console.WriteLine("Hello World!");
var blobStorage = new BlobStorage();
var container = await blobStorage.GetContainerReference("static1icstor", "eventimages");
if (container == null) {
    Console.WriteLine("Container Not Found");
    return;
}

// var prefix = "07bf6d76-2493-4567-8a38-cd25eff61ba4/";
string? prefix = null;

var blobs = container.GetBlobsByHierarchyAsync(traits: BlobTraits.Metadata, prefix: prefix, delimiter: "/");
await foreach (var blob in blobs) {
    Console.WriteLine($"  IsBlob:{blob.IsBlob} IsPrefix:{blob.IsPrefix} Name:{blob.Blob?.Name} Prefix:{blob.Prefix}");
}

// IsBlob:True IsPrefix:False Name:07bf6d76-2493-4567-8a38-cd25eff61ba4/tag-der-offenen-brennereien_2519.jpg Prefix:
// IsBlob:False IsPrefix:True Name: Prefix:dd86ca33-ee85-48f4-bdff-65ea8b3e043b/

//await foreach (var account in blobStorage.GetAccounts()) {
//    try {
//        Console.WriteLine($" {account.Name}");
//        foreach (var container in account.Client.GetBlobContainers(BlobContainerTraits.Metadata).AsPages().SelectMany(_ => _.Values)) {
//            Console.WriteLine($"  - {container.Name}");
//        }
//    }
//    catch (Azure.RequestFailedException e) {
//        Console.WriteLine(e.Message);
//    }
//}
Console.WriteLine(" --- press any key --- ");
Console.ReadLine();
