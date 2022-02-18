namespace UpdateClientService.API.Services.FileSets;

public interface IFileSetTransition
{
	FileSetTransition Initialize(ClientFileSetRevision revision);
}
