using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DownloadService;

public interface IDownloader : IPersistentData
{
	DownloadData DownloadData { get; }

	Task<List<Error>> ProcessDownload(DownloadData data);

	Task<List<Error>> SaveDownload(DownloadData data);

	Task<List<Error>> DeleteDownload(DownloadData data);

	Task<List<Error>> Complete(DownloadData data);

	List<Error> Cleanup(List<DownloadData> data);
}
