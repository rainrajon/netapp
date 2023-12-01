using Azure;
using Azure.Data.Tables;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StorageCondition_API.Model;
using System.Runtime.CompilerServices;
using System.Xml;
using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Reflection.Metadata;
using Azure.Storage.Blobs.Models;
using ExcelDataReader;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure;
//using System.Text.Json;
using Azure.Storage.Blobs.Specialized;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using NLog;
using StorageCondition_API.Controllers;
using Microsoft.Extensions.Logging;
using System.IO;
//using Newtonsoft.Json;

namespace StorageCondition_API.services
{
    public class TrackRunService : ITrackRunService
    {
        private const string TableName = "trytest";
        private readonly IConfiguration _configuration;
        private TableClient tableClient;
        private readonly BlobServiceClient _blobServiceClient;
        int pageIndex = 1;
        private int numberOfRecPerPage = 2;
        //To check the paging direction according to use selection.
        private enum PagingMode
        { First = 1, Next = 2, Previous = 3, Last = 4, PageCountChange = 5 };
        //private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        ILogger<TrackRunService> _logger;
        public TrackRunService(IConfiguration configuration, BlobServiceClient blobServiceClient, ILogger<TrackRunService> logger)
        {
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task<List<TrackRun>> getTrackRunFromNDCIds(string ndcId,int pageIndex,int numberOfRecPerPage, Nullable<DateTime> startDate, Nullable<DateTime> endDate)
        {
            var tableClient = await getTableClient();
            List<TrackRun> linqEntities=new List<TrackRun>();
            try
            {


                var a = startDate.ToString() == "" ? null : startDate;
                //var ss = Convert.ToDateTime(startDate);
                var b = endDate.ToString() == "" ? null : endDate;

                var c = Convert.ToDateTime(startDate);

                //if(pageIndex)
                //if (pageIndex == 1)
                //    pageIndex = 0;

                if (ndcId != null && a != null && b != null)
                {
                    linqEntities.ToList().ForEach(x => x.Count = tableClient.Query<TrackRun>(customer => customer.NDC == ndcId &&
                    ((Convert.ToDateTime(customer.Created_Date) >= a
                    && Convert.ToDateTime(customer.Created_Date) <= b))).Count());

                    linqEntities = tableClient.Query<TrackRun>(customer => customer.NDC == ndcId &&
                    ((Convert.ToDateTime(customer.Created_Date) >= a
                    && Convert.ToDateTime(customer.Created_Date) <= b))).Skip(pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage).ToList();
                }
                else if (a != null && b != null)
                {
                    linqEntities.ToList().ForEach(x => x.Count = tableClient.Query<TrackRun>(customer => Convert.ToDateTime(customer.Created_Date) >= a
                    && Convert.ToDateTime(customer.Created_Date) <= b).Count());

                    linqEntities = tableClient.Query<TrackRun>(customer => Convert.ToDateTime(customer.Created_Date) >= a
                    && Convert.ToDateTime(customer.Created_Date) <= b).Skip(pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage).ToList();
                }
                else if (ndcId != null)
                {
                    linqEntities = tableClient.Query<TrackRun>(customer => customer.NDC.Equals(ndcId.ToString())).Skip(pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage).ToList();
                    linqEntities.ToList().ForEach(x => x.Count = tableClient.Query<TrackRun>(customer => customer.NDC.Equals(ndcId)).Count());

                }
                else
                {
                    

                    linqEntities = tableClient.Query<TrackRun>(maxPerPage: 1).Skip(pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage).ToList();
                    linqEntities.ToList().ForEach(x => x.Count = tableClient.Query<TrackRun>().Select(x=>x.NDC).Count());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);  
                _logger.LogError(ex.ToString());
                throw ex;
                //_logger.Error(ex);
            }
            return linqEntities.ToList();
        }
        private async Task<TableClient> getTableClient()
        {
            var serviceClient = new TableServiceClient(_configuration["StorageConnectionString"]);

            var tableClient = serviceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

       
        public async Task<List<NDCIdListFromExcel>> GetCSVBlobData()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("storageobj");

            // Retrieve reference to a blob named "test.csv"
            CloudBlockBlob blockBlobReference = container.GetBlockBlobReference("StorageConditions-Output/output.csv");

            BlobContainerClient containerClient = new BlobContainerClient(_configuration["StorageConnectionString"], "storageobj");
            var bl = containerClient.GetBlobClient("StorageConditions-Output/output.csv");
            BlobDownloadResult download = null;
            download = await bl.DownloadContentAsync();

            DataSet dataSet = new();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            IExcelDataReader excelReader = null;

            excelReader = ExcelReaderFactory.CreateCsvReader(download.Content.ToStream(), null);


            ExcelDataSetConfiguration dsconfig = new()
            {
                ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                {
                    //UseHeaderRow = true, // use a header row
                    //ReadHeaderRow = rowReader =>
                    //{
                    //    rowReader.Read(); // skip the first row before reading the header
                    //}
                }
            };

            dataSet = excelReader.AsDataSet(dsconfig);
            var myData = dataSet.Tables[0].AsEnumerable();

            List<NDCIdListFromExcel> target = myData.AsEnumerable()
   .Select(row => new NDCIdListFromExcel
   {
       NDC = row.Field<string?>(0),
       StorageCondition = row.Field<string>(1),
       Reason = row.Field<string>(2),
       Status = row.Field<string>(3),
       created_date = row.Field<string>(4)
   }).Skip(1).ToList();


            excelReader.Close();

            return target;
        }
        public async Task<string> DownloadAsync()
        {
            // Get a reference to a container named in appsettings.json
            BlobContainerClient client = new BlobContainerClient(_configuration["StorageConnectionString"], "testdata");

            try
            {
                // Get a reference to the blob uploaded earlier from the API in the container from configuration settings
                BlobClient file = client.GetBlobClient("output.csv");

                // Check if the file exists in the container
                if (await file.ExistsAsync())
                {
                    var data = await file.OpenReadAsync();
                    Stream blobContent = data;

                    // Download the file details async
                    var content = await file.DownloadContentAsync();

                    // Add data to variables in order to return a BlobDto
                    string name = "output.csv";
                    string contentType = content.Value.Details.ContentType;

                    // Create new BlobDto with blob data from variables
                    //return new BlobDto { Content = blobContent, Name = name, ContentType = contentType };
                    return name;
                }
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // Log error to console
                //_logger.LogError($"File {blobFilename} was not found.");
            }

            // File does not exist, return null and handle that in requesting method
            return null;
        }

        public async Task<List<JsonPrompt>> GetJSONPrompt()
        {
            List<JsonResult> result = new List<JsonResult>();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("storageobj");

            // Retrieve reference to a blob named "test.csv"
            CloudBlockBlob blockBlobReference = container.GetBlockBlobReference("Prompts/prompt.json");

            BlobContainerClient containerClient = new BlobContainerClient(_configuration["StorageConnectionString"], "storageobj");
            var bl = containerClient.GetBlobClient("Prompts/prompt.json");
            //BlobDownloadResult download = null;
            //download = await bl.DownloadContentAsync();

            //DataSet dataSet = new();

            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var blobData = await bl.DownloadContentAsync();
            // Convert the buffer to a string (assuming JSON data)
            //const jsonData = blobData.toString();

            //var a = blobData.Value;
            var a = blobData.Value.Content.ToString();
            BlobOpenReadOptions readOptions = new(allowModifications: false);
            //using Stream stream = await blobClient.OpenReadAsync(readOptions, ct).ConfigureAwait(false);
            //string stream=await bl.OpenReadAsync().ToString();

            // read json
            JsonSerializerOptions jsonOpions = new();

            //JsonPrompt? myDeserializedObject = await JsonSerializer
            //    .DeserializeAsync<JsonPrompt>(stream, jsonOpions)
            //    .ConfigureAwait(false);
            List<JsonPrompt> promptList = JsonSerializer.Deserialize<List<JsonPrompt>>(a);

            return promptList;
            //return    
        }

        public async Task<JsonPrompt> getJsonPromptFromPromptName(string promptName)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("storageobj");

            // Retrieve reference to a blob named "test.csv"
            CloudBlockBlob blockBlobReference = container.GetBlockBlobReference("Prompts/prompt.json");

            BlobContainerClient containerClient = new BlobContainerClient(_configuration["StorageConnectionString"], "storageobj");
            var bl = containerClient.GetBlobClient("Prompts/prompt.json");
            var blobData = await bl.DownloadContentAsync();

            var a = blobData.Value.Content.ToString();
            BlobOpenReadOptions readOptions = new(allowModifications: false);

            JsonSerializerOptions jsonOpions = new();

            //JsonPrompt? myDeserializedObject = await JsonSerializer
            //    .DeserializeAsync<JsonPrompt>(stream, jsonOpions)
            //    .ConfigureAwait(false);
            List<JsonPrompt> promptList = JsonSerializer.Deserialize<List<JsonPrompt>>(a).ToList();


            JsonPrompt promptDetails = promptList.Where(x => x.prompt_id.Equals(promptName)).FirstOrDefault();



            return promptDetails;
        }

