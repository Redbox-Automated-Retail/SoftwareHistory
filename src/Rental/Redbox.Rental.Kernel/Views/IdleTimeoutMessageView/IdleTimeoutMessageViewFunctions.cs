using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.IdleTimeoutMessageView;

public class IdleTimeoutMessageViewFunctions
{
	[KernelFunction(Name = "kernel.configure_idle_timeout_message_view")]
	internal static void ConfigureIdleTimeoutMessageView(int timeout, Action noButtonAction, Action yesButtonAction)
	{
		IdleTimeoutMessageViewController.ConfigureIdleTimeoutMessageView(timeout, noButtonAction, yesButtonAction);
	}
}
