using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.DownloadService;

public class BaseDownloader : IDownloader, IPersistentData
{
	private readonly IPersistentDataCacheService _cache;

	DownloadData IDownloader.DownloadData
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public BaseDownloader(IPersistentDataCacheService cache)
	{
		_cache = cache;
	}

	public virtual async Task<List<Error>> ProcessDownload(DownloadData data)
	{
		return await Task.Run(() => new List<Error>());
	}

	public virtual async Task<List<Error>> SaveDownload(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		try
		{
			if (!(await _cache.Write(data, data.FileName, UpdateClientServiceConstants.DownloadDataFolder)))
			{
				errors.Add(new Error
				{
					Message = "BaseDownloader -> " + data.FileName + " did not write successfully. Check logs for more details."
				});
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "BaseDownloader -> Unhandled exception occurred DeleteDownload. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	public virtual async Task<List<Error>> DeleteDownload(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		try
		{
			await _cache.Delete(data.FileName, UpdateClientServiceConstants.DownloadDataFolder);
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "BaseDownloader -> Unhandled exception occurred DeleteDownload. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	public virtual async Task<List<Error>> Complete(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		try
		{
			data.DownloadState = DownloadState.Complete;
			await SaveDownload(data);
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "BaseDownloader -> An unhandled exception occurred in Cleanup. Exception " + e.GetFullMessage()
			});
		}
		return errors;
	}

	public virtual List<Error> Cleanup(List<DownloadData> data)
	{
		return new List<Error>();
	}
}
