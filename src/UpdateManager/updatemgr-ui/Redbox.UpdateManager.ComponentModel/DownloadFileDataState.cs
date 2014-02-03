namespace Redbox.UpdateManager.ComponentModel;

internal enum DownloadFileDataState
{
	None,
	Error,
	PendingDownload,
	Downloading,
	PostDownload,
	PendingInstall,
	Installing,
	PostInstall,
	Complete
}
