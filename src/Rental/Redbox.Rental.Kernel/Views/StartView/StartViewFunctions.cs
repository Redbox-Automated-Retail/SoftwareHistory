using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.StartView;

public static class StartViewFunctions
{
	[KernelFunction(Name = "kernel.configure_start_view")]
	internal static void ConfigureStartView(Action toggleCultureCallback, Action startIdleTimerCallback, Action showBrowseViewCollectionsCallback, Action<LuaTable> showCollectionItemsCallback, Action<LuaTable, bool, LuaTable, string> checkForWarningPopupsCallback, Func<LuaTable, bool> isProductInShoppingCart, Func<LuaTable, bool> isProductEarlyRelease)
	{
		StartViewController.ConfigureStartView(toggleCultureCallback, startIdleTimerCallback, showBrowseViewCollectionsCallback, showCollectionItemsCallback, checkForWarningPopupsCallback, isProductInShoppingCart, isProductEarlyRelease);
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
