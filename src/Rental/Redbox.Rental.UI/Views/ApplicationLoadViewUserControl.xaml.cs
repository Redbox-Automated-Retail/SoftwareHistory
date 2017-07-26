using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Views;

public partial class ApplicationLoadViewUserControl : UserControl, IComponentConnector
{
	public ApplicationLoadViewUserControl()
	{
		InitializeComponent();
		Spinner.IsAnimated = true;
	}
}
