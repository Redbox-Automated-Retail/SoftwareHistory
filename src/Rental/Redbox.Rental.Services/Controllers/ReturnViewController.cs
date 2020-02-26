using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ReturnViewController : BaseViewController
{
	public static void ConfigureReturnView()
	{
		OrchestrationService.SuspendIdleTimer("idle_timer");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			IAd ad = null;
			if (!BaseViewController.ApplicationState.IsADAAccessible)
			{
				ad = ServiceLocator.Instance.GetService<IAdService>()?.GetScreenAd(AdLocation.Return);
			}
			ReturnViewModel returnViewModel = (ReturnViewModel)(viewUserControl.DataContext = new ReturnViewModel
			{
				MessageText = Resources.disc_return_message,
				AdImageSource = ad?.Image,
				AdImageVisibility = ((ad == null) ? Visibility.Collapsed : Visibility.Visible),
				AdImpression = ((ad != null) ? new AdImpression
				{
					Ad = ad,
					AdLocation = AdLocation.Return
				} : null)
			});
			returnViewModel.AdImpression?.Start();
		}
		else
		{
			LogHelper.Instance.Log("user control is not of type ReturnViewUserControl");
		}
	}

	public static void OnLeave(IViewFrameInstance viewFrameInstance)
	{
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<ReturnViewModel>(viewFrameInstance.Id)?.AdImpression);
	}
}
