using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using MVC_WebApplication.Data.Model;
using Newtonsoft.Json.Converters;

namespace MVC_WebApplication.Data.DataAccess
{
    public class BlobStorageDataAccess
    {
        private const string ConnectionStringSettingName = "StorageConnectionString";
        private const string ContainerName = "products";

        #region Upload & Download

        public void Upload(byte[] file, string fileName)
        {
            UploadFileInBlocks(file, fileName);
        }

        public void SimpleUpload(byte[] file, string fileName)
        {
            CloudBlobContainer container = GetContainerReference();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            blockBlob.UploadFromByteArray(file, 0, file.Length);
        }

        public void UploadFileInBlocksWithOptions(byte[] file, string fileName)
        {
            CloudBlobContainer cloudBlobContainer = GetContainerReference();
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(fileName));

            BlobRequestOptions requestOptions = new BlobRequestOptions
            {
                // This setting determines whether the blob will be uploaded in one request (Put Blob) or multiple requests (Put Block). 
                // It does not determine the block size. It basically says "if the file is smaller than this size, upload it as one block. 
                // If the file size is larger than this value, break it into blocks and upload it."
                // In this example we set this value to equal 10 MB
                SingleBlobUploadThresholdInBytes = 10 * 1024 * 1024,

                // This specifies how many parallel PutBlock requests should be sent to Azure.
                // If this value is set to be > 1, the value set in SingleBlobUploadThresholdInBytes property is ignored.
                // Since you have stated that you want to do upload in parallel threads, it is obvious, that it can only be done
                // using Put Block operations, no matter what file size is.
                ParallelOperationThreadCount = 2
            };

            // This sets the size of the block to upload with Put Block request. 
            // This value is used when your file is larger than SingleBlobUploadThresholdInBytes property,
            // or you have selected parallel upload.
            // This value is set to 5 MB.
            blob.StreamWriteSizeInBytes = 5 * 1024 * 1024;

            // So, if the file is larger than 10 MB, it will be uploaded in blocks of size 5 MB in 1 thread.
            // If the file is smaller than 10 MB, only one request will be made.
            blob.UploadFromByteArray(file, 0, file.Length, null, requestOptions);
        }

        public void UploadFileInBlocks(byte[] file, string fileName)
        {
            CloudBlobContainer cloudBlobContainer = GetContainerReference();
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(fileName));

            // This is important to understand, that if the same file was already uploaded in ANY other way (e.g. like a single blob or by blocks but with other IDs
            // the upload will most definitely fail. This will happen because the block IDs that you associate with a blob must all be the same length.
            // So, if you have previously uploaded file as a one blob, Azure will assign is its own block IDs which will not be the same as yours and the upload will fail.
            blob.DeleteIfExists();

            List<string> blockIDs = new List<string>();

            int blockSize = 5 * 1024 * 1024;
            long fileSize = file.Length;

            int blockId = 0;

            while (fileSize > 0)
            {
                // For one chunk we will read either block size (1 MB) or remaining amount of blocks
                int blockLength = (int)Math.Min(blockSize, fileSize);

                // Block ID is required to be Base64 format
                string blockIdEncoded = GetBase64BlockId(blockId);
                blockIDs.Add(blockIdEncoded);

                byte[] bytesToUpload = new byte[blockLength];
                Array.Copy(file, blockId * blockSize, bytesToUpload, 0, blockLength);

                // Upload 1 file part of size 1 MB with specified block ID
                // File part (block) will be uploaded, but it will not become a part of the file (blob) just yet
                // Uploaded block that is not part of a blob yet is called 'Uncommitted block'
                // To make block a part of a blob, you need to commit list of block IDs.
                // You have one week to commit blocks to a blob before they are discarded. 
                // All uncommitted blocks are also discarded when a block list commitment operation occurs but does not include them.
                using (MemoryStream memoryStream = new MemoryStream(bytesToUpload, 0, blockLength))
                {
                    blob.PutBlock(blockIdEncoded, memoryStream, null, null, new BlobRequestOptions
                    {
                        RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 1)
                    });
                }
                
