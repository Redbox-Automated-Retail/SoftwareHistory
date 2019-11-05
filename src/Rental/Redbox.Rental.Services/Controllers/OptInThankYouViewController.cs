using System.Windows;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class OptInThankYouViewController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.SetCurrentUICulture();
			OptInThankYouViewModel optInThankYouViewModel = new OptInThankYouViewModel();
			optInThankYouViewModel.Image = Resources.opt_in_thank_you_view_image;
			viewUserControl.DataContext = optInThankYouViewModel;
			IIdleTimer idleTimer = OrchestrationService.CreateIdleTimer("optin_thank_you_timer");
			idleTimer.Initialize(8000, delegate
			{
				OrchestrationService.ViewGoBackSafe("optin_thank_you_view timer", popOnly: false);
				CheckoutFunctions.ContinueVend();
			});
			idleTimer.Start();
			if (OrchestrationService.InvokeIsVendJobCompletedCallback())
			{
				OrchestrationService.ViewGoBackSafe("optin_thank_you_view vend job already completed", popOnly: true);
			}
		}
	}

	public static void LeaveView()
	{
		OrchestrationService.StopIdleTimer("optin_thank_you_timer");
	}
}
