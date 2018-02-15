using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ConfirmEmailView")]
public partial class ConfirmEmailViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private ConfirmEmailModel Model => base.DataContext as ConfirmEmailModel;

	public ConfirmEmailViewUserControl()
	{
		InitializeComponent();
	}

	private void Button1_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessButton1Action();
	}

	private void Button2_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessButton2Action();
	}
}
