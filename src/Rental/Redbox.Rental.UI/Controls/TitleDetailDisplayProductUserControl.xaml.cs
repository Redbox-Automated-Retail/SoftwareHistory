using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
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
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(go_back_button);
		obj?.SetStyle(collection_button);
		obj?.SetStyle(buy_button);
		obj?.SetStyle(add_button_primary);
		obj?.SetStyle(add_button_blu_ray);
	}
}
