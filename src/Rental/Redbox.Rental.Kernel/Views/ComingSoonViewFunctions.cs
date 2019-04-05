using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class ComingSoonViewFunctions
{
	[KernelFunction(Name = "kernel.register_get_coming_soon_products_callback")]
	internal static void RegisterGetComingSoonProductsCallback(Func<LuaTable> callback)
	{
		ComingSoonViewController.GetComingSoonProductsCallback = callback;
	}
}
