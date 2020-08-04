using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.IoT.UpdateServiceRequests;

namespace UpdateClientService.API.Services.IoT.Commands.DownloadFiles;

public class DownloadFilesService : IDownloadFilesService
{
	private readonly IHttpService _httpService;

	private readonly IStoreService _store;

	private readonly IPersistentDataCacheService _cache;

	private readonly IIoTService _ioTService;

	private readonly ILogger<DownloadFilesService> _logger;

	private const string DownloadFileListFileName = "downloadFileList.json";

	public DownloadFilesService(IHttpService httpService, ILogger<DownloadFilesService> logger, IStoreService store, IPersistentDataCacheService cache, IMqttProxy mqttProxy, IIoTService ioTService)
	{
		_httpService = httpService;
		_logger = logger;
		_store = store;
		_cache = cache;
		_ioTService = ioTService;
	}

	public async Task HandleDownloadFileJob(DownloadFileJob job)
	{
		DownloadFileJobExecution execution = new DownloadFileJobExecution
		{
			DownloadFileJobId = job.DownloadFileJobId,
			KioskId = _store.KioskId,
			Status = JobStatus.Scheduled
		};
		_ = job.StartDateUtc;
		if (job.StartDateUtc == default(DateTimeOffset) || job.StartDateUtc.Add(job.StartTime) < DateTimeOffset.Now)
		{
			await ExecuteDownloadFileJob(job);
			return;
		}
		PersistentDataWrapper<DownloadFileJobList> persistentDataWrapper = await _cache.Read<DownloadFileJobList>("downloadFileList.json", useCache: true);
		if (persistentDataWrapper.Data == null)
		{
			persistentDataWrapper.Data = new DownloadFileJobList();
		}
		persistentDataWrapper.Data.JobList.Add(job);
		await _cache.Write(persistentDataWrapper.Data, "downloadFileList.json");
		await _ioTService.UpdateServiceRequest(new UpdateServiceRequest
		{
			RequestType = UpdateServiceRequestEnum.DownloadFileJobExecutionStatusUpdate,
			Payload = new MqttResponse<DownloadFileJobExecution>
			{
				Data = execution
			}.ToJson()
		});
	}

