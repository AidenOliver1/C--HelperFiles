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

using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AidenOliver.HelperFiles
{
    public class azureStorageHelper
    {
        public long blobQueryLength { get; set; }
        public string blobQueryName { get; set; }
        private BlobServiceClient _blobServiceClient { get; set; }

        private BlobContainerClient _blobContainerClient { get; set; }

        public azureStorageHelper(string connectionString)
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

        // Deprecated as of 25/05/2023 ~ AO
        /*public void downloadFromBlob(string connectionString, string fileDirectory, string azureContainerName, string queryFileName)
        {
            var extractFilenamePrefix = "{QueriedFileName}"; // $"aduser-{DateTime.Now.ToString("yyyyMMdd")}";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();

            var container = serviceClient.GetContainerReference(azureContainerName);

            var blobResultSegment = container.ListBlobsSegmentedAsync(prefix: extractFilenamePrefix, currentToken: null);

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
        }*/

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
