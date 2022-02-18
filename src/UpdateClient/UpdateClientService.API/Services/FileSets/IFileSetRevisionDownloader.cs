using System.Collections.Generic;
using System.Threading.Tasks;
using UpdateClientService.API.Services.DownloadService;

namespace UpdateClientService.API.Services.FileSets;

public interface IFileSetRevisionDownloader
{
	bool Exists(long fileSetId, long revisionId, long patchRevisionId);

	bool DownloadExists(DownloadData downloadData);

	bool IsDownloadError(DownloadData downloadData);

	bool IsDownloadComplete(long fileSetId, long revisionId, long patchRevisionId, DownloadData downloadData);

	Task<DownloadData> AddDownload(long fileSetId, long revisionId, long patchRevisionId, string hash, string path, DownloadPriority downloadPriority);

	ClientFileSetRevision GetRevision(long fileSetId, long revisionId, long patchRevisionId);

	bool CompleteDownload(long fileSetId, long revisionId, long patchRevisionId, DownloadData downloader);

	DownloadData GetCurrentDownloadData(long fileSetId, long revisionId, long patchRevisionId, List<DownloadData> downloads);
}
