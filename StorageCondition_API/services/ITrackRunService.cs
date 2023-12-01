using Azure;
using Microsoft.AspNetCore.Mvc;
using StorageCondition_API.Model;
using System.Runtime.CompilerServices;

namespace StorageCondition_API.services
{
    public interface ITrackRunService
    {
        Task<List<TrackRun>> getTrackRunFromNDCIds(string ndcId, int pageIndex, int numberOfRecPerPage, Nullable<DateTime> startDate, Nullable<DateTime> endDate);

        Task<List<NDCIdListFromExcel>> GetCSVBlobData();

        Task<string> DownloadAsync();

        Task<List<JsonPrompt>> GetJSONPrompt();

        Task<JsonPrompt> getJsonPromptFromPromptName(string promptName);

        Task<TrackRun> getStorageDetails(string rowKey);

        Task<byte[]> getImageByte(string imageLocation);

         Task<TrackRun> getPaginationDate();
        //IAsyncEnumerable<Page<TrackRun>> GetVideosPaginatedAsync(int pageNumber, int perPageCount);
    }
}
