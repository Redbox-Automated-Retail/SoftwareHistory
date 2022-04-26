using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetDownloader : IFileSetDownloader
{
	private readonly IFileCacheService _fileCacheService;

	private readonly IDownloadService _downloadService;

	private readonly IZipDownloadHelper _zipHelper;

	private readonly IDownloader _downloader;

	private readonly AppSettings _settings;

	private readonly ILogger<FileSetDownloader> _logger;

	public FileSetDownloader(IFileCacheService fileCacheService, IDownloadService downloadService, IZipDownloadHelper zipHelper, IDownloader downloaderInterface, IOptionsMonitor<AppSettings> settings, ILogger<FileSetDownloader> logger)
	{
		_fileCacheService = fileCacheService;
		_downloadService = downloadService;
		_zipHelper = zipHelper;
		_downloader = downloaderInterface;
		_settings = settings.CurrentValue;
		_logger = logger;
	}

	public bool IsDownloading(List<DownloadData> downloads)
	{
		bool result = false;
		downloads.ForEach(delegate(DownloadData _downloader)
		{
			if (_downloader.DownloadState == DownloadState.Downloading || _downloader.DownloadState == DownloadState.Error)
			{
				result = true;
			}
		});
		return result;
	}

	public bool IsDownloaded(ClientFileSetRevision revision)
	{
		List<FileSetFileInfo> fileSetInfoList = GetFileSetInfoList(revision);
		return GetDownloadMethod(revision, fileSetInfoList) == DownloadMethod.None;
	}

	public async Task<List<Error>> DownloadFileSet(ClientFileSetRevision revision, List<DownloadData> downloads, DownloadPriority priority)
	{
		List<Error> errors = new List<Error>();
		try
		{
			List<FileSetFileInfo> fileSetInfoList = GetFileSetInfoList(revision);
			DownloadMethod downloadMethod = GetDownloadMethod(revision, fileSetInfoList);
			_logger.LogInfoWithSource($"Download method is {downloadMethod} for FileSet {revision.FileSetId}, RevisionId {revision.RevisionId}, PatchRevisionId {revision.PatchRevisionId}", "DownloadFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			switch (downloadMethod)
			{
			case DownloadMethod.None:
				return errors;
			case DownloadMethod.FileSet:
			{
				List<Error> list = errors;
				list.AddRange(await DownloadByFileSet(revision, downloads, priority));
				break;
			}
			case DownloadMethod.PatchSet:
			{
				List<Error> list = errors;
				list.AddRange(await DownloadByPatchSet(revision, fileSetInfoList, downloads, priority));
				break;
			}
			case DownloadMethod.Files:
			{
				List<Error> list = errors;
				list.AddRange(await DownloadByFiles(fileSetInfoList, downloads, priority));
				break;
			}
			case DownloadMethod.Patches:
			{
				List<Error> list = errors;
				list.AddRange(await DownloadByPatches(fileSetInfoList, downloads, priority));
				break;
			}
			}
			return errors;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "DownloadFileSet");
			errors.Add(new Error
			{
				Message = $"FileSetDownloader An unhandled exception occurred in DownloadFileSet . Exception: {ex}"
			});
		}
		return errors;
	}

	public List<Error> CompleteDownloads(ClientFileSetRevision revision, List<DownloadData> downloads)
	{
		List<Error> errors = new List<Error>();
		downloads.ForEach(delegate(DownloadData _downloader)
		{
			if (_downloader.DownloadState == DownloadState.PostDownload)
			{
				string path = _downloader.Path;
				List<Error> list = _zipHelper.Extract(path, revision.FileSetId, revision.RevisionId, revision.PatchRevisionId);
				if (!list.Any())
				{
					this._downloader.Complete(_downloader);
				}
				errors.AddRange(list);
			}
		});
		return errors;
	}

	private List<FileSetFileInfo> GetFileSetInfoList(ClientFileSetRevision revision)
	{
		List<FileSetFileInfo> fileSetInfoList = new List<FileSetFileInfo>();
		revision.Files.ToList().ForEach(delegate(ClientFileSetFile f)
		{
			FileSetFileInfo fileSetFileInfo = new FileSetFileInfo
			{
				FileId = f.FileId,
				FileRevisionId = f.FileRevisionId,
				Key = GetFileKey(revision.FileSetId, f.FileId, f.FileRevisionId),
				FileSize = f.FileSize,
				PatchSize = 0L,
				FileHash = f.FileHash
			};
			fileSetFileInfo.Exists = _fileCacheService.FileExists(revision.FileSetId, f.FileId, f.FileRevisionId);
			fileSetFileInfo.Url = GetFileUrl(f, revision.FileSetId, f.FileRevisionId, 0L);
			if (!fileSetFileInfo.Exists)
			{
				ClientPatchFileSetFile clientPatchFileSetFile = revision.PatchFiles.FirstOrDefault((ClientPatchFileSetFile pf) => pf.FileId == f.FileId);
				if (clientPatchFileSetFile != null)
				{
					string fileKey = GetFileKey(revision.FileSetId, f.FileId, clientPatchFileSetFile.PatchFileRevisionId);
					if (_fileCacheService.FileExists(revision.FileSetId, f.FileId, clientPatchFileSetFile.PatchFileRevisionId))
					{
						fileSetFileInfo.PatchSize = clientPatchFileSetFile.FileSize;
						fileSetFileInfo.PatchUrl = GetFileUrl(f, revision.FileSetId, f.FileRevisionId, clientPatchFileSetFile.PatchFileRevisionId);
						fileSetFileInfo.PatchKey = fileKey;
						fileSetFileInfo.PatchHash = clientPatchFileSetFile.FileHash;
					}
				}
			}
			fileSetInfoList.Add(fileSetFileInfo);
		});
		return fileSetInfoList;
	}

	private DownloadMethod GetDownloadMethod(ClientFileSetRevision revision, List<FileSetFileInfo> fileSetInfoList)
	{
		long num = fileSetInfoList.Sum((FileSetFileInfo item) => item.Exists ? 0 : item.FileSize);
		long num2 = 0L;
		if (revision.PatchSetFileSize > 0)
		{
			num2 = fileSetInfoList.Sum(delegate(FileSetFileInfo item)
			{
				if (item.Exists)
				{
					return 0L;
				}
				return (item.PatchSize > 0) ? item.PatchSize : item.FileSize;
			});
		}
		long num3 = 0L;
		if (revision.PatchSetFileSize > 0)
		{
			num3 = fileSetInfoList.Sum(delegate(FileSetFileInfo item)
			{
				if (item.Exists)
				{
					return 0L;
				}
				return (item.PatchSize == 0L) ? item.FileSize : 0;
			});
			num3 += revision.PatchSetFileSize;
		}
		if (num == 0L)
		{
			return DownloadMethod.None;
		}
		if (num3 > 0 && (num2 == 0L || num3 <= num2) && num3 < num && num3 < revision.SetFileSize)
		{
			return DownloadMethod.PatchSet;
		}
		if (num2 > 0 && num2 < num && num2 < revision.SetFileSize)
		{
			return DownloadMethod.Patches;
		}
		if (num < revision.SetFileSize)
		{
			return DownloadMethod.Files;
		}
		return DownloadMethod.FileSet;
	}

	private async Task<List<Error>> DownloadByFileSet(ClientFileSetRevision revision, List<DownloadData> downloads, DownloadPriority priority)
	{
		List<Error> errors = new List<Error>();
		try
		{
			string key = GetRevisionSetKey(revision);
			string fileSetUrl = GetFileSetUrl(revision);
			if (!downloads.Any((DownloadData d) => d.Key == key))
			{
				var (collection, item) = await AddDownload(key, revision.SetFileHash, fileSetUrl, priority);
				errors.AddRange(collection);
				if (!errors.Any())
				{
					downloads.Add(item);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "DownloadByFileSet");
			errors.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in DownloadByFileSet. Exception: {ex}"
			});
		}
		return errors;
	}

	private async Task<List<Error>> DownloadByPatchSet(ClientFileSetRevision revision, List<FileSetFileInfo> fileSetInfoList, List<DownloadData> downloads, DownloadPriority priority)
	{
		List<Error> errors = new List<Error>();
		try
		{
			string key = GetRevisionPatchSetKey(revision);
			string patchSetFileUrl = GetPatchSetFileUrl(revision);
			if (!downloads.Any((DownloadData d) => d.Key == key))
			{
				var (collection, item) = await AddDownload(key, revision.PatchSetFileHash, patchSetFileUrl, priority);
				errors.AddRange(collection);
				if (errors.Any())
				{
					return errors;
				}
				downloads.Add(item);
			}
			fileSetInfoList.ForEach(async delegate(FileSetFileInfo fileSetFileInfo)
			{
				if (!fileSetFileInfo.Exists && fileSetFileInfo.PatchSize == 0L && !downloads.Any((DownloadData d) => d.Key == fileSetFileInfo.Key))
				{
					var (collection2, item2) = await AddDownload(fileSetFileInfo.Key, fileSetFileInfo.FileHash, fileSetFileInfo.Url, priority);
					errors.AddRange(collection2);
					if (!errors.Any())
					{
						downloads.Add(item2);
					}
				}
			});
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "DownloadByPatchSet");
			errors.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in DownloadByPatchSet. Exception: {ex}"
			});
		}
		return errors;
	}

	private async Task<List<Error>> DownloadByPatches(List<FileSetFileInfo> fileSetInfoList, List<DownloadData> downloads, DownloadPriority priority)
	{
		List<Error> errors = new List<Error>();
		try
		{
			foreach (FileSetFileInfo item in fileSetInfoList)
			{
				if (item.Exists)
				{
					continue;
				}
				if (item.PatchSize == 0L)
				{
					if (!downloads.Any((DownloadData d) => d.Key == item.Key))
					{
						var (collection, item2) = await AddDownload(item.Key, item.FileHash, item.Url, priority);
						errors.AddRange(collection);
						if (!errors.Any())
						{
							downloads.Add(item2);
						}
					}
				}
				else if (!downloads.Any((DownloadData d) => d.Key == item.PatchKey))
				{
					var (collection2, item3) = await AddDownload(item.PatchKey, item.PatchHash, item.PatchUrl, priority);
					errors.AddRange(collection2);
					if (!errors.Any())
					{
						downloads.Add(item3);
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "DownloadByPatches");
			errors.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in DownloadByPatches . Exception: {ex}"
			});
		}
		return errors;
	}

	private async Task<List<Error>> DownloadByFiles(List<FileSetFileInfo> fileSetInfoList, List<DownloadData> downloads, DownloadPriority downloadPriority)
	{
		List<Error> errors = new List<Error>();
		try
		{
			fileSetInfoList.ForEach(async delegate(FileSetFileInfo item)
			{
				if (!item.Exists && !downloads.Any((DownloadData d) => d.Key == item.Key))
				{
					var (collection, item2) = await AddDownload(item.Key, item.FileHash, item.Url, downloadPriority);
					errors.AddRange(collection);
					if (!errors.Any())
					{
						downloads.Add(item2);
					}
				}
			});
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "DownloadByFiles");
			errors.Add(new Error
			{
				Message = $"ChangeSetFile An unhandled exception occurred in DownloadByFiles. Exception: {ex}"
			});
		}
		return errors;
	}

	private async Task<(List<Error> errors, DownloadData downloadData)> AddDownload(string key, string hash, string url, DownloadPriority downloadPriority)
	{
		return await _downloadService.AddRetrieveDownload(key, hash, url, downloadPriority, completeOnFinish: false);
	}

	private string GetFileKey(long fileSetId, long fileId, long fileRevisionId)
	{
		return $"{2},{fileSetId},{fileId},{fileRevisionId}";
	}

	private string GetFileUrl(ClientFileSetFile file, long fileSetId, long fileRevisionId, long patchFileRevisionId = 0L)
	{
		string text = Uri.EscapeDataString($"filesets/{fileSetId}/File/{file.FileId}-{fileRevisionId}-{patchFileRevisionId}.zip");
		return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", text, DownloadPathType.None)).ToString();
	}

	private string GetFilePatchKey(long fileSetId, long fileId, long fileRevisionId, long patchFileRevisionId)
	{
		return $"{3},{fileSetId},{fileId},{fileRevisionId},{patchFileRevisionId}";
	}

	private string GetRevisionSetKey(ClientFileSetRevision revision)
	{
		return $"{4},{revision.FileSetId},{revision.RevisionId}";
	}

	private string GetRevisionPatchSetKey(ClientFileSetRevision revision)
	{
		return $"{5},{revision.FileSetId},{revision.RevisionId}";
	}

	private string GetPatchSetFileUrl(ClientFileSetRevision revision)
	{
		return GetProxiedS3Url(revision.PatchSetPath);
	}

	private string GetFileSetUrl(ClientFileSetRevision revision)
	{
		return GetProxiedS3Url(revision.SetPath);
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
