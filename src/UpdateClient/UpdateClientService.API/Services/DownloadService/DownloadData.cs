using System;

namespace UpdateClientService.API.Services.DownloadService;

public class DownloadData : IPersistentData
{
	public string Key { get; set; }

	public string Url { get; set; }

	public string Hash { get; set; }

	public string Path { get; set; }

	public DownloadState DownloadState { get; set; }

	public DownloadType DownloadType { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public Guid FileGuid { get; set; }

	public Guid BitsGuid { get; set; }

	public string Message { get; set; }

	public int RetryCount { get; set; }

	public string FileName => Key + DownloadDataConstants.DownloadExtension + ".json";

	public bool CompleteOnFinish { get; set; }

	public long BitsTransferred { get; set; }

	public long BitsTotal { get; set; }
}
