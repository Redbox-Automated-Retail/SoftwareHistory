using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public class ReturnFunctions
{
	[KernelFunction(Name = "kernel.show_machine_full_warning")]
	internal static void ShowMachineFullWarning()
	{
		Redbox.Rental.Services.Controllers.ReturnFunctions.ShowMachineFullWarning();
	}
}
