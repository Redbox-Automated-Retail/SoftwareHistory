using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PleaseWaitView")]
public partial class PleaseWaitViewControl : ViewUserControl, IComponentConnector
{
	private class TTSConstants
	{
		public const string SpeechPartOne = "SpeechPartOne";
	}

	public string titleText => Redbox.Rental.UI.Properties.Resources.just_a_sec_label;

	public string messageText => Redbox.Rental.UI.Properties.Resources.alot_goingon_label;

	public PleaseWaitViewControl()
	{
		InitializeComponent();
		ApplyTheme();
		base.DataContext = this;
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		foreach (Control item in RentalUserControl.FindLogicalChildren<Control>(MainControl))
		{
			theme?.SetStyle(item);
		}
	}

	public override ISpeechControl GetSpeechControl()
	{
		SpeechControl speechControl = new SpeechControl();
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "SpeechPartOne",
			StartPause = 100
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = titleText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = messageText
			});
		};
		((ISpeechControl)speechControl).SpeechParts.Add(speechPart);
		return speechControl;
	}
}
