using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class VendViewController : BaseViewController
{
	public static void ConfigureVendView()
	{
		if (OrchestrationService.InvokeIsVendJobCompletedCallback())
		{
			LogHelper.Instance.Log("Vend View not shown: Vend job already complete.");
			OrchestrationService.ViewGoBack("Vend screen: Vend job finished.", popOnly: false);
		}
		else
		{
			OrchestrationService.SuspendIdleTimer("idle_timer");
		}
	}

	public static void LeaveVendView(IViewFrameInstance viewFrameInstance)
	{
		ServiceLocator.Instance.GetService<ICallbackService>()?.Flush();
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<VendViewModel>(viewFrameInstance.Id)?.AdImpression);
	}

	public static string GetAnimation()
	{
		VendViewType vendViewType = VendViewType.None;
		if (BaseViewController.GetViewUserControl() is VendViewUserControl vendViewUserControl)
		{
			vendViewType = vendViewUserControl.VendType;
		}
		return vendViewType.ToString();
	}

	public static void ShowAnimation(string stringVendViewAnimationType)
	{
		VendViewType result = VendViewType.None;
		if (Enum.TryParse<VendViewType>(stringVendViewAnimationType, out result))
		{
			VendViewUserControl vendViewUserControl = BaseViewController.GetViewUserControl() as VendViewUserControl;
			if (vendViewUserControl?.DataContext is VendViewModel vendViewModel)
			{
				BaseViewController.RecordAdImpression(vendViewModel?.AdImpression);
			}
			if (vendViewUserControl != null)
			{
				VendViewModel vendViewModel2 = new VendViewModel();
				OrchestrationService.SetCurrentUICulture();
				IAd ad = ((!BaseViewController.ApplicationState.IsADAAccessible) ? BaseViewController.GetAd(AdLocation.Vending) : null);
				string text = string.Empty;
				string text2 = string.Empty;
				vendViewModel2.IsPickup = BaseViewController.GetPickupStatus();
				switch (result)
				{
				case VendViewType.PleaseWait:
					text = Resources.vending_disc_please_wait_title;
					text2 = Resources.vending_disc_please_wait_message;
					break;
				case VendViewType.RentalsOnTheirWay:
					text = Resources.vending_disc_rentals_on_their_way_title;
					text2 = "";
					break;
				case VendViewType.RecomendationOnPickupVend:
					text = Resources.vending_disc_rentals_on_their_way_charged_once_title;
					text2 = Resources.vending_disc_rentals_on_their_way_charged_once_message;
					break;
				}
				vendViewModel2.IsSignedIn = PersonalizationFunctions.IsSignedIn;
				vendViewModel2.IsLoyaltySignedIn = PersonalizationFunctions.IsLoyaltySignedIn;
				vendViewModel2.TextMessages = new string[5]
				{
					text,
					text2,
					Resources.vending_racking_points,
					Resources.vending_signing_out,
					Resources.vending_serving_discs
				};
				vendViewModel2.VendType = result;
				vendViewModel2.BannerImage = ad?.Image;
				vendViewModel2.AdImpression = ((ad != null) ? new AdImpression
				{
					Ad = ad,
					AdLocation = AdLocation.Vending
				} : null);
				vendViewUserControl.DataContext = vendViewModel2;
				vendViewModel2.AdImpression?.Start();
			}
			return;
		}
		throw new ArgumentException("Invalid stringVendViewAnimationType parameter value: {0}", stringVendViewAnimationType);
	}
}
