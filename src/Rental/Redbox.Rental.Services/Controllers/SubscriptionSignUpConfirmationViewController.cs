using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class SubscriptionSignUpConfirmationViewController : BaseViewController
{
	private class Constants
	{
		public const string TimerName = "subscription_signup_confirmation_timer";
	}

	private static object lockObject = new object();

	public static void ConfigureSubscriptionSignUpConfirmationView(SubscriptionSignUpConfirmationViewParameters subscriptionSignUpConfirmationViewParameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			SubscriptionSignUpConfirmationViewModel subscriptionSignUpConfirmationViewModel = new SubscriptionSignUpConfirmationViewModel
			{
				SubscriptionProduct = subscriptionSignUpConfirmationViewParameters?.SubscriptionProduct
			};
			subscriptionSignUpConfirmationViewModel.ContinueAction = subscriptionSignUpConfirmationViewParameters?.ContinueAction;
			ConfigureText(subscriptionSignUpConfirmationViewModel);
			CreateTimer();
			subscriptionSignUpConfirmationViewModel.GotItButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				GotItButtonCommand_Execute(ContinueAction);
			}, (Func<bool>)null);
			viewUserControl.DataContext = subscriptionSignUpConfirmationViewModel;
		}
	}

	private static void ConfigureText(SubscriptionSignUpConfirmationViewModel subscriptionSignUpConfirmationViewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		bool valueOrDefault = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsCPNewInThisSession == true;
		if (subscriptionSignUpConfirmationViewModel?.SubscriptionProduct is IStarzSubscriptionProduct starzSubscriptionProduct)
		{
			subscriptionSignUpConfirmationViewModel.RedboxPlusExistingCustomerTextVisibility = Visibility.Collapsed;
			subscriptionSignUpConfirmationViewModel.RedboxPlusNewCustomerTextVisibility = Visibility.Collapsed;
			subscriptionSignUpConfirmationViewModel.StazTextVisibility = Visibility.Visible;
			if (valueOrDefault)
			{
				subscriptionSignUpConfirmationViewModel.RegisteredTrademarkSymbol = starzSubscriptionProduct.RegisteredTrademarkSymbolNewCP;
				subscriptionSignUpConfirmationViewModel.RegisteredTrademarkSymbolMessage = starzSubscriptionProduct.RegisteredTrademarkSymbolNewCPMessage;
				subscriptionSignUpConfirmationViewModel.TitleText1 = starzSubscriptionProduct.ConfirmationNewCPTitleText1;
				subscriptionSignUpConfirmationViewModel.TitleText2 = starzSubscriptionProduct.ConfirmationNewCPTitleText2;
				subscriptionSignUpConfirmationViewModel.MessageText1 = starzSubscriptionProduct.ConfirmationNewCPMessageText1;
				subscriptionSignUpConfirmationViewModel.MessageText2 = starzSubscriptionProduct.ConfirmationNewCPMessageText2;
				subscriptionSignUpConfirmationViewModel.SubMessageText = starzSubscriptionProduct.ConfirmationNewCPSubMessageText;
			}
			else
			{
				subscriptionSignUpConfirmationViewModel.RegisteredTrademarkSymbol = starzSubscriptionProduct.RegisteredTrademarkSymbol;
				subscriptionSignUpConfirmationViewModel.RegisteredTrademarkSymbolMessage = starzSubscriptionProduct.RegisteredTrademarkSymbolMessage;
				subscriptionSignUpConfirmationViewModel.TitleText1 = starzSubscriptionProduct.ConfirmationTitleText1;
				subscriptionSignUpConfirmationViewModel.TitleText2 = starzSubscriptionProduct.ConfirmationTitleText2;
				subscriptionSignUpConfirmationViewModel.MessageText1 = starzSubscriptionProduct.ConfirmationMessageText1;
				subscriptionSignUpConfirmationViewModel.MessageText2 = starzSubscriptionProduct.ConfirmationMessageText2;
				subscriptionSignUpConfirmationViewModel.SubMessageText = string.Empty;
			}
		}
		else
		{
			subscriptionSignUpConfirmationViewModel.StazTextVisibility = Visibility.Collapsed;
			subscriptionSignUpConfirmationViewModel.RedboxPlusExistingCustomerTextVisibility = (valueOrDefault ? Visibility.Collapsed : Visibility.Visible);
			subscriptionSignUpConfirmationViewModel.RedboxPlusNewCustomerTextVisibility = ((!valueOrDefault) ? Visibility.Collapsed : Visibility.Visible);
			if (subscriptionSignUpConfirmationViewModel?.SubscriptionProduct is IRedboxPlusSubscriptionProduct)
			{
				if (valueOrDefault)
				{
					subscriptionSignUpConfirmationViewModel.TitleText1 = Resources.redbox_plus_signup_confirmation_new_customer_title;
					subscriptionSignUpConfirmationViewModel.MessageText1 = Resources.redbox_plus_signup_confirmation_new_customer_message_1;
					subscriptionSignUpConfirmationViewModel.MessageText2 = Resources.redbox_plus_signup_confirmation_new_customer_message_2;
					subscriptionSignUpConfirmationViewModel.SubMessageText = Resources.redbox_plus_signup_confirmation_new_customer_submessage_1;
					subscriptionSignUpConfirmationViewModel.SubMessageText2 = Resources.redbox_plus_signup_confirmation_new_customer_submessage_2;
				}
				else
				{
					subscriptionSignUpConfirmationViewModel.TitleText1 = Resources.redbox_plus_signup_confirmation_existing_customer_title;
					subscriptionSignUpConfirmationViewModel.MessageText1 = Resources.redbox_plus_signup_confirmation_existing_customer_message_1;
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in subscription_sign_up_confirmation_view: Unable to find subscription product.");
			}
		}
		subscriptionSignUpConfirmationViewModel.ContinueButtonText = Resources.got_it_button.ToUpper();
	}

	private static void CreateTimer()
	{
		int num = 8000;
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null)
		{
			num = service.GetTimeout("shorter", num);
		}
		(ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer("subscription_signup_confirmation_timer", num, num, delegate
		{
			ContinueAction();
		}))?.Start();
	}

	private static void ContinueAction()
	{
		SubscriptionSignUpConfirmationViewModel viewModel = BaseViewController.GetViewModel<SubscriptionSignUpConfirmationViewModel>();
		if (viewModel == null)
		{
			return;
		}
		lock (lockObject)
		{
			if (!viewModel.HasActionExecuted)
			{
				viewModel.HasActionExecuted = true;
				OrchestrationService.ViewGoBackSafe("Subscription sign up confiration view continue", popOnly: false);
				ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
				service.GetTimer("subscription_signup_confirmation_timer")?.Stop();
				service?.RemoveTimer("subscription_signup_confirmation_timer");
				viewModel?.ContinueAction();
			}
		}
	}

	private static void GotItButtonCommand_Execute(Action continueAction)
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("SubscriptionSignUpConfirmationGotItButton");
		continueAction?.Invoke();
	}
}
