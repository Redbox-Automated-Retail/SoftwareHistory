using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SignInViewController : BaseViewController
{
	public static void ConfigureSignInView()
	{
		BaseViewController.StartIdleTimer();
		OrchestrationService.AnalyticsAddViewEvent("context", "SignIn");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			SignInViewModel signInViewModel = new SignInViewModel();
			signInViewModel.HeaderLabelText = Resources.sign_in_header_text;
			signInViewModel.EmailAndPasswordButtonText = Resources.sign_in_email_and_password_button_text;
			signInViewModel.PhoneAndPinButtonText = Resources.sign_in_phone_and_pin_button_text;
			signInViewModel.CancelButtonText = Resources.sign_in_cancel_button_text;
			signInViewModel.OnEmailAndPasswordButtonClicked = (Action)Delegate.Combine(signInViewModel.OnEmailAndPasswordButtonClicked, new Action(SignInViewModel_OnEmailAndPasswordButtonClicked));
			signInViewModel.OnPhoneAndPinButtonClicked = (Action)Delegate.Combine(signInViewModel.OnPhoneAndPinButtonClicked, new Action(SignInViewModel_OnPhoneAndPinButtonClicked));
			signInViewModel.OnCancelButtonClicked = (Action)Delegate.Combine(signInViewModel.OnCancelButtonClicked, new Action(SignInViewModel_OnCancelButtonClicked));
			ConfigureTTS(signInViewModel);
			viewUserControl.DataContext = signInViewModel;
		}
	}

	private static SignInViewModel GetSignInViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as SignInViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(SignInViewModel signInViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			signInViewModel.OnGetSpeechControl = () => SignInViewTextToSpeech.GetSpeechControl(GetSignInViewModel);
		}
	}

	private static void SignInViewModel_OnEmailAndPasswordButtonClicked()
	{
		BaseViewController.StopIdleTimer();
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "emailpassword");
		OrchestrationService.ViewGoBackSafe("SignInViewController email sign in", popOnly: true);
		PersonalizationFunctions.ShowEmailLogin(SignInCancelButtonTypes.Back);
	}

	private static void SignInViewModel_OnPhoneAndPinButtonClicked()
	{
		BaseViewController.StopIdleTimer();
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "phonepin");
		OrchestrationService.ViewGoBackSafe("PromptSignInController phone sign in", popOnly: true);
		PersonalizationFunctions.ShowPhoneNumberLogin(SignInCancelButtonTypes.Back);
	}

	private static void SignInViewModel_OnCancelButtonClicked()
	{
		BaseViewController.StopIdleTimer();
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "cancel");
		OrchestrationService.ViewGoBackSafe("PromptSignInController cancel", popOnly: false);
	}
}
