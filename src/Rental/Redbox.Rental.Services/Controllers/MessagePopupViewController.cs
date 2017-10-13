using System;
using System.Windows;
using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MessagePopupViewController
{
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
		ShowMessagePopupView(ServiceLocator.Instance.GetService<IMacroService>()["ADAPurchaseWarning:Header"], "font_target_not_as_large_overlays", null, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "ADAPurchaseWarning");
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
		ShowMessagePopupView(titleText, "font_target_not_as_large_overlays", messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "MatureWarning");
	}

	public static void ShowBlurayAlert(Action timeOutAction, Action backAction, Action continueAction, bool hideBackground)
	{
		LogHelper.Instance.Log("Showing Bluray Alert message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert_title"), "font_target_not_as_large_overlays", BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert"), null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), null, null, BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), null, null, GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "BlurayAlert", hideBackground, null);
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
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "vend_error_title"), "font_target_not_as_large_overlays", BundleFunctions.GetDictionaryStringValue("strings", "vend_error_message"), "font_target_smaller_overlays", null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction, "ViewVendError");
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
		});
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
		ShowMessagePopupView(titleText, titleStyle, messageText, messageStyle, button1Text, null, null, button2Text, null, null, timeout, button1Action, button2Action, timeOutAction, actorTag, hideBackground: false, null);
	}

	public static void ShowMessagePopupView(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button1TextStyle, string button1Style, string button2Text, string button2TextStyle, string button2Style, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, bool hideBackground, UserControl contentUserControl)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.Push("message_popup_view");
			service.Show();
			ConfigureMessagePopupView(titleText, titleStyle, messageText, messageStyle, button1Text, button1TextStyle, button1Style, button2Text, button2TextStyle, button2Style, timeout, button1Action, button2Action, timeOutAction, actorTag, hideBackground, contentUserControl);
		}
	}

	public static void ConfigureMessagePopupView(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button1TextStyle, string button1Style, string button2Text, string button2TextStyle, string button2Style, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, bool hideBackground, UserControl contentUserControl)
	{
		IActor actorByNameAndTag = ActorFunctions.GetActorByNameAndTag("MessagePopupViewUserControl", null);
		if (actorByNameAndTag == null)
		{
			return;
		}
		actorByNameAndTag.Tag = actorTag;
		if (!(actorByNameAndTag.WPFFrameworkElement is MessagePopupViewUserControl messagePopupViewUserControl))
		{
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		MessagePopupViewModel dataContext = new MessagePopupViewModel
		{
			Title = titleText,
			Message = messageText,
			BackgroundOpacity = ((hideBackground || (service?.Peek(1) != Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView && Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView != null)) ? 0.0 : 0.6),
			TitleStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, titleStyle ?? "font_target_large_overlays"),
			MessageStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, messageStyle ?? "font_target_small_overlays"),
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
			ContentUserControl = contentUserControl
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
		return ServiceLocator.Instance.GetService<ITextToSpeechService>()?.GetTimeout(timeoutType, defaultTimeout) ?? defaultTimeout;
	}
}
