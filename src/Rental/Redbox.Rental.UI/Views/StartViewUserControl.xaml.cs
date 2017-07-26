using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "StartView")]
public partial class StartViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	public IActor Actor { get; set; }

	private StartViewModel StartViewModel => base.DataContext as StartViewModel;

	public event WPFHitHandler OnWPFHit;

	public StartViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("RentOrBuyMoviesCommand", Redbox.Rental.UI.Commands.RentOrBuyMoviesCommand);
		RegisterRoutedCommand("RentOrBuyGamesCommand", Redbox.Rental.UI.Commands.RentOrBuyGamesCommand);
		RegisterRoutedCommand("BuyMoviesOrGamesCommand", Redbox.Rental.UI.Commands.BuyMoviesOrGamesCommand);
		RegisterRoutedCommand("PickupProductCommand", Redbox.Rental.UI.Commands.PickupProductCommand);
		RegisterRoutedCommand("ReturnProductCommand", Redbox.Rental.UI.Commands.ReturnProductCommand);
		RegisterRoutedCommand("HelpCommand", Redbox.Rental.UI.Commands.HelpCommand);
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(all_movies_button);
		obj?.SetStyle(games_button);
		obj?.SetStyle(buy_movies_or_games_button);
		obj?.SetStyle(return_button);
		obj?.SetStyle(online_pickup_button);
		obj?.SetStyle(handicap_button);
		obj?.SetStyle(espanol_button);
		obj?.SetStyle(help_button);
	}

	private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessRentMovieButtonClick();
		HandleWPFHit();
	}

	private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = all_movies_button.Visibility == Visibility.Visible;
	}

	private void ToggleLanguageModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessToggleLanguage();
		HandleWPFHit();
	}

	private void ToggleLanguageModeCommandBingind_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = true;
	}

	private void BuyMoviesOrGameCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = buy_movies_or_games_button.Visibility == Visibility.Visible;
	}

	private void RentGamesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = games_button.Visibility == Visibility.Visible;
	}

	private void BuyMoviesOrGamesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessBuyMoviesOrGamesButtonClick();
		HandleWPFHit();
	}

	private void RentOrBuyGamesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessRentOrBuyGameButtonClick();
		HandleWPFHit();
	}

	private void PickupProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessPickupButtonClick();
		HandleWPFHit();
	}

	private void ReturnProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessReturnButtonClicked();
		HandleWPFHit();
	}

	private void HandleWPFHit()
	{
		OnWPFHit?.Invoke(Actor);
	}

	private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessHelpButtonClicked();
		HandleWPFHit();
	}

	private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessToggleADAMode();
		HandleWPFHit();
	}

	private void ExitBurnInViewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel?.ProcessExitBurnInView();
		HandleWPFHit();
	}

	private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
	}

	private void BannerClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StartViewModel.ProcessBannerClicked();
		HandleWPFHit();
	}
}
