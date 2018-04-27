using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class VendViewController : BaseViewController
{
	public static string GetAnimation()
	{
		VendViewType vendViewType = VendViewType.None;
		IActor actor = ActorFunctions.GetActor("VendViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is VendViewUserControl vendViewUserControl)
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
			IActor actor = ActorFunctions.GetActor("VendViewUserControl");
			if (actor != null && actor.WPFFrameworkElement is VendViewUserControl vendViewUserControl)
			{
				string text = string.Empty;
				string text2 = string.Empty;
				vendViewUserControl.IsPickup = BaseViewController.GetPickupStatus();
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
				MarketingAd marketingAd = BaseViewController.GetMarketingAd(MarketingAdLocation.Pickup, createImpression: true);
				vendViewUserControl.BannerImage = ((marketingAd != null && marketingAd.Visible) ? BaseViewController.GetBundleBitmapImage(marketingAd.ImageName) : null);
				vendViewUserControl.IsSignedIn = PersonalizationFunctions.IsSignedIn;
				vendViewUserControl.IsLoyaltySignedIn = PersonalizationFunctions.IsLoyaltySignedIn;
				vendViewUserControl.TextMessages = new string[5]
				{
					text,
					text2,
					Resources.vending_racking_points,
					Resources.vending_signing_out,
					Resources.vending_serving_discs
				};
				vendViewUserControl.VendType = result;
			}
			return;
		}
		throw new ArgumentException("Invalid stringVendViewAnimationType parameter value: {0}", stringVendViewAnimationType);
	}
}
