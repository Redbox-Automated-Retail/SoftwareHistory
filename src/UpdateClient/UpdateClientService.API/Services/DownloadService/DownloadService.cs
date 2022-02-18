using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.DownloadService;

public class DownloadService : IDownloadService, IInvocable
{
	private readonly ILogger<DownloadService> _logger;

	private readonly IPersistentDataCacheService _cache;

	private readonly IDownloader _downloader;

	private readonly IIoTCommandClient _iotCommandClient;

	private readonly TimeSpan _proxiedS3UrlRequestTimeout = TimeSpan.FromSeconds(60.0);

	public const string DownloadExtension = ".dldat";

	private static SemaphoreSlim Lock => DownloadServiceLock.Lock;

	public DownloadService(ILogger<DownloadService> logger, IPersistentDataCacheService cache, IDownloader downloader, IIoTCommandClient iotCommandClient)
	{
		_logger = logger;
		_cache = cache;
		_downloader = downloader;
		_iotCommandClient = iotCommandClient;
	}

	public async Task Invoke()
	{
		try
		{
			if (!(await Lock.WaitAsync(0)))
			{
				_logger.LogWarningWithSource("DownloadService is already being invoked. Skipping...", "Invoke");
				return;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Could not acquire lock");
			return;
		}
		try
		{
			List<Error> source = await ProcessDownloads();
			if (source.Any())
			{
				_logger.LogErrorWithSource(string.Join(",", source.Select((Error e) => e.Message)), "Invoke");
			}
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, "An unhandled exception occurred.", "Invoke");
		}
		finally
		{
			SemaphoreSlim semaphoreSlim = Lock;
			if (semaphoreSlim != null && semaphoreSlim.CurrentCount == 0)
			{
				Lock.Release();
			}
		}
	}

	public async Task<List<DownloadData>> GetDownloads(string pattern = null)
	{
		List<DownloadData> result = new List<DownloadData>();
		try
		{
			result = (from d in await _cache.ReadLike<DownloadData>(string.IsNullOrWhiteSpace(pattern) ? DownloadDataConstants.DownloadExtension : pattern, UpdateClientServiceConstants.DownloadDataFolder)
				where d?.Data != null
				select d.Data).ToList();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An unhandled exception occurred");
		}
		return result;
	}

