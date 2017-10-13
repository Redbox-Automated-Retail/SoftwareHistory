using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.StartView;

public static class StartViewFunctions
{
	[KernelFunction(Name = "kernel.configure_start_view")]
	internal static void ConfigureStartView(Action toggleCultureCallback, Action startIdleTimerCallback)
	{
		StartViewController.ConfigureStartView(toggleCultureCallback, startIdleTimerCallback);
	}

	[KernelFunction(Name = "kernel.reload_carousel")]
	internal static void ReloadCarousel(bool force)
	{
		StartViewController.ReloadCarousel(force);
	}

	[KernelFunction(Name = "kernel.is_product_in_carousel")]
	internal static bool IsProductInCarousel(long productId)
	{
		return StartViewController.IsProductInCarousel(productId);
	}

	[KernelFunction(Name = "kernel.leave_start_view")]
	internal static void LeaveStartView()
	{
		StartViewController.LeaveStartView();
	}
}
