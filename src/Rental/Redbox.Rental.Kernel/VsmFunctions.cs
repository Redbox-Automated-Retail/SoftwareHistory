using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public static class VsmFunctions
{
	[KernelFunction(Name = "kernel.register_update_vsm_impression_callback")]
	internal static void RegisterUpdateVsmImpressionCallback(Action<string> updateVsmImpressionCallback)
	{
		VsmViewController.RegisterUpdateVsmImpressionCallback(updateVsmImpressionCallback);
	}

	[KernelFunction(Name = "kernel.is_vsm_ad_available")]
	internal static bool IsVsmAdAvailable()
	{
		return VsmViewController.IsVsmAdAvailable();
	}
}
