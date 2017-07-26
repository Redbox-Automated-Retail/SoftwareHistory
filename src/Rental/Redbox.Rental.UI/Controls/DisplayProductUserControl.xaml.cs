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

public partial class DisplayProductUserControl : UserControl, IComponentConnector
{
	private DisplayProductModel _displayProductModel;

	public DisplayProductUserControl()
	{
		InitializeComponent();
		ApplyStyle();
	}

	public void ApplyStyle()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(AddButton);
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_displayProductModel = base.DataContext as DisplayProductModel;
		SetAddButtonType();
		SetAddButtonPosition();
	}

	public void SetAddButtonPosition()
	{
		if (_displayProductModel != null)
		{
			switch (_displayProductModel.AddButtonPosition)
			{
			case DisplayProductModel.AddButtonPositions.LowerLeftCorner:
				AddButton.Margin = new Thickness(0.0, 0.0, 3.0, 3.0);
				break;
			case DisplayProductModel.AddButtonPositions.OverhangLowerLeftCorner:
				AddButton.Margin = new Thickness(0.0, 0.0, -34.0, -34.0);
				break;
			}
		}
	}

	private void SetAddButtonType()
	{
		if (_displayProductModel != null)
		{
			switch (_displayProductModel.AddButtonType)
			{
			case DisplayProductModel.AddButtonTypes.PlusSign:
				AddButton.Width = 40.0;
				AddButton.Height = 40.0;
				AddButtonText.Visibility = Visibility.Collapsed;
				break;
			case DisplayProductModel.AddButtonTypes.PlusSignWithText:
				AddButton.Width = 220.0;
				AddButton.Height = 68.0;
				AddButtonText.Visibility = Visibility.Visible;
				break;
			}
		}
	}
}
