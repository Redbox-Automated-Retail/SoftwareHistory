using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class TitleDetailViewFunctions
{
	[KernelFunction(Name = "kernel.configure_title_detail_view")]
	public static void ConfigureTitleDetails()
	{
		TitleDetailsController.ConfigureView();
	}
}
