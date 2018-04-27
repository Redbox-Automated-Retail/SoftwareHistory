using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public class ReservationFunctions
{
	[KernelFunction(Name = "kernel.register_get_reservation_info_callback")]
	internal static void RegisterGetReservationInfoCallback(Func<LuaTable> getReservationInfoCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterGetReservationInfoCallback(getReservationInfoCallback);
	}

	[KernelFunction(Name = "kernel.register_vend_only_reserved_titles_callback")]
	internal static void RegisterVendOnlyReservedTitlesCallback(Action<string, string> vendOnlyReservedTitlesCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterVendOnlyReservedTitlesCallback(vendOnlyReservedTitlesCallback);
	}

	[KernelFunction(Name = "kernel.register_reservation_authorize_callback")]
	internal static void RegisterReservationAuthorizeCallback(Action<string, string> reservationAuthorizeCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterReservationAuthorizeCallback(reservationAuthorizeCallback);
	}

	[KernelFunction(Name = "kernel.get_recommended_product_count")]
	internal static int GetRecommendedProductCount()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.GetRecommendedProductCount();
	}

	[KernelFunction(Name = "kernel.add_recommended_products_to_analytics")]
	internal static void AddRecommendedProductsToAnalytics()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.AddRecommendedProductsToAnalytics();
	}
}
