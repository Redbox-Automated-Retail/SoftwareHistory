using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel;

public static class ControlPanelFunctions
{
	[KernelFunction(Name = "kernel.update_user_name")]
	internal static void UpdateUserName(string userName)
	{
		IControlPanelService service = ServiceLocator.Instance.GetService<IControlPanelService>();
		if (service != null)
		{
			service.LoggedInUser = userName;
		}
	}
}