        public async Task<TrackRun> getStorageDetails(string rowKey)
        {
            var tableClient = await getTableClient();
            TrackRun ndcDetails = new TrackRun();
            if (rowKey != null)
                ndcDetails = tableClient.Query<TrackRun>(x => x.RowKey == rowKey).FirstOrDefault();

            var imagebyte =await getImageByte(ndcDetails.ImageLoc);

            ndcDetails.ImageByte = imagebyte;
            return ndcDetails;
            //var a = startDate.ToString() == "" ? null : "";
            ////var ss = Convert.ToDateTime(startDate);
            //var b = endDate.ToString() == "" ? null : "";
            //List<TrackRun> linqEntities;
            //if (ndcId != "null" && a != null && b != null)
            //    linqEntities = tableClient.Query<TrackRun>(customer => customer.NDC == ndcId && customer.Created_Date == a
            //    && customer.Created_Date == b).ToList();
            //else if (a != null && b != null)
            //    linqEntities = tableClient.Query<TrackRun>(customer => customer.Created_Date == a
            //    && customer.Created_Date == b).ToList();
            //else if (ndcId != null)
            //    linqEntities = tableClient.Query<TrackRun>(customer => customer.NDC == ndcId).ToList();
            //else
            //    linqEntities = tableClient.Query<TrackRun>(maxPerPage: 1).Take(10).ToList();

        }

       

