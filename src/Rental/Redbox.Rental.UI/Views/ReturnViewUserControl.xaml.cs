using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

public partial class ReturnViewUserControl : UserControl, IWPFActor, IComponentConnector
{
	public IActor Actor { get; set; }

	public string BackButtonText
	{
		get
		{
			return ButtonGoBack_Text.Text;
		}
		set
		{
			ButtonGoBack_Text.Text = value;
		}
	}

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

	public Visibility BackButtonVisibility
	{
		get
		{
			return back_button.Visibility;
		}
		set
		{
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				back_button.Visibility = value;
			});
		}
	}

	public event WPFHitHandler OnWPFHit;

	public event Action OnGoBack;

	public ReturnViewUserControl()
	{
		InitializeComponent();
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		back_button.Visibility = Visibility.Hidden;
		if (OnGoBack != null)
		{
			OnGoBack();
		}
		HandleWPFHit();
	}

	private void HandleWPFHit()
	{
		if (OnWPFHit != null)
		{
			OnWPFHit(Actor);
		}
	}
}
