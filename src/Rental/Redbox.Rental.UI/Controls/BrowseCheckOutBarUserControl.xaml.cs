using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class BrowseCheckOutBarUserControl : UserControl, IWPFActor, IComponentConnector
{
	public IActor Actor { get; set; }

	private BrowseViewModel BrowseViewModel => base.DataContext as BrowseViewModel;

	public event WPFHitHandler OnWPFHit;

	public BrowseCheckOutBarUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(checkout_button);
		obj?.SetStyle(SwitchProductFamilyButton);
	}

	private void HandleWPFHit()
	{
		OnWPFHit?.Invoke(Actor);
	}

	private void CheckOutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseViewModel?.ProcessOnCheckOutButtonClicked();
		HandleWPFHit();
	}

	private void CheckOutCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void ToggleTitleFamilyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		BrowseViewModel?.ProcessOnToggleTitleFamilyButtonClicked();
		HandleWPFHit();
	}
}
