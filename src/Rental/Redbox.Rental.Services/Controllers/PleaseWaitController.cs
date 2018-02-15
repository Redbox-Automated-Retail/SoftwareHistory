using System.Windows;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PleaseWaitController : BaseViewController
{
	public static void ConfigureView(ActionCallbackInfo callbackInfo)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			callbackInfo?.CallbackAction?.Invoke(viewUserControl);
		}
	}
}