        public async Task<TrackRun> getPaginationDate()
        {
            TrackRun t = new TrackRun();

            TableQuery<TrackRunCopy> d = new TableQuery<TrackRunCopy>().
                Where(
                    TableQuery.GenerateFilterCondition("", QueryComparisons.Equal, "")
                );



            //TableContinuationToken token = null;
            //d.TakeCount = 10;
            //do
            //{
            //    TableQuerySegment<TrackRunCopy> segment=
            //        await 
            //}


            return t;
        }


        public async Task<byte[]> getImageByte(string imageLocation)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("storageobj");

            // Retrieve reference to a blob named "test.csv"
            CloudBlockBlob blockBlobReference = container.GetBlockBlobReference(imageLocation);

            BlobContainerClient containerClient = new BlobContainerClient(_configuration["StorageConnectionString"], "storageobj");
            var bl = containerClient.GetBlobClient(imageLocation);
            BlobDownloadResult download = null;

            //var storageaccount = new cloudstorageaccount(new microsoft.windowsazure.storage.auth.storagecredentials(storageaccountname, storageaccountkey), true);
            //var blobclient = storageaccount.createcloudblobclient();
            //var container = blobclient.getcontainerreference("");
            // var blobs = container.listblobs();
            //var blockblobreference = container.getblockblobreference(sourceblobfilename);
            var memorystream = new MemoryStream();

            bl.DownloadTo(memorystream);
            byte[] content = memorystream.ToArray();
            return content;
        }





    }
}
