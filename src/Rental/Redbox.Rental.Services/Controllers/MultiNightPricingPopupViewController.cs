using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class MultiNightPricingPopupViewController : BaseViewController
{
	public static void ConfigureMultiNightPricingPopupView(ITitleProduct titleProduct)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		OrchestrationService.AnalyticsAddViewEvent("context", "MNPDialog");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = new MultiNightPricingPopupViewModel();
			multiNightPricingPopupViewModel.TitleProduct = titleProduct;
			multiNightPricingPopupViewModel.HeaderText = GetHeaderText(titleProduct);
			multiNightPricingPopupViewModel.DescriptionText = Resources.multi_night_pricing_popup_description_text;
			IList<IPricingRecord> list = (multiNightPricingPopupViewModel.PricingRecords = ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(titleProduct.ProductId));
			bool flag = list != null && list.Count > 0;
			bool flag2 = list != null && list.Count > 1;
			multiNightPricingPopupViewModel.OneNightButtonVisibility = ((!flag) ? Visibility.Hidden : Visibility.Visible);
			multiNightPricingPopupViewModel.OneNightButtonText = (flag ? GetButtonText(list[0]) : string.Empty);
			multiNightPricingPopupViewModel.MultiNightButtonVisibility = ((!flag2) ? Visibility.Hidden : Visibility.Visible);
			multiNightPricingPopupViewModel.MultiNightButtonText = (flag2 ? GetButtonText(list[1]) : string.Empty);
			multiNightPricingPopupViewModel.CancelButtonText = Resources.multi_night_pricing_popup_cancel_button_text;
			multiNightPricingPopupViewModel.DisclaimerText = (flag2 ? GetDisclaimerText(list[1]) : string.Empty);
			multiNightPricingPopupViewModel.OnOneNightButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnOneNightButtonClicked, new Action(MultiNightPricingPopupViewModel_OnOneNightButtonClicked));
			multiNightPricingPopupViewModel.OnMultiNightButtonClicked = (Action)Delegate.Combine(multiNightPricingPopupViewModel.OnMultiNightButtonClicked, new Action(MultiNightPricingPopupViewModel_OnMultiNightButtonClicked));
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

	private static string GetHeaderText(ITitleProduct titleProduct)
	{
		return $"{titleProduct.ProductName}{((TitleTypeProperties.Properties.ContainsKey(titleProduct.TitleType) && TitleTypeProperties.Properties[titleProduct.TitleType].GetName(Resources.ResourceManager) != null) ? $" - {TitleTypeProperties.Properties[titleProduct.TitleType].GetName(Resources.ResourceManager)}" : string.Empty)}";
	}

	private static string GetButtonText(IPricingRecord pricingRecord)
	{
		return string.Format(Resources.multi_night_pricing_popup_button_text, pricingRecord.InitialNight, pricingRecord.InitialDays ?? 1, (pricingRecord.InitialDays > 1) ? "s" : string.Empty);
	}

	private static string GetDisclaimerText(IPricingRecord pricingRecord)
	{
		return string.Format(Resources.multi_night_pricing_popup_disclaimer_text, pricingRecord.ExtraNight);
	}

	private static void MultiNightPricingPopupViewModel_OnOneNightButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "oneNightRental");
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null && multiNightPricingPopupViewModel.PricingRecords != null && multiNightPricingPopupViewModel.PricingRecords.Count > 0)
		{
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: false, multiNightPricingPopupViewModel.PricingRecords[0]);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnMultiNightButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "multiNightRental");
		MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = GetMultiNightPricingPopupViewModel();
		if (multiNightPricingPopupViewModel != null && multiNightPricingPopupViewModel.TitleProduct != null && multiNightPricingPopupViewModel.PricingRecords != null && multiNightPricingPopupViewModel.PricingRecords.Count > 1)
		{
			BrowseFunctions.AddProductToCart(multiNightPricingPopupViewModel.TitleProduct, isPurchase: false, multiNightPricingPopupViewModel.PricingRecords[1]);
		}
	}

	private static void MultiNightPricingPopupViewModel_OnCancelButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "cancel");
		OrchestrationService.ViewGoBackSafe("MultiNightPricingPopupViewController cancel", popOnly: false);
	}
}
