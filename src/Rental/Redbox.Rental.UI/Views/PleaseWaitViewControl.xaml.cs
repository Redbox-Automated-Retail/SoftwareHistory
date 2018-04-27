using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PleaseWaitView")]
public partial class PleaseWaitViewControl : ViewUserControl, IComponentConnector
{
	public string titleText => Redbox.Rental.UI.Properties.Resources.just_a_sec_label;

	public string messageText => Redbox.Rental.UI.Properties.Resources.alot_goingon_label;

	public PleaseWaitViewControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		base.DataContext = this;
		base.SpeechViewName = "please_wait_view";
	}

	protected override void InitSpeechPart(ISpeechPart speechPart)
	{
		speechPart.Texts.Add(new TextPart
		{
			Text = titleText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = messageText
		});
	}
}
