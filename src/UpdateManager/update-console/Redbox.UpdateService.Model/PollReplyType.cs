namespace Redbox.UpdateService.Model;

internal enum PollReplyType
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
