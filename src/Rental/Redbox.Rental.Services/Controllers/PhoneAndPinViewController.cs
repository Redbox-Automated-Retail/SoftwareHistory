using System;
using System.Windows;
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
		OrchestrationService.SetCurrentUICulture();
		if (phoneAndPinInfo.IsSignUp)
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "EarlyIdTextClubOptIn");
		}
		else
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "PinSignIn");
			BaseViewController.StartIdleTimer();
		}
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		if (phoneAndPinInfo.PhoneAndPinViewModel != null)
		{
			phoneAndPinInfo.PhoneAndPinViewModel.IsStartButtonEnabled = true;
			viewUserControl.DataContext = phoneAndPinInfo.PhoneAndPinViewModel;
			return;
		}
		PhoneAndPinViewModel phoneAndPinViewModel = new PhoneAndPinViewModel();
		phoneAndPinViewModel.KeypadModel = new KeypadModel();
		phoneAndPinViewModel.KeypadModel.ClearAllButtonText = Resources.keypad_clear_all_button_text;
		phoneAndPinViewModel.KeypadModel.BackButtonText = Resources.keypad_back_button_text;
		KeypadModel keypadModel = phoneAndPinViewModel.KeypadModel;
		keypadModel.OnNumberButtonClicked = (Action<string>)Delegate.Combine(keypadModel.OnNumberButtonClicked, new Action<string>(KeypadModel_OnNumberButtonClicked));
		KeypadModel keypadModel2 = phoneAndPinViewModel.KeypadModel;
		keypadModel2.OnClearAllButtonClicked = (Action)Delegate.Combine(keypadModel2.OnClearAllButtonClicked, new Action(KeypadModel_OnClearAllButtonClicked));
		KeypadModel keypadModel3 = phoneAndPinViewModel.KeypadModel;
		keypadModel3.OnBackButtonClicked = (Action)Delegate.Combine(keypadModel3.OnBackButtonClicked, new Action(KeypadModel_OnBackButtonClicked));
		phoneAndPinViewModel.PhoneAndPinInfo = phoneAndPinInfo;
		phoneAndPinViewModel.PhoneRequiredLength = 10;
		phoneAndPinViewModel.PinRequiredLength = 4;
		phoneAndPinViewModel.SetDefaultDisplays();
		phoneAndPinViewModel.PhoneLabelText = Resources.phone_and_pin_phone_label_text;
		phoneAndPinViewModel.PinLabelText = Resources.phone_and_pin_pin_label_text;
		phoneAndPinViewModel.PinConfirmLabelText = Resources.phone_and_pin_confirm_pin_label_text;
		phoneAndPinViewModel.ErrorMessageText = "";
		phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
		if (!string.IsNullOrEmpty(phoneAndPinInfo.MobilePhoneNumber))
		{
			phoneAndPinViewModel.PhoneDisplayText = phoneAndPinInfo.MobilePhoneNumber;
		}
		if (phoneAndPinInfo.IsSignUp)
		{
			if (phoneAndPinInfo.IsAfterSwipe)
			{
				phoneAndPinViewModel.HeaderLabelText = Resources.phone_and_pin_sign_up_after_swipe_header_text;
				phoneAndPinViewModel.SubHeadLabelText = Resources.phone_and_pin_sign_up_after_swipe_sub_header_text;
			}
			else
			{
				phoneAndPinViewModel.HeaderLabelText = Resources.phone_and_pin_sign_up_header_text;
				phoneAndPinViewModel.SubHeadLabelText = Resources.phone_and_pin_sign_up_sub_header_text;
			}
			phoneAndPinViewModel.ADAPhoneButtonVisibility = Visibility.Collapsed;
			phoneAndPinViewModel.ADAPinButtonVisibility = Visibility.Collapsed;
			phoneAndPinViewModel.RightSideButtonBarVisibility = ((!Redbox.Rental.Services.ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.BottomButtonBarVisibility = (Redbox.Rental.Services.ApplicationState.Instance.IsADAAccessible ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.StartButtonText = ((phoneAndPinInfo.SignInCancelButtonType == SignInCancelButtonTypes.Back) ? Resources.cancel_button : Resources.phone_and_pin_sign_up_back_button_text);
			phoneAndPinViewModel.SignInButtonText = Resources.phone_and_pin_sign_up_button_text;
			phoneAndPinViewModel.TermsTitleText = Resources.phone_and_pin_terms_title_text;
			phoneAndPinViewModel.TermsText = Resources.phone_and_pin_terms_text;
			phoneAndPinViewModel.TermsButtonText = Resources.terms_button_text;
			phoneAndPinViewModel.TermsBarVisibility = Visibility.Visible;
			phoneAndPinViewModel.PinConfirmVisibility = Visibility.Visible;
			phoneAndPinViewModel.TextClubOptionVisibility = ((!phoneAndPinInfo.ShowTextClubOption) ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.TextClubText = Resources.text_club_option_text;
			phoneAndPinViewModel.SignUpForTextClub = phoneAndPinInfo.ShowTextClubOption;
			phoneAndPinViewModel.OnStartButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnStartButtonClicked, new Action(OnMaybeLaterButtonClicked));
			phoneAndPinViewModel.OnSignInButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnSignInButtonClicked, new Action(OnSignUpButtonClicked));
			phoneAndPinViewModel.OnTextClubClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnTextClubClicked, new Action(OnTextClubCheckboxCliked));
			phoneAndPinViewModel.MobilePhoneErrorMessageText = string.Empty;
		}
		else
		{
			phoneAndPinViewModel.HeaderLabelText = Resources.phone_and_pin_header_text;
			phoneAndPinViewModel.SubHeadLabelText = Resources.phone_and_pin_sub_header_text;
			phoneAndPinViewModel.ADAPhoneButtonText = Resources.phone_and_pin_ada_phone_button_text;
			phoneAndPinViewModel.ADAPhoneButtonVisibility = ((!Redbox.Rental.Services.ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.ADAPinButtonText = Resources.phone_and_pin_ada_pin_button_text;
			phoneAndPinViewModel.ADAPinButtonVisibility = ((!Redbox.Rental.Services.ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
			phoneAndPinViewModel.StartButtonText = ((phoneAndPinInfo.SignInCancelButtonType == SignInCancelButtonTypes.Back) ? Resources.cancel_button : Resources.phone_and_pin_start_button_text);
			phoneAndPinViewModel.SignInButtonText = Resources.phone_and_pin_sign_in_button_text;
			phoneAndPinViewModel.OnStartButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnStartButtonClicked, new Action(PhoneAndPinViewModel_OnStartButtonClicked));
			phoneAndPinViewModel.OnSignInButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnSignInButtonClicked, new Action(PhoneAndPinViewModel_OnSignInButtonClicked));
		}
		phoneAndPinViewModel.OnDisplayClicked = (Action<PhoneAndPinViewModel.DisplayType>)Delegate.Combine(phoneAndPinViewModel.OnDisplayClicked, new Action<PhoneAndPinViewModel.DisplayType>(PhoneAndPinViewModel_OnDisplayClicked));
		phoneAndPinViewModel.OnTermsButtonClicked = (Action)Delegate.Combine(phoneAndPinViewModel.OnTermsButtonClicked, (Action)delegate
		{
			PhoneAndPinViewModel_OnTermsButtonClicked(phoneAndPinInfo.IdleTimerName);
		});
		ConfigureTTS(phoneAndPinViewModel);
		phoneAndPinInfo.PhoneAndPinViewModel = phoneAndPinViewModel;
		viewUserControl.DataContext = phoneAndPinViewModel;
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
		phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
		if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
		{
			if (string.IsNullOrEmpty(phoneAndPinViewModel.PhoneDisplayText))
			{
				phoneAndPinViewModel.PhoneDisplayText = "";
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText.Length < phoneAndPinViewModel.PhoneRequiredLength)
			{
				phoneAndPinViewModel.PhoneDisplayText += key;
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText.Length == phoneAndPinViewModel.PhoneRequiredLength)
			{
				PhoneAndPinViewModel_OnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
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
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.PinConfirm)
		{
			if (phoneAndPinViewModel.PinConfirmDisplayText == Resources.phone_and_pin_default_pin_display)
			{
				phoneAndPinViewModel.PinConfirmDisplayText = "";
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinConfirmDisplayText.Length < phoneAndPinViewModel.PinRequiredLength)
			{
				phoneAndPinViewModel.PinConfirmDisplayText += key;
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
			phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
			if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
			{
				phoneAndPinViewModel.PhoneDisplayText = string.Empty;
				UpdateIsSignInButtonEnabled();
			}
			else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
			{
				phoneAndPinViewModel.PinDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
			else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.PinConfirm)
			{
				phoneAndPinViewModel.PinConfirmDisplayText = Resources.phone_and_pin_default_pin_display;
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
		phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
		if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
		{
			if (phoneAndPinViewModel.PhoneDisplayText.Length > 0)
			{
				phoneAndPinViewModel.PhoneDisplayText = phoneAndPinViewModel.PhoneDisplayText.Remove(phoneAndPinViewModel.PhoneDisplayText.Length - 1);
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PhoneDisplayText == "")
			{
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
		else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.PinConfirm)
		{
			if (phoneAndPinViewModel.PinConfirmDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinConfirmDisplayText.Length > 0)
			{
				phoneAndPinViewModel.PinConfirmDisplayText = phoneAndPinViewModel.PinConfirmDisplayText.Remove(phoneAndPinViewModel.PinConfirmDisplayText.Length - 1);
				UpdateIsSignInButtonEnabled();
			}
			if (phoneAndPinViewModel.PinConfirmDisplayText == "")
			{
				phoneAndPinViewModel.PinConfirmDisplayText = Resources.phone_and_pin_default_pin_display;
				UpdateIsSignInButtonEnabled();
			}
		}
	}

	private static void UpdateIsSignInButtonEnabled()
	{
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.IsSignInButtonEnabled = phoneAndPinViewModel.PhoneDisplayText.Length == phoneAndPinViewModel.PhoneRequiredLength && phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length == phoneAndPinViewModel.PinRequiredLength;
			if (phoneAndPinViewModel.PhoneAndPinInfo.IsSignUp)
			{
				UpdatePinConfirmView(phoneAndPinViewModel);
			}
		}
	}

	private static void UpdatePinConfirmView(PhoneAndPinViewModel phoneAndPinViewModel)
	{
		bool flag = phoneAndPinViewModel.PinConfirmDisplayText == phoneAndPinViewModel.PinDisplayText;
		phoneAndPinViewModel.IsSignInButtonEnabled &= flag;
		if (!flag)
		{
			phoneAndPinViewModel.ConfirmPinButtonState = 1;
			phoneAndPinViewModel.ErrorMessageText = Resources.phone_and_pin_confirm_error_text;
		}
		else
		{
			phoneAndPinViewModel.ConfirmPinButtonState = 0;
			phoneAndPinViewModel.ErrorMessageText = string.Empty;
		}
	}

	private static void PhoneAndPinViewModel_OnDisplayClicked(PhoneAndPinViewModel.DisplayType displayType)
	{
		BaseViewController.ProcessWPFHit();
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			phoneAndPinViewModel.ErrorMessageText = "";
			phoneAndPinViewModel.MobilePhoneErrorMessageText = "";
			phoneAndPinViewModel.ObfuscatePins();
			phoneAndPinViewModel.DisplayTypeFocus = displayType;
			switch (displayType)
			{
			case PhoneAndPinViewModel.DisplayType.Phone:
				UpdatePinConfirmView(phoneAndPinViewModel);
				break;
			case PhoneAndPinViewModel.DisplayType.Pin:
				UpdatePinConfirmView(phoneAndPinViewModel);
				break;
			case PhoneAndPinViewModel.DisplayType.PinConfirm:
				UpdatePinConfirmView(phoneAndPinViewModel);
				break;
			}
		}
	}

	private static void PhoneAndPinViewModel_OnStartButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "startbrowsing");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinStartBrowsing");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		OrchestrationService.ViewGoBackSafe("Phone and pin login cancel", popOnly: false);
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction();
		}
	}

	private static void PhoneAndPinViewModel_OnSignInButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "signin");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinSignIn");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.SignInAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.SignInAction(phoneAndPinViewModel.PhoneDisplayText, BaseViewController.StringToSecureString(phoneAndPinViewModel.PinDisplayText), arg3: false);
		}
	}

	private static void PhoneAndPinViewModel_OnTermsButtonClicked(string idleTimerName)
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "terms");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinTerms");
		HelpViewController.ShowMobileFaqAndPrivacy("perks_terms", idleTimerName);
	}

	private static void OnMaybeLaterButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "maybelater");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinMaybeLater");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.ContinueAction();
		}
	}

	private static void OnSignUpButtonClicked()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "signup");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PhoneAndPinSignUp");
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null && phoneAndPinViewModel.PhoneAndPinInfo != null && phoneAndPinViewModel.PhoneAndPinInfo.SignInAction != null)
		{
			phoneAndPinViewModel.PhoneAndPinInfo.SignInAction(phoneAndPinViewModel.PhoneDisplayText, BaseViewController.StringToSecureString(phoneAndPinViewModel.PinDisplayText), phoneAndPinViewModel.SignUpForTextClub);
		}
	}

	private static void OnTextClubCheckboxCliked()
	{
		PhoneAndPinViewModel phoneAndPinViewModel = GetPhoneAndPinViewModel();
		if (phoneAndPinViewModel != null)
		{
			if (phoneAndPinViewModel.SignUpForTextClub)
			{
				OrchestrationService.AnalyticsAddViewEvent("checkBoxSelected", "textcluboptin,true");
			}
			else
			{
				OrchestrationService.AnalyticsAddViewEvent("checkBoxSelected", "textcluboptin,false");
			}
			BaseViewController.AnalyticsService?.AddCheckBoxEvent("PhoneAndPinTextClubOptin", phoneAndPinViewModel.SignUpForTextClub);
		}
	}
}
