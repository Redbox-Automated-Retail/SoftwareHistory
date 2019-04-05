using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MultiNightPricingPopupViewController : BaseViewController
{
	public static void ConfigureMultiNightPricingPopupView(MultiNightPricingPopupInfo multiNightPricingPopupInfo)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		OrchestrationService.AnalyticsAddViewEvent("context", "shownFromView," + multiNightPricingPopupInfo.ShownFromView);
		OrchestrationService.AnalyticsAddViewEvent("context", "productId," + multiNightPricingPopupInfo.TitleProduct.ProductId);
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = new MultiNightPricingPopupViewModel();
			multiNightPricingPopupViewModel.TitleProduct = multiNightPricingPopupInfo.TitleProduct;
			bool flag = ShowBuyButton(multiNightPricingPopupInfo.ShownFromView, multiNightPricingPopupInfo.TitleProduct);
			multiNightPricingPopupViewModel.HeaderText = multiNightPricingPopupInfo.TitleProduct.ProductName;
			multiNightPricingPopupViewModel.DescriptionText = (flag ? Resources.multi_night_pricing_popup_description_b_text : Resources.multi_night_pricing_popup_description_text);
			IList<IPricingRecord> list = (multiNightPricingPopupViewModel.PricingRecords = ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(multiNightPricingPopupInfo.TitleProduct.ProductId));
			bool flag2 = list != null && list.Count > 0;
			bool flag3 = list != null && list.Count > 1;
			multiNightPricingPopupViewModel.OneNightButtonVisibility = ((!flag2) ? Visibility.Collapsed : Visibility.Visible);
			multiNightPricingPopupViewModel.OneNightButtonText = (flag2 ? GetButtonText(list[0]) : string.Empty);
			if (flag2)
			{
				OrchestrationService.AnalyticsAddViewEvent("context", "oneNightPrice," + list[0].InitialNight + ",initialDays," + (list[0].InitialDays ?? 1));
			}
			multiNightPricingPopupViewModel.MultiNightButtonVisibility = ((!flag3) ? Visibility.Collapsed : Visibility.Visible);
			multiNightPricingPopupViewModel.MultiNightButtonText = (flag3 ? GetButtonText(list[1]) : string.Empty);
			if (flag3)
			{
				OrchestrationService.AnalyticsAddViewEvent("context", "multiNightPrice," + list[1].InitialNight + ",initialDays," + (list[1].InitialDays ?? 1));
			}
			multiNightPricingPopupViewModel.BuyButtonVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
			multiNightPricingPopupViewModel.BuyButtonText = (flag ? GetBuyButtonText(multiNightPricingPopupInfo.TitleProduct) : string.Empty);
			if (flag)
			{
				OrchestrationService.AnalyticsAddViewEvent("context", "purchasePrice," + multiNightPricingPopupInfo.TitleProduct.PurchasePrice);
			}
			multiNightPricingPopupViewModel.CancelButtonText = Resources.multi_night_pricing_popup_cancel_button_text;
			multiNightPricingPopupViewModel.DisclaimerText = (((Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "C" || (Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "D" && !flag3)) & flag) ? GetDisclaimerText(list[0], flag, flag3) : (flag3 ? GetDisclaimerText(list[1], flag, flag3) : string.Empty));
			multiNightPricingPopupViewModel.OnOneNightButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnOneNightButtonClicked, new Action(MultiNightPricingPopupViewModel_OnOneNightButtonClicked));
			multiNightPricingPopupViewModel.OnMultiNightButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnMultiNightButtonClicked, new Action(MultiNightPricingPopupViewModel_OnMultiNightButtonClicked));
			multiNightPricingPopupViewModel.OnBuyButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnBuyButtonClicked, new Action(MultiNightPricingPopupViewModel_OnBuyButtonClicked));
			multiNightPricingPopupViewModel.OnCancelButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnCancelButtonClicked, new Action(MultiNightPricingPopupViewModel_OnCancelButtonClicked));
			ConfigureTTS(multiNightPricingPopupViewModel);
			viewUserControl.DataContext = multiNightPricingPopupViewModel;
		}
	}

	public static void LeaveMultiNightPricingPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static MultiNightPricingPopupViewModel GetMultiNightPricingPopupViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as MultiNightPricingPopupViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(MultiNightPricingPopupViewModel multiNightPricingPopupViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			multiNightPricingPopupViewModel.OnGetSpeechControl = () => MultiNightPricingPopupViewTextToSpeech.GetSpeechControl(GetMultiNightPricingPopupViewModel);
		}
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int value = OrchestrationService.TTSGetTimeout("medium", 15000);
		(service?.CreateTimer("MultiNightPricingPopupViewTimer", value, value, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "timeOut");
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("MultiNightPricingPopupViewTimer"))?.Stop();
		service?.RemoveTimer("MultiNightPricingPopupViewTimer");
	}

	private static bool ShowBuyButton(string shownFromView, ITitleProduct titleProduct)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "B" || (Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "C" && !titleProduct.HasMultiNightPrice) || Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "D")
		{
			switch (shownFromView)
			{
			case "start_view":
			case "browse_view":
			case "product_detail_view":
			case "purchase_popup_view":
				if (titleProduct.IsSellableAndInStock)
				{
					return titleProduct.IsMovie;
				}
				break;
			}
		}
		return false;
	}

	private static string GetButtonText(IPricingRecord pricingRecord)
	{
		return string.Format(Resources.multi_night_pricing_popup_button_text, pricingRecord.InitialNight, pricingRecord.InitialDays ?? 1, (pricingRecord.InitialDays > 1) ? "s" : string.Empty);
	}

	private static string GetBuyButtonText(ITitleProduct titleProduct)
	{
		return $"${titleProduct.PurchasePrice:F2} {Resources.multi_night_pricing_popup_buy_button_text}";
	}

	private static string GetDisclaimerText(IPricingRecord pricingRecord, bool showBuyButton, bool hasMultiNight)
	{
		if ((Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "B") & showBuyButton)
		{
			return Resources.multi_night_pricing_popup_disclaimer_b_text;
		}
		if ((Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "C") & showBuyButton)
		{
			return string.Format(Resources.multi_night_pricing_popup_disclaimer_c_text, pricingRecord.ExtraNight);
		}
		if ((Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "D") & showBuyButton & hasMultiNight)
		{
			return Resources.multi_night_pricing_popup_disclaimer_b_text;
		}
		if (((Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "D") & showBuyButton) && !hasMultiNight)
		{
			return string.Format(Resources.multi_night_pricing_popup_disclaimer_c_text, pricingRecord.ExtraNight);
		}
		return string.Format(Resources.multi_night_pricing_popup_disclaimer_text, pricingRecord.ExtraNight);
	}

	private static void MultiNightPricingPopupViewModel_OnOneNightButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "oneNightRental");
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null && multiNightPricingPopupViewModel.PricingRecords != null && multiNightPricingPopupViewModel.PricingRecords.Count > 0)
		{
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController oneNightRental", popOnly: false);
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: false, multiNightPricingPopupViewModel.PricingRecords[0]);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnMultiNightButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "multiNightRental");
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null && multiNightPricingPopupViewModel.PricingRecords != null && multiNightPricingPopupViewModel.PricingRecords.Count > 1)
		{
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController multiNightRental", popOnly: false);
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: false, multiNightPricingPopupViewModel.PricingRecords[1]);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnBuyButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "purchase");
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null)
		{
			OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController buy", popOnly: false);
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: true);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnCancelButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "cancel");
		OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController cancel", popOnly: false);
	}
}
