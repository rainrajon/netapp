using StorageCondition_API.Model;

namespace StorageCondition_API.services
{
    public interface IPromptAuditService
    {
        Task<string> SavePromptAudit(PromptAudit promptAudit);

        Task<string> updatePromptFile(List<JsonPrompt> promptDetails);
    }
}
