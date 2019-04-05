using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.IdleTimeoutMessageView;

public class IdleTimeoutMessageViewFunctions
{
	[KernelFunction(Name = "kernel.configure_idle_timeout_message_view")]
	internal static void ConfigureIdleTimeoutMessageView(Action noButtonAction)
	{
	}
}