                blockId++;
                fileSize -= blockLength;
            }

            // Commit all uploaded blocks.
            // When this operation completes, you will be able to see the blob file in storage
            blob.PutBlockList(blockIDs);
        }

        private void UploadFileInBlocksParallel(byte[] file, string fileName)
        {
            CloudBlobContainer cloudBlobContainer = GetContainerReference();
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(fileName));

            int numberOfTreads = 10;

            long fileSize = file.Length;
            int blockSize = 1024 * 1024;

            // First, you need to prepare the queue for all blocks that need to be uploaded
            // The queue item is a KVP with key 'blockID' and value 'blockLength'
            ConcurrentQueue<KeyValuePair<int, int>> queue = new ConcurrentQueue<KeyValuePair<int, int>>();

            // Prepare a block list to commit later
            List<string> blockList = new List<string>();

            int blockId = 0;

            // Iterate all file to divide it in chucks 
            while (fileSize > 0)
            {
                var blockLength = (int)Math.Min(blockSize, fileSize);

                string blockIdEncoded = GetBase64BlockId(blockId);
                KeyValuePair<int, int> blockIdAndLengthPair = new KeyValuePair<int, int>(blockId, blockLength);
                queue.Enqueue(blockIdAndLengthPair);

                blockList.Add(blockIdEncoded);

                blockId++;
                fileSize -= blockLength;
            }

            // This is important to understand, that if the same file was already uploaded in ANY other way (e.g. like a single blob or by blocks but with other IDs
            // the upload will most definitely fail. This will happen because the block IDs that you associate with a blob must all be the same length.
            // So, if you have previously uploaded file as a one blob, Azure will assign is its own block IDs which will not be the same as yours and the upload will fail.
            blob.DeleteIfExists();

            BlobRequestOptions options = new BlobRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 1),
            };

            // Launch threads to upload blocks.
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < numberOfTreads; i++)
            {
                Thread thread = new Thread(() =>
                {
                    while (!queue.IsEmpty)
                    {
                        KeyValuePair<int, int> blockIdAndLengthPair;
                        if (queue.TryDequeue(out blockIdAndLengthPair))
                        {
                            int blockIdFromQueue = blockIdAndLengthPair.Key;
                            int blockLengthFromQueue = blockIdAndLengthPair.Value;

                            byte[] bytesToUpload = new byte[blockLengthFromQueue];
                            Array.Copy(file, blockIdFromQueue * blockSize, bytesToUpload, 0, blockLengthFromQueue);

                            using (MemoryStream memoryStream = new MemoryStream(bytesToUpload, 0, blockLengthFromQueue))
                            {
                                string blockIdEncoded = GetBase64BlockId(blockIdFromQueue);
                                blob.PutBlock(blockIdEncoded, memoryStream, null, null, new BlobRequestOptions
                                {
                                    RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 1)
                                });
                            }
                        }
                    }
                });

                thread.Start();
                threads.Add(thread);
            }

            // Wait for all threads to complete uploading data
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            // Commit the list of blocks
            blob.PutBlockList(blockList, null, options);
        }

        public byte[] DownloadFileInBlocks(string fileName)
        {
            CloudBlobContainer cloudBlobContainer = GetContainerReference();
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(fileName));

            int blockSize = 1024 * 1024; // 1 MB block size

            blob.FetchAttributes();
            long fileSize = blob.Properties.Length;

            byte[] blobContents = new byte[fileSize];
            int position = 0;

            while (fileSize > 0)
            {
                int blockLength = (int)Math.Min(blockSize, fileSize);

                blob.DownloadRangeToByteArray(blobContents, position, position, blockLength);

                position += blockLength;
                fileSize -= blockSize;
            }

            return blobContents;
        }

        public byte[] DownloadFileImBlocksParallel(string fileName)
        {
            CloudBlobContainer cloudBlobContainer = GetContainerReference();
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(Path.GetFileName(fileName));

            int blockSize = 1024 * 1024; // 1 MB block size

            blob.FetchAttributes();
            long fileSize = blob.Properties.Length;

            byte[] blobContents = new byte[fileSize];
            
            int position = 0;
            ConcurrentQueue<KeyValuePair<int, int>> queue = new ConcurrentQueue<KeyValuePair<int, int>>();

            while (fileSize > 0)
            {
                var blockLength = (int)Math.Min(blockSize, fileSize);

                KeyValuePair<int, int> blockIdAndLengthPair = new KeyValuePair<int, int>(position, blockLength);
                queue.Enqueue(blockIdAndLengthPair);

                position += blockLength;
                fileSize -= blockLength;
            }

            object lockObject = new object();
            int numberOfTreads = 10;
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < numberOfTreads; i++)
            {
                Thread thread = new Thread(() =>
                {
                    while (!queue.IsEmpty)
                    {
                        KeyValuePair<int, int> positionAndLengthPair;
                        if (queue.TryDequeue(out positionAndLengthPair))
                        {
                            int positionFromQueue = positionAndLengthPair.Key;
                            int blockLengthFromQueue = positionAndLengthPair.Value;

                            byte[] localBlobContent = new byte[blockLengthFromQueue];

                            blob.DownloadRangeToByteArray(localBlobContent, 0, positionFromQueue, blockLengthFromQueue);

                            lock (lockObject)
                            {
                                Array.Copy(localBlobContent, 0, blobContents, positionFromQueue, blockLengthFromQueue);
                            }
                        }
                    }
                });

                thread.Start();
                threads.Add(thread);
            }

            // Wait for all threads to complete uploading data
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            return blobContents;
        }

        private string GetBase64BlockId(int blockId)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}", blockId.ToString("0000000"))));
        }

        #endregion

        #region Service SAS

        public string GetBlobSas(string path)
        {
            CloudBlobContainer container = GetContainerReference();
            CloudBlockBlob blob = container.GetBlockBlobReference(path);

            var policy = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(5),
                Permissions = SharedAccessBlobPermissions.Read
            };

            string sasContainerToken = blob.GetSharedAccessSignature(policy);
            return blob.Uri + sasContainerToken;
        }

        public string GetContainerSas()
        {
            CloudBlobContainer container = GetContainerReference();

            var policy = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15),
                Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read
            };

            string sasContainerToken = container.GetSharedAccessSignature(policy);

            // to use with not .NET libs, but with google for ex. should add 'restype=container&comp=list' to URI
            return container.Uri + sasContainerToken;
        }

        public List<IListBlobItem> GetBlobListWithSas()
        {
            string sas = GetContainerSas();
            var cloudBlobContainer = new CloudBlobContainer(new Uri(sas));

            List<IListBlobItem> list = cloudBlobContainer.ListBlobs(null, true).ToList();
            return list;
        }

        #endregion

        #region Account SAS

        public string CreateAccountSasUrl()
        {
            string connectionString = CloudConfigurationManager.GetSetting(ConnectionStringSettingName);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            var policy = new SharedAccessAccountPolicy
            {
                Services = SharedAccessAccountServices.Blob | SharedAccessAccountServices.Table,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                ResourceTypes = SharedAccessAccountResourceTypes.Container,
                Protocols = SharedAccessProtocol.HttpsOrHttp,
                Permissions = SharedAccessAccountPermissions.Create,
                IPAddressOrRange = new IPAddressOrRange("195.177.74.50", "195.177.74.56")
            };

            string sas = storageAccount.GetSharedAccessSignature(policy);

            // create container
            var containerUri = string.Format("{0}{1}", storageAccount.BlobEndpoint, "azure-container-test");
            var blobContainer = new CloudBlobContainer(new Uri(containerUri), new StorageCredentials(sas));

            //blobContainer.FetchAttributes();
            //blobContainer.Create();
            blobContainer.CreateIfNotExists();

            // create table
            var tableUri = string.Format("{0}{1}", storageAccount.TableEndpoint, "AzureTableTest");
            var table = new CloudTable(new Uri(tableUri), new StorageCredentials(sas));
            table.CreateIfNotExists();

            // create storage queue
            var queueUri = string.Format("{0}{1}", storageAccount.QueueEndpoint, "azure-queue-test");
            var queue = new CloudQueue(new Uri(queueUri), new StorageCredentials(sas));
            queue.CreateIfNotExists();

            return sas;
        }

        public string CreateStoredAccessPolicy()
        {
            // creating policy
            CloudBlobContainer container = GetContainerReference();
            var storedPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read
            };

            var containerPermissions = new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off };
            containerPermissions.SharedAccessPolicies.Add("TPfs05", storedPolicy);

            container.SetPermissions(containerPermissions);

            // using policy
            var policy = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            string sasContainerToken = container.GetSharedAccessSignature(policy, "TPfs05");
            return container.Uri + sasContainerToken;
        }

        public void ClearStoredAccessPolicies()
        {
            CloudBlobContainer container = GetContainerReference();

            var permissions = container.GetPermissions();
            permissions.SharedAccessPolicies.Clear();
            container.SetPermissions(permissions);
        }

        public string GetInvalidContainerSasUriBasedOnPolicy()
        {
            CloudBlobContainer container = GetContainerReference();
            var policy = new SharedAccessBlobPolicy { SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7) };

            string invalidSasContainerToken = container.GetSharedAccessSignature(policy, "SomeInvalidPolicy");
            return container.Uri + invalidSasContainerToken;
        }


        #endregion

        #region Anonymous access
        
        public List<IListBlobItem> ListBlobsAnonymously()
        {
            var container = new CloudBlobContainer(new Uri(@"https://productdbstorage.blob.core.windows.net/products"));

            List<IListBlobItem> list = container.ListBlobs(null, true).ToList();

            return list;
        }

        #endregion

        public CloudBlobContainer GetContainerReference()
        {
            string connectionString = CloudConfigurationManager.GetSetting(ConnectionStringSettingName);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);

            container.CreateIfNotExists();

            return container;
        }
    }
}
