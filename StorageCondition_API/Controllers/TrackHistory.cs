using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StorageCondition_API.Model;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Azure.Data.Tables;
using StorageCondition_API.services;
using Microsoft.WindowsAzure.Storage.Blob;

namespace StorageCondition_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackHistory : ControllerBase
    {
        public TableServiceClient _tableServiceClient;
        public TableClient _tableClient;
        private readonly ITrackRunService _trackRunService;
        
        private readonly IConfiguration _configuration;
        public TrackHistory(ITrackRunService trackRunService, IConfiguration configuration) //, IPromptAuditService promptAuditService
        {
            _trackRunService = trackRunService ?? throw new ArgumentException(nameof(trackRunService));
            _configuration = configuration;
            //this._promptAuditService = promptAuditService;
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetTrackRunsFromNDCID(int pageIndex, int numberOfRecPerPage, string? ndcId=null, Nullable<DateTime> startDate=null, Nullable<DateTime> endDate = null)
        {
            //if (ndcId is null)
            //{
            //    throw new ArgumentNullException(nameof(ndcId));
            //}

            return Ok(await _trackRunService.getTrackRunFromNDCIds(ndcId, pageIndex, numberOfRecPerPage,startDate, endDate));
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> GetDataFromExcelFile()
        {
            return Ok(await _trackRunService.GetCSVBlobData());
            //return Ok(await _trackRunService.GetCSVBlobData());
        }

        [HttpPost(nameof(StorageConditionsFile))]
        public async Task<IActionResult> StorageConditionsFile()
        {
            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["StorageConnectionString"]);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference("testdata");

                // Retrieve reference to a blob named "test.csv"
                //CloudBlockBlob blockBlobReference = container.GetBlockBlobReference("output.csv");

                //string blobstorageconnection = _configuration.GetValue<string>("BlobConnectionString");
                //CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(blobstorageconnection);
                //CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                //CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(_configuration.GetValue<string>("BlobContainerName"));
                blockBlob = container.GetBlockBlobReference("output.csv");
                await blockBlob.DownloadToStreamAsync(memoryStream);
            }
            Stream blobStream = blockBlob.OpenReadAsync().Result;
            return File(blobStream, blockBlob.Properties.ContentType, blockBlob.Name);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> getPromptDetails()
        {
            return Ok(await _trackRunService.GetJSONPrompt());
        }

        [HttpGet]
        [Route("[action]/{promptName}")]
        public async Task<IActionResult> getPromptDetailsFromName (string promptName)
        {
            return Ok(await _trackRunService.getJsonPromptFromPromptName(promptName));
        }

        [HttpGet]
        [Route("[action]/{rowKey}")]
        public async Task<IActionResult> getNdcDetailsFromRowKey(string rowKey)
        {
            return Ok(await _trackRunService.getStorageDetails(rowKey));
        }

      


        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> getpagination()
        {
            return Ok(_trackRunService.getPaginationDate());
        }
        #region prompt Audit


        #endregion
    }
}
