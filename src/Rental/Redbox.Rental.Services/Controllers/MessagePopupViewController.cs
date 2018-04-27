using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MessagePopupViewController : BaseViewController
{
	public static void ShowRecommendationOnPickupPrompt(Action timeOutAction, Action pickupAction, Action continueBrowsingAction)
	{
		LogHelper.Instance.Log("Showing Recommendation On Pickup prompt");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.reco_need_more_time, (string)null, (string)null, (string)null, Resources.pickup, (string)null, (string)null, $"{Resources.keep_browsing_1} {Resources.keep_browsing_2}", "font_target_small_medium", (string)null, GetDefaultMessageTimeOut(), pickupAction, continueBrowsingAction, timeOutAction, "RecommendationOnPickupPrompt", false, (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowSimpleMessage(string cultureSpecificTitle, string cultureSpecificMessage, string cultureSpecificButtonText, Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Simple Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(cultureSpecificTitle, "font_montserrat_medium_rbpurple", cultureSpecificMessage, "font_montserrat_small_rbpurple", (string)null, (string)null, (string)null, cultureSpecificButtonText, "button_text_auto_size_style", "message_popup_warmpurple_auto_size_button_style", GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "SimpleMessage", false, (UserControl)null, "White", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowLogOutWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Log out warning message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.sign_out_dialog_title, "font_montserrat_medium_rbpurple", Resources.sign_out_dialog_message, "font_montserrat_small_rbpurple", Resources.back_button, "font_museoslab_900_28_rubine", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.sign_out_button, "font_museoslab_900_28_white", "message_popup_warmpurple_button_style", GetDefaultMessageTimeOut(30000, "long"), backAction, continueAction, timeOutAction, "SignOutWarningMessage", false, (UserControl)null, "Transparent", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowSimpleOkMessage(string cultureSpecificTitle, string cultureSpecificMessage, Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Ok Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(cultureSpecificTitle, "font_montserrat_medium_rbpurple", cultureSpecificMessage, "font_montserrat_small_rbpurple", (string)null, (string)null, (string)null, Resources.okay, (string)null, "message_popup_warmpurple_button_style", GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "OkMessage", false, (UserControl)null, "Transparent", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowLoyaltyIsDownMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down message");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.personalization_service_error_KLI0005, "font_montserrat_medium_rbpurple", (string)null, (string)null, (string)null, (string)null, (string)null, Resources.okay, (string)null, "message_popup_warmpurple_button_style", GetDefaultMessageTimeOut(30000, "long"), (Action)null, continueAction, timeOutAction, "LoyaltyIsDownMessage", false, (UserControl)null, "White", new Thickness(233.0, 208.0, 233.0, 208.0), (string)null, (string)null));
	}

	public static void ShowInCartOfferMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer message");
		InCartOfferMessageText inCartOfferMessageText = PromotionService.Instance.GetInCartOfferMessageText();
		ShowMessagePopupView(new ShowMessagePopupViewParameter(inCartOfferMessageText.Header, "message_popup_dialog_title_style2", inCartOfferMessageText.Message, (string)null, (PromotionService.Instance.IsFreeInCartOfferRunning || (PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance.ValidateRentTwoInCartPromo().IsCartValid)) ? null : inCartOfferMessageText.NoButtonText, (string)null, (string)null, inCartOfferMessageText.YesButtonText, (string)null, (string)null, GetDefaultMessageTimeOut(30000, "long"), backAction, continueAction, timeOutAction, "InCartOfferMessage", false, (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, (string)null, Resources.do_not_use_promo_button, "font_target_small_medium", (string)null, Resources.use_promo_button, "font_target_small_medium", (string)null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PromoCodeInvalidWarning", false, (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing title level promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, null, null, null, null, Resources.continue_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "TitleLevelPromoCodeInvalidWarning", hideBackground: false, null, null, default(Thickness), "titlelevelpromoWarning"));
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.oops, null, message, null, Resources.do_not_use_promo_button, "font_target_small_medium", null, Resources.use_promo_button, "font_target_small_medium", null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PromoCodeInvalidWarning", hideBackground: false, null, null, default(Thickness), "rxgxWarning"));
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing rxgx promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.oops, null, message, null, null, null, null, Resources.continue_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeRxGxInvalidWarning", hideBackground: false, null, null, default(Thickness), "rxgxWarning"));
	}

	public static void ShowMultiNightPricingPromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string title)
	{
		LogHelper.Instance.Log("Showing rxgx promo code invalid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(title, (string)null, (string)null, (string)null, (string)null, (string)null, (string)null, Resources.continue_button, (string)null, (string)null, GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "PromoCodeMultiNightPricingInvalidWarning", false, (UserControl)null, (string)null, default(Thickness), (string)null, (string)null));
	}

	public static void ShowPromoCodeAlreadyUsedWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code already used warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_already_used_message_title, null, Resources.promo_code_already_used_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeAlreadyUsedWarning", hideBackground: false, null, null, default(Thickness), "promoAlreadyUsed"));
	}

	public static void ShowPromoCodeNotValidAtThisLocationWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid at this location warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_error_title, null, Resources.promo_code_not_valid_location_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeNotValidAtThisLocationWarning", hideBackground: false, null, null, default(Thickness), "promoInvalidLocation"));
	}

	public static void ShowPromoCodeValidationFailureWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code validation failure warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_validation_failure_message_title, null, Resources.promo_code_validation_failure_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeValidationFailureWarning", hideBackground: false, null, null, default(Thickness), "promoCantBeUsed"));
	}

	public static void ShowPromoCodeGameRequiredWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code game required warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_game_required_title, null, Resources.promo_code_game_required_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeGameRequiredWarning", hideBackground: false, null, null, default(Thickness), "promoCantBeUsed"));
	}

	public static void ShowPromoCodeNotValidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid warning");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.promo_code_not_valid_message_title, null, Resources.promo_code_not_valid_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeNotValidWarning", hideBackground: false, null, null, default(Thickness), "promoNotValid"));
	}

	public static void ShowInCartCancelWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer cancel warning message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.in_cart_remove_item_warning_header, (string)null, Resources.in_cart_remove_item_warning_message, (string)null, Resources.back_button, Resources.continue_button, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "InCartCancelWarning", (string)null));
	}

	public static void ShowSubPlatformWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Sub Platform Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["Subplatform:Header"];
		string messageText = service["Subplatform:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, null, messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "SubPlatformWarning", "subplatformWarning"));
	}

	public static void ShowADAPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing ADA Purchase Warning message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(ServiceLocator.Instance.GetService<IMacroService>()["ADAPurchaseWarning:Header"], "message_popup_dialog_title_style2", (string)null, (string)null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "ADAPurchaseWarning", (string)null));
	}

	public static void ShowPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Purchase Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["PurchaseWarning:Header"];
		string messageText = service["PurchaseWarning:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, null, messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PurchaseWarning", "purchaseWarning"));
	}

	public static void ShowMatureWarning(Action timeOutAction, Action backAction, Action continueAction, List<string> analyticsContexts)
	{
		LogHelper.Instance.Log("Showing Mature Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["MatureWarning:Header"];
		string messageText = service["MatureWarning:Message"];
		ShowMessagePopupView(new ShowMessagePopupViewParameter(titleText, "message_popup_dialog_title_style2", messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "MatureWarning", analyticsContexts));
	}

	public static void ShowBlurayAlert(Action timeOutAction, Action backAction, Action continueAction, bool hideBackground)
	{
		LogHelper.Instance.Log("Showing Bluray Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert_title"), "message_popup_dialog_title_style2", BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert"), null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), null, null, BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), null, null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "BlurayAlert", hideBackground, null, null, default(Thickness), "blurayWarning"));
	}

	public static void Show4KAlert(Action timeOutAction, Action backAction, Action continueAction, bool hideBackground, string analyticsContext)
	{
		LogHelper.Instance.Log("Showing 4K UHD Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(Resources.four_k_alert_title, "message_popup_dialog_title_style3", Resources.four_k_alert, "message_popup_dialog_mesg_style3", Resources.cancel_button, "font_museo_slab_900_20", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.got_it_button, "font_museo_slab_900_20", "message_popup_warmpurple_button_style", GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "4KUHDAlert", hideBackground, null, "White", new Thickness(180.0, 192.0, 180.0, 192.0), analyticsContext, "4K_warning_popup_view"));
	}

	public static void ShowDigitalCodeAlert(Action backAction, Action okAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code Alert message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_alert_title"), BundleFunctions.GetDictionaryStringValue("strings", "digital_code_alert_message"), BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, okAction, timeOutAction, "DigitalCodeAlert", "digitalcodeincart_message_view"));
	}

	public static void ShowMessagePopupViewReturnSuccess(Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Return Success message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "title_thanks"), BundleFunctions.GetDictionaryStringValue("strings", "inventory_updated_message"), (string)null, (string)null, GetDefaultMessageTimeOut(5000, "short"), (Action)null, (Action)null, timeOutAction, "ViewReturnSuccess", (string)null));
	}

	public static void ShowMessageCartFull(Action timeoutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Cart Full message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "cart_full_header"), BundleFunctions.GetDictionaryStringValue("strings", "cart_full_body"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), OrchestrationService.GetIdleTimerTimeout(), (Action)null, continueAction, timeoutAction, "CartFull", (string)null));
	}

	public static void ShowMessagePopupViewHardwareError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Hardware Error message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter((string)null, BundleFunctions.GetDictionaryStringValue("strings", "hardware_error_message"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewHardwareError", (string)null));
	}

	public static void ShowMessagePopupViewReturnBackwardInserted(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Backward Inserted message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "return_error"), BundleFunctions.GetDictionaryStringValue("strings", "backward_insert_error"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewReturnBackwardInserted", (string)null));
	}

	public static void ShowMessagePopupViewReturnUnrecognizedBarcode(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Unrecognized Barcode message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "oops"), BundleFunctions.GetDictionaryStringValue("strings", "unrecognized_barcode_returned_message"), (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewReturnUnrecognizedBarcode", (string)null));
	}

	public static void ShowMessagePopupViewCardNotRecognized(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(null, BundleFunctions.GetDictionaryStringValue("strings", "invalid_card_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewCardNotRecognized", "cardNotRecognized"));
	}

	public static void ShowMessagePopupViewVendError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Vend Error message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "vend_error_title"), "message_popup_dialog_title_style2", BundleFunctions.GetDictionaryStringValue("strings", "vend_error_message"), "message_popup_dialog_mesg_style2", (string)null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), (Action)null, continueAction, timeOutAction, "ViewVendError", (string)null));
	}

	public static void ShowMessagePopupViewDigitalCode(string price, Action backAction, Action buyAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code message popup");
		ShowMessagePopupView(new ShowMessagePopupViewParameter(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_title"), null, BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), "font_target_smaller_overlays", null, string.Format(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_buy_button"), price), "font_black_target_smaller_overlays", "classic_wide_yellow_button_style", GetDefaultMessageTimeOut(), backAction, buyAction, timeOutAction, "ViewDigitalCode", hideBackground: false, new DigitalCodePopupUserControl
		{
			DataContext = new
			{
				MessageLine1 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_1"),
				MessageLine2 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_2"),
				MessageLine3 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_3")
			}
		}, null, default(Thickness), "whatdigitalcode_message_view"));
	}

	public static void HideMessagePopupView(bool popOnly)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null && service.Peek() == "message_popup_view")
		{
			MessagePopupViewStopTimer();
			service.GoBackSafe("messageviewmanager:hide()", popOnly);
		}
	}

	public static bool IsMessagePopupViewShowing()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			return service.Peek() == "message_popup_view";
		}
		return false;
	}

	public static void MessagePopupViewStopTimer()
	{
		IActor actor = ActorFunctions.GetActor("MessagePopupViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is MessagePopupViewUserControl messagePopupViewUserControl)
		{
			messagePopupViewUserControl.StopTimer();
		}
	}

	public static void ShowMessagePopupView(ShowMessagePopupViewParameter parameter)
	{
		OrchestrationService.ViewShow("message_popup_view", parameter);
	}

	public static void ConfigureMessagePopupView(ShowMessagePopupViewParameter parameters)
	{
		if (parameters.AnalyticsContextList != null)
		{
			foreach (string analyticsContext in parameters.AnalyticsContextList)
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("context", analyticsContext);
			}
		}
		Thickness thickness = default(Thickness);
		MessagePopupViewUserControl messagePopupViewUserControl = (MessagePopupViewUserControl)BaseViewController.GetViewUserControl();
		if (messagePopupViewUserControl != null)
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			MessagePopupViewModel viewModel = (MessagePopupViewModel)(messagePopupViewUserControl.DataContext = new MessagePopupViewModel
			{
				Title = parameters.TitleText,
				Message = parameters.MessageText,
				BackgroundOpacity = ((parameters.HideBackground || (service?.Peek(1) != Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView && Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView != null && Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView != "multi_night_pricing_popup_view")) ? 0.0 : 0.6),
				TitleStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.TitleStyle ?? "message_popup_dialog_title_style"),
				MessageStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.MessageStyle ?? "message_popup_dialog_mesg_style"),
				Button1Text = parameters.Button1Text,
				Button1TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1TextStyle ?? "button1_text_style"),
				Button1Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1Style ?? "button1_style"),
				Button1Visibility = (string.IsNullOrEmpty(parameters.Button1Text) ? Visibility.Collapsed : Visibility.Visible),
				Button2Text = parameters.Button2Text,
				Button2TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2TextStyle ?? "button2_text_style"),
				Button2Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2Style ?? "button2_style"),
				Button2Visibility = (string.IsNullOrEmpty(parameters.Button2Text) ? Visibility.Collapsed : Visibility.Visible),
				TimeOut = parameters.Timeout,
				OnButton1Clicked = parameters.Button1Action,
				OnButton2Clicked = parameters.Button2Action,
				OnTimeOut = parameters.TimeOutAction,
				ContentUserControl = parameters.ContentUserControl,
				BorderBrushColor = (string.IsNullOrEmpty(parameters.BorderBrushColor) ? "#E42045" : parameters.BorderBrushColor),
				MainGridMargin = (parameters.MainGridMargin.Equals(thickness) ? new Thickness(128.0, 96.0, 128.0, 96.0) : parameters.MainGridMargin)
			});
			LogHelper.Instance.Log("Setting MessagePopupView: Title:{0} Message:{1}", parameters.TitleText, parameters.MessageText);
			ConfigureTTS(viewModel);
		}
	}

	private static void ConfigureTTS(MessagePopupViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => MessagePopupViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static MessagePopupViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as MessagePopupViewModel;
		}
		return null;
	}

	private static int GetDefaultMessageTimeOut()
	{
		return GetDefaultMessageTimeOut(15000, "medium");
	}

	private static int GetDefaultMessageTimeOut(int defaultTimeout, string timeoutType)
	{
		return OrchestrationService.TTSGetTimeout(timeoutType, defaultTimeout);
	}
}
