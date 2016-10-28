using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class ReturnViewFunctions
{
	[KernelFunction(Name = "kernel.configure_return_view")]
	internal static void ConfigureReturnView(Action goBackAction)
	{
		ReturnViewController.ConfigureReturnView(goBackAction);
	}

	[KernelFunction(Name = "kernel.set_return_view_back_button_isvisible")]
	internal static void SetBackButtonIsVisible(bool isVisible)
	{
		ReturnViewController.SetBackButtonIsVisible(isVisible);
	}
}
