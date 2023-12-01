using Azure;
//using Azure.Data.Tables;
using Microsoft.WindowsAzure.Storage.Table;
namespace StorageCondition_API.Model
{
    public class PromptAudit : TableEntity
    {
        //public string    Guid { get; set; }

        
        public string PromptId { get; set; }
        public string OldPrompt { get; set; }
        public string UpdatePrompt { get; set; }

        public string Desc { get; set; }

        public string Type { get; set; }

        public string IsActive { get; set; }

        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        //public string PartitionKey { get; set; }
        //public string RowKey { get; set; }
        //public DateTimeOffset? Timestamp { get; set; }
        
        //public ETag ETag { get; set; }
    }
}
