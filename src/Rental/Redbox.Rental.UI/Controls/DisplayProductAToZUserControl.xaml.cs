using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class DisplayProductAToZUserControl : UserControl, IComponentConnector
{
	private DisplayProductModel _displayProductModel;

	private DisplayGroupModel _displayGroupModel;

	public DisplayProductAToZUserControl()
	{
		InitializeComponent();
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_displayProductModel = base.DataContext as DisplayProductModel;
		_displayGroupModel = base.DataContext as DisplayGroupModel;
		MainGrid.Visibility = ((_displayProductModel == null && _displayGroupModel == null) ? Visibility.Collapsed : Visibility.Visible);
		UpdateTextStyle();
	}

	private void UpdateTextStyle()
	{
		if (_displayGroupModel != null)
		{
			ProductName.FontSize = Math.Max(0.34 * base.ActualHeight, 1.0);
		}
		else if (_displayProductModel != null)
		{
			ProductName.FontSize = Math.Max(0.25 * base.ActualHeight, 1.0);
		}
	}

	private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdateTextStyle();
	}
}
