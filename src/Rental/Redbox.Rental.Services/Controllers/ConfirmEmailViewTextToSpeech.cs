using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class ConfirmEmailViewTextToSpeech
{
	private static class TTSConstants
	{
		internal static string Intro = "Intro";

		internal static string Buttons = "Buttons";
	}

	public static SpeechControl GetSpeechControl(Func<ConfirmEmailModel> getModel)
	{
		SpeechControl obj = new SpeechControl
		{
			Name = "tts_confirm_email_view",
			SpeechParts = { AddIntroSpeechPart(getModel) }
		};
		int i = 0;
		obj.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.Sequence = i++;
		});
		return obj;
	}

	public static ISpeechPart AddIntroSpeechPart(Func<ConfirmEmailModel> getModel)
	{
		SpeechPart part = new SpeechPart
		{
			Name = TTSConstants.Intro,
			Loop = true,
			StartPause = 250,
			EndPause = 10000
		};
		part.Refresh = delegate
		{
			part.Clear();
			ConfirmEmailModel model = getModel();
			part.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "1",
				Action = delegate
				{
					model?.ProcessButton1Action();
				}
			});
			part.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "2",
				Action = delegate
				{
					model?.ProcessButton2Action();
				}
			});
			part.Texts.Add(new TextPart
			{
				Text = model?.HeaderText
			});
			part.Texts.Add(new TextPart
			{
				Text = model?.BodyText
			});
			ConfirmEmailModel confirmEmailModel = model;
			if (confirmEmailModel != null && confirmEmailModel.OptionalBodyTextVisibility == Visibility.Visible)
			{
				part.Texts.Add(new TextPart
				{
					Text = model?.OptionalBodyText
				});
			}
		};
		return part;
	}
}
