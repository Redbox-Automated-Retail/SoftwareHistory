using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PromptSignInView")]
public partial class PromptSignInUserControl : ViewUserControl, IComponentConnector
{
	private PromptSignInModel Model => base.DataContext as PromptSignInModel;

	public PromptSignInUserControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}
}