	public async Task<List<DownloadData>> GetDownloads(Regex pattern)
	{
		List<DownloadData> result = new List<DownloadData>();
		try
		{
			result = (from d in await _cache.ReadLike<DownloadData>(pattern, UpdateClientServiceConstants.DownloadDataFolder)
				where d?.Data != null
				select d.Data).ToList();
		}
		catch (Exception ex)
		{
			_logger.LogCriticalWithSource(ex, "An unhandled exception occurred", "GetDownloads", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
		}
		return result;
	}

	public async Task<List<Error>> AddDownload(string key, string hash, string url, DownloadPriority priority, bool completeOnFinish)
	{
		List<Error> errors = new List<Error>();
		_logger.LogInfoWithSource($"Adding download key: {key} hash: {hash} url: {url} priority: {priority}", "AddDownload");
		try
		{
			DownloadData data = InitializeDownloadData(key, hash, url, priority, completeOnFinish);
			List<Error> list = errors;
			list.AddRange(await _downloader.SaveDownload(data));
			if (errors.Any())
			{
				return errors;
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "DownloadService -> An unhandled exception occurred in AddDownload. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	public async Task<(List<Error> errors, DownloadData downloadData)> AddRetrieveDownload(string key, string hash, string url, DownloadPriority priority, bool completeOnFinish)
	{
		List<Error> errors = new List<Error>();
		DownloadData data = null;
		_logger.LogInfoWithSource($"Adding download key: {key} hash: {hash} url: {url} priority: {priority}", "AddRetrieveDownload");
		try
		{
			data = InitializeDownloadData(key, hash, url, priority, completeOnFinish);
			List<Error> list = errors;
			list.AddRange(await _downloader.SaveDownload(data));
			if (errors.Any())
			{
				return (errors: errors, downloadData: data);
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "DownloadService -> An unhandled exception occurred in AddDownload. Exception -> " + e.GetFullMessage()
			});
		}
		return (errors: errors, downloadData: data);
	}

	public async Task<List<Error>> CancelDownload(string key)
	{
		List<Error> errors = new List<Error>();
		_logger.LogInfoWithSource("Deleting download matching key: " + key, "CancelDownload");
		try
		{
			DownloadData downloadData = await GetDownloadData(key);
			if (downloadData != null)
			{
				List<Error> list = errors;
				list.AddRange(await _downloader.DeleteDownload(downloadData));
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "DownloadService -> An unhandled exception occurred in AddDownload. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	public async Task<List<Error>> CompleteDownload(string key)
	{
		List<Error> errors = new List<Error>();
		_logger.LogInfoWithSource("Completing download matching key: " + key, "CompleteDownload");
		try
		{
			DownloadData downloadData = await GetDownloadData(key);
			if (downloadData != null)
			{
				List<Error> list = errors;
				list.AddRange(await _downloader.Complete(downloadData));
			}
			else
			{
				errors.Add(new Error
				{
					Code = 404.ToString(),
					Message = "No download found matching key: " + key
				});
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "DownloadService -> An unhandled exception occurred in AddDownload. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	public async Task<string> GetProxiedS3Url(string key, DownloadPathType type, bool isHead = false)
	{
		PerformIoTCommandParameters performIoTCommandParameters = new PerformIoTCommandParameters
		{
			RequestTimeout = _proxiedS3UrlRequestTimeout,
			WaitForResponse = true
		};
		if (!Debugger.IsAttached)
		{
			performIoTCommandParameters.IoTTopic = "$aws/rules/kioskrestcall";
		}
		string text = await _iotCommandClient.PerformIoTCommandWithStringResult(new IoTCommandModel
		{
			Version = 2,
			Command = CommandEnum.GetPresignedS3Url,
			Payload = new PresignedS3UrlRequest
			{
				Key = key,
				Type = type,
				IsHead = isHead
			}
		}, performIoTCommandParameters);
		if (string.IsNullOrEmpty(text) || !Uri.TryCreate(text, UriKind.Absolute, out Uri _))
		{
			_logger.LogErrorWithSource("Url '" + text + "' is not a valid Url", "GetProxiedS3Url");
			text = null;
		}
		return text;
	}

	private async Task<DownloadData> GetDownloadData(string key)
	{
		return (await GetDownloads()).Where((DownloadData d) => d.Key == key).FirstOrDefault();
	}

	private async Task<List<Error>> ProcessDownloads()
	{
		List<Error> errors = new List<Error>();
		try
		{
			List<DownloadData> downloads = await GetDownloads();
			if (downloads != null && downloads.Count > 0)
			{
				_logger.LogInfoWithSource($"Start processing {downloads.Count} downloads", "ProcessDownloads");
			}
			foreach (DownloadData item in downloads)
			{
				List<Error> list = errors;
				list.AddRange(await _downloader.ProcessDownload(item));
			}
			errors.AddRange(_downloader.Cleanup(downloads));
			if (downloads != null && downloads.Count > 0)
			{
				_logger.LogInfoWithSource("Finished executing", "ProcessDownloads");
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "DownloadService -> An unhandled exception occurred in ProcessDownloads. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	private DownloadData InitializeDownloadData(string key, string hash, string url, DownloadPriority downloadPriority, bool completeOnFinish)
	{
		return new DownloadData
		{
			Key = key,
			Url = url,
			DownloadState = DownloadState.None,
			RetryCount = 0,
			FileGuid = Guid.NewGuid(),
			Hash = hash,
			DownloadType = DownloadType.Bits,
			DownloadPriority = downloadPriority,
			CompleteOnFinish = completeOnFinish
		};
	}
}
