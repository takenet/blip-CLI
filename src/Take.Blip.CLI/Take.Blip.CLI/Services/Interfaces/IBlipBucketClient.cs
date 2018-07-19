using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Handlers;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IBlipBucketClient
    {
        Task<DocumentCollection> GetAllDocumentKeysAsync(BucketNamespace bucketNamespace);
        Task<IEnumerable<KeyValuePair<string, Document>>> GetAllDocumentsAsync(DocumentCollection keysCollection, BucketNamespace bucketNamespace);
        Task<KeyValuePair<string, Document>?> GetDocumentAsync(string key, BucketNamespace bucketNamespace);
        Task AddDocumentAsync(string key, Document document, BucketNamespace bucketNamespace);
    }
}
