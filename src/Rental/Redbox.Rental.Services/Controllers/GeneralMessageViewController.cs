using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class GeneralMessageViewController : BaseViewController
{
	public static void ConfigureMessageView(GeneralMessageViewModel messageViewModel)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			viewUserControl.DataContext = messageViewModel;
			if (messageViewModel != null && messageViewModel.DisableIdleTimer)
			{
				(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer("idle_timer"))?.Suspend();
			}
		}
	}

	public static void LeaveMessageView(GeneralMessageViewModel messageViewModel)
	{
		if (messageViewModel != null && messageViewModel.DisableIdleTimer)
		{
			(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer("idle_timer"))?.Resume();
		}
	}
}
