using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel.Views.Browse;

public static class BrowseFunctions
{
	[KernelFunction(Name = "kernel.cache_product_images")]
	internal static void CacheProductImages()
	{
		BrowseViewController.CacheProductImages();
	}

	[KernelFunction(Name = "kernel.configure_browse_view")]
	internal static void ConfigureBrowseView()
	{
		BrowseViewController.ConfigureBrowseView();
	}

	[KernelFunction(Name = "kernel.cleanup_browse_view")]
	internal static void CleanupBrowseView()
	{
		BrowseViewController.CleanupBrowseView();
	}

	[KernelFunction(Name = "kernel.show_cart_full_message")]
	internal static void ShowCartFullMessage()
	{
		Redbox.Rental.Services.Controllers.BrowseFunctions.ShowCartFullMessage();
	}

	[KernelFunction(Name = "kernel.get_visible_browse_products")]
	internal static LuaTable GetVisibleBrowseProducts()
	{
		return Session.Instance?.BrowseState?.BrowseProductsShownLuaTable;
	}

	[KernelFunction(Name = "kernel.is_product_thinned")]
	private static bool IsProductThinned(long productId)
	{
		if (ServiceLocator.Instance.GetService(typeof(IDataService)) is IDataService dataService)
		{
			return dataService.GetIsProductThinned(productId);
		}
		return false;
	}
}
