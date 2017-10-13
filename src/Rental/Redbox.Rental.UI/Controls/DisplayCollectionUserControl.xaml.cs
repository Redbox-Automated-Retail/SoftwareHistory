using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class DisplayCollectionUserControl : UserControl, IComponentConnector
{
	public DisplayCollectionUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(SelectButton);
	}

	private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (SelectButton != null)
		{
			SelectButton.CornerRadius = SelectButton.ActualHeight / 2.0;
		}
		if (SelectButtonText != null)
		{
			SelectButtonText.FontSize = Math.Max(1.0, SelectButton.ActualHeight * 0.37);
		}
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		DisplayCollectionModel displayCollectionModel = base.DataContext as DisplayCollectionModel;
		MainGrid.Visibility = ((displayCollectionModel == null) ? Visibility.Collapsed : Visibility.Visible);
	}
}
