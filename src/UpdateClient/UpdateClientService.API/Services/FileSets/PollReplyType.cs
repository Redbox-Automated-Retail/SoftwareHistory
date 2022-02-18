namespace UpdateClientService.API.Services.FileSets;

public enum PollReplyType
{
	None,
	UpdateRepositories,
	RepositoryChangeSet,
	UpdateConfigFiles,
	ConfigFileChangeSet,
	StoreInfoChangeSet,
	UpdateStoreInfo,
	Work,
	DownloadFile,
	StoreScheduleInfo,
	StoreFileChangeSet,
	FileSetChangeSet
}
