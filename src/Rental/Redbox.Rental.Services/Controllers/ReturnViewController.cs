using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReturnViewController
{
	public static void ConfigureReturnView(Action goBackAction)
	{
		IActor actor = ActorFunctions.GetActor("ReturnViewUserControl");
		if (actor != null)
		{
			if (actor.WPFFrameworkElement is ReturnViewUserControl returnViewUserControl)
			{
				returnViewUserControl.OnGoBack += goBackAction;
				string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "back_button");
				string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "disc_return_message");
				returnViewUserControl.BackButtonText = dictionaryStringValue;
				returnViewUserControl.MessageText = dictionaryStringValue2;
				BitmapImage adImage = null;
				MarketingAd marketingAd = ServiceLocator.Instance.GetService<IMarketingAdService>()?.GetScreenAd(MarketingAdLocation.Return, createImpression: true);
				if (marketingAd != null && marketingAd.Visible && !string.IsNullOrEmpty(marketingAd.ImageName))
				{
					adImage = ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetBitmapImage(marketingAd.ImageName);
				}
				returnViewUserControl.AdImage = adImage;
			}
			else
			{
				LogHelper.Instance.Log("actor is not of type ReturnViewUserControl");
			}
		}
		else
		{
			LogHelper.Instance.Log("Unable to find actor ReturnViewUserControl");
		}
	}

	public static void SetBackButtonIsVisible(bool isEnabled)
	{
		IActor actor = ActorFunctions.GetActor("ReturnViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is ReturnViewUserControl returnViewUserControl)
		{
			returnViewUserControl.BackButtonVisibility = ((!isEnabled) ? Visibility.Hidden : Visibility.Visible);
		}
	}
}
