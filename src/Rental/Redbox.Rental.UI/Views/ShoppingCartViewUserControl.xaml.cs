using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ShoppingCartView")]
public partial class ShoppingCartViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private ShoppingCartViewModel Model => base.DataContext as ShoppingCartViewModel;

	public ShoppingCartViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.ProcessGetSpeechControl();
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnBackButtonClicked();
		HandleWPFHit();
	}

	private void TermsAndPrivacyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnTermsAndPrivacyButtonClicked();
		HandleWPFHit();
	}

	private void PromoCodeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnPromoCodeButtonClicked();
		HandleWPFHit();
	}

	private void PromoCodeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = Model?.PromoCodeButtonEnabled ?? false;
	}

	private void PayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnPayButtonClicked();
		HandleWPFHit();
	}

	private void PayCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = Model?.PayButtonEnabled ?? false;
	}

	private void UseCreditsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnUseCreditsButtonClicked();
		HandleWPFHit();
	}

	private void AddMovieCommandCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnAddMovieButtonClicked();
		HandleWPFHit();
	}

	private void AddGameCommandCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnAddGameButtonClicked();
		HandleWPFHit();
	}

	private void ShoppingCartViewUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		PopulateDisplayProductModels();
	}

	private void PopulateDisplayProductModels()
	{
		List<DisplayProductCheckoutUserControl> list = new List<DisplayProductCheckoutUserControl>();
		foreach (object child in ShoppingCartStackPanel.Children)
		{
			if (child is DisplayProductCheckoutUserControl)
			{
				list.Add(child as DisplayProductCheckoutUserControl);
			}
		}
		foreach (DisplayProductCheckoutUserControl item in list)
		{
			ShoppingCartStackPanel.Children.Remove(item);
		}
		if (Model?.DisplayProductModels == null)
		{
			return;
		}
		foreach (DisplayCheckoutProductModel displayProductModel in Model.DisplayProductModels)
		{
			DisplayProductCheckoutUserControl element = new DisplayProductCheckoutUserControl
			{
				DataContext = displayProductModel,
				Width = 590.0,
				Height = 94.0
			};
			ShoppingCartStackPanel.Children.Insert(Model.DisplayProductModels.IndexOf(displayProductModel), element);
		}
	}

	private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		DisplayCheckoutProductModel displayProductModel = (e.OriginalSource as FrameworkElement)?.DataContext as DisplayCheckoutProductModel;
		if (Model != null)
		{
			Model.ProcessOnCancelBrowseItemModel(displayProductModel, e.Parameter);
		}
		HandleWPFHit();
	}

	private void UseCreditsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = Model?.UseCreditsButtonEnabled ?? false;
	}

	private void UpdateBagCommandCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnUpdateBagButtonClicked();
	}

	private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnSignInButtonClicked();
	}

	private void UsePointsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = Model?.UsePointsButtonEnabled ?? false;
	}

	private void UsePointsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOnUsePointsButtonClicked();
	}
}
