using System;
using System.Runtime.CompilerServices;
using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public static class LoyaltySignUpController
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Action<AnalyticsTypes> _003C_003E9__4_4;

		public static Action<bool> _003C_003E9__4_5;

		public static Action<AnalyticsTypes> _003C_003E9__5_2;

		public static Action<AnalyticsTypes> _003C_003E9__6_1;

		public static Action<IUpdateAccountResult> _003C_003E9__10_0;

		internal void _003CShowJoinPerksPromptView_003Eb__4_4(AnalyticsTypes AnalyticsType)
		{
			IAnalyticsService service = ServiceLocator.Instance.GetService<IAnalyticsService>();
			switch (AnalyticsType)
			{
			case AnalyticsTypes.context:
				OrchestrationService.AnalyticsAddViewEvent("context", "JoinPerks");
				service?.AddButtonPressEvent("LoyaltySignUpJoinPerks");
				break;
			case AnalyticsTypes.button1Press:
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", "createaccount");
				service?.AddButtonPressEvent("LoyaltySignUpCreateAcccount");
				break;
			case AnalyticsTypes.button2Press:
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", "cancel");
				service?.AddButtonPressEvent("LoyaltySignUpCancel");
				break;
			case AnalyticsTypes.button3Press:
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", "terms");
				service?.AddButtonPressEvent("LoyaltySignUpTerms");
				break;
			}
		}

		internal void _003CShowJoinPerksPromptView_003Eb__4_5(bool isChecked)
		{
			OrchestrationService.AnalyticsAddViewEvent("checkBoxSelected", $"marketingoptin,{isChecked.ToString()}");
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddCheckBoxEvent("LoyaltySignUpJoinPerksMarketingOptin", isChecked);
		}

		internal void _003CShowJoinedPerksConfirmation_003Eb__5_2(AnalyticsTypes AnalyticsType)
		{
			switch (AnalyticsType)
			{
			case AnalyticsTypes.context:
				OrchestrationService.AnalyticsAddViewEvent("context", "WelcomeToPerks");
				break;
			case AnalyticsTypes.button1Press:
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", "Okay");
				break;
			}
		}

		internal void _003CShowJoinedTextClubView_003Eb__6_1(AnalyticsTypes AnalyticsType)
		{
			switch (AnalyticsType)
			{
			case AnalyticsTypes.context:
				OrchestrationService.AnalyticsAddViewEvent("context", "TextMessageAlert");
				break;
			case AnalyticsTypes.button1Press:
				OrchestrationService.AnalyticsAddViewEvent("buttonPress", "Okay");
				break;
			}
		}

		internal void _003CQueueUpdateAccount_003Eb__10_0(IUpdateAccountResult result)
		{
			string empty = string.Empty;
			empty = GetErrorsAsString(result.RemoteServiceErrors, empty);
			LogHelper.Instance.Log($"QueueUpdateAccount Queueing of UpdateAccount unsuccessful. {empty}");
		}
	}

	private const string IdleTimerName = "loyalty_signup_idle_timer";

	private static IIdleTimer IdleTimer { get; set; }

	public static void BeginAfterSwipeLoyaltySignUpFlow(Action continueCheckoutCallback)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.LoyaltySignUpAfterSwipeEnabled)
		{
			if (string.IsNullOrEmpty(Redbox.Rental.Services.Session.Session.Instance?.CustomerProfileNumber))
			{
				OrchestrationService.StopIdleTimer("idle_timer");
				string text = string.Empty;
				string accountNumber = string.Empty;
				if (Redbox.Rental.Services.Session.Session.Instance.UserInfo != null)
				{
					text = Redbox.Rental.Services.Session.Session.Instance.UserInfo.Email;
					accountNumber = Redbox.Rental.Services.Session.Session.Instance.UserInfo.AccountNumber;
				}
				if (string.IsNullOrEmpty(text))
				{
					LogHelper.Instance.Log("BeginAfterSwipeLoyaltySignUpFlow was called but no user email was stored in the session.");
					ContinueCheckout(continueCheckoutCallback);
					return;
				}
				ServiceLocator.Instance.GetService<IPersonalizationService>();
				LoyaltySignupFlowValues signUpFlowData = new LoyaltySignupFlowValues
				{
					EmailAddress = text,
					IsAfterSwipe = true
				};
				signUpFlowData.ContinueAction = delegate
				{
					ContinueCheckout(continueCheckoutCallback);
				};
				IMarketingOptinServices service = ServiceLocator.Instance.GetService<IMarketingOptinServices>();
				long num = ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0;
				service.SendEmailConfirm(completeCallback: delegate(IRemoteServiceResult result)
				{
					ProcessConfirmEmailResult(result, signUpFlowData, continueCheckoutCallback);
				}, storeNumber: num.ToString(), emailAddress: text, accountNumber: accountNumber);
			}
			else
			{
				LogHelper.Instance.Log($"BeginAfterSwipeLoyaltySignUpFlow disabled due to existence of customer profile number {Redbox.Rental.Services.Session.Session.Instance?.CustomerProfileNumber}.");
				ContinueCheckout(continueCheckoutCallback);
			}
		}
		else
		{
			LogHelper.Instance.Log("BeginAfterSwipeLoyaltySignUpFlow disabled due to configuration.");
			ContinueCheckout(continueCheckoutCallback);
		}
	}

	public static void BeginAfterSignInLoyaltySignUpFlow(string customerProfileNumber, string emailAddress, string mobilePhoneNumber, bool promptForPerks, bool promptForEarlyId, bool textClubEnabled, Action afterFlowAction)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.LoyaltySignUpEnabled)
		{
			LoyaltySignupFlowValues loyaltySignupFlowValues = new LoyaltySignupFlowValues
			{
				IsAfterSwipe = false,
				CustomerProfileNumber = customerProfileNumber,
				EmailAddress = emailAddress,
				PromptForPerks = promptForPerks,
				PromptForEarlyId = promptForEarlyId,
				TextClubEnabled = textClubEnabled,
				MobilePhoneNumber = mobilePhoneNumber,
				ContinueAction = delegate
				{
					RemoveIdleTimer();
					OrchestrationService.StartIdleTimer("idle_timer");
					OrchestrationService.ViewGoBackSafe("Join Perks ContinueAction", popOnly: false);
					afterFlowAction();
				}
			};
			if (loyaltySignupFlowValues.PromptForPerks)
			{
				OrchestrationService.StopIdleTimer("idle_timer");
				ConfigureIdleTimer(loyaltySignupFlowValues);
				ShowJoinPerksPromptView(loyaltySignupFlowValues);
			}
			else if (loyaltySignupFlowValues.PromptForEarlyId)
			{
				OrchestrationService.StopIdleTimer("idle_timer");
				ConfigureIdleTimer(loyaltySignupFlowValues);
				ShowSignUpForEarlyIdView(loyaltySignupFlowValues);
			}
			else
			{
				LogHelper.Instance.Log("BeginAfterSignInLoyaltySignUpFlow: No prompts necessary.");
				afterFlowAction();
			}
		}
		else
		{
			LogHelper.Instance.Log("BeginAfterSignInLoyaltySignUpFlow disabled due to configuration.");
			afterFlowAction();
		}
	}

	private static void ProcessConfirmEmailResult(IRemoteServiceResult result, LoyaltySignupFlowValues signUpFlowData, Action continueCheckoutCallback)
	{
		if (result.Success)
		{
			signUpFlowData.CustomerProfileNumber = result.GetProperty<string>("customer_profile_number");
			signUpFlowData.MobilePhoneNumber = result.GetProperty<string>("mobile_phone_number");
			signUpFlowData.PromptForPerks = result.GetProperty<bool>("prompt_for_perks");
			signUpFlowData.PromptForEarlyId = result.GetProperty<bool>("prompt_for_early_id");
			signUpFlowData.TextClubEnabled = result.GetProperty<bool>("text_club_enabled");
			ConfigureIdleTimer(signUpFlowData);
			if (string.IsNullOrEmpty(signUpFlowData.CustomerProfileNumber) && signUpFlowData.PromptForPerks)
			{
				ShowJoinPerksPromptView(signUpFlowData);
				return;
			}
			if (string.IsNullOrEmpty(signUpFlowData.CustomerProfileNumber) && signUpFlowData.PromptForEarlyId)
			{
				ShowSignUpForEarlyIdView(signUpFlowData);
				return;
			}
			LogHelper.Instance.Log(string.Format("ProcessConfirmEmailResult Not prompting for perks. {0} PromptForPerks = {1} PromptForEarlyID = {2}", string.IsNullOrEmpty(signUpFlowData.CustomerProfileNumber) ? "no CP Number" : "Customer has CP", signUpFlowData.PromptForPerks, signUpFlowData.SignUpForEarlyID));
			ContinueCheckout(continueCheckoutCallback);
		}
		else
		{
			string empty = string.Empty;
			empty = GetErrorsAsString(result.Errors, empty);
			LogHelper.Instance.Log($"ProcessConfirmEmailResult ConfirmEmail unsuccessful. {empty}");
			ContinueCheckout(continueCheckoutCallback);
		}
	}

	private static string GetErrorsAsString(ErrorList errors, string errorMessages)
	{
		if (errors != null)
		{
			foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
			{
				errorMessages += $" [{error.Code}] {error.Description}.";
			}
		}
		return errorMessages;
	}

	private static void ShowJoinPerksPromptView(LoyaltySignupFlowValues signUpFlowData)
	{
		LogHelper.Instance.Log("Prompt to Join Perks: starting.");
		SimpleInfo simpleInfo = new SimpleInfo
		{
			Obj = signUpFlowData,
			UseIdleTimer = false,
			AnalyticsAction = delegate(AnalyticsTypes AnalyticsType)
			{
				IAnalyticsService service = ServiceLocator.Instance.GetService<IAnalyticsService>();
				switch (AnalyticsType)
				{
				case AnalyticsTypes.context:
					OrchestrationService.AnalyticsAddViewEvent("context", "JoinPerks");
					service?.AddButtonPressEvent("LoyaltySignUpJoinPerks");
					break;
				case AnalyticsTypes.button1Press:
					OrchestrationService.AnalyticsAddViewEvent("buttonPress", "createaccount");
					service?.AddButtonPressEvent("LoyaltySignUpCreateAcccount");
					break;
				case AnalyticsTypes.button2Press:
					OrchestrationService.AnalyticsAddViewEvent("buttonPress", "cancel");
					service?.AddButtonPressEvent("LoyaltySignUpCancel");
					break;
				case AnalyticsTypes.button3Press:
					OrchestrationService.AnalyticsAddViewEvent("buttonPress", "terms");
					service?.AddButtonPressEvent("LoyaltySignUpTerms");
					break;
				}
			},
			CheckBoxChangedAction = delegate(bool isChecked)
			{
				OrchestrationService.AnalyticsAddViewEvent("checkBoxSelected", $"marketingoptin,{isChecked.ToString()}");
				ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddCheckBoxEvent("LoyaltySignUpJoinPerksMarketingOptin", isChecked);
			}
		};
		simpleInfo.CancelAction = delegate
		{
			lock (simpleInfo.LockObject)
			{
				if (!simpleInfo.HasActionExecuted)
				{
					simpleInfo.HasActionExecuted = true;
					IdleTimer.Stop();
					signUpFlowData.SignUpForPerks = false;
					signUpFlowData.ContinueAction?.Invoke();
				}
			}
		};
		simpleInfo.ContinueAction = delegate
		{
			lock (simpleInfo.LockObject)
			{
				if (!simpleInfo.HasActionExecuted)
				{
					simpleInfo.HasActionExecuted = true;
					IdleTimer.Stop();
					signUpFlowData.SignUpForPerks = true;
					OrchestrationService.ViewGoBackSafe("Prompt to Join Perks closed", popOnly: true);
					ShowJoinedPerksConfirmation(signUpFlowData);
				}
			}
		};
		simpleInfo.OtherAction = delegate
		{
			lock (simpleInfo.LockObject)
			{
				if (!simpleInfo.HasActionExecuted)
				{
					simpleInfo.HasActionExecuted = true;
					IdleTimer.Stop();
					HelpViewController.ShowJoinPerks(delegate
					{
						simpleInfo.HasActionExecuted = false;
						IdleTimer.Start();
						ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("LoyaltySignUpBack");
						OrchestrationService.AnalyticsAddViewEvent("buttonPress", "back");
						OrchestrationService.ViewGoBackSafe("ShowJoinPerks() back", popOnly: false);
					});
				}
			}
		};
		signUpFlowData.TimeoutAction = delegate
		{
			simpleInfo.CancelAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("join_perks_promo_view", simpleInfo);
	}

	private static void ShowJoinedPerksConfirmation(LoyaltySignupFlowValues signUpFlowData)
	{
		LogHelper.Instance.Log("Confirmation of Joining Perks: starting.");
		SimpleInfo simpleInfo = new SimpleInfo
		{
			UseIdleTimer = false,
			AnalyticsAction = delegate(AnalyticsTypes AnalyticsType)
			{
				switch (AnalyticsType)
				{
				case AnalyticsTypes.context:
					OrchestrationService.AnalyticsAddViewEvent("context", "WelcomeToPerks");
					break;
				case AnalyticsTypes.button1Press:
					OrchestrationService.AnalyticsAddViewEvent("buttonPress", "Okay");
					break;
				}
			}
		};
		simpleInfo.ContinueAction = delegate
		{
			lock (simpleInfo.LockObject)
			{
				if (!simpleInfo.HasActionExecuted)
				{
					simpleInfo.HasActionExecuted = true;
					IdleTimer.Stop();
					if (signUpFlowData.PromptForEarlyId)
					{
						OrchestrationService.ViewGoBackSafe("Confirmation of Joining Perks closed", popOnly: true);
						ShowSignUpForEarlyIdView(signUpFlowData);
					}
					else
					{
						QueueUpdateAccount(signUpFlowData);
						signUpFlowData.ContinueAction?.Invoke();
					}
				}
			}
		};
		signUpFlowData.TimeoutAction = delegate
		{
			simpleInfo.ContinueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("joined_perks_view", simpleInfo);
	}

	private static void ShowJoinedTextClubView(LoyaltySignupFlowValues signUpFlowData)
	{
		LogHelper.Instance.Log("Promo to Joined Text Club: starting.");
		SimpleInfo simpleInfo = new SimpleInfo
		{
			UseIdleTimer = false,
			AnalyticsAction = delegate(AnalyticsTypes AnalyticsType)
			{
				switch (AnalyticsType)
				{
				case AnalyticsTypes.context:
					OrchestrationService.AnalyticsAddViewEvent("context", "TextMessageAlert");
					break;
				case AnalyticsTypes.button1Press:
					OrchestrationService.AnalyticsAddViewEvent("buttonPress", "Okay");
					break;
				}
			},
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				signUpFlowData.ContinueAction?.Invoke();
			}
		};
		signUpFlowData.TimeoutAction = delegate
		{
			simpleInfo.ContinueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("join_text_club_promo_view", simpleInfo);
	}

	private static void ShowSignUpForEarlyIdView(LoyaltySignupFlowValues signUpFlowData)
	{
		LogHelper.Instance.Log("Starting login with phone number.");
		PhoneAndPinInfo phoneAndPinInfo = new PhoneAndPinInfo
		{
			SignInCancelButtonType = SignInCancelButtonTypes.MaybeLater,
			MobilePhoneNumber = signUpFlowData.MobilePhoneNumber,
			IsSignUp = true,
			IsAfterSwipe = signUpFlowData.IsAfterSwipe,
			ShowTextClubOption = !signUpFlowData.TextClubEnabled,
			IdleTimerName = "loyalty_signup_idle_timer",
			SignInAction = delegate(string phone, SecureString pin, bool isTextClubChecked)
			{
				IdleTimer.Stop();
				signUpFlowData.SignUpForEarlyID = true;
				signUpFlowData.MobilePhoneNumber = phone;
				signUpFlowData.Pin = pin;
				signUpFlowData.TextClubEnabled = isTextClubChecked;
				CallUpdateAccountFromEarlyIdView(signUpFlowData);
			},
			ContinueAction = delegate
			{
				IdleTimer.Stop();
				signUpFlowData.SignUpForEarlyID = false;
				signUpFlowData.MobilePhoneNumber = null;
				signUpFlowData.Pin = null;
				signUpFlowData.TextClubEnabled = false;
				QueueUpdateAccount(signUpFlowData);
				signUpFlowData.ContinueAction?.Invoke();
			}
		};
		signUpFlowData.TimeoutAction = delegate
		{
			phoneAndPinInfo.ContinueAction?.Invoke();
		};
		IdleTimer.Start();
		OrchestrationService.ViewShow("phone_and_pin_view", phoneAndPinInfo);
	}

	private static void CallUpdateAccountFromEarlyIdView(LoyaltySignupFlowValues signUpFlowData)
	{
		ServiceLocator.Instance.GetService<IPersonalizationService>().UpdateAccount(resultCallback: delegate(IUpdateAccountResult result)
		{
			UpdateAcountFromEarlyIdViewCallback(result, signUpFlowData);
		}, customerProfileNumber: signUpFlowData.CustomerProfileNumber, emailAddress: signUpFlowData.LoginEmailAddress, phoneNumber: signUpFlowData.MobilePhoneNumber, pin: signUpFlowData.Pin, punchcardOptIn: signUpFlowData.SignUpForPerks, textClubOptIn: signUpFlowData.TextClubEnabled);
	}

	private static void UpdateAcountFromEarlyIdViewCallback(IUpdateAccountResult result, LoyaltySignupFlowValues signUpFlowData)
	{
		if (result.Success)
		{
			OrchestrationService.ViewGoBackSafe("Early ID Signup closed", popOnly: true);
			ShowJoinedTextClubView(signUpFlowData);
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		IViewFrameInstance viewFrameInstance = service?.PeekViewFrame();
		if (service != null && viewFrameInstance != null && viewFrameInstance?.ViewFrame.ViewName == "phone_and_pin_view")
		{
			LogHelper.Instance.Log("returning to phone and pin view to display error");
			PhoneAndPinInfo phoneAndPinInfo = viewFrameInstance.Parameter as PhoneAndPinInfo;
			if (phoneAndPinInfo?.PhoneAndPinViewModel != null)
			{
				phoneAndPinInfo.PhoneAndPinViewModel.MobilePhoneErrorMessageText = result.ErrorMessage;
				phoneAndPinInfo.PhoneAndPinViewModel.PinDisplayText = Resources.phone_and_pin_default_pin_display;
				phoneAndPinInfo.PhoneAndPinViewModel.PinConfirmDisplayText = Resources.phone_and_pin_default_pin_display;
			}
			IdleTimer.Start();
			service.Show();
		}
		else
		{
			IdleTimer.Stop();
			signUpFlowData.ContinueAction?.Invoke();
		}
	}

	private static void QueueUpdateAccount(LoyaltySignupFlowValues signUpFlowData)
	{
		if (!signUpFlowData.SignUpForPerks && !signUpFlowData.SignUpForEarlyID)
		{
			return;
		}
		IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
		LogHelper.Instance.Log("Putting the call to UpdateAccount on the queue.");
		if (_003C_003Ec._003C_003E9__10_0 == null)
		{
			_003C_003Ec._003C_003E9__10_0 = delegate(IUpdateAccountResult result)
			{
				string empty = string.Empty;
				empty = GetErrorsAsString(result.RemoteServiceErrors, empty);
				LogHelper.Instance.Log($"QueueUpdateAccount Queueing of UpdateAccount unsuccessful. {empty}");
			};
		}
		service.UpdateAccount(signUpFlowData.CustomerProfileNumber, signUpFlowData.LoginEmailAddress, signUpFlowData.MobilePhoneNumber, signUpFlowData.Pin, signUpFlowData.SignUpForPerks, signUpFlowData.TextClubEnabled, null, queued: true);
	}

	private static void ContinueCheckout(Action continueCheckoutCallback)
	{
		RemoveIdleTimer();
		continueCheckoutCallback?.Invoke();
	}

	private static void ConfigureIdleTimer(LoyaltySignupFlowValues signUpFlowData)
	{
		IdleTimer = ServiceLocator.Instance.GetService<IIdleTimerService>().CreateIdleTimer("loyalty_signup_idle_timer");
		int timeout = OrchestrationService.TTSGetTimeout("Medium", 20000);
		IdleTimer.Initialize(timeout, delegate
		{
			IdleTimer.Stop();
			signUpFlowData.TimeoutAction?.Invoke();
		});
	}

	private static void RemoveIdleTimer()
	{
		ServiceLocator.Instance.GetService<IIdleTimerService>().RemoveIdleTimer("loyalty_signup_idle_timer");
	}
}
