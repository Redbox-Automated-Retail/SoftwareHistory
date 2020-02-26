using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SignInViewController : BaseViewController
{
	public static void ConfigureSignInView()
	{
		BaseViewController.StartIdleTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			SignInViewModel signInViewModel = new SignInViewModel();
			signInViewModel.HeaderLabelText = Resources.sign_in_header_text;
			signInViewModel.AlreadyMemberText = Resources.sign_in_already_member_text;
			signInViewModel.SignInLabelText = Resources.sign_in;
			signInViewModel.EmailAndPasswordButtonText = Resources.sign_in_email_and_password_button_text;
			signInViewModel.PhoneAndPinButtonText = Resources.sign_in_phone_and_pin_button_text;
			signInViewModel.Bullet1Text = Resources.sign_in_point_one_text;
			signInViewModel.Bullet2Text = Resources.sign_in_point_two_text;
			signInViewModel.Bullet3Text = Resources.sign_in_point_three_text;
			signInViewModel.CancelButtonText = Resources.sign_in_cancel_button_text;
			signInViewModel.BottomMessageText = Resources.sign_in_bottom_message_text;
			signInViewModel.LegalText = Resources.sign_in_legal_text;
			signInViewModel.SignUpButtonText = Resources.sign_up;
			signInViewModel.InfoButtonText = Resources.sign_up_info_button_text;
			signInViewModel.TermsButtonText = Resources.terms_link_text;
			signInViewModel.PrivacyText = (string.IsNullOrEmpty(Redbox.Rental.Services.Configuration.Configuration.Instance.CAPrivacyNoticeDocument) ? null : Resources.privacy_notice_text);
			signInViewModel.ButtonTextStyle = (OrchestrationService.IsCultureSpanish() ? WPFFunctions.GetStyle(viewUserControl, "font_museo_slab_900_24") : WPFFunctions.GetStyle(viewUserControl, "font_museo_slab_900_28"));
			signInViewModel.EmailAndPasswordButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				EmailAndPasswordButtonClicked();
			}, (Func<bool>)null);
			signInViewModel.PhoneAndPinButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				PhoneAndPinButtonClicked();
			}, (Func<bool>)null);
			signInViewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				CancelButtonClicked();
			}, (Func<bool>)null);
			signInViewModel.SignUpButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				SignUpButtonAction();
			}, (Func<bool>)null);
			signInViewModel.InfoButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				InfoButtonAction();
			}, (Func<bool>)null);
			signInViewModel.TermsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				TermsButtonAction();
			}, (Func<bool>)null);
			ConfigureTTS(signInViewModel);
			viewUserControl.DataContext = signInViewModel;
		}
	}

	private static void TermsButtonAction()
	{
		HelpViewController.ShowTermsAndConditions();
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

	private static void EmailAndPasswordButtonClicked()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignInEmail");
		OrchestrationService.ViewGoBackSafe("SignInViewController email sign in", popOnly: true);
		PersonalizationFunctions.ShowEmailLogin(SignInCancelButtonTypes.Back);
	}

	private static void PhoneAndPinButtonClicked()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignInPhone");
		OrchestrationService.ViewGoBackSafe("SignInViewController phone sign in", popOnly: true);
		PersonalizationFunctions.ShowPhoneNumberLogin(SignInCancelButtonTypes.Back);
	}

	private static void CancelButtonClicked()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignInCancel");
		OrchestrationService.ViewGoBackSafe("SignInViewController cancel", popOnly: false);
	}

	private static void SignUpButtonAction()
	{
		BaseViewController.StopIdleTimer();
		OrchestrationService.SetCurrentUICulture();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignUp");
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		string parentView = service.Peek(1);
		ServiceLocator.Instance.GetService<IPerksSignupService>()?.StartSignupFlow(delegate
		{
			OrchestrationService.UnwindToSafe(parentView, discard: true, "SignInViewController sign up");
		}, SignUpFlowContext.SignUp, delegate
		{
			OrchestrationService.InvokeIdleTimerExpireSessionCallback();
		});
	}

	private static void InfoButtonAction()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SignUpInfo");
		OrchestrationService.ViewShow("perks_signup_info_popup_view");
	}
}
