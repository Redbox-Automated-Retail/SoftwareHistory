using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
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

	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

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
			SignInAction = delegate(string phone, SecureString pin, bool textClub)
			{
				StartLoginProcess(null, null, phone, pin);
			},
			IsSignUp = false
		});
	}

	public static void ShowLogoutDialog()
	{
		MessagePopupViewController.ShowLogOutWarning(delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		}, delegate
		{
			SessionAnalyticsService?.AddViewEvent("context", "SignOutDialog");
			SessionAnalyticsService?.AddViewEvent("buttonPress", "sign_out_back");
			AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsSignOutBack");
			OrchestrationService.ViewGoBackSafe("Sign Out view:hide()", popOnly: false);
		}, delegate
		{
			SessionAnalyticsService?.AddViewEvent("buttonPress", "sign_out");
			AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsSignOut");
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

	public static void ShowLoyaltyIsDownOnLoginDialog()
	{
		MessagePopupViewController.ShowLoyaltyIsDownOnLoginMessage(delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down on login message popup timeout", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Loyalty is down on login message popup continue", popOnly: false);
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
					Redbox.Rental.Services.Session.Session.Instance.UserInfo = new UserInfo
					{
						Email = loginResult.PersonalizationSession.LoginEmailAddress
					};
					CheckCartForOfferCodes();
					CheckCartForTivoQueryId();
					CheckCartForTags();
					OrchestrationService.AnalyticsAddLoggedInCustomerProfileNumber(loginResult.PersonalizationSession?.CustomerProfileNumber);
				}
				OrchestrationService.ResetApplicationIdleTimerTimeout();
				bool flag = "keyboard_sign_in_view".Equals(service?.PeekViewFrame()?.ViewFrame?.ViewName);
				OrchestrationService.ViewGoBackSafe("Login process finished - pop keyboard / phone view", popOnly: false);
				if (IsSignedIn & flag)
				{
					LoyaltySignUpController.BeginAfterSignInLoyaltySignUpFlow(loginResult.PersonalizationSession.CustomerProfileNumber, loginResult.PersonalizationSession.LoginEmailAddress, loginResult.PersonalizationSession.MobilePhoneNumber, loginResult.PersonalizationSession.PromptForPerks, loginResult.PersonalizationSession.PromptForEarlyId, loginResult.PersonalizationSession.TextClubEnabled, delegate
					{
						ShowRecommendedTitlesIfNeeded(loginResult);
					});
				}
				else if (IsSignedIn)
				{
					ShowRecommendedTitlesIfNeeded(loginResult);
				}
			}
			else if (loginResult.KioskLoginResponseError == KioskLoginResponseErrors.LoyaltyIsDown)
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null)
				{
					Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession = loginResult.PersonalizationSession;
				}
				OrchestrationService.ViewGoBackSafe("Login process finished - pop Keyboard view", popOnly: true);
				ShowLoyaltyIsDownOnLoginDialog();
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

	private static void ShowRecommendedTitlesIfNeeded(ILoginResult loginResult)
	{
		if (loginResult.PersonalizationSession.RecommendedTitles != null && loginResult.PersonalizationSession.RecommendedTitles.Count > 0 && loginResult.PersonalizationSession.RecommendedTitles[0] != null && loginResult.PersonalizationSession.RecommendedTitles[0].Count > 0)
		{
			IRecommendedTitles recommendedTitles = (ApplicationState.Instance.IsADAAccessible ? RecommendedTitlesPopupViewController.UnrollRecommendedTitles(loginResult.PersonalizationSession.RecommendedTitles[0]) : loginResult.PersonalizationSession.RecommendedTitles[0]);
			int numberOfTitlesToShow = 20;
			List<IBrowseItemModel> list = RecommendedTitlesPopupViewController.CreateBrowseItemsFromRecommendedTitles(recommendedTitles, numberOfTitlesToShow);
			if (list != null && list.Count > 0)
			{
				RecommendedTitlesPopupInfo recommendedTitlesPopupInfo = new RecommendedTitlesPopupInfo();
				recommendedTitlesPopupInfo.RecommendedTitles = recommendedTitles;
				recommendedTitlesPopupInfo.NumberOfTitlesToShow = numberOfTitlesToShow;
				recommendedTitlesPopupInfo.ShowOfferText = recommendedTitles.OfferCode == "RC";
				recommendedTitlesPopupInfo.ShownFromView = "join_perks_promo_view";
				OrchestrationService.ViewShow("recommended_titles_popup_view", recommendedTitlesPopupInfo);
			}
		}
	}

	private static void CheckCartForOfferCodes()
	{
		IRentalShoppingCart currentRentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart;
		if (currentRentalShoppingCart == null || currentRentalShoppingCart.Items.Count <= 0)
		{
			return;
		}
		foreach (IRentalShoppingCartItem item in currentRentalShoppingCart.Items)
		{
			item.OfferCode = GetLoyaltyOfferCode(item.TitleProduct);
		}
	}

	private static void CheckCartForTivoQueryId()
	{
		IRentalShoppingCart currentRentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart;
		if (currentRentalShoppingCart == null || currentRentalShoppingCart.Items.Count <= 0)
		{
			return;
		}
		foreach (IRentalShoppingCartItem item in currentRentalShoppingCart.Items)
		{
			item.TivoQueryId = GetTivoQueryId(item.TitleProduct);
		}
	}

	private static void CheckCartForTags()
	{
		IRentalShoppingCart currentRentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().CurrentRentalShoppingCart;
		if (currentRentalShoppingCart == null || currentRentalShoppingCart.Items.Count <= 0)
		{
			return;
		}
		foreach (IRentalShoppingCartItem item in currentRentalShoppingCart.Items)
		{
			item.Tags = GetTags(item.TitleProduct);
		}
	}

	public static void ShowMyPointsError()
	{
		MessagePopupViewController.ShowSimpleMessage(Resources.my_points_error_title, Resources.my_points_error_message, Resources.my_points_error_btn_text, delegate
		{
			SessionAnalyticsService?.AddViewEvent("context", "NotEnoughPointsDialog");
			SessionAnalyticsService?.AddViewEvent("buttonPress", "not_enough_points_gotit");
			AnalyticsService?.AddButtonPressEvent("PersonalizationFunctionsNotEnoughPointsGotit");
			OrchestrationService.ViewGoBackSafe("Message button GotIt pressed", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		});
	}

	public static void ShowNotEnoughPointsError()
	{
		MessagePopupViewController.ShowSimpleMessage(Resources.sorry, Resources.not_enough_points, Resources.okay, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message button Okay pressed", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("Message view Timed out.", popOnly: false);
		});
	}

	public static string GetLoyaltyOfferCode(ITitleProduct titleProduct)
	{
		return GetDataFromRecommendedTitlesForGivenTitleProduct(titleProduct, (IRecommendedTitles x, IRecommendedTitleId y) => x.OfferCode);
	}

	public static string GetTivoQueryId(ITitleProduct titleProduct)
	{
		return GetDataFromRecommendedTitlesForGivenTitleProduct(titleProduct, (IRecommendedTitles x, IRecommendedTitleId y) => x.TivoQueryId);
	}

	public static List<string> GetTags(ITitleProduct titleProduct)
	{
		return GetDataFromRecommendedTitlesForGivenTitleProduct(titleProduct, (IRecommendedTitles x, IRecommendedTitleId y) => y.Tags);
	}

	private static T GetDataFromRecommendedTitlesForGivenTitleProduct<T>(ITitleProduct titleProduct, Func<IRecommendedTitles, IRecommendedTitleId, T> dataToGet)
	{
		T result = default(T);
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (personalizationSession != null && personalizationSession.RecommendedTitles != null)
		{
			foreach (IRecommendedTitles recommendedTitle in personalizationSession.RecommendedTitles)
			{
				foreach (IRecommendedTitleId item in recommendedTitle)
				{
					if (item.IdType == RecommendedTitlesIdType.ProductGroupID)
					{
						if (item.Id == titleProduct.TitleRollupProductGroupId)
						{
							result = dataToGet(recommendedTitle, item);
							break;
						}
					}
					else if (item.Id == titleProduct.ProductId)
					{
						result = dataToGet(recommendedTitle, item);
						break;
					}
				}
			}
		}
		return result;
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
		object lockObject = new object();
		bool hasActionExecuted = false;
		Action continueAction = (continueAction = delegate
		{
			lock (lockObject)
			{
				if (!hasActionExecuted)
				{
					hasActionExecuted = true;
					timerService?.RemoveTimer("email_next_timer");
					IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
					if (rentalShoppingCart != null)
					{
						CheckoutFunctions.MarketingOptInFlow(rentalShoppingCart.MarketingOptInConfirmationStatus);
					}
				}
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
