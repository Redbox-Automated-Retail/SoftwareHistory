using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PromptSignInController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			PromptSignInModel promptSignInModel = new PromptSignInModel
			{
				ContinueButtonText = Resources.sign_in_continue_button_text,
				EmailCodeButtonText = Resources.sign_in_email_button_text,
				PhonePinButtonText = Resources.sign_in_phone_button_text,
				SignInLeftTitle = Resources.sign_in_left_title,
				SignInRightTitle = Resources.sign_in_right_title,
				SignInMessage = Resources.sign_in_message
			};
			PromptSignInLogic logic = new PromptSignInLogic(promptSignInModel)
			{
				Instructions = true,
				EmailCodeAction = ProcessEmailCodeAction,
				PhonePinAction = ProcessPhonePinAction,
				ContinueAction = ProcessContinueAction
			};
			promptSignInModel.Logic = logic;
			ConfigureTTS(promptSignInModel);
			viewUserControl.DataContext = promptSignInModel;
			BaseViewController.StartIdleTimer();
		}
	}

	private static void ProcessEmailCodeAction()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsAddViewEvent("buttonPress", "signnin-email-home");
		OrchestrationService.ViewGoBackSafe("PromptSignInController email sign in", popOnly: true);
		PersonalizationFunctions.ShowEmailLogin(SignInCancelButtonTypes.StartBrowsing);
	}

	private static void ProcessPhonePinAction()
	{
		BaseViewController.StopIdleTimer();
		BaseViewController.AnalyticsAddViewEvent("buttonPress", "signnin-phone-home");
		OrchestrationService.ViewGoBackSafe("PromptSignInController phone sign in", popOnly: true);
		PersonalizationFunctions.ShowPhoneNumberLogin(SignInCancelButtonTypes.StartBrowsing);
	}

	private static void ProcessContinueAction()
	{
		BaseViewController.StopIdleTimer();
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "continueasguest");
		OrchestrationService.ViewGoBackSafe("PromptSignInController continue", popOnly: false);
	}

	private static void ConfigureTTS(PromptSignInModel model)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			model.ProcessGetSpeechControl = () => PromptSignInTTSController.GetSpeechControl(model.Logic);
		}
	}
}
