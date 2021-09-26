using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Azure.Storage.Blobs.Models;


namespace FsAzureStorage.Windows {
    public class DesignTimePropertiesViewModel : PropertiesViewModel {
        public DesignTimePropertiesViewModel() : base(new BlobProperties {
            Metadata = {
                {"Key 1", "Value 1"},
                {"Key 2", "Value 2"},
                {"Key 3", "Value 3"},
            }
        })
        {
        }
    }

    public class PropertiesViewModel : INotifyPropertyChanged {
        private ListCollectionView<MetadataValue> _metadata = null!;
        private ObservableCollection<PropertyValue> _properties = null!;

        public ListCollectionView<MetadataValue> Metadata {
            get => _metadata;
            set {
                _metadata = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PropertyValue> Properties {
            get => _properties;
            set {
                _properties = value;
                OnPropertyChanged();
            }
        }

        public PropertiesViewModel(BlobProperties properties)
        {
            Metadata = new ListCollectionView<MetadataValue>(properties.Metadata.Select(_ => new MetadataValue {Key = _.Key, Value = _.Value}));
            Properties = new ObservableCollection<PropertyValue> {
                new(
                    name: nameof(properties.BlobType),
                    value: properties.BlobType.ToString(),
                    desc: "The blob's type."
                ),
                new(
                    name: nameof(properties.ContentLength),
                    value: properties.ContentLength.ToString(),
                    desc: "The number of bytes present in the response body."
                ),
                new(
                    name: nameof(properties.ContentType),
                    value: properties.ContentType,
                    desc: "The content type specified for the blob. The default content type is 'application/octet-stream'."
                ),
                new(
                    name: nameof(properties.ETag),
                    value: properties.ETag.ToString(),
                    desc: "The ETag contains a value that you can use to perform operations conditionally.\nIf the request version is 2011-08-18 or newer, the ETag value will be in quotes."
                ),
                new(
                    name: nameof(properties.ContentHash),
                    value: properties.ContentHash?.Aggregate(string.Empty, (c, n) => c + n.ToString("x2")),
                    desc: "If the blob has an MD5 hash and this operation is to read the full blob, this response header is\nreturned so that the client can check for message content integrity."
                ),
                new(
                    name: nameof(properties.ContentEncoding),
                    value: properties.ContentEncoding,
                    desc: "This header returns the value that was specified for the Content-Encoding request header."
                ),
                new(
                    name: nameof(properties.ContentDisposition),
                    value: properties.ContentDisposition,
                    desc: "This header returns the value that was specified for the 'x-ms-blob-content-disposition' header.\nThe Content-Disposition response header field conveys additional information about how to process\nthe response payload, and also can be used to attach additional metadata. For example, if set to\nattachment, it indicates that the user-agent should not display the response, but instead show a\nSave As dialog with a filename other than the blob name specified."
                ),
                new(
                    name: nameof(properties.ContentLanguage),
                    value: properties.ContentLanguage,
                    desc: "This header returns the value that was specified for the Content-Language request header."
                ),
                new(
                    name: nameof(properties.CacheControl),
                    value: properties.CacheControl,
                    desc: "This header is returned if it was previously specified for the blob."
                ),
                new(
                    name: nameof(properties.LastModified),
                    value: properties.LastModified.ToString("g"),
                    desc: "Returns the date and time the blob was last modified. Any operation that modifies the blob,\nincluding an update of the blob's metadata or properties, changes the last-modified time of the blob."
                ),
                new(
                    name: nameof(properties.CreatedOn),
                    value: properties.CreatedOn.ToString("g"),
                    desc: "Returns the date and time the blob was created."
                ),
                new(
                    name: nameof(properties.LastAccessed),
                    value: properties.LastAccessed.ToString("g"),
                    desc: "Returns the date and time the blob was read or written to."
                ),

                new(
                    name: nameof(properties.CopyCompletedOn),
                    value: properties.CopyCompletedOn.ToString("g"),
                    desc: "Conclusion time of the last attempted Copy Blob operation where this blob was the destination blob.\nThis value can specify the time of a completed, aborted, or failed copy attempt. This header does\nnot appear if a copy is pending, if this blob has never been the destination in a Copy Blob operation,\nor if this blob has been modified after a concluded Copy Blob operation using Set Blob Properties,\nPut Blob, or Put Block List."
                ),
                new(
                    name: nameof(properties.CopyStatusDescription),
                    value: properties.CopyStatusDescription,
                    desc: "Only appears when x-ms-copy-status is failed or pending. Describes the cause of the last fatal or\nnon-fatal copy operation failure. This header does not appear if this blob has never been the destination\nin a Copy Blob operation, or if this blob has been modified after a concluded Copy Blob operation using\nSet Blob Properties, Put Blob, or Put Block List"
                ),
                new(
                    name: nameof(properties.CopyId),
                    value: properties.CopyId,
                    desc: "String identifier for this copy operation. Use with Get Blob Properties to check the status of this copy\noperation, or pass to Abort Copy Blob to abort a pending copy."
                ),
                new(
                    name: nameof(properties.CopyProgress),
                    value: properties.CopyProgress,
                    desc: "Contains the number of bytes copied and the total bytes in the source in the last attempted Copy Blob\noperation where this blob was the destination blob. Can show between 0 and Content-Length bytes copied.\nThis header does not appear if this blob has never been the destination in a Copy Blob operation, or\nif this blob has been modified after a concluded Copy Blob operation using Set Blob Properties, Put\nBlob, or Put Block List."
                ),
                new(
                    name: nameof(properties.CopySource),
                    value: properties.CopySource?.ToString(),
                    desc: "URL up to 2 KB in length that specifies the source blob or file used in the last attempted Copy Blob\noperation where this blob was the destination blob. This header does not appear if this blob has never\nbeen the destination in a Copy Blob operation, or if this blob has been modified after a concluded\nCopy Blob operation using Set Blob Properties, Put Blob, or Put Block List."
                ),
                new(
                    name: nameof(properties.CopyStatus),
                    value: properties.CopyStatus.ToString(),
                    desc: "State of the copy operation identified by x-ms-copy-id."
                ),
                new(
                    name: nameof(properties.IsIncrementalCopy),
                    value: properties.IsIncrementalCopy.ToString(),
                    desc: "Included if the blob is incremental copy blob."
                ),
                new(
                    name: nameof(properties.DestinationSnapshot),
                    value: properties.DestinationSnapshot,
                    desc: "Included if the blob is incremental copy blob or incremental copy snapshot, if x-ms-copy-status is success.\nSnapshot time of the last successful incremental copy snapshot for this blob."
                ),
                new(
                    name: nameof(properties.LeaseDuration),
                    value: properties.LeaseDuration.ToString(),
                    desc: "When a blob is leased, specifies whether the lease is of infinite or fixed duration."
                ),
                new(
                    name: nameof(properties.LeaseState),
                    value: properties.LeaseState.ToString(),
                    desc: "Lease state of the blob."
                ),
                new(
                    name: nameof(properties.LeaseStatus),
                    value: properties.LeaseStatus.ToString(),
                    desc: "The current lease status of the blob."
                ),

                new(
                    name: nameof(properties.BlobSequenceNumber),
                    value: properties.BlobSequenceNumber.ToString(),
                    desc: "The current sequence number for a page blob. This header is not returned for block blobs or append blobs."
                ),
                new(
                    name: nameof(properties.AcceptRanges),
                    value: properties.AcceptRanges,
                    desc: "Indicates that the service supports requests for partial blob content."
                ),
                new(
                    name: nameof(properties.BlobCommittedBlockCount),
                    value: properties.BlobCommittedBlockCount.ToString(),
                    desc: "The number of committed blocks present in the blob. This header is returned only for append blobs."
                ),

                new(
                    name: nameof(properties.IsServerEncrypted),
                    value: properties.IsServerEncrypted.ToString(),
                    desc: "The value of this header is set to true if the blob data and application metadata are completely encrypted\nusing the specified algorithm. Otherwise, the value is set to false (when the blob is unencrypted, or if\nonly parts of the blob/application metadata are encrypted)."
                ),
                new(
                    name: nameof(properties.EncryptionKeySha256),
                    value: properties.EncryptionKeySha256,
                    desc: "The SHA-256 hash of the encryption key used to encrypt the metadata. This header is only returned when the\nmetadata was encrypted with a customer-provided key."
                ),
                new(
                    name: nameof(properties.EncryptionScope),
                    value: properties.EncryptionScope,
                    desc: "Returns the name of the encryption scope used to encrypt the blob contents and application metadata.\nNote that the absence of this header implies use of the default account encryption scope."
                ),

                new(
                    name: nameof(properties.AccessTier),
                    value: properties.AccessTier,
                    desc: "The tier of page blob on a premium storage account or tier of block blob on blob storage LRS accounts.\nFor a list of allowed premium page blob tiers, see\nhttps://docs.microsoft.com/en-us/azure/virtual-machines/windows/premium-storage#features. For blob\nstorage LRS accounts, valid values are Hot/Cool/Archive."
                ),
                new(
                    name: nameof(properties.AccessTierInferred),
                    value: properties.AccessTierInferred.ToString(),
                    desc: "For page blobs on a premium storage account only. If the access tier is not explicitly set on the blob,\nthe tier is inferred based on its content length and this header will be returned with true value."
                ),
                new(
                    name: nameof(properties.ArchiveStatus),
                    value: properties.ArchiveStatus,
                    desc: "For blob storage LRS accounts, valid values are rehydrate-pending-to-hot/rehydrate-pending-to-cool.\nIf the blob is being rehydrated and is not complete then this header is returned indicating that\nrehydrate is pending and also tells the destination tier."
                ),
                new(
                    name: nameof(properties.AccessTierChangedOn),
                    value: properties.AccessTierChangedOn.ToString(),
                    desc: "The time the tier was changed on the object. This is only returned if the tier on the block blob was ever set."
                ),

                new(
                    name: nameof(properties.VersionId),
                    value: properties.VersionId,
                    desc: "A DateTime value returned by the service that uniquely identifies the blob. The value of this header\nindicates the blob version, and may be used in subsequent requests to access this version of the blob."
                ),
                new(
                    name: nameof(properties.IsLatestVersion),
                    value: properties.IsLatestVersion.ToString(),
                    desc: "The value of this header indicates whether version of this blob is a current version, see also x-ms-version-id header."
                ),

                new(
                    name: nameof(properties.TagCount),
                    value: properties.TagCount.ToString(),
                    desc: "The number of tags associated with the blob."
                ),
                new(
                    name: nameof(properties.ExpiresOn),
                    value: properties.ExpiresOn.ToString(),
                    desc: "The time this blob will expire."
                ),
                new(
                    name: nameof(properties.IsSealed),
                    value: properties.IsSealed.ToString(),
                    desc: "If this blob has been sealed."
                ),
                new(
                    name: nameof(properties.RehydratePriority),
                    value: properties.RehydratePriority,
                    desc: "If this blob is in rehydrate pending state, this indicates the rehydrate priority."
                ),
                new(
                    name: nameof(properties.ImmutabilityPolicy),
                    value: $"{properties.ImmutabilityPolicy?.PolicyMode} expires: {properties.ImmutabilityPolicy?.ExpiresOn:g}",
                    desc: "The mode of the Immutability Policy and the date and time when the it expires.\nValid values are 'Locked' and 'Unlocked'."
                ),
                new(
                    name: nameof(properties.HasLegalHold),
                    value: properties.HasLegalHold.ToString(),
                    desc: "Indicates if the blob has a legal hold."
                ),
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class PropertyValue {
        public string Name { get; }
        public string? Value { get; }
        public string Desc { get; }

        public PropertyValue(string name, string? value, string desc)
        {
            Name = name;
            Value = value;
            Desc = desc;
        }
    }

    public class MetadataValue {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }


    public class ListCollectionView<T> : ListCollectionView, IEnumerable<T> where T : class, new() {
        public ListCollectionView(IEnumerable<T> list) : base(list.ToList())
        {
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>) this.SourceCollection).GetEnumerator();
    }
}
