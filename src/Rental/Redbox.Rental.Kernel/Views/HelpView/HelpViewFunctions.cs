using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.HelpView;

public class HelpViewFunctions
{
	[KernelFunction(Name = "kernel.configure_help_view")]
	internal static void ConfigureHelpView(bool giftCardEnabled, Action goBackAction, Action clickRedboxLogoAction)
	{
		HelpViewController.ConfigureHelpView(giftCardEnabled, goBackAction, clickRedboxLogoAction);
	}
}
