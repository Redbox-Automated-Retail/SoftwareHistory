using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel;

public static class HealthServiceFunctions
{
	[KernelFunction(Name = "kernel.queuekioskfunctioncheck")]
	internal static void QueueKioskStaticFunctionCheck()
	{
		ServiceLocator.Instance.GetService<IRentalHealthService>().KioskFunctionCheck();
	}
}
