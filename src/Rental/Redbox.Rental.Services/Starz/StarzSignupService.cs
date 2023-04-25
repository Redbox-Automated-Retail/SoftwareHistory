using System;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.Services.Starz;

public class StarzSignupService
{
	private static string _emailAddress;

	private static IStarzSubscriptionProduct _starzSubscriptionProduct;

	private const string IdleTimerName = "starz_signup_idle_timer";

	public static void StartStarzSignup(IStarzSubscriptionProduct starzSubscriptionProduct)
	{
		_starzSubscriptionProduct = starzSubscriptionProduct;
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null && personalizationSession.IsLoggedIn)
		{
			_emailAddress = personalizationSession.LoginEmailAddress;
			ConfirmAccountIsEligibleForStarzOffer();
			return;
		}
		SignupFunctions.ShowEnterEmailView(new SignupFunctions.EmailEntryInfo
		{
			IdleTimerName = "starz_signup_idle_timer",
			SetEmailAddressAction = SetEmailAddress,
			ProcessSuccessfulConfirmEmailResult = ProcessSuccessfulConfirmEmailResult,
			KeyboardMode = KeyboardMode.STARZ_SIGN_UP,
			KeyboardViewName = "keyboard_starz_signup_view"
		});
	}

	private static void ConfirmAccountIsEligibleForStarzOffer()
	{
		bool flag = true;
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null)
		{
			if (personalizationSession?.SubscriptionData?.SubscriptionServiceIsDown ?? false)
			{
				flag = false;
				MessagePopupViewController.ShowStarzGenericErrorMessage(continueAction);
			}
			else if (personalizationSession?.SubscriptionData?.HasActiveStarzSubscription ?? false)
			{
				flag = false;
				MessagePopupViewController.ShowStarzExistingSubscriptionErrorMessage(continueAction);
			}
			else if (personalizationSession?.SubscriptionData != null && !personalizationSession.SubscriptionData.IsEligibleForSubscription(_starzSubscriptionProduct?.SubscriptionId))
			{
				flag = false;
				MessagePopupViewController.ShowStarzEligibilityErrorMessage(continueAction);
			}
		}
		if (flag)
		{
			PromptUserToConfirmEnrollment();
		}
		static void continueAction()
		{
			SignupFunctions.ReturnToShoppingCartOrBrowseView(null);
		}
	}

	private static void PromptUserToConfirmEnrollment()
	{
		OrchestrationService.ViewShow("starz_terms_acceptance_view", new StarzTermsAcceptanceViewParameters
		{
			StarzSubscriptionProduct = _starzSubscriptionProduct,
			EmailAddress = _emailAddress
		});
	}

	public static void AddStarzSubscriptionPrerequisites(IStarzSubscriptionProduct starzSubscriptionProduct)
	{
		OrchestrationService.ViewShow("please_wait_view");
		SignupFunctions.UpdateMarketingOptInConfirmationStatus(_emailAddress);
		if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession == null || !((!(Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.PromptForPerks)) ?? true))
		{
			Action<IUpdateAccountResult> callback = delegate(IUpdateAccountResult updateAccountResult)
			{
				StarzSignUpService_HandlelAccountUpdateResult(updateAccountResult);
			};
			SignupFunctions.CallUpdateAccountImmediate(null, null, _emailAddress, Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.CustomerProfileNumber, optinForTextClub: false, acceptedPerksSignup: true, callback, AccountSourceType.Starz);
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.SignupFlowState.WasPerksSignupOfferPresented = true;
		}
		else
		{
			SignupFunctions.AddSubscriptionAndReturnToShoppingCartOrBrowseView(_starzSubscriptionProduct);
		}
	}

	private static void StarzSignUpService_HandlelAccountUpdateResult(IUpdateAccountResult updateAccountResult)
	{
		if (updateAccountResult.Success)
		{
			if (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession == null)
			{
				SignupFunctions.SetNewAccountLoggedInState(updateAccountResult, _emailAddress, null, textClubEnabled: false);
			}
			else
			{
				Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.PromptForPerks = false;
			}
			SignupFunctions.AddSubscriptionAndReturnToShoppingCartOrBrowseView(_starzSubscriptionProduct);
		}
		else
		{
			MessagePopupViewController.ShowStarzGenericErrorMessage(continueAction);
		}
		static void continueAction()
		{
			SignupFunctions.ReturnToShoppingCartOrBrowseView("Unable to update account and add Starz subscription to cart");
		}
	}

	private static void SetEmailAddress(string emailAddress)
	{
		_emailAddress = emailAddress;
	}

	private static void ProcessSuccessfulConfirmEmailResult(IEmailConfirmResult result)
	{
		if (!string.IsNullOrEmpty(result.CustomerProfileNumber))
		{
			MessagePopupViewController.ShowStarzExistingAccountErrorMessage(ContinueAction);
		}
		else
		{
			ConfirmAccountIsEligibleForStarzOffer();
		}
		static void ContinueAction()
		{
			OrchestrationService.UnwindToSafe("sign_in_view", discard: true, null);
		}
	}
}
