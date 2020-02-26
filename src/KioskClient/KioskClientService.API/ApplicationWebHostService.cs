using System;
using KioskClientService.API.Models;
using KioskClientService.API.Services.Analytics.KioskDataService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

public class ApplicationWebHostService : WebHostService
{
	private ILogger _logger;

	private IAnalyticsService _analytics;

	public ApplicationWebHostService(IWebHost host, ILogger<ApplicationWebHostService> logger, IAnalyticsService analytics)
		: base(host)
	{
		_logger = logger;
		_analytics = analytics;
	}

	protected override void OnStarting(string[] args)
	{
		_logger?.LogInformation("WebHost: OnStarting");
		_analytics?.ServiceEvent(KioskClientEvent.ServiceStarting);
		_analytics?.PurgeOldAnalytics(TimeSpan.FromDays(30.0));
		base.OnStarting(args);
	}

	protected override void OnStarted()
	{
		_logger?.LogInformation("WebHost: OnStarted");
		_analytics?.ServiceEvent(KioskClientEvent.ServiceStarted);
		base.OnStarted();
	}

	protected override void OnStopping()
	{
		_logger?.LogInformation("WebHost: OnStopping");
		_analytics?.ServiceEvent(KioskClientEvent.ServiceStopping);
		base.OnStopping();
	}

	protected override void OnStopped()
	{
		_logger?.LogInformation("WebHost: OnStopped");
		_analytics?.ServiceEvent(KioskClientEvent.ServiceStopped);
		base.OnStopped();
	}
}
