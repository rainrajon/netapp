using Azure.Data.Tables;
using Azure;
using Azure.Storage.Blobs;

namespace StorageCondition_API.Model
{
    public class TrackRun : ITableEntity
    {
        public string NDC { get; set; }

        public string NDC11 { get; set; }
        public string NDC10 { get; set; }

        public string Drug_Name { get; set; }
        public string StorageCondition { get; set; }
        public string BatchId { get; set; }

        public string DocID { get; set; }
        public string SetID { get; set; }
        public string S3Key { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public DateTime Created_Date { get; set; } //tagging reason

        public string ImageLoc { get; set; }
        public string Response { get; set; }
        public string Prompt { get; set; }
        public string Execution_Time { get; set; }

        public string Error_Message { get; set; }

        public byte[] ImageByte { get; set; }

        //public string ImageLoc { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public int Count { get; set; }

        public string PaginationString { get; set; }
    }
}
