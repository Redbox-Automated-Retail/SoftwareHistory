using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.Transfer;

namespace UpdateClientService.API.Services.DownloadService;

public class BitsDownloader : BaseDownloader
{
	private readonly ILogger<BitsDownloader> _logger;

	private readonly ITransferService _transferService;

	private const string DownloadTypeName = "Bits downloader";

	private const string BitsNameFormat = "<UCS::~{0}~::UCS>";

	private const string BitsNameLeft = "<UCS::~";

	private const string BitsNameRight = "~::UCS>";

	public BitsDownloader(ILogger<BitsDownloader> logger, ITransferService transferService, IPersistentDataCacheService cache)
		: base(cache)
	{
		_logger = logger;
		_transferService = transferService;
	}

	public override async Task<List<Error>> ProcessDownload(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		SetBitTransferStatus(data);
		if (data.DownloadState == DownloadState.None)
		{
			List<Error> list = errors;
			list.AddRange(await CheckNone(data));
		}
		if (data.DownloadState == DownloadState.Downloading)
		{
			List<Error> list = errors;
			list.AddRange(await CheckDownloading(data));
		}
		if (data.DownloadState == DownloadState.PostDownload)
		{
			List<Error> list = errors;
			list.AddRange(await CheckPostDownload(data));
		}
		if (data.DownloadState == DownloadState.Complete)
		{
			List<Error> list = errors;
			list.AddRange(await CheckComplete(data));
		}
		if (data.DownloadState == DownloadState.Error)
		{
			List<Error> list = errors;
			list.AddRange(await CheckError(data));
		}
		return errors;
	}

