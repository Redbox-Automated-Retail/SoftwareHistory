using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReturnViewController : BaseViewController
{
	public static void ConfigureReturnView(Action goBackAction)
	{
		OrchestrationService.SuspendIdleTimer("idle_timer");
		if (BaseViewController.GetViewUserControl() is ReturnViewUserControl returnViewUserControl)
		{
			returnViewUserControl.OnGoBack += goBackAction;
			_ = Resources.back_button;
			string disc_return_message = Resources.disc_return_message;
			returnViewUserControl.MessageText = disc_return_message;
			BitmapImage adImage = null;
			MarketingAd marketingAd = ServiceLocator.Instance.GetService<IMarketingAdService>()?.GetScreenAd(MarketingAdLocation.Return, createImpression: true);
			if (marketingAd != null && marketingAd.Visible && !string.IsNullOrEmpty(marketingAd.ImageName))
			{
				Image image = ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetBitmap(marketingAd.ImageName);
				adImage = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
			}
			returnViewUserControl.AdImage = adImage;
		}
		else
		{
			LogHelper.Instance.Log("user control is not of type ReturnViewUserControl");
		}
	}

	public static void SetBackButtonIsVisible(bool isEnabled)
	{
		_ = BaseViewController.GetViewUserControl() is ReturnViewUserControl;
	}
}
