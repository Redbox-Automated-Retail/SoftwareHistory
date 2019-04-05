using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ReturnView")]
public partial class ReturnViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	public new IActor Actor { get; set; }

	public string MessageText
	{
		get
		{
			return InsertDiscAnimation.MessageText;
		}
		set
		{
			InsertDiscAnimation.MessageText = value;
		}
	}

	public ImageSource AdImage
	{
		get
		{
			return ad_image.Source;
		}
		set
		{
			ad_image.Source = value;
		}
	}

	public new event WPFHitHandler OnWPFHit;

	public event Action OnGoBack;

	public ReturnViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_return_view";
		AddSpeechPartInstructions(result);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl result)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "Instructions",
			Loop = true,
			StartPause = 0,
			EndPause = 10000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Please insert your disc in the return slot located on the right side of the kiosk."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "Insert the disc by holding it in your hand with the opening tab facing the return slot."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The raised plastic pieces will be in the back."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The Kiosk will return to the welcome screen momentarily."
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private new void HandleWPFHit()
	{
		if (OnWPFHit != null)
		{
			OnWPFHit(Actor);
		}
	}
}
