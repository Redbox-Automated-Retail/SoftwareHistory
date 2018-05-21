using System;
using System.Windows;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PhoneAndPinViewController : BaseViewController
{
	public static void ConfigurePhoneAndPinView(PhoneAndPinInfo phoneAndPinInfo)
	{
		BaseViewController.StartIdleTimer();
		OrchestrationService.AnalyticsAddViewEvent("context", "PinSignIn");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			if (phoneAndPinInfo.PhoneAndPinViewModel != null)
			{
				viewUserControl.DataContext = phoneAndPinInfo.PhoneAndPinViewModel;
				return;
			}
			PhoneAndPinViewModel phoneAndPinViewModel = new PhoneAndPinViewModel();
			phoneAndPinViewModel.KeypadModel = new KeypadModel();
			phoneAndPinViewModel.KeypadModel.ClearAllButtonText = BundleFunctions.GetDictionaryStringValue("strings", "keypad_clear_all_button_text");
			phoneAndPinViewModel.KeypadModel.BackButtonText = BundleFunctions.GetDictionaryStringValue("strings", "keypad_back_button_text");
			KeypadModel keypadModel = phoneAndPinViewModel.KeypadModel;
			keypadModel.OnNumberButtonClicked = (Action<string>)Delegate.Combine(keypadModel.OnNumberButtonClicked, new Action<string>(KeypadModel_OnNumberButtonClicked));
			KeypadModel keypadModel2 = phoneAndPinViewModel.KeypadModel;
			keypadModel2.OnClearAllButtonClicked = (Action)Delegate.Combine(keypadModel2.OnClearAllButtonClicked, new Action(KeypadModel_OnClearAllButtonClicked));
			KeypadModel keypadModel3 = phoneAndPinViewModel.KeypadModel;
			keypadModel3.OnBackButtonClicked = (Action)Delegate.Combine(keypadModel3.OnBackButtonClicked, new Action(KeypadModel_OnBackButtonClicked));
			phoneAndPinViewModel.SetDefaultDisplays();
			phoneAndPinViewModel.PhoneAndPinInfo = phoneAndPinInfo;
			phoneAndPinViewModel.PhoneRequiredLength = 10;
			phoneAndPinViewModel.PinRequiredLength = 4;
			phoneAndPinViewModel.HeaderLabelText = Resources.phone_and_pin_header_text;
			phoneAndPinViewModel.SubHeadLabelText = Resources.phone_and_pin_sub_header_text;
			phoneAndPinViewModel.PhoneLabelText = Resources.phone_and_pin_phone_label_text;
			phoneAndPinViewModel.PinLabelText = Resources.phone_and_pin_pin_label_text;
			phoneAndPinViewModel.ErrorMessageText = "";
			phoneAndPinViewModel.ADAPhoneButtonText = Resources.phone_and_pin_ada_phone_button_text;
			phoneAndPinViewModel.ADAPhoneButtonVisibility = ((!Redbox.Rental.Services.ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.ADAPinButtonText = Resources.phone_and_pin_ada_pin_button_text;
			phoneAndPinViewModel.ADAPinButtonVisibility = ((!Redbox.Rental.Services.ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.StartButtonText = ((phoneAndPinInfo.SignInCancelButtonType == SignInCancelButtonTypes.Back) ? Resources.back_button : Resources.phone_and_pin_start_button_text);
			phoneAndPinViewModel.SignInButtonText = Resources.phone_and_pin_sign_in_button_text;
			phoneAndPinViewModel.OnDisplayClicked = (Action<PhoneAndPinViewModel.DisplayType>)Delegate.Combine(phoneAndPinViewModel.OnDisplayClicked, new Action<PhoneAndPinViewModel.DisplayType>(PhoneAndPinViewModel_OnDisplayClicked));
			phoneAndPinViewModel.OnStartButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnStartButtonClicked, new Action(PhoneAndPinViewModel_OnStartButtonClicked));
			phoneAndPinViewModel.OnSignInButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnSignInButtonClicked, new Action(PhoneAndPinViewModel_OnSignInButtonClicked));
			ConfigureTTS(phoneAndPinViewModel);
			viewUserControl.DataContext = phoneAndPinViewModel;
		}
	}

	public static void LeavePhoneAndPinView(PhoneAndPinInfo phoneAndPinInfo)
	{
		BaseViewController.StopIdleTimer();
		phoneAndPinInfo.PhoneAndPinViewModel = GetPhoneAndPinViewModel();
	}

	private static PhoneAndPinViewModel GetPhoneAndPinViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as PhoneAndPinViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(PhoneAndPinViewModel phoneAndPinViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			phoneAndPinViewModel.OnGetSpeechControl = () => PhoneAndPinViewTextToSpeech.GetSpeechControl(GetPhoneAndPinViewModel);
		}
	}

	private static void KeypadModel_OnNumberButtonClicked(string key)
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel == null || key == null)
		{
			return;
		}
		phoneAndPinViewModel.ErrorMessageText = "";
		if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
		{
			if (phoneAndPinViewModel.PhoneDisplayText == Resources.phone_and_pin_default_phone_display)
			{
				phoneAndPinViewModel.PhoneDisplayText = "";
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText.Length < phoneAndPinViewModel.PhoneRequiredLength)
			{
				phoneAndPinViewModel.PhoneDisplayText += key;
				UpdateIsSignInButtonEnabled();
			}
		}
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
		{
			if (phoneAndPinViewModel.PinDisplayText == Resources.phone_and_pin_default_pin_display)
			{
				phoneAndPinViewModel.PinDisplayText = "";
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinDisplayText.Length < phoneAndPinViewModel.PinRequiredLength)
			{
				phoneAndPinViewModel.PinDisplayText += key;
				UpdateIsSignInButtonEnabled();
			}
		}
	}

	private static void KeypadModel_OnClearAllButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.ErrorMessageText = "";
			if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
			{
				phoneAndPinViewModel.PhoneDisplayText = Resources.phone_and_pin_default_phone_display;
				UpdateIsSignInButtonEnabled();
			}
			else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
			{
				phoneAndPinViewModel.PinDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
		}
	}

	private static void KeypadModel_OnBackButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel == null)
		{
			return;
		}
		phoneAndPinViewModel.ErrorMessageText = "";
		if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
		{
			if (phoneAndPinViewModel.PhoneDisplayText != Resources.phone_and_pin_default_phone_display && phoneAndPinViewModel.PhoneDisplayText.Length > 0)
			{
				phoneAndPinViewModel.PhoneDisplayText = phoneAndPinViewModel.PhoneDisplayText.Remove(phoneAndPinViewModel.PhoneDisplayText.Length - 1);
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText == "")
			{
				phoneAndPinViewModel.PhoneDisplayText = Resources.phone_and_pin_default_phone_display;
				UpdateIsSignInButtonEnabled();
			}
		}
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
		{
			if (phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length > 0)
			{
				phoneAndPinViewModel.PinDisplayText = phoneAndPinViewModel.PinDisplayText.Remove(phoneAndPinViewModel.PinDisplayText.Length - 1);
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinDisplayText == "")
			{
				phoneAndPinViewModel.PinDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
		}
	}

	private static void UpdateIsSignInButtonEnabled()
	{
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.IsSignInButtonEnabled = phoneAndPinViewModel.PhoneDisplayText != Resources.phone_and_pin_default_phone_display && phoneAndPinViewModel.PhoneDisplayText.Length == phoneAndPinViewModel.PhoneRequiredLength && phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length == phoneAndPinViewModel.PinRequiredLength;
		}
	}

	private static void PhoneAndPinViewModel_OnDisplayClicked(PhoneAndPinViewModel.DisplayType displayType)
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.ErrorMessageText = "";
			phoneAndPinViewModel.DisplayTypeFocus = displayType;
			switch (displayType)
			{
			case PhoneAndPinViewModel.DisplayType.Phone:
				phoneAndPinViewModel.PhoneDisplayBorderBrush = new SolidColorBrush(Color.FromRgb(120, 45, 141));
				phoneAndPinViewModel.PhoneDisplayBorderShadowDepth = 2.0;
				phoneAndPinViewModel.PhoneDisplayBorderColor = Color.FromRgb(120, 45, 141);
				phoneAndPinViewModel.PinDisplayBorderBrush = new SolidColorBrush(Color.FromRgb(165, 165, 165));
				phoneAndPinViewModel.PinDisplayBorderShadowDepth = 4.0;
				phoneAndPinViewModel.PinDisplayBorderColor = Color.FromRgb(225, 225, 225);
				break;
			case PhoneAndPinViewModel.DisplayType.Pin:
				phoneAndPinViewModel.PhoneDisplayBorderBrush = new SolidColorBrush(Color.FromRgb(165, 165, 165));
				phoneAndPinViewModel.PhoneDisplayBorderShadowDepth = 4.0;
				phoneAndPinViewModel.PhoneDisplayBorderColor = Color.FromRgb(225, 225, 225);
				phoneAndPinViewModel.PinDisplayBorderBrush = new SolidColorBrush(Color.FromRgb(120, 45, 141));
				phoneAndPinViewModel.PinDisplayBorderShadowDepth = 2.0;
				phoneAndPinViewModel.PinDisplayBorderColor = Color.FromRgb(120, 45, 141);
				break;
			}
		}
	}

	private static void PhoneAndPinViewModel_OnStartButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "startbrowsing");
		OrchestrationService.ViewGoBackSafe("Phone and pin login cancel", popOnly: false);
	}

	private static void PhoneAndPinViewModel_OnSignInButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "signin");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.SignInAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.SignInAction(phoneAndPinViewModel.PhoneDisplayText, BaseViewController.StringToSecureString(phoneAndPinViewModel.PinDisplayText));
		}
	}
}
