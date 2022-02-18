using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.DownloadService;

namespace UpdateClientService.API.Services.FileSets;

public interface IFileSetDownloader
{
	bool IsDownloading(List<DownloadData> downloads);

	bool IsDownloaded(ClientFileSetRevision revision);

	Task<List<Error>> DownloadFileSet(ClientFileSetRevision revision, List<DownloadData> downloads, DownloadPriority priority);

	List<Error> CompleteDownloads(ClientFileSetRevision revision, List<DownloadData> downloads);
}
