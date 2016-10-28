using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MessagePopupViewController
{
	public static void ShowMessagePopupViewReturnSuccess(Action timeOutAction)
	{
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "title_thanks"), BundleFunctions.GetDictionaryStringValue("strings", "inventory_updated_message"), null, null, GetDefaultMessageTimeOut(5000, "short"), null, null, timeOutAction);
	}

	public static void ShowMessagePopupViewHardwareError(Action timeOutAction, Action continueAction)
	{
		ShowMessagePopupView(null, BundleFunctions.GetDictionaryStringValue("strings", "hardware_error_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction);
	}

	public static void ShowMessagePopupViewReturnBackwardInserted(Action timeOutAction, Action continueAction)
	{
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "return_error"), BundleFunctions.GetDictionaryStringValue("strings", "backward_insert_error"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction);
	}

	public static void ShowMessagePopupViewReturnUnrecognizedBarcode(Action timeOutAction, Action continueAction)
	{
		ShowMessagePopupView(BundleFunctions.GetDictionaryStringValue("strings", "Oops!"), BundleFunctions.GetDictionaryStringValue("strings", "unrecognized_barcode_returned_message"), null, BundleFunctions.GetDictionaryStringValue("strings", "continue_button"), GetDefaultMessageTimeOut(), null, continueAction, timeOutAction);
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

	public static void ShowMessagePopupView(string titleText, string messageText, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.Push("message_popup_view");
			service.Show();
			ConfigureMessagePopupView(titleText, messageText, button1Text, button2Text, timeout, button1Action, button2Action, timeOutAction);
		}
	}

	public static void ConfigureMessagePopupView(string titleText, string messageText, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction)
	{
		IActor actor = ActorFunctions.GetActor("MessagePopupViewUserControl");
		if (actor == null || !(actor.WPFFrameworkElement is MessagePopupViewUserControl messagePopupViewUserControl))
		{
			return;
		}
		messagePopupViewUserControl.Title = titleText;
		messagePopupViewUserControl.Message = messageText;
		messagePopupViewUserControl.Button1Text = button1Text;
		messagePopupViewUserControl.Button2Text = button2Text;
		messagePopupViewUserControl.Timeout = timeout;
		messagePopupViewUserControl.OnButton1Clicked += button1Action;
		messagePopupViewUserControl.OnButton2Clicked += button2Action;
		messagePopupViewUserControl.OnTimeOut += timeOutAction;
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service2 != null && service2.TTSEnabled)
			{
				service["TTS:MessageViewTitle"] = titleText;
				service["TTS:MessageViewText"] = messageText;
				service["TTS:MessageViewButton1Text"] = button1Text;
				service["TTS:MessageViewButton2Text"] = button2Text;
				service["TTS:MessageViewButton3Text"] = string.Empty;
				service["TTS:MessageViewButton4Text"] = string.Empty;
				service2.TTSRepeatSequence();
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
