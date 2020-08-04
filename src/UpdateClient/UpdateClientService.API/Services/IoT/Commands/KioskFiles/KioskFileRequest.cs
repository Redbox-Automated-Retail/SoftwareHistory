namespace UpdateClientService.API.Services.IoT.Commands.KioskFiles;

public class KioskFileRequest
{
	public string KioskId { get; set; }

	public FileTypeEnum FileType { get; set; }

	public string FileName { get; set; }

	public string S3PreSignedUrl { get; set; }
}
