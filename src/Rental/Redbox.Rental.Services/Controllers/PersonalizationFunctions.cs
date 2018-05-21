using System;
using System.Security;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class PersonalizationFunctions
{
	private static DateTime _loginStartTime;

	public static bool IsSignedIn => Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn ?? false;

	public static bool IsSignInEnabled
	{
		get
		{
			IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
			if (!IsSignedIn)
			{
				return service?.IsEarlyIdEnabled ?? false;
			}
			return false;
		}
	}

	public static bool IsLoyaltyEnabled => ServiceLocator.Instance.GetService<IPersonalizationService>()?.IsLoyaltyEnabled ?? false;

	public static bool IsLoyaltySignedIn
	{
		get
		{
			if (IsSignedIn && IsLoyaltyEnabled && Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession != null)
			{
				return !Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession.LoyaltySession.SignInError;
			}
			return false;
		}
	}

	public static bool IsLoyaltySignInError
	{
		get
		{
			if (IsSignedIn && IsLoyaltyEnabled)
			{
				return Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.SignInError ?? false;
			}
			return false;
		}
	}

	private static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	public static void ShowEmailLogin(SignInCancelButtonTypes signInCancelButtonType)
	{
		LogHelper.Instance.Log("Starting login with email.");
		OrchestrationService.ViewShow("keyboard_sign_in_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = "",
			CancelAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Email login cancel", popOnly: false);
			},
			IsValidEntries = ValidationServices.IsValidEmailAndCode,
			SignInCancelButtonType = signInCancelButtonType,
			ContinueCodeAction = delegate(string emailAddress, SecureString password)
			{
				StartLoginProcess(emailAddress, password, null, null);
			}
		});
	}

	public static void ShowPhoneNumberLogin(SignInCancelButtonTypes signInCancelButtonType)
	{
		LogHelper.Instance.Log("Starting login with phone number.");
		OrchestrationService.ViewShow("phone_and_pin_view", new PhoneAndPinInfo
		{
			SignInCancelButtonType = signInCancelButtonType,
			SignInAction = delegate(string phone, SecureString pin)
			{
				StartLoginProcess(null, null, phone, pin);
			}
		});
	}

	public static void ShowLogoutDialog()
	{
		MessagePopupViewController.ShowLogOutWarning(delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		}, delegate
		{
			SessionAnalyticsService?.AddViewEvent("buttonPress", "back");
			OrchestrationService.ViewGoBackSafe("Sign Out view:hide()", popOnly: false);
		}, delegate
		{
			SessionAnalyticsService?.AddViewEvent("buttonPress", "sign_out");
			Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOut();
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart.ClearAllLoyaltyPointsRecords();
			LogHelper.Instance.Log("User has signed out.  Closing session and returning to welcome screen.");
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
		});
	}

	public static void ShowLoyaltyIsDownDialog()
	{
		MessagePopupViewController.ShowLoyaltyIsDownMessage(delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down message popup timeout", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down message popup continue", popOnly: false);
		});
	}

	private static void StartLoginProcess(string emailAddress, SecureString password, string phoneNumber, SecureString pin)
	{
		OrchestrationService.ViewShow("please_wait_view", null);
		IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
		_loginStartTime = DateTime.Now;
		if (!string.IsNullOrEmpty(emailAddress))
		{
			service.LoginWithEmail(emailAddress, password, ProcessLoginResult);
		}
		else
		{
			service.LoginWithPhoneNumber(phoneNumber, pin, ProcessLoginResult);
		}
	}

	private static void ProcessLoginResult(ILoginResult loginResult)
	{
		TimeSpan timeRemaining = _loginStartTime.AddSeconds(1.0).Subtract(DateTime.Now);
		Task.Run(async delegate
		{
			if (timeRemaining > new TimeSpan(0L))
			{
				await Task.Delay(timeRemaining);
			}
		}).ContinueWith(delegate
		{
			LogHelper.Instance.Log("Processing login result: success={0} responseError={1}", loginResult?.Success, loginResult?.KioskLoginResponseError);
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			OrchestrationService.ViewGoBackSafe("Login process finished - pop PleaseWaitView", popOnly: true);
			ILoginResult loginResult2 = loginResult;
			if ((loginResult2 != null && loginResult2.Success) || loginResult.KioskLoginResponseError == KioskLoginResponseErrors.LoyaltyAccountNotFound)
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession = loginResult.PersonalizationSession;
					OrchestrationService.AnalyticsAddLoggedInCustomerProfileNumber(loginResult.PersonalizationSession?.CustomerProfileNumber);
				}
				OrchestrationService.ResetApplicationIdleTimerTimeout();
				string value = service?.PeekViewFrame()?.ViewFrame?.ViewName;
				"phone_and_pin_view".Equals(value);
				"keyboard_sign_in_view".Equals(value);
				OrchestrationService.ViewGoBackSafe("Login process finished - pop Keyboard / phone view", popOnly: false);
			}
			else if (loginResult.KioskLoginResponseError == KioskLoginResponseErrors.LoyaltyIsDown)
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession = loginResult.PersonalizationSession;
				}
				OrchestrationService.ViewGoBackSafe("Login process finished - pop Keyboard view", popOnly: true);
				ShowLoyaltyIsDownDialog();
			}
			else
			{
				IViewFrameInstance viewFrameInstance = service?.PeekViewFrame();
				if (service != null && viewFrameInstance != null)
				{
					if (viewFrameInstance?.ViewFrame.ViewName == "phone_and_pin_view")
					{
						LogHelper.Instance.Log("returning to phone and pin view to display error");
						PhoneAndPinInfo phoneAndPinInfo = viewFrameInstance.Parameter as PhoneAndPinInfo;
						if (phoneAndPinInfo?.PhoneAndPinViewModel != null)
						{
							phoneAndPinInfo.PhoneAndPinViewModel.ErrorMessageText = loginResult.ErrorMessage;
							phoneAndPinInfo.PhoneAndPinViewModel.SetDefaultDisplays();
						}
					}
					else
					{
						LogHelper.Instance.Log("returning to keyboard view to display error");
						if (viewFrameInstance.Parameter is KeyboardInfo keyboardInfo)
						{
							keyboardInfo.ErrorMessage = loginResult.ErrorMessage;
							keyboardInfo.KeyboardText = string.Empty;
							keyboardInfo.KeyboardCode = string.Empty;
						}
					}
					service.Show();
				}
			}
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	public static void ShowMyPointsError()
	{
		MessagePopupViewController.ShowSimpleMessage(Resources.my_points_error_title, Resources.my_points_error_message, Resources.my_points_error_btn_text, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message button GotIt pressed", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		});
	}

	public static void ShowNotEnoughPointsError()
	{
		MessagePopupViewController.ShowSimpleMessage(Resources.womp_womp, Resources.not_enough_points, Resources.okay, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message button Okay pressed", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		});
	}

	public static void ShowEmailReceiptAddress(string emailAddress)
	{
		LogHelper.Instance.Log("Email Receipt starting.");
		int num = 8000;
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null)
		{
			num = service.GetTimeout("shorter", num);
		}
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		Action continueAction = (continueAction = delegate
		{
			timerService?.RemoveTimer("email_next_timer");
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			if (rentalShoppingCart != null)
			{
				OrchestrationService.InvokeMarketingOptInFlowCallback(rentalShoppingCart.MarketingOptInConfirmationStatus.ToString());
			}
		});
		(timerService?.CreateTimer("email_next_timer", num, num, delegate
		{
			continueAction();
		}))?.Start();
		OrchestrationService.ViewShow("email_receipt_view", new EmailReceiptInfo
		{
			ContinueAction = continueAction,
			EmailAddress = emailAddress
		});
	}

	public static void ShowJoinedPerksConformation(SimpleInfo simpleInfo)
	{
		if (simpleInfo != null)
		{
			simpleInfo.Model = null;
			LogHelper.Instance.Log("Conformation of Joining Perks: starting.");
			simpleInfo.ContinueAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Joined Text Club canceled", popOnly: false);
			};
			OrchestrationService.ViewShow("joined_perks_view", simpleInfo);
		}
	}

	public static void ShowPromoToJoinPerks(object otherInfo)
	{
		LogHelper.Instance.Log("Promo to Join Perks: starting.");
		SimpleInfo simpleInfo = new SimpleInfo
		{
			Obj = otherInfo,
			CancelAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Promo to Join Perks canceled", popOnly: false);
			}
		};
		simpleInfo.ContinueAction = delegate
		{
			OrchestrationService.ViewGoBackSafe("Promo to Join Perks closed", popOnly: false);
			ShowJoinedPerksConformation(simpleInfo);
		};
		OrchestrationService.ViewShow("join_perks_promo_view", simpleInfo);
	}

	public static void ShowPromoToJoinTextClub()
	{
		LogHelper.Instance.Log("Promo to Joined Text Club: starting.");
		OrchestrationService.ViewShow("joined_text_club_promo_view", new SimpleInfo
		{
			ContinueAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Joined Text Club canceled", popOnly: false);
			}
		});
	}

	public static void ShowMemberPerks()
	{
		if (!IsLoyaltySignedIn)
		{
			return;
		}
		LogHelper.Instance.Log("Member Perks starting.");
		ILoyaltySession loyaltySession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession;
		if (loyaltySession != null)
		{
			OrchestrationService.ViewShow("member_perks_view", new PerksMemberInfo
			{
				CancelAction = delegate
				{
					OrchestrationService.ViewGoBackSafe("Member Perks cancel", popOnly: false);
				},
				ExpireDate = loyaltySession.PointsExpirationDate,
				ExpirePoints = (uint)loyaltySession.PointsExpiring,
				Points = (uint)loyaltySession.PointsBalance,
				YearRentals = (uint)loyaltySession.LoyaltyTierCounter,
				TierName = loyaltySession.LoyaltyTierName
			});
		}
	}

	public static void ShowUseMemberPoints()
	{
		if (!IsLoyaltySignedIn)
		{
			return;
		}
		LogHelper.Instance.Log("Member Perks starting.");
		ILoyaltySession loyaltySession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession;
		if (loyaltySession == null)
		{
			return;
		}
		PointsMemberInfo pointsMemberInfo = new PointsMemberInfo
		{
			Points = (uint)loyaltySession.PointsBalance,
			CancelAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Member Points canceled", popOnly: false);
			},
			ContinueAction = delegate
			{
				OrchestrationService.ViewGoBackSafe("Member Points redeemed", popOnly: false);
			}
		};
		MemberPointsViewController.GetRedemptionPoints(delegate(IEstimateRedemptionPointsResult estimatePointsAccruedResult)
		{
			if (estimatePointsAccruedResult != null)
			{
				bool success = estimatePointsAccruedResult.Success;
				OrchestrationService.ViewGoBackSafe("Removed PleaseWaitView", success);
				if (success)
				{
					pointsMemberInfo.EstimateRedemptionPoints = estimatePointsAccruedResult;
					OrchestrationService.ViewShow("member_points_view", pointsMemberInfo);
				}
				else
				{
					ShowLoyaltyIsDownDialog();
				}
			}
			else
			{
				OrchestrationService.ViewGoBackSafe("MemberPoints problem: fail to get EstimateRedemptionPoints", popOnly: true);
			}
		});
		OrchestrationService.ViewShow("please_wait_view", null);
	}
}
