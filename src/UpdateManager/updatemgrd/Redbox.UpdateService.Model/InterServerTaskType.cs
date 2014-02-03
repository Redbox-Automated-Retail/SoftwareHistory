namespace Redbox.UpdateService.Model;

internal enum InterServerTaskType
{
	CreateRepositoryChangeSet,
	UpdateConfigFiles,
	ClearAllConfigFileSyncIds,
	UpdateStoreInfo,
	ClearAllStoreInfoSyncIds,
	ClearAllSyncIds,
	UpdateDownloadList,
	UpdateClientRepositoryData,
	CreateFileSetChangeSet
}
