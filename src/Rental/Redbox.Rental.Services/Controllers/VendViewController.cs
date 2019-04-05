using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Marketing;
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
			return;
		}
		OrchestrationService.SuspendIdleTimer("idle_timer");
		if (!(BaseViewController.GetViewUserControl() is VendViewUserControl vendViewUserControl))
		{
			return;
		}
		vendViewUserControl.OnGetSpeechControl = delegate
		{
			ISpeechControl result = new SpeechControl
			{
				Name = "tts_vend_view"
			};
			AddSpeechPartMessage(result);
			result.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.SpeechControl = result;
			});
			return result;
		};
	}

	private static void AddSpeechPartMessage(ISpeechControl result)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Message",
			Loop = true,
			StartPause = 500,
			EndPause = 10000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Your rentals are on their way."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Thank you for using Redbox!"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Please retrieve your disc from the vending slot."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The vending slot is located on the far right."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Thanks!"
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	public static void LeaveVendView()
	{
		ServiceLocator.Instance.GetService<ICallbackService>()?.Flush();
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
			if (BaseViewController.GetViewUserControl() is VendViewUserControl vendViewUserControl)
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
