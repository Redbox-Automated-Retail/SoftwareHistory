using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UpdateClientService.API.Services.IoT.DownloadFiles;

public class DownloadFilesServiceJob : IInvocable
{
	private ILogger<DownloadFilesServiceJob> _logger;

	private IDownloadFilesService _downloadFilesService;

	private DownloadFilesSettings _settings;

	public DownloadFilesServiceJob(ILogger<DownloadFilesServiceJob> logger, IDownloadFilesService downloadFilesService, IOptionsMonitor<AppSettings> settings)
	{
		_logger = logger;
		_downloadFilesService = downloadFilesService;
		_settings = settings.CurrentValue.DownloadFiles;
	}

	public async Task Invoke()
	{
		if (_settings?.Enabled ?? false)
		{
			_logger.LogInfoWithSource("HandleScheduledJobs", "Invoke");
			await _downloadFilesService.HandleScheduledJobs();
		}
	}
}
