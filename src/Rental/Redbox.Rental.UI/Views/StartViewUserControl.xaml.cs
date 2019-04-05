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

public partial class StartViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private StartViewModel StartViewModel => base.DataContext as StartViewModel;

	public StartViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("RentOrBuyMoviesCommand", Redbox.Rental.UI.Commands.RentOrBuyMoviesCommand);
		RegisterRoutedCommand("RentOrBuyGamesCommand", Redbox.Rental.UI.Commands.RentOrBuyGamesCommand);
		RegisterRoutedCommand("RentTVCommand", Redbox.Rental.UI.Commands.RentTVCommand);
		RegisterRoutedCommand("BuyMoviesCommand", Redbox.Rental.UI.Commands.BuyMoviesCommand);
		RegisterRoutedCommand("BuyGamesCommand", Redbox.Rental.UI.Commands.BuyGamesCommand);
		RegisterRoutedCommand("PickupProductCommand", Redbox.Rental.UI.Commands.PickupProductCommand);
		RegisterRoutedCommand("ReturnProductCommand", Redbox.Rental.UI.Commands.ReturnProductCommand);
		RegisterRoutedCommand("HelpCommand", Redbox.Rental.UI.Commands.HelpCommand);
		RegisterRoutedCommand("SignInCommand", Redbox.Rental.UI.Commands.SignInCommand);
		PostHealthActivity();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(all_movies_button);
		obj?.SetStyle(games_button);
		obj?.SetStyle(buy_movies_button);
		obj?.SetStyle(buy_games_button);
		obj?.SetStyle(return_button);
		obj?.SetStyle(online_pickup_button);
		obj?.SetStyle(handicap_button);
		obj?.SetStyle(espanol_button);
		obj?.SetStyle(help_button);
	}

	private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			StartViewModel?.ProcessRentMovieButtonClick();
			HandleWPFHit();
		});
	}

	private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = all_movies_button.Visibility == Visibility.Visible;
	}

	private void RentTVCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			StartViewModel?.ProcessRentTVButtonClick();
			HandleWPFHit();
		});
	}

	private void RentTVCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = tv_button.Visibility == Visibility.Visible;
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

	private void BuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = buy_movies_button.Visibility == Visibility.Visible;
	}

	private void BuyGamesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = buy_games_button.Visibility == Visibility.Visible;
	}

	private void RentGamesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = games_button.Visibility == Visibility.Visible;
	}

	private void BuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessBuyMoviesButtonClick();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void BuyGamesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessBuyGamesButtonClick();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void RentOrBuyGamesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessRentOrBuyGameButtonClick();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void PickupProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessPickupButtonClick();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void ReturnProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessReturnButtonClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessHelpButtonClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessToggleADAMode();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void ExitBurnInViewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessExitBurnInView();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
	}

	private void BannerClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel.ProcessBannerClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				StartViewModel?.ProcessSignInButtonClicked();
				HandleWPFHit();
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}
}
