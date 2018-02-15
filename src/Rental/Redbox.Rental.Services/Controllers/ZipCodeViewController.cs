using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ZipCodeViewController
{
	private static bool _hasNextBeenClicked;

	public static void ConfigureZipCodeView(string note2LabelText, Action<string> nextAction)
	{
		_hasNextBeenClicked = false;
		IActor actor = ActorFunctions.GetActor("ZipCodeViewUserControl");
		if (actor == null || !(actor.WPFFrameworkElement is ZipCodeViewUserControl zipCodeViewUserControl))
		{
			return;
		}
		ZipCodeViewModel zipCodeViewModel = new ZipCodeViewModel();
		zipCodeViewModel.KeypadModel = new KeypadModel();
		zipCodeViewModel.KeypadModel.ClearAllButtonText = BundleFunctions.GetDictionaryStringValue("strings", "keypad_clear_all_button_text");
		zipCodeViewModel.KeypadModel.BackButtonText = BundleFunctions.GetDictionaryStringValue("strings", "keypad_back_button_text");
		KeypadModel keypadModel = zipCodeViewModel.KeypadModel;
		keypadModel.OnNumberButtonClicked = (Action<string>)Delegate.Combine(keypadModel.OnNumberButtonClicked, new Action<string>(KeypadModel_OnNumberButtonClicked));
		KeypadModel keypadModel2 = zipCodeViewModel.KeypadModel;
		keypadModel2.OnClearAllButtonClicked = (Action)Delegate.Combine(keypadModel2.OnClearAllButtonClicked, new Action(KeypadModel_OnClearAllButtonClicked));
		KeypadModel keypadModel3 = zipCodeViewModel.KeypadModel;
		keypadModel3.OnBackButtonClicked = (Action)Delegate.Combine(keypadModel3.OnBackButtonClicked, new Action(KeypadModel_OnBackButtonClicked));
		zipCodeViewModel.RequiredLength = 5;
		zipCodeViewModel.DisplayText = "";
		zipCodeViewModel.HeaderLabelText = BundleFunctions.GetDictionaryStringValue("strings", "zip_code_header_text");
		zipCodeViewModel.Note1LabelText = BundleFunctions.GetDictionaryStringValue("strings", "zip_code_note_text");
		zipCodeViewModel.Note2LabelText = note2LabelText;
		zipCodeViewModel.NextButtonText = BundleFunctions.GetDictionaryStringValue("strings", "zip_code_next_button_text");
		zipCodeViewModel.IsNextButtonEnabled = false;
		zipCodeViewModel.OnNextButtonClicked = delegate(string s)
		{
			if (!_hasNextBeenClicked)
			{
				_hasNextBeenClicked = true;
				nextAction?.Invoke(s);
			}
		};
		ConfigureAdImage(zipCodeViewModel);
		ConfigureTTS(zipCodeViewModel);
		zipCodeViewUserControl.DataContext = zipCodeViewModel;
	}

	private static ZipCodeViewModel GetZipCodeViewModel()
	{
		IActor actor = ActorFunctions.GetActor("ZipCodeViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is ZipCodeViewUserControl zipCodeViewUserControl)
		{
			return zipCodeViewUserControl.DataContext as ZipCodeViewModel;
		}
		return null;
	}

	private static void ConfigureAdImage(ZipCodeViewModel zipCodeViewModel)
	{
		BitmapImage bitmapImage = null;
		MarketingAd marketingAd = ServiceLocator.Instance.GetService<IMarketingAdService>()?.GetScreenAd(MarketingAdLocation.ZipCode, createImpression: true);
		if (marketingAd != null && marketingAd.Visible && !string.IsNullOrEmpty(marketingAd.ImageName))
		{
			bitmapImage = ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetBitmapImage(marketingAd.ImageName);
		}
		zipCodeViewModel.AdImageSource = bitmapImage;
		zipCodeViewModel.AdImageVisibility = ((bitmapImage == null) ? Visibility.Collapsed : Visibility.Visible);
		zipCodeViewModel.StackPanelMargin = ((bitmapImage != null) ? new Thickness(0.0, 0.0, 0.0, 0.0) : new Thickness(0.0, 58.0, 0.0, 0.0));
	}

	private static void ConfigureTTS(ZipCodeViewModel zipCodeViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			zipCodeViewModel.OnGetSpeechControl = () => ZipCodeViewTextToSpeech.GetSpeechControl(GetZipCodeViewModel);
		}
	}

	private static void KeypadModel_OnNumberButtonClicked(string key)
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null && key != null && zipCodeViewModel.DisplayText.Length < zipCodeViewModel.RequiredLength)
		{
			zipCodeViewModel.DisplayText += key;
			UpdateIsNextButtonEnabled();
		}
	}

	private static void KeypadModel_OnClearAllButtonClicked()
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null)
		{
			zipCodeViewModel.DisplayText = "";
			UpdateIsNextButtonEnabled();
		}
	}

	private static void KeypadModel_OnBackButtonClicked()
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null && zipCodeViewModel.DisplayText.Length > 0)
		{
			zipCodeViewModel.DisplayText = zipCodeViewModel.DisplayText.Remove(zipCodeViewModel.DisplayText.Length - 1);
			UpdateIsNextButtonEnabled();
		}
	}

	private static void UpdateIsNextButtonEnabled()
	{
		ZipCodeViewModel zipCodeViewModel = GetZipCodeViewModel();
		if (zipCodeViewModel != null)
		{
			zipCodeViewModel.IsNextButtonEnabled = zipCodeViewModel.DisplayText.Length == zipCodeViewModel.RequiredLength;
			zipCodeViewModel.NextButtonArrowVisibility = ((!zipCodeViewModel.IsNextButtonEnabled) ? Visibility.Collapsed : Visibility.Visible);
		}
	}
}
