using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.Services.Controllers;

public class PromptSignInTTSController
{
	private class TTSConstants
	{
		public const string SpeechPartOne = "SpeechPartOne";
	}

	internal static ISpeechControl GetSpeechControl(PromptSignInLogic logic)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = string.Format("tts_{0}", "prompt_sign_in_view");
		AddSpeechPart(result, logic);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPart(ISpeechControl speechControl, PromptSignInLogic logic)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "SpeechPartOne",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Sign in to get some points"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To sign in with email and password press 1."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To sign in with phone number and four digit pin press 2."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To skip signing in and continue as a guest press 3."
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "1",
				Action = delegate
				{
					logic.EmailCodeButtonCommand_Execute();
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "2",
				Action = delegate
				{
					logic.PhonePinButtonCommand_Execute();
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "3",
				Action = delegate
				{
					logic.ContinueButtonCommand_Execute();
				}
			});
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