	public async Task ExecuteDownloadFileJob(DownloadFileJob job)
	{
		DownloadFileJobExecution executionResult = new DownloadFileJobExecution
		{
			DownloadFileJobId = job.DownloadFileJobId,
			KioskId = _store.KioskId,
			EventTime = DateTime.Now,
			Status = JobStatus.InProgress
		};
		UpdateServiceRequest result = new UpdateServiceRequest
		{
			RequestType = UpdateServiceRequestEnum.DownloadFileJobExecutionStatusUpdate,
			Payload = new MqttResponse<DownloadFileJobExecution>
			{
				Data = executionResult
			}
		};
		await _ioTService.UpdateServiceRequest(result);
		try
		{
			await IfPreSignedUrlExpiredGetNewOne(job);
			if (!string.IsNullOrWhiteSpace(job.Metadata?.FileName) && !string.IsNullOrWhiteSpace(job.PresignedFileUrl))
			{
				await SaveFileToDestination(job, executionResult);
			}
			if (!string.IsNullOrWhiteSpace(job.Metadata?.ActivationScriptName) && !string.IsNullOrWhiteSpace(job.PresignedScriptUrl) && executionResult.Status != JobStatus.Error)
			{
				await DownloadAndExecuteScript(job, executionResult);
			}
			if (executionResult.Status != JobStatus.Error)
			{
				executionResult.Status = JobStatus.Complete;
			}
			executionResult.CompletedOn = DateTime.Now;
			await _ioTService.UpdateServiceRequest(result);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "An error occurred while executing the download file job.");
			executionResult.CompletedOn = DateTime.Now;
			executionResult.Status = JobStatus.Error;
			await _ioTService.UpdateServiceRequest(result);
		}
	}

	public async Task HandleScheduledJobs()
	{
		bool renewCache = false;
		PersistentDataWrapper<DownloadFileJobList> cachedItems = await _cache.Read<DownloadFileJobList>("downloadFileList.json");
		if (cachedItems.Data == null)
		{
			cachedItems.Data = new DownloadFileJobList();
		}
		DownloadFileJobList jobListToRemove = new DownloadFileJobList();
		_logger.LogInfoWithSource("Handling scheduled jobs, " + (cachedItems.Data.JobList?.ToJson() ?? "null"), "HandleScheduledJobs");
		foreach (DownloadFileJob job in cachedItems.Data.JobList)
		{
			if (job.StartDateUtc.Add(job.StartTime) < DateTimeOffset.Now)
			{
				await ExecuteDownloadFileJob(job);
				jobListToRemove.JobList.Add(job);
				renewCache = true;
			}
		}
		if (renewCache)
		{
			cachedItems.Data.JobList.RemoveAll((DownloadFileJob x) => jobListToRemove.JobList.Contains(x));
			await _cache.Write(cachedItems.Data, "downloadFileList.json");
		}
	}

	private async Task IfPreSignedUrlExpiredGetNewOne(DownloadFileJob job)
	{
		if (DateTimeOffset.UtcNow > job.CreatedOnUtc.AddMinutes(10.0))
		{
			DownloadFileJob downloadFileJob = job;
			string presignedFileUrl = (string.IsNullOrWhiteSpace(job.PresignedFileUrl) ? null : (await _ioTService.UpdateServiceRequest(new UpdateServiceRequest
			{
				RequestType = UpdateServiceRequestEnum.GetDownloadFileUrl,
				Payload = job.FileKey
			})));
			downloadFileJob.PresignedFileUrl = presignedFileUrl;
			downloadFileJob = job;
			string presignedScriptUrl = (string.IsNullOrWhiteSpace(job.PresignedScriptUrl) ? null : (await _ioTService.UpdateServiceRequest(new UpdateServiceRequest
			{
				RequestType = UpdateServiceRequestEnum.GetActivationScriptUrl,
				Payload = job.FileKey
			})));
			downloadFileJob.PresignedScriptUrl = presignedScriptUrl;
		}
	}

	private async Task SaveFileToDestination(DownloadFileJob job, DownloadFileJobExecution executionResult)
	{
		APIResponse<byte[]> aPIResponse = await _httpService.DownloadFile(job.PresignedFileUrl.ToString(), (int)TimeSpan.FromHours(1.0).TotalMilliseconds);
		if (aPIResponse.IsSuccessStatusCode)
		{
			string path = Path.Combine(job.Metadata.DestinationPath, job.Metadata.FileName);
			Directory.CreateDirectory(job.Metadata.DestinationPath);
			await File.WriteAllBytesAsync(path, aPIResponse.Response);
		}
		else
		{
			_logger.LogErrorWithSource("Could not successfully download file from " + (job.PresignedFileUrl ?? "null"), "SaveFileToDestination");
			executionResult.Status = JobStatus.Error;
		}
	}

	private async Task DownloadAndExecuteScript(DownloadFileJob job, DownloadFileJobExecution executionResult)
	{
		DownloadFileMetadata metadata = job.Metadata;
		int num;
		if (metadata == null)
		{
			num = 0;
		}
		else
		{
			string activationScriptName = metadata.ActivationScriptName;
			num = ((((activationScriptName != null) ? new bool?(!activationScriptName.EndsWith(".ps1")) : ((bool?)null)) == true) ? 1 : 0);
		}
		if (num != 0)
		{
			_logger.LogErrorWithSource("Script " + (job.Metadata.ActivationScriptName ?? "null") + " does not have a valid PowerShell extension of '.ps1'", "DownloadAndExecuteScript");
			executionResult.Status = JobStatus.Error;
			return;
		}
		APIResponse<byte[]> aPIResponse = await _httpService.DownloadFile(job.PresignedScriptUrl);
		if (aPIResponse.IsSuccessStatusCode)
		{
			string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ps1");
			Directory.CreateDirectory(Path.GetTempPath());
			await File.WriteAllBytesAsync(path, aPIResponse.Response);
			if (!TryExecuteScript(path))
			{
				executionResult.Status = JobStatus.Error;
			}
			File.Delete(path);
		}
		else
		{
			_logger.LogErrorWithSource("Could not successfully download file from " + (job.PresignedScriptUrl ?? "null") + ", Errors: " + aPIResponse.Errors?.ToJson(), "DownloadAndExecuteScript");
			executionResult.Status = JobStatus.Error;
		}
	}

	private bool TryExecuteScript(string path)
	{
		Process process = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				FileName = "powershell",
				Arguments = "-file \"" + path + "\""
			}
		};
		process.Start();
		string text = process.StandardOutput.ReadToEnd();
		string text2 = process.StandardError.ReadToEnd();
		process.WaitForExit((int)TimeSpan.FromMinutes(5.0).TotalMilliseconds);
		if (!string.IsNullOrWhiteSpace(text))
		{
			_logger.LogInfoWithSource(text, "TryExecuteScript");
		}
		if (!string.IsNullOrWhiteSpace(text2))
		{
			_logger.LogErrorWithSource(text2, "TryExecuteScript");
		}
		return string.IsNullOrWhiteSpace(text2);
	}
}
