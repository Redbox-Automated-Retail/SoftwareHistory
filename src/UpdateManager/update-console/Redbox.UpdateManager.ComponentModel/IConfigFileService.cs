namespace Redbox.UpdateManager.ComponentModel;

internal interface IConfigFileService
{
	ErrorList ActivateConfigFile(long configFileId);
}
