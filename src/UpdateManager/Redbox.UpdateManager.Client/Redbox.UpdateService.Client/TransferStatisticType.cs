namespace Redbox.UpdateService.Client;

internal enum TransferStatisticType
{
	StartedDownload,
	FinishedDownload,
	Error,
	StartedUpload,
	FinishedUpload,
	AverageSpeedInKPS
}
