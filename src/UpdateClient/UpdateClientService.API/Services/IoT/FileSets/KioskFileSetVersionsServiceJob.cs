using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.IoT.FileSets;

public class KioskFileSetVersionsServiceJob : IInvocable
{
	private ILogger<KioskFileSetVersionsServiceJob> _logger;

	private IKioskFileSetVersionsService _kioskFileSetVersionsService;

	public KioskFileSetVersionsServiceJob(ILogger<KioskFileSetVersionsServiceJob> logger, IKioskFileSetVersionsService kioskFileSetVersionsService)
	{
		_logger = logger;
		_kioskFileSetVersionsService = kioskFileSetVersionsService;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("ReportFileVersions", "Invoke");
		await _kioskFileSetVersionsService.ReportFileVersions();
	}
}
