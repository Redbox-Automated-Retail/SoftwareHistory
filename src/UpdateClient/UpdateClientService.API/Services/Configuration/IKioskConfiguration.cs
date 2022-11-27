namespace UpdateClientService.API.Services.Configuration;

public interface IKioskConfiguration
{
	Operations Operations { get; }

	FileSets FileSets { get; }
}
