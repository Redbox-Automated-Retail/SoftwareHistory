using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel;

public static class FieldMaintenanceFunctions
{
	[KernelFunction(Name = "kernel.fieldmaintenancegetcurrentsafetytipmessage")]
	internal static string GetCurrentSafetyTipMessage()
	{
		return (ServiceLocator.Instance.GetService<IFieldMaintenanceService>()?.GetCurrentSafetyTip())?.Message;
	}
}
