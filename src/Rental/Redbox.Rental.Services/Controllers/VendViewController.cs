using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class VendViewController
{
	public static string GetAnimation()
	{
		return GetAnimation2().ToString();
	}

	public static VendViewType GetAnimation2()
	{
		VendViewType result = VendViewType.None;
		IActor actor = ActorFunctions.GetActor("VendViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is VendViewUserControl vendViewUserControl)
		{
			result = vendViewUserControl.VendType;
		}
		return result;
	}

	public static void ShowAnimation(string stringVendViewAnimationType)
	{
		VendViewType result = VendViewType.None;
		if (Enum.TryParse<VendViewType>(stringVendViewAnimationType, out result))
		{
			ShowAnimation2(result);
			return;
		}
		throw new ArgumentException("Invalid stringVendViewAnimationType parameter value: {0}", stringVendViewAnimationType);
	}

	public static void ShowAnimation2(VendViewType vendViewType)
	{
		IActor actor = ActorFunctions.GetActor("VendViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is VendViewUserControl vendViewUserControl)
		{
			vendViewUserControl.VendType = vendViewType;
			string titleText = null;
			string messageText = null;
			switch (vendViewType)
			{
			case VendViewType.PleaseWait:
				titleText = BundleFunctions.GetDictionaryStringValue("strings", "vending_disc_please_wait_title");
				messageText = BundleFunctions.GetDictionaryStringValue("strings", "vending_disc_please_wait_message");
				break;
			case VendViewType.RentalsOnTheirWay:
				titleText = BundleFunctions.GetDictionaryStringValue("strings", "vending_disc_rentals_on_their_way_title");
				break;
			case VendViewType.RecomendationOnPickupVend:
				titleText = BundleFunctions.GetDictionaryStringValue("strings", "vending_disc_rentals_on_their_way_charged_once_title");
				messageText = BundleFunctions.GetDictionaryStringValue("strings", "vending_disc_rentals_on_their_way_charged_once_message");
				break;
			}
			vendViewUserControl.TitleText = titleText;
			vendViewUserControl.MessageText = messageText;
		}
	}
}
