using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

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
		_logger.LogInfoWithSource("ReportFileVersions", "Invoke", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsServiceJob.cs");
		await _kioskFileSetVersionsService.ReportFileVersions();
	}
}
