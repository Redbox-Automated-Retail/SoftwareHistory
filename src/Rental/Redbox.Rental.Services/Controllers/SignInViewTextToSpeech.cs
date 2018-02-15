using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class SignInViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<SignInViewModel> getSignInViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_sign_in_view";
		AddSpeechPartInstructions(result, getSignInViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<SignInViewModel> getSignInViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "Instructions",
			Sequence = 1,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			SignInViewModel signInViewModel = getSignInViewModelFunction();
			if (signInViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						signInViewModel.ProcessOnCancelButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						signInViewModel.ProcessOnEmailAndPasswordButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						signInViewModel.ProcessOnPhoneAndPinButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Sign in to get some points."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To sign in, using email and password, press 1."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To sign in, using phone and pin, press 2."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To cancel, without signing in, press cancel."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
