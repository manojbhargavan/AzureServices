using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;

namespace AzureServiceLibrary.StorageAccount
{
    public class ContainerWrapper
    {
        public string storageAccountName { get; private set; }
        public string sharedAccessSignatureKey { get; private set; }
        private string ConnectionString { get; }
        public BlobServiceClient BlobServiceClient { get; private set; }
        public Pageable<BlobContainerItem> BlobContainerItems { get; private set; }
        public Dictionary<string, BlobContainerClient> BlobContainerClient { get; private set; }

        public ContainerWrapper(string storageAccountName, string sharedAccessSignatureKey)
        {
            ConnectionString = $"BlobEndpoint=https://{storageAccountName}.blob.core.windows.net/;SharedAccessSignature={sharedAccessSignatureKey}";
            BlobServiceClient = new BlobServiceClient(ConnectionString);
            RefreshContainerDetails();
        }

        public void RefreshContainerDetails()
        {
            BlobContainerClient = new Dictionary<string, BlobContainerClient>();
            BlobContainerItems = BlobServiceClient.GetBlobContainers();
            foreach (var container in BlobContainerItems)
            {
                BlobContainerClient.Add(container.Name, BlobServiceClient.GetBlobContainerClient(container.Name));
            }
        }

        public Pageable<BlobItem> GetContainerContents(string containerName)
        {
            BlobContainerClient blobContainerClient;
            bool gotBolb = BlobContainerClient.TryGetValue(containerName, out blobContainerClient);
            Pageable<BlobItem> items;
            if (gotBolb)
            {
                items = blobContainerClient.GetBlobs();
            }
            else
            {
                items = null;
            }
            return items;
        }

        public Response<BlobContainerClient> CreateContainer(string containerName)
        {
            try
            {
                var creationResult = BlobServiceClient.CreateBlobContainer(containerName);
                RefreshContainerDetails();
                return creationResult;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
