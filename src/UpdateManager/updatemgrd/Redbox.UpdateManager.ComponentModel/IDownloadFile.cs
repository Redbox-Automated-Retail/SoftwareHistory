namespace Redbox.UpdateManager.ComponentModel;

internal interface IDownloadFile
{
	IDownloadFileData DownloadFileData { get; }

	ErrorList ProcessDownloadFile();

	ErrorList SaveDownloadFile();

	ErrorList DeleteDownloadFile();
}
