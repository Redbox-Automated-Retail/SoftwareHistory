using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class IdleTimeoutMessageViewController
{
	public static void ConfigureIdleTimeoutMessageView(int timeout, Action noButtonAction, Action yesButtonAction)
	{
		IActor actor = ActorFunctions.GetActor("IdleTimeoutMessageViewUserControl");
		if (actor == null || !(actor.WPFFrameworkElement is IdleTimeoutMessageViewUserControl idleTimeoutMessageViewUserControl))
		{
			return;
		}
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "more_time_prompt_message");
		string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "no_thanks_button");
		string dictionaryStringValue3 = BundleFunctions.GetDictionaryStringValue("strings", "yes_button");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			service["MessageTimeoutTitle"] = dictionaryStringValue;
			ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service2 != null && service2.TTSEnabled)
			{
				service["TTS:MessageViewTitle"] = dictionaryStringValue;
				service["TTS:MessageViewButton1Text"] = dictionaryStringValue2;
				service["TTS:MessageViewButton2Text"] = dictionaryStringValue3;
				service2.TTSRepeatSequence();
			}
		}
		idleTimeoutMessageViewUserControl.Message = dictionaryStringValue;
		idleTimeoutMessageViewUserControl.ButtonNoText = dictionaryStringValue2;
		idleTimeoutMessageViewUserControl.ButtonYesTest = dictionaryStringValue3;
		idleTimeoutMessageViewUserControl.OnButtonNoClicked += noButtonAction;
		idleTimeoutMessageViewUserControl.OnButtonYesClicked += yesButtonAction;
		idleTimeoutMessageViewUserControl.Timeout = timeout;
	}
}
