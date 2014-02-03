namespace Redbox.UpdateService.Model;

internal enum PollRequestType
{
	None,
	Repositories,
	ConfigFiles,
	StoreInfo,
	WorkResult,
	StatusMessage,
	TransferStatisticReport
}
