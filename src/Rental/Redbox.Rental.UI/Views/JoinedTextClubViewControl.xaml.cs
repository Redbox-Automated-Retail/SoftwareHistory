using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "JoinedTextClubView")]
public partial class JoinedTextClubViewControl : ViewUserControl, IComponentConnector
{
	public JoinedTextClubViewControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		base.SpeechViewName = "joind_text_club_view";
	}

	protected override void InitSpeechPart(ISpeechPart speechPart)
	{
		speechPart.Texts.Add(new TextPart
		{
			Text = TitleTextElem.Text
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = MessageTextElem.Text
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = CommentTextElem.Text
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = "To continue press ENTER"
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Function = "TTSRepeatSpeechPart",
			Action = delegate
			{
				ViewUserControl.InvokeRoutedCommand(GetModel<SimpleModel>()?.ContinueButtonCommand);
			}
		});
		speechPart.Loop = true;
	}
}
