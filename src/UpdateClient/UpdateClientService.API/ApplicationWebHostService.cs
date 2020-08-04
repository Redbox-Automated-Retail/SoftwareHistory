using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API;

public class ApplicationWebHostService : WebHostService
{
	private ILogger<ApplicationWebHostService> _logger;

	public ApplicationWebHostService(IWebHost host, ILogger<ApplicationWebHostService> logger)
		: base(host)
	{
		_logger = logger;
	}

	protected override void OnStarting(string[] args)
	{
		_logger?.LogInfoWithSource("WebHost: OnStarting", "OnStarting");
		base.OnStarting(args);
	}

	protected override void OnStarted()
	{
		_logger?.LogInfoWithSource("WebHost: OnStarted", "OnStarted");
		base.OnStarted();
	}

	protected override void OnStopping()
	{
		_logger?.LogInfoWithSource("WebHost: OnStopping", "OnStopping");
		base.OnStopping();
	}

	protected override void OnStopped()
	{
		_logger?.LogInfoWithSource("WebHost: OnStopped", "OnStopped");
		base.OnStopped();
	}
}
