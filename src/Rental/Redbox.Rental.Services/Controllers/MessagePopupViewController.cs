using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Session;
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
		ShowMessagePopupView(titleText, null, messageText, null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "MatureWarning");
	}

	public static void ShowBlurayAlert(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Bluray Alert message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert_title"), null, BundleFunctions.GetDictionaryStringValue("strings", "blu_ray_alert"), null, BundleFunctions.GetDictionaryStringValue("strings", "back_button"), BundleFunctions.GetDictionaryStringValue("strings", "okay_button"), GetDefaultMessageTimeOut(), backAction, continueAction, timeOutAction, "BlurayAlert");
	}

	public static void ShowMessagePopupViewReturnSuccess(Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Return Success message popup");
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "title_thanks"), BundleFunctions.GetDictionaryStringValue("strings", "inventory_updated_message"), null, null, GetDefaultMessageTimeOut(5000, "short"), null, null, timeOutAction, "ViewReturnSuccess");
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
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.Push("message_popup_view");
			service.Show();
			ConfigureMessagePopupView(titleText, titleStyle, messageText, messageStyle, button1Text, button2Text, timeout, button1Action, button2Action, timeOutAction, actorTag);
		}
	}

	public static void ConfigureMessagePopupView(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag)
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
		messagePopupViewUserControl.Title = titleText;
		if (!string.IsNullOrWhiteSpace(titleStyle))
		{
			messagePopupViewUserControl.TitleStyle = titleStyle;
		}
		messagePopupViewUserControl.Message = messageText;
		if (!string.IsNullOrWhiteSpace(messageStyle))
		{
			messagePopupViewUserControl.MessageStyle = messageStyle;
		}
		messagePopupViewUserControl.Button1Text = button1Text;
		messagePopupViewUserControl.Button2Text = button2Text;
		messagePopupViewUserControl.Timeout = timeout;
		messagePopupViewUserControl.OnButton1Clicked += button1Action;
		messagePopupViewUserControl.OnButton2Clicked += button2Action;
		messagePopupViewUserControl.OnTimeOut += timeOutAction;
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null && service.Peek(1) != Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView)
		{
			messagePopupViewUserControl.BackgroundOpacity = 0.0;
		}
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
				service2["TTS:MessageViewButton3Text"] = string.Empty;
				service2["TTS:MessageViewButton4Text"] = string.Empty;
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
