using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel.Views;

public class MaintenanceViewFunctions
{
	[KernelFunction(Name = "kernel.show_maintenance_view")]
	public static void ShowMaintenanceView(string errorCode)
	{
		OrchestrationService.ViewShow("maintenance_view", errorCode);
	}
}
