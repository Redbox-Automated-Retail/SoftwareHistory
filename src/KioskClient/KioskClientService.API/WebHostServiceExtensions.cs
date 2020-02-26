using System.ServiceProcess;
using KioskClientService.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API;

public static class WebHostServiceExtensions
{
	public static void RunAsDeviceServiceService(this IWebHost host)
	{
		using IServiceScope serviceScope = host.Services.CreateScope();
		ApplicationWebHostService service = new ApplicationWebHostService(host, host.Services.GetRequiredService<ILogger<ApplicationWebHostService>>(), serviceScope.ServiceProvider.GetRequiredService<IAnalyticsService>());
		ServiceBase.Run(service);
	}
}
