using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Kernel.Views.Browse;

public static class BrowseFunctions
{
	[KernelFunction(Name = "kernel.get_visible_browse_products")]
	internal static LuaTable GetVisibleBrowseProducts()
	{
		return Session.Instance?.BrowseState?.BrowseProductsShownLuaTable;
	}
}
