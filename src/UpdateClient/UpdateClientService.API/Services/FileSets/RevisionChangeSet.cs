using System;
using UpdateClientService.API.Services.DownloadService;

namespace UpdateClientService.API.Services.FileSets;

public class RevisionChangeSet : RevisionMetadata
{
	public DateTime? Received { get; set; }

	public DateTime? Downloaded { get; set; }

	public DateTime? Staged { get; set; }

	public DateTime? Activated { get; set; }

	public ChangesetState State { get; set; }

	public string Message { get; set; }

	public int RetryCount { get; set; }

	public string Path { get; set; }

	public int CompressionType { get; set; }

	public string ContentHash { get; set; }

	public string FileHash { get; set; }

	public DateTime DownloadOn { get; set; }

	public DownloadPriority DownloadPriority { get; set; }

	public DateTime ActiveOn { get; set; }

	public string ActivateStartTime { get; set; }

	public string ActivateEndTime { get; set; }

	public string DownloadUrl { get; set; }

	public string StateText => Enum.GetName(typeof(ChangesetState), State);
}
