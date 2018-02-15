using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public static class ZipCodeViewFunctions
{
	[KernelFunction(Name = "kernel.configure_zip_code_view")]
	internal static void ConfigureZipCodeView(string note2LabelText, Action<string> nextAction)
	{
		ZipCodeViewController.ConfigureZipCodeView(note2LabelText, nextAction);
	}
}
