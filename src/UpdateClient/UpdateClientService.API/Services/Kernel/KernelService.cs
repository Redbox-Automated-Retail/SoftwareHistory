using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API.Services.Kernel;

public class KernelService : IKernelService
{
	private readonly ICommandLineService _cmd;

	private readonly ILogger<KernelService> _logger;

	private readonly KernelServiceSettings _settings;

	public KernelService(ICommandLineService cmd, ILogger<KernelService> logger, IOptionsMonitor<AppSettings> settings)
	{
		_cmd = cmd;
		_logger = logger;
		_settings = settings?.CurrentValue?.KernelService;
	}

	public bool PerformShutdown(ShutdownType shutdownType)
	{
		_logger.LogInfoWithSource($"Performing {shutdownType}", "PerformShutdown");
		if (!_settings.ShutdownEnabled)
		{
			_logger.LogWarningWithSource($"Cannot {shutdownType} because ShutdownEnabled is set to {_settings.ShutdownEnabled}", "PerformShutdown");
			return false;
		}
		switch (shutdownType)
		{
		case ShutdownType.Reboot:
			return _cmd.TryExecutePowerShellScript("Restart-Computer -Force");
		case ShutdownType.Shutdown:
			return _cmd.TryExecutePowerShellScript("Stop-Computer -Force");
		default:
			_logger.LogWarningWithSource($"{shutdownType} was unsuccessful", "PerformShutdown");
			return false;
		}
	}
}
