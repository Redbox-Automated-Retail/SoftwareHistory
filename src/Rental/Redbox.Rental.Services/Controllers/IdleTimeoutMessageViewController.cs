using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class IdleTimeoutMessageViewController
{
	public static void ConfigureIdleTimeoutMessageView(Action noButtonAction)
	{
		IActor actor = ActorFunctions.GetActor("IdleTimeoutMessageViewUserControl");
		if (actor == null)
		{
			return;
		}
		IdleTimeoutMessageViewUserControl idleTimeoutMessageViewUserControl = actor.WPFFrameworkElement as IdleTimeoutMessageViewUserControl;
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (idleTimeoutMessageViewUserControl == null)
		{
			return;
		}
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "more_time_prompt_message");
		string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "no_thanks_button");
		string dictionaryStringValue3 = BundleFunctions.GetDictionaryStringValue("strings", "yes_button");
		IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
		if (service2 != null)
		{
			service2["MessageTimeoutTitle"] = dictionaryStringValue;
			if (service != null && service.TTSEnabled)
			{
				service2["TTS:MessageViewTitle"] = dictionaryStringValue;
				service2["TTS:MessageViewButton1Text"] = dictionaryStringValue2;
				service2["TTS:MessageViewButton2Text"] = dictionaryStringValue3;
				service.TTSRepeatSequence();
			}
		}
		idleTimeoutMessageViewUserControl.Message = dictionaryStringValue;
		idleTimeoutMessageViewUserControl.ButtonNoText = dictionaryStringValue2;
		idleTimeoutMessageViewUserControl.ButtonYesTest = dictionaryStringValue3;
		idleTimeoutMessageViewUserControl.OnButtonNoClicked += noButtonAction;
		idleTimeoutMessageViewUserControl.OnButtonYesClicked += delegate
		{
			IViewService service3 = ServiceLocator.Instance.GetService<IViewService>();
			if (service3?.Peek(1) == service3.GetStartOverViewName() && (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn ?? false))
			{
				ApplicationState.Instance.KeepSessionOnStartViewEnter = true;
			}
			OrchestrationService.ViewGoBackSafe("need_more_time:second_button", popOnly: false);
		};
		int num = 15000;
		num = service?.GetTimeout("medium", num) ?? num;
		idleTimeoutMessageViewUserControl.Timeout = num;
	}
}
