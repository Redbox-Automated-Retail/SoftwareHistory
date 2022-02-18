namespace UpdateClientService.API.Services.FileSets;

public enum PollRequestType
{
	None,
	Repositories,
	ConfigFiles,
	StoreInfo,
	WorkResult,
	StatusMessage,
	TransferStatisticReport,
	GiveMeRepositoryChangesets,
	StoreFiles,
	FileSets
}
