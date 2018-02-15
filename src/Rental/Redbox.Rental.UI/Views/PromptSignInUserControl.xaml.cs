using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "PromptSignInView")]
public partial class PromptSignInUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private PromptSignInModel Model => base.DataContext as PromptSignInModel;

	public PromptSignInUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		_ = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}
}
