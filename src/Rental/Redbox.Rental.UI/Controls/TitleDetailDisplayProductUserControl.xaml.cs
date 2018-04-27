using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Controls;

public partial class TitleDetailDisplayProductUserControl : UserControl, IComponentConnector
{
	public TitleDetailDisplayProductUserControl()
	{
		InitializeComponent();
		base.Loaded += TitleDetailDisplayProductUserControl_Loaded;
	}

	private void TitleDetailDisplayProductUserControl_Loaded(object sender, RoutedEventArgs e)
	{
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		theme?.SetStyle(go_back_button);
		theme?.SetStyle(buy_button);
		for (int i = 0; i < FormatButtonsItemsControl.Items.Count; i++)
		{
			FrameworkElement frameworkElement = (FrameworkElement)FormatButtonsItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
			if (frameworkElement != null)
			{
				theme?.SetStyle(frameworkElement);
			}
		}
	}
}
