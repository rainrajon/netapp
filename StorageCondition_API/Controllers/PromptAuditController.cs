using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StorageCondition_API.Model;
using StorageCondition_API.services;

namespace StorageCondition_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromptAuditController : ControllerBase
    {
        public TableServiceClient _tableServiceClient;
        public TableClient _tableClient;
        private readonly IPromptAuditService _promptAuditService;
        private readonly IConfiguration _configuration;
        //private readonly IPromptAuditService _promptAuditService;
        public PromptAuditController(IPromptAuditService promptAuditService, IConfiguration configuration)
        {
            _promptAuditService = promptAuditService ?? throw new ArgumentException(nameof(promptAuditService));
            _configuration = configuration;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> InsertAuditPrompt(PromptAudit promptAudit)
        {
            promptAudit.PartitionKey = promptAudit.PromptId;
            promptAudit.RowKey = Guid.NewGuid().ToString();
            //promptAudit.Guid = Guid.NewGuid().ToString();

            var response = await _promptAuditService.SavePromptAudit(promptAudit);

            return Ok(response);
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> updatePromptDetails(List<JsonPrompt> promptFile)
        {
            //var a=_trackRunService.updatePromptFile(promptFile);
            //if (a != null || a.ToString() != "null")
            //{
            //    _promptAuditService.SavePromptAudit()
            //}
            return Ok(_promptAuditService.updatePromptFile(promptFile));
        }

    }
}
