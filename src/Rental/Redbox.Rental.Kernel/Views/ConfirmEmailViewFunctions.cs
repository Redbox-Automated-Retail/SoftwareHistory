using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class ConfirmEmailViewFunctions
{
	[KernelFunction(Name = "kernel.configure_confirm_email_view")]
	internal static void ConfigureConfirmEmailView(bool confirm, string email, Action callback)
	{
		ConfirmEmailViewController.ConfigureView(confirm, email, callback);
	}
}
