using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Coravel.Invocable;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DownloadService;

public interface IDownloadService : IInvocable
{
	Task<List<DownloadData>> GetDownloads(string pattern = null);

	Task<List<DownloadData>> GetDownloads(Regex pattern);

	Task<List<Error>> AddDownload(string key, string hash, string url, DownloadPriority priority, bool completeOnFinish = false);

	Task<(List<Error> errors, DownloadData downloadData)> AddRetrieveDownload(string key, string hash, string url, DownloadPriority priority, bool completeOnFinish);

	Task<List<Error>> CancelDownload(string key);

	Task<List<Error>> CompleteDownload(string key);

	Task<string> GetProxiedS3Url(string key, DownloadPathType type, bool isHead = false);
}
