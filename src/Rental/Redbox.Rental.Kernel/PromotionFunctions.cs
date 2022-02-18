using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class PromotionFunctions
{
	[KernelFunction(Name = "kernel.get_in_cart_promo_code")]
	internal static string GetInCartPromoCode()
	{
		return PromotionService.Instance.GetInCartPromoCode();
	}

	[KernelFunction(Name = "kernel.rental_promotion_free_in_cart_template_is_running")]
	internal static bool IsFreeInCartOfferRunning()
	{
		return PromotionService.Instance.IsFreeInCartOfferRunning;
	}

	[KernelFunction(Name = "kernel.rental_promotion_rent_two_in_cart_template_is_running")]
	internal static bool IsRentTwoInCartOfferRunning()
	{
		return PromotionService.Instance.IsRentTwoInCartOfferRunning;
	}

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
