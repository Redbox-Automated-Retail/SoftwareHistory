using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsDevView")]
public partial class TitleDetailsDevViewUserControl : RentalUserControl, IWPFActor, IComponentConnector
{
	private TitleDetailDevModel Model => base.DataContext as TitleDetailDevModel;

	public new IActor Actor { get; set; }

	public new event WPFHitHandler OnWPFHit;

	public TitleDetailsDevViewUserControl()
	{
		InitializeComponent();
		base.Loaded += delegate
		{
			Keyboard.Focus(this);
			if (Model.GenreIds != null)
			{
				for (int i = 0; i < Model.GenreIds.Count; i++)
				{
					TextBlock textBlock = new TextBlock();
					textBlock.Text = Model.GenreIds[i].ToString() + " " + Enum.Parse(typeof(Genres), Model.GenreIds[i].ToString());
					textBlock.Text = textBlock.Text.ToUpper();
					textBlock.Height = 19.0;
					Style style = (Style)FindResource("font_akzidenz_regular_8");
					textBlock.Margin = new Thickness(10.0, 0.0, 0.0, 0.0);
					textBlock.Style = style;
					textBlock.SetValue(Grid.ColumnProperty, 0);
					textBlock.SetValue(Grid.RowProperty, 24 + i);
					MainGrid.RowDefinitions.Add(new RowDefinition
					{
						MaxHeight = 20.0
					});
					MainGrid.Children.Add(textBlock);
				}
			}
			foreach (KeyValuePair<string, BarcodeProduct> barCode in Model.BarCodes)
			{
				lstBarCodes.Items.Add("( " + barCode.Value.Deck + "-" + barCode.Value.Slot + ") " + barCode.Value.Barcode + " " + barCode.Value.Status.ToUpper());
			}
			title_rollup_button.IsEnabled = false;
			bluray_button.IsEnabled = Model.IsBluRay;
			digital_button.IsEnabled = Model.IsDigitalTitle;
			imgPic.Source = Model.ImgFile;
		};
	}

	private void close_button_Click(object sender, RoutedEventArgs e)
	{
		if (Model?.OnCloseButtonClicked != null)
		{
			Model.OnCloseButtonClicked();
		}
	}

	private void dvd_button_Click(object sender, RoutedEventArgs e)
	{
		dvd_button.IsEnabled = false;
		title_rollup_button.IsEnabled = true;
		bluray_button.IsEnabled = Model.IsBluRay;
		digital_button.IsEnabled = Model.IsDigitalTitle;
		if (Model?.OnDvdButtonClicked != null)
		{
			Model.OnDvdButtonClicked();
		}
	}

	private void bluray_button_Click(object sender, RoutedEventArgs e)
	{
		title_rollup_button.IsEnabled = true;
		dvd_button.IsEnabled = true;
		bluray_button.IsEnabled = false;
		digital_button.IsEnabled = Model.IsDigitalTitle;
		if (Model?.OnBlurayButtonClicked != null)
		{
			Model.OnBlurayButtonClicked();
		}
	}

	private void title_rollup_button_Click(object sender, RoutedEventArgs e)
	{
		title_rollup_button.IsEnabled = false;
		dvd_button.IsEnabled = true;
		bluray_button.IsEnabled = Model.IsBluRay;
		digital_button.IsEnabled = Model.IsDigitalTitle;
		if (Model?.OnTitleRollupButtonClicked != null)
		{
			Model.OnTitleRollupButtonClicked();
		}
	}
}
