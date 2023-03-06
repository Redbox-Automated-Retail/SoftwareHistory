namespace UpdateClientService.API.Services.Configuration;

public interface IKioskConfiguration
{
	long ConfigurationVersion { get; }

	Operations Operations { get; }

	FileSets FileSets { get; }

	void Log(ConfigLogParameters loggingParameters = null);
}
