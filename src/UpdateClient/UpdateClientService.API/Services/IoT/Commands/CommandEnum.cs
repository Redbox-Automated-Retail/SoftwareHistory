namespace UpdateClientService.API.Services.IoT.Commands;

public enum CommandEnum
{
	Default,
	PeekFiles,
	GetFile,
	RenewCertificates,
	ExecuteDownloadFileJob,
	ExecuteHttpRequest,
	RenewIoTCertificates,
	Ping
}
