using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class OrchestrationFunctions
{
	[KernelFunction(Name = "kernel.view_analytics_start")]
	internal static void ViewAnalyticsStart(string sessionType)
	{
		OrchestrationService.CreateStartViewAnalytics(sessionType);
	}
}
