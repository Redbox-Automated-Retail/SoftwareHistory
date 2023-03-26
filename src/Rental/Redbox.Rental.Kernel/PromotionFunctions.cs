using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class PromotionFunctions
{
	[KernelFunction(Name = "kernel.rental_promotion_load_incart_data")]
	internal static void LoadInCartData()
	{
		PromotionService.Instance.LoadInCartData();
	}

	[KernelFunction(Name = "kernel.rental_promotion_clear_promotion")]
	internal static void ClearPromotion()
	{
		PromotionService.Instance.ClearPromotion();
	}
}
