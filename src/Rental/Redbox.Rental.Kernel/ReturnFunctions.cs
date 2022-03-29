using System;
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

	[KernelFunction(Name = "kernel.can_show_return_visit_promo_offer_view")]
	internal static bool CanShowReturnVisitPromoOfferView()
	{
		return Redbox.Rental.Services.Controllers.ReturnFunctions.CanShowReturnVisitPromoOfferView();
	}

	[KernelFunction(Name = "kernel.show_return_visit_promo_offer_view")]
	internal static void ShowReturnVisitPromoOfferView(Action closeAction)
	{
		Redbox.Rental.Services.Controllers.ReturnFunctions.ShowReturnVisitPromoOfferView(closeAction);
	}
}
