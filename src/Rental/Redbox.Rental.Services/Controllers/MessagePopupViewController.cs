using System;
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

public class MessagePopupViewController
{
	public static void ShowSimpleMessage(string cultureSpecificTitle, string cultureSpecificMessage, string cultureSpecificButtonText, Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Simple Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(cultureSpecificTitle, "font_montserrat_medium_rbpurple", cultureSpecificMessage, "font_montserrat_small_rbpurple", null, null, null, cultureSpecificButtonText, "button_text_auto_size_style", "message_popup_warmpurple_auto_size_button_style", GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "SimpleMessage", hideBackground: false, null, "White", new Thickness(233.0, 208.0, 233.0, 208.0));
	}

	public static void ShowLogOutWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Log out warning message");
		ShowMessagePopupView(Resources.sign_out_dialog_title, "font_montserrat_medium_rbpurple", Resources.sign_out_dialog_message, "font_montserrat_small_rbpurple", Resources.back_button, "font_museoslab_900_28_rubine", "message_popup_white_rubine_to_warm_purple_border_button_with_dropshadow_style", Resources.sign_out_button, "font_museoslab_900_28_white", "message_popup_warmpurple_button_style", GetDefaultMessageTimeOut(30000, "long"), backAction, continueAction, timeOutAction, "SignOutWarningMessage", hideBackground: false, null, "Transparent", new Thickness(233.0, 208.0, 233.0, 208.0));
	}

	public static void ShowSimpleOkMessage(string cultureSpecificTitle, string cultureSpecificMessage, Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Ok Message: {0}", cultureSpecificTitle);
		ShowMessagePopupView(cultureSpecificTitle, "font_montserrat_medium_rbpurple", cultureSpecificMessage, "font_montserrat_small_rbpurple", null, null, null, Resources.okay, null, "message_popup_warmpurple_button_style", GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "OkMessage", hideBackground: false, null, "Transparent", new Thickness(233.0, 208.0, 233.0, 208.0));
	}

	public static void ShowLoyaltyIsDownMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down message");
		ShowMessagePopupView(Resources.personalization_service_error_KLI0005, "font_montserrat_medium_rbpurple", null, null, null, null, null, Resources.okay, null, "message_popup_warmpurple_button_style", GetDefaultMessageTimeOut(30000, "long"), null, continueAction, timeOutAction, "LoyaltyIsDownMessage", hideBackground: false, null, "White", new Thickness(233.0, 208.0, 233.0, 208.0));
	}

	public static void ShowInCartOfferMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer message");
		InCartOfferMessageText inCartOfferMessageText = PromotionService.Instance.GetInCartOfferMessageText();
		ShowMessagePopupView(inCartOfferMessageText.Header, "message_popup_dialog_title_style2", inCartOfferMessageText.Message, null, (PromotionService.Instance.IsFreeInCartOfferRunning || (PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance.ValidateRentTwoInCartPromo().IsCartValid)) ? null : inCartOfferMessageText.NoButtonText, null, null, inCartOfferMessageText.YesButtonText, null, null, GetDefaultMessageTimeOut(30000, "long"), backAction, continueAction, timeOutAction, "InCartOfferMessage", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowMessagePopupView(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, null, Resources.do_not_use_promo_button, "font_target_small_medium", null, Resources.use_promo_button, "font_target_small_medium", null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PromoCodeInvalidWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing title level promo code invalid warning");
		ShowMessagePopupView(Resources.tsp_invalid_header, "message_popup_dialog_title_style2", Resources.tsp_invalid_message, null, null, null, null, Resources.continue_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "TitleLevelPromoCodeInvalidWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowMessagePopupView(Resources.oops, null, message, null, Resources.do_not_use_promo_button, "font_target_small_medium", null, Resources.use_promo_button, "font_target_small_medium", null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PromoCodeInvalidWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing rxgx promo code invalid warning");
		ShowMessagePopupView(Resources.oops, null, message, null, null, null, null, Resources.continue_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeRxGxInvalidWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowMultiNightPricingPromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string title)
	{
		LogHelper.Instance.Log("Showing rxgx promo code invalid warning");
		ShowMessagePopupView(title, null, null, null, null, null, null, Resources.continue_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeMultiNightPricingInvalidWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowPromoCodeAlreadyUsedWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code already used warning");
		ShowMessagePopupView(Resources.promo_code_already_used_message_title, null, Resources.promo_code_already_used_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeAlreadyUsedWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowPromoCodeNotValidAtThisLocationWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid at this location warning");
		ShowMessagePopupView(Resources.promo_code_error_title, null, Resources.promo_code_not_valid_location_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeNotValidAtThisLocationWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowPromoCodeValidationFailureWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code validation failure warning");
		ShowMessagePopupView(Resources.promo_code_validation_failure_message_title, null, Resources.promo_code_validation_failure_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeValidationFailureWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowPromoCodeGameRequiredWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code game required warning");
		ShowMessagePopupView(Resources.promo_code_game_required_title, null, Resources.promo_code_game_required_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeGameRequiredWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowPromoCodeNotValidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid warning");
		ShowMessagePopupView(Resources.promo_code_not_valid_message_title, null, Resources.promo_code_not_valid_message, null, null, null, null, Resources.okay_button, null, null, GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "PromoCodeNotValidWarning", hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowInCartCancelWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer cancel warning message popup");
		ShowMessagePopupView(Resources.in_cart_remove_item_warning_header, null, Resources.in_cart_remove_item_warning_message, null, Resources.back_button, Resources.continue_button, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "InCartCancelWarning");
	}

	public static void ShowSubPlatformWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Sub Platform Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["Subplatform:Header"];
		string messageText = service["Subplatform:Message"];
		ShowMessagePopupView(titleText, null, messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "SubPlatformWarning");
	}

	public static void ShowADAPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing ADA Purchase Warning message popup");
		ShowMessagePopupView(ServiceLocator.Instance.GetService<IMacroService>()["ADAPurchaseWarning:Header"], "message_popup_dialog_title_style2", null, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "ADAPurchaseWarning");
	}

	public static void ShowPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Purchase Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["PurchaseWarning:Header"];
		string messageText = service["PurchaseWarning:Message"];
		ShowMessagePopupView(titleText, null, messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "PurchaseWarning");
	}

	public static void ShowMatureWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Mature Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string titleText = service["MatureWarning:Header"];
		string messageText = service["MatureWarning:Message"];
		ShowMessagePopupView(titleText, "message_popup_dialog_title_style2", messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "MatureWarning");
	}

	public static void ShowBlurayAlert(Action timeOutAction, Action backAction, Action continueAction, bool hideBackground)
	{
		LogHelper.Instance.Log("Showing Bluray Alert message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert_title"), "message_popup_dialog_title_style2", BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert"), null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), null, null, BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), null, null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "BlurayAlert", hideBackground, null, null, default(Thickness));
	}

	public static void ShowDigitalCodeAlert(Action backAction, Action okAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code Alert message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_alert_title"), BundleFunctions.GetDictionaryStringValue("strings", "digital_code_alert_message"), BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, okAction, timeOutAction, "DigitalCodeAlert");
	}

	public static void ShowMessagePopupViewReturnSuccess(Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Return Success message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "title_thanks"), BundleFunctions.GetDictionaryStringValue("strings", "inventory_updated_message"), null, null, GetDefaultMessageTimeOut(5000, "short"), null, null, timeOutAction, "ViewReturnSuccess");
	}

	public static void ShowMessageCartFull(Action timeoutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Cart Full message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "cart_full_header"), BundleFunctions.GetDictionaryStringValue("strings", "cart_full_body"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), OrchestrationService.GetIdleTimerTimeout(), null, continueAction, timeoutAction, "CartFull");
	}

	public static void ShowMessagePopupViewHardwareError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Hardware Error message popup");
		ShowMessagePopupView(null, BundleFunctions.GetDictionaryStringValue("strings", "hardware_error_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewHardwareError");
	}

	public static void ShowMessagePopupViewReturnBackwardInserted(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Backward Inserted message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "return_error"), BundleFunctions.GetDictionaryStringValue("strings", "backward_insert_error"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewReturnBackwardInserted");
	}

	public static void ShowMessagePopupViewReturnUnrecognizedBarcode(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Unrecognized Barcode message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "oops"), BundleFunctions.GetDictionaryStringValue("strings", "unrecognized_barcode_returned_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewReturnUnrecognizedBarcode");
	}

	public static void ShowMessagePopupViewCardNotRecognized(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized message popup");
		ShowMessagePopupView(null, BundleFunctions.GetDictionaryStringValue("strings", "invalid_card_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewCardNotRecognized");
	}

	public static void ShowMessagePopupViewVendError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Vend Error message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "vend_error_title"), "message_popup_dialog_title_style2", BundleFunctions.GetDictionaryStringValue("strings", "vend_error_message"), "message_popup_dialog_mesg_style2", null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewVendError");
	}

	public static void ShowMessagePopupViewDigitalCode(string price, Action backAction, Action buyAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_title"), null, BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), "font_target_smaller_overlays", null, string.Format(BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_buy_button"), price), "font_black_target_smaller_overlays", "classic_wide_yellow_button_style", GetDefaultMessageTimeOut(), backAction, buyAction, timeOutAction, "ViewDigitalCode", hideBackground: false, new DigitalCodePopupUserControl
		{
			DataContext = new
			{
				MessageLine1 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_1"),
				MessageLine2 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_2"),
				MessageLine3 = BundleFunctions.GetDictionaryStringValue("strings", "digital_code_popup_message_line_3")
			}
		}, null, default(Thickness));
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

	public static void ShowMessagePopupView(string titleText, string messageText, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag)
	{
		ShowMessagePopupView(titleText, null, messageText, null, button1Text, button2Text, timeout, button1Action, button2Action, timeOutAction, actorTag);
	}

	public static void ShowMessagePopupView(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag)
	{
		ShowMessagePopupView(titleText, titleStyle, messageText, messageStyle, button1Text, null, null, button2Text, null, null, timeout, button1Action, button2Action, timeOutAction, actorTag, hideBackground: false, null, null, default(Thickness));
	}

	public static void ShowMessagePopupView(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button1TextStyle, string button1Style, string button2Text, string button2TextStyle, string button2Style, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, bool hideBackground, UserControl contentUserControl, string borderBrushColor, Thickness mainGrifMargin)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.Push("message_popup_view");
			service.Show();
			ConfigureMessagePopupView(titleText, titleStyle, messageText, messageStyle, button1Text, button1TextStyle, button1Style, button2Text, button2TextStyle, button2Style, timeout, button1Action, button2Action, timeOutAction, actorTag, hideBackground, contentUserControl, borderBrushColor, mainGrifMargin);
		}
	}

	public static void ConfigureMessagePopupView(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button1TextStyle, string button1Style, string button2Text, string button2TextStyle, string button2Style, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, bool hideBackground, UserControl contentUserControl, string borderBrushColor, Thickness mainGridMargin)
	{
		IActor actorByNameAndTag = ActorFunctions.GetActorByNameAndTag("MessagePopupViewUserControl", null);
		if (actorByNameAndTag == null)
		{
			return;
		}
		actorByNameAndTag.Tag = actorTag;
		Thickness thickness = default(Thickness);
		if (!(actorByNameAndTag.WPFFrameworkElement is MessagePopupViewUserControl messagePopupViewUserControl))
		{
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		MessagePopupViewModel dataContext = new MessagePopupViewModel
		{
			Title = titleText,
			Message = messageText,
			BackgroundOpacity = ((hideBackground || (service?.Peek(1) != Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView && Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView != null && Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView != "multi_night_pricing_popup_view")) ? 0.0 : 0.6),
			TitleStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, titleStyle ?? "message_popup_dialog_title_style"),
			MessageStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, messageStyle ?? "message_popup_dialog_mesg_style"),
			Button1Text = button1Text,
			Button1TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, button1TextStyle ?? "button1_text_style"),
			Button1Style = WPFFunctions.GetStyle(messagePopupViewUserControl, button1Style ?? "button1_style"),
			Button1Visibility = (string.IsNullOrEmpty(button1Text) ? Visibility.Collapsed : Visibility.Visible),
			Button2Text = button2Text,
			Button2TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, button2TextStyle ?? "button2_text_style"),
			Button2Style = WPFFunctions.GetStyle(messagePopupViewUserControl, button2Style ?? "button2_style"),
			Button2Visibility = (string.IsNullOrEmpty(button2Text) ? Visibility.Collapsed : Visibility.Visible),
			TimeOut = timeout,
			OnButton1Clicked = button1Action,
			OnButton2Clicked = button2Action,
			OnTimeOut = timeOutAction,
			ContentUserControl = contentUserControl,
			BorderBrushColor = (string.IsNullOrEmpty(borderBrushColor) ? "#E42045" : borderBrushColor),
			MainGridMargin = (mainGridMargin.Equals(thickness) ? new Thickness(128.0, 96.0, 128.0, 96.0) : mainGridMargin)
		};
		messagePopupViewUserControl.DataContext = dataContext;
		LogHelper.Instance.Log("Setting MessagePopupView: Title:{0} Message:{1}", titleText, messageText);
		IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
		if (service2 != null)
		{
			ITextToSpeechService service3 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service3 != null && service3.TTSEnabled)
			{
				service2["TTS:MessageViewTitle"] = titleText;
				service2["TTS:MessageViewText"] = messageText;
				service2["TTS:MessageViewButton1Text"] = button1Text;
				service2["TTS:MessageViewButton2Text"] = button2Text;
				service3.TTSRepeatSequence();
			}
		}
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
