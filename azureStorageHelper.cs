/*
.SYNOPSIS
    
.PREREQUISITES
    
.DESCRIPTION

    
.EXAMPLE
    Call from Controller / Program using the following syntax:
        
.OUTPUTS
    
.NOTES
    Created on: 14.10.2022
    Author:     Aiden Oliver
*/

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace /*InsertNamespaceHere*/.azureHelperFiles
{
    internal class azureStorageHelper
    {
        public long blobQueryLength { get; set; }
        public string blobQueryName { get; set; }
        private readonly BlobServiceClient _blobServiceClient { get; set; }
        private BlobContainerClient _blobContainerClient { get; set; }

        public StorageHelper(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);            
        }

        public async Task<string> downloadFromBlobAsync(string connectionString, string fileDirectory, string azureContainerName)
        {
            var extractFilenamePrefix = "{QueriedFileName}"; // $"aduser-{DateTime.Now.ToString("yyyyMMdd")}";

            BlobContinuationToken continuationToken = null;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();

            var container = serviceClient.GetContainerReference(azureContainerName);

            var blobResultSegment = await container.ListBlobsSegmentedAsync(extractFilenamePrefix, continuationToken);

            var blobList = blobResultSegment.Results.Select(i => i.Uri.Segments.Last()).ToList();

            if (blobList.Count > 0)
            {
                foreach (var blobName in blobList)
                {

                    var b = container.GetBlockBlobReference(blobName);
                    await b.FetchAttributesAsync();
                    long len = b.Properties.Length;

                    if (b.Properties.Length >= blobQueryLength)
                    {
                        blobQueryName = blobName;
                        blobQueryLength = len;
                    }
                }

                CloudBlockBlob blob = container.GetBlockBlobReference(blobQueryName);

                await blob.DownloadToFileAsync($"{fileDirectory}\\{blobQueryName}", FileMode.Create);

                return blobQueryName;
            }
            return null;
        }

        public void downloadFromBlob(string connectionString, string fileDirectory, string azureContainerName)
        {
            var extractFilenamePrefix = "{QueriedFileName}"; // $"aduser-{DateTime.Now.ToString("yyyyMMdd")}";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();

            var container = serviceClient.GetContainerReference(azureContainerName);

            var blobResultSegment = container.ListBlobsSegmentedAsync(extractFilenamePrefix);

            var blobList = blobResultSegment.Results.Select(i => i.Uri.Segments.Last()).ToList();

            if (blobList.Count > 0)
            {
                foreach (var blobName in blobList)
                {

                    var b = container.GetBlockBlobReference(blobName);
                    b.FetchAttributesAsync();
                    long len = b.Properties.Length;

                    if (b.Properties.Length >= blobQueryLength)
                    {
                        blobQueryName = blobName;
                        blobQueryLength = len;
                    }
                }

                CloudBlockBlob blob = container.GetBlockBlobReference(blobQueryName);

                blob.DownloadToFileAsync($"{fileDirectory}\\{blobQueryName}", FileMode.Create);

                return blobQueryName;
            }
            return null;
        }
        
        public void UploadFileToStorage(string fileDirectory, string fileName, string containerName, bool bOverWrite = true)
        {
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            _blobContainerClient.CreateIfNotExists();

            BlobClient blob = _blobContainerClient.GetBlobClient(fileName);

            using (FileStream file = File.OpenRead(fileDirectory))
            {
                blob.Upload(file, overwrite: bOverWrite);
            }
        }
        
        public async Task UploadFileToStorageAsync(string fileDirectory, string fileName, string containerName, bool bOverWrite = true)
        {
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            _blobContainerClient.CreateIfNotExists();

            BlobClient blob = _blobContainerClient.GetBlobClient(fileName);

            using (FileStream file = File.OpenRead(fileDirectory))
            {
                await blob.UploadAsync(file, overwrite: bOverWrite);
            }
        }
    }
}
   
        
        
