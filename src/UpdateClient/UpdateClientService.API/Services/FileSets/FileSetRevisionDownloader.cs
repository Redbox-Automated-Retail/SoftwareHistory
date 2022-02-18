using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetRevisionDownloader : IFileSetRevisionDownloader
{
	private IFileCacheService _fileCacheService;

	private IDownloadService _downloadService;

	private readonly IZipDownloadHelper _zipHelper;

	private readonly AppSettings _settings;

	private readonly IDownloader _downloader;

	public FileSetRevisionDownloader(IFileCacheService fileCacheService, IDownloadService downloadService, IZipDownloadHelper zipHelper, IDownloader downloaderInterface, IOptionsMonitor<AppSettings> settings)
	{
		_fileCacheService = fileCacheService;
		_downloadService = downloadService;
		_zipHelper = zipHelper;
		_downloader = downloaderInterface;
		_settings = settings.CurrentValue;
	}

	public bool Exists(long fileSetId, long revisionId, long patchRevisionId)
	{
		return _fileCacheService.RevisionExists(fileSetId, revisionId, patchRevisionId);
	}

	public bool DownloadExists(DownloadData downloadData)
	{
		return downloadData != null;
	}

	public bool IsDownloadError(DownloadData downloadData)
	{
		if (downloadData == null)
		{
			return false;
		}
		return downloadData.DownloadState == DownloadState.Error;
	}

	public bool IsDownloadComplete(long fileSetId, long revisionId, long patchRevisionId, DownloadData downloadData)
	{
		if (Exists(fileSetId, revisionId, patchRevisionId))
		{
			return true;
		}
		if (downloadData == null)
		{
			return false;
		}
		return downloadData.DownloadState == DownloadState.PostDownload;
	}

	public async Task<DownloadData> AddDownload(long fileSetId, long revisionId, long patchRevisionId, string hash, string path, DownloadPriority downloadPriority)
	{
		var (source, result) = await AddDownload(GetRevisionKey(fileSetId, revisionId, patchRevisionId), hash, GetRevisionUrl(path), downloadPriority);
		if (source.Any())
		{
			return null;
		}
		return result;
	}

	private async Task<(List<Error> errors, DownloadData downloadData)> AddDownload(string key, string hash, string url, DownloadPriority downloadPriority)
	{
		return await _downloadService.AddRetrieveDownload(key, hash, url, downloadPriority, completeOnFinish: false);
	}

	public bool CompleteDownload(long fileSetId, long revisionId, long patchRevisionId, DownloadData downloadData)
	{
		string path = downloadData.Path;
		List<Error> source = _zipHelper.Extract(path, fileSetId, revisionId, patchRevisionId);
		if (!source.Any())
		{
			_downloader.Complete(downloadData);
		}
		return !source.Any();
	}

	public ClientFileSetRevision GetRevision(long fileSetId, long revisionId, long patchRevisionId)
	{
		if (!_fileCacheService.GetRevision(fileSetId, revisionId, patchRevisionId, out var data))
		{
			return null;
		}
		return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
	}

	public DownloadData GetCurrentDownloadData(long fileSetId, long revisionId, long patchRevisionId, List<DownloadData> downloads)
	{
		string key = GetRevisionKey(fileSetId, revisionId, patchRevisionId);
		return downloads.Where((DownloadData d) => d.Key == key).FirstOrDefault();
	}

	private string GetRevisionKey(long fileSetId, long revisionId, long patchRevisionId)
	{
		return $"{1},{fileSetId},{revisionId}-{patchRevisionId}";
	}

	private string GetRevisionUrl(string path)
	{
		return GetProxiedS3Url(path);
	}

	private string GetProxiedS3Url(string path)
	{
		if (!string.IsNullOrWhiteSpace(path))
		{
			string text = Uri.EscapeDataString(path);
			return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", text, DownloadPathType.None)).ToString();
		}
		return string.Empty;
	}
}