	public override List<Error> Cleanup(List<DownloadData> downloadList)
	{
		List<Error> list = new List<Error>();
		try
		{
			list.AddRange(GetDownloadFileBitsJobs(out var jobs));
			if (list.Any())
			{
				return list;
			}
			foreach (ITransferJob job in jobs)
			{
				DownloadData downloadData = downloadList.Where((DownloadData d) => d.BitsGuid == job.ID).FirstOrDefault();
				if (downloadData == null)
				{
					_logger.LogInfoWithSource($"Clearing bits job, guid {job.ID} that has no download file changeset.", "Cleanup", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
					list.AddRange(job.Cancel());
				}
			}
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = "BitsDownloader -> An unhandled exception occurred in BitsDownloadFile.Cleanup. Exception -> " + e.GetFullMessage()
			});
		}
		return list;
	}

	private void SetBitTransferStatus(DownloadData data)
	{
		if (data.BitsGuid == default(Guid) || data.DownloadState == DownloadState.Error)
		{
			return;
		}
		if (data.DownloadState == DownloadState.PostDownload)
		{
			data.BitsTransferred = data.BitsTotal;
			return;
		}
		_transferService.GetJob(data.BitsGuid, out var job);
		if (job == null)
		{
			_logger.LogInfoWithSource("Job was null for " + data.ToJson(), "SetBitTransferStatus", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			return;
		}
		try
		{
			data.BitsTotal = ((job.TotalBytes == ulong.MaxValue) ? long.MaxValue : Convert.ToInt64(job.TotalBytes));
			data.BitsTransferred = Convert.ToInt64(job.TotalBytesTransfered);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Something went wrong while updating transfer status.", "SetBitTransferStatus", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
	}

	private TransferJobPriority GetPriority(DownloadData data)
	{
		if (data.DownloadPriority == DownloadPriority.Low)
		{
			return TransferJobPriority.Low;
		}
		if (data.DownloadPriority == DownloadPriority.Normal)
		{
			return TransferJobPriority.Normal;
		}
		if (data.DownloadPriority == DownloadPriority.High)
		{
			return TransferJobPriority.High;
		}
		if (data.DownloadPriority == DownloadPriority.Foreground)
		{
			return TransferJobPriority.Foreground;
		}
		return TransferJobPriority.Low;
	}

	private async Task<List<Error>> CheckNone(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		if (data.DownloadState != DownloadState.None)
		{
			return errors;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", data.DownloadState, data.Key), "CheckNone", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		try
		{
			string name = $"<UCS::~{data.Key}~::UCS>";
			errors.AddRange(_transferService.CreateDownloadJob(name, out var job));
			errors.AddRange(job.SetMinimumRetryDelay(60u));
			errors.AddRange(job.SetNoProgressTimeout(0u));
			errors.AddRange(job.SetPriority(GetPriority(data)));
			errors.AddRange(job.AddItem(data.Url, Path.GetTempFileName()));
			List<Error> list;
			if (errors.Any())
			{
				errors.AddRange(job.Cancel());
				if (data.RetryCount == 10)
				{
					data.DownloadState = DownloadState.Error;
					errors.Add(new Error
					{
						Message = $"Retry limit exceeded for {job.ID}"
					});
				}
				else
				{
					data.DownloadState = DownloadState.None;
				}
				data.Message = string.Join(",", from error in errors
					where !string.IsNullOrWhiteSpace(error.Message)
					select error.Message);
				data.RetryCount++;
				list = errors;
				list.AddRange(await SaveDownload(data));
				return errors;
			}
			errors.AddRange(job.Resume());
			if (errors.Any())
			{
				data.DownloadState = DownloadState.Error;
				data.Message = errors[0].Message;
				list = errors;
				list.AddRange(await SaveDownload(data));
				return errors;
			}
			data.BitsGuid = job.ID;
			data.DownloadState = DownloadState.Downloading;
			list = errors;
			list.AddRange(await SaveDownload(data));
			_logger.LogInfoWithSource($"Download Name: {data.Key} has been put on the download queue. BITS job started with name: {name} and guid: {job.ID}.", "CheckNone", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "BitsDownloader -> An unhandled exception occurred in CheckNone. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	private async Task<List<Error>> CheckDownloading(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		if (data.DownloadState != DownloadState.Downloading)
		{
			return errors;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", data.DownloadState, data.Key), "CheckDownloading", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		try
		{
			errors.AddRange(GetDownloadFileBitsJobs(out var jobs));
			if (errors.Any())
			{
				data.DownloadState = DownloadState.Error;
				data.Message = errors[0].Message;
				List<Error> list = errors;
				list.AddRange(await SaveDownload(data));
				return errors;
			}
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == data.BitsGuid).FirstOrDefault();
			if (transferJob == null)
			{
				data.DownloadState = DownloadState.Error;
				data.Message = "Bits job missing";
				List<Error> list = errors;
				list.AddRange(await SaveDownload(data));
				return errors;
			}
			if (transferJob.Status == TransferStatus.Error)
			{
				errors.AddRange(transferJob.Cancel());
				data.DownloadState = DownloadState.Error;
				data.Message = "Bits job error";
				List<Error> list = errors;
				list.AddRange(await SaveDownload(data));
				return errors;
			}
			if (transferJob.Status == TransferStatus.Suspended)
			{
				errors.AddRange(transferJob.Cancel());
				data.DownloadState = DownloadState.Error;
				data.Message = "Bits job error";
				List<Error> list = errors;
				list.AddRange(await SaveDownload(data));
				return errors;
			}
			if (transferJob.Status == TransferStatus.TransientError)
			{
				errors.Add(new Error
				{
					Message = "BITS job status is TransientError for " + data.Key
				});
			}
			if (transferJob.Status == TransferStatus.Transferred)
			{
				List<Error> list = errors;
				list.AddRange(await FinishBitsDownload(transferJob, data));
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "BitsDownloader -> An unhandled exception occurred in CheckDownloading. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	private async Task<List<Error>> FinishBitsDownload(ITransferJob job, DownloadData data)
	{
		List<Error> errors = new List<Error>();
		try
		{
			_logger.LogInfoWithSource("Bits downloader FinishBitsDownload for " + data.Key, "FinishBitsDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			errors.AddRange(job.GetItems(out var items));
			errors.AddRange(job.Complete());
			List<Error> list;
			if (errors.Any())
			{
				errors.Add(new Error
				{
					Message = $"BitsDownloader -> Job {job.ID} get items or completion failed. This download will be canceled."
				});
				errors.AddRange(job.Cancel());
				data.DownloadState = DownloadState.Error;
				data.Message = errors[0].Message;
				list = errors;
				list.AddRange(await SaveDownload(data));
				return errors;
			}
			ITransferItem transferItem = items[0];
			string downloadPath = Path.Combine(transferItem.Path, transferItem.Name);
			errors.AddRange(CheckHash(downloadPath, data.Hash, out var matched, out var fileHash));
			if (!string.IsNullOrWhiteSpace(data.Hash) && !matched)
			{
				errors.Add(new Error
				{
					Message = "BitsDownloader -> Download file: " + data.Key + " path: " + downloadPath + " should have hash " + data.Hash + " but it has hash " + fileHash + ". This download failed and will need to be restarted."
				});
				data.DownloadState = DownloadState.Error;
				data.Message = errors[0].Message;
				list = errors;
				list.AddRange(await SaveDownload(data));
				File.Delete(downloadPath);
				return errors;
			}
			data.Path = downloadPath;
			data.DownloadState = DownloadState.PostDownload;
			list = errors;
			list.AddRange(await SaveDownload(data));
			if (string.IsNullOrWhiteSpace(data.Hash))
			{
				_logger.LogInfoWithSource("Download file: " + data.Key + " was not provided with a hash to compare against. Ignoring check.", "FinishBitsDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
			}
		}
		catch (Exception e)
		{
			errors.Add(new Error
			{
				Message = "BitsDownloader -> An unhandled exception occurred in FinishBitsDownload. Exception -> " + e.GetFullMessage()
			});
		}
		return errors;
	}

	private async Task<List<Error>> CheckPostDownload(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		if (data.DownloadState != DownloadState.PostDownload)
		{
			return errors;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", data.DownloadState, data.Key), "CheckPostDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		if (data.CompleteOnFinish)
		{
			List<Error> list = errors;
			list.AddRange(await Complete(data));
		}
		return errors;
	}

	private async Task<List<Error>> CheckComplete(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		if (data.DownloadState != DownloadState.Complete)
		{
			return errors;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", data.DownloadState, data.Key), "CheckComplete", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		List<Error> list = errors;
		list.AddRange(await DeleteDownload(data));
		return errors;
	}

	private async Task<List<Error>> CheckError(DownloadData data)
	{
		List<Error> errors = new List<Error>();
		if (data.DownloadState != DownloadState.Error)
		{
			return errors;
		}
		_logger.LogInfoWithSource(string.Format("{0} checking state: {1} for {2}", "Bits downloader", data.DownloadState, data.Key), "CheckError", "/sln/src/UpdateClientService.API/Services/DownloadService/BitsDownloader.cs");
		_ = data.BitsGuid;
		errors.AddRange(GetDownloadFileBitsJobs(out var jobs));
		if (!errors.Any())
		{
			ITransferJob transferJob = jobs.Where((ITransferJob j) => j.ID == data.BitsGuid).FirstOrDefault();
			if (transferJob != null)
			{
				errors.AddRange(transferJob.GetErrors());
				errors.AddRange(transferJob.Cancel());
			}
		}
		List<Error> list = errors;
		list.AddRange(await DeleteDownload(data));
		return errors;
	}

	private List<Error> GetDownloadFileBitsJobs(out List<ITransferJob> jobs)
	{
		List<Error> list = new List<Error>();
		list.AddRange(_transferService.GetJobs(out jobs, allUsers: false));
		if (list.Any())
		{
			jobs = new List<ITransferJob>();
		}
		else
		{
			jobs = jobs.Where((ITransferJob j) => j.Name.StartsWith("<UCS::~") && j.Name.EndsWith("~::UCS>")).ToList();
		}
		return list;
	}

	private List<Error> CheckHash(string filePath, string hash, out bool matched, out string fileHash)
	{
		List<Error> list = new List<Error>();
		matched = false;
		fileHash = string.Empty;
		try
		{
			using FileStream stream = File.OpenRead(filePath);
			fileHash = stream.GetSHA1Hash();
			matched = fileHash == hash;
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = "BitsDownloader -> An unhandled exception occurred in CheckHash. Exception -> " + e.GetFullMessage()
			});
		}
		return list;
	}
}
