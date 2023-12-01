using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Table;
using StorageCondition_API.Model;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace StorageCondition_API.services
{
    public class PromptAuditService : IPromptAuditService
    {
        private const string tableName = "tblPromptAudit";
        private readonly IConfiguration _configuration;

        public PromptAuditService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> SavePromptAudit(PromptAudit promptAudit)
        {
            //var tableClient = await getTableClient();
            //await tableClient.UpdateEntityAsync(promptAudit);
            var tableClient = await getTableClientUpdate();
            var operation= TableOperation.Insert(promptAudit);
            await tableClient.ExecuteAsync(operation);
            return "updated successfully";
        }

        public async Task<CloudTable> getTableClientUpdate()
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);
            var cloudTableClient = storageAccount.CreateCloudTableClient();
            var mytable = cloudTableClient.GetTableReference(tableName);
            await mytable.CreateIfNotExistsAsync();
            return mytable;
        }
        public async Task<TableClient> getTableClient()
        {
            
            var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);
            var tableClient = serviceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task<string> updatePromptFile(List<JsonPrompt> promptDetails)
        {

            var storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);
            var client = storageAccount.CreateCloudBlobClient();

            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["StorageConnectionString"]);

            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("storageobj");

            container.CreateIfNotExistsAsync().Wait();

            //container.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            CloudBlockBlob cloudBlockBlob = new CloudBlockBlob(container.Uri);

            var jsonToUplaod = Newtonsoft.Json.JsonConvert.SerializeObject(promptDetails);

            //cloudBlockBlob.UploadTextAsync(jsonToUplaod).Wait();
            BlobClient blob = container.GetBlobClient("Prompts/prompt.json");
            //string a = 
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonToUplaod)))
            {
                blob.DeleteIfExists(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);
                blob.Upload(ms);
                return blob.Uri.ToString();
            }
        }

    }


}
