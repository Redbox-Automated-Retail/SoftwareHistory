using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class StartViewController : BaseViewController
{
	public class Constants
	{
		public static string BurnInIdleTimerName = "burn-in-timer";

		public static string BurnInViewDurationTimerName = "burn-in-view-duration-timer";

		public static string StartIdleTimerName = "start-idle-timer";

		public static int StartIdleTimerDuration = 30000;

		public static string AdaIdleTimerName = "ada_idle_timer";

		public static int AdaIdleTimerDuration = 15000;

		public static string MarketingAdTimerName = "marketing_ad_check_for_new_active_campaign";

		public static string ZeroTouchDisconnectedCardReaderTimerName = "ZeroTouchDisconnectedCardReaderTimer";

		public static int MarketingAdTimerDuration = 60000;

		public static int MinimumCarouselItems = 8;

		public static int MaximumCarouselItems = 11;
	}

	private static int _startingCardReadFlag;

	private static bool _cardReadUserInteractionStarted;

	private static object _lockObject = new object();

	private const decimal _rentalPrice = 0.99m;

	private static readonly ManualResetEvent _cardReadComplete = new ManualResetEvent(initialState: true);

	private static bool VasEnabled
	{
		get
		{
			IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.ContactlessEnabled && Redbox.Rental.Services.Configuration.Configuration.Instance.PerksPassEnabled && Redbox.Rental.Services.Configuration.Configuration.Instance.ZeroTouchVasEnabled)
			{
				return service?.SupportsVas ?? false;
			}
			return false;
		}
	}

	private static bool IsDarkMode
	{
		get
		{
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.StartViewDarkModeEnabled)
			{
				if (!TimeSpan.TryParse(Redbox.Rental.Services.Configuration.Configuration.Instance.StartViewDarkModeStartTime, out var result))
				{
					result = new TimeSpan(19, 0, 0);
				}
				if (!TimeSpan.TryParse(Redbox.Rental.Services.Configuration.Configuration.Instance.StartViewDarkModeEndTime, out var result2))
				{
					result2 = new TimeSpan(7, 0, 0);
				}
				TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
				if (!(result < result2))
				{
					if (!(timeOfDay >= result))
					{
						return timeOfDay <= result2;
					}
					return true;
				}
				if (timeOfDay >= result)
				{
					return timeOfDay <= result2;
				}
				return false;
			}
			return false;
		}
	}

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static ITimerService TimerService => ServiceLocator.Instance.GetService<ITimerService>();

	private static IAdService AdService => ServiceLocator.Instance.GetService<IAdService>();

	private static string ABTestVersion => Redbox.Rental.Services.Configuration.Configuration.Instance.WelcomeScreenABTestVersion;

	private static string WelcomeScreenTimingABTestVersion => Redbox.Rental.Services.Configuration.Configuration.Instance.WelcomeScreenTimingABTestVersion;

	private static string StartViewAdABTestVersion => Redbox.Rental.Services.Configuration.Configuration.Instance.StartViewAdABTestVersion;

	private static string StartViewRentAndBuyABTestVersion => Redbox.Rental.Services.Configuration.Configuration.Instance.StartViewRentAndBuyABTestVersion;

	private static bool IsTestCarouselSelected
	{
		get
		{
			if (!(ABTestVersion == "C") && !(ABTestVersion == "D") && !(StartViewAdABTestVersion != "A"))
			{
				if (StartViewRentAndBuyABTestVersion != "A" && StartViewRentAndBuyABTestVersion != "F" && StartViewRentAndBuyABTestVersion != "G")
				{
					return StartViewRentAndBuyABTestVersion != "H";
				}
				return false;
			}
			return true;
		}
	}

	private static bool IsClassicCarouselSelected => !IsTestCarouselSelected;

	private static bool IsTestScreenSaverSelected
	{
		get
		{
			if (!(ABTestVersion == "B") && !(ABTestVersion == "D"))
			{
				return WelcomeScreenTimingABTestVersion == "B";
			}
			return true;
		}
	}

	private static bool IsClassicScreenSaverSelected => !IsTestScreenSaverSelected;

	private static bool UseContactless
	{
		get
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			IEMVReaderHealth service2 = ServiceLocator.Instance.GetService<IEMVReaderHealth>();
			if (service.ZeroTouchContactlessEnabled)
			{
				return !service2.ContactlessOffDueToDisconnectThresholdReached;
			}
			return false;
		}
	}

	public static void ConfigureStartView()
	{
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
		FrameworkElement startViewUserControl = GetStartViewUserControl();
		if (startViewUserControl != null)
		{
			StartViewModel startViewModel = new StartViewModel();
			ConfigureScreenSaver(startViewModel);
			ConfigureButtonsAndText(startViewModel, startViewUserControl);
			startViewModel.OnToggleLanguage += ToggleLanguage;
			startViewModel.OnToggleADAMode += ToggleAdaMode;
			startViewModel.OnRentMoviesButtonClicked += GoToMovieBrowseView;
			startViewModel.OnRentOrBuyGameButtonClicked += GoToGameView;
			startViewModel.OnRentAndBuyMoviesButtonClicked += GoToRentAndBuyMoviesBrowseView;
			startViewModel.OnRentAndBuyGamesButtonClicked += GoToRentAndBuyGamesBrowseView;
			startViewModel.OnPickupButtonClicked += GoToPickupReservationSwipeView;
			startViewModel.OnReturnButtonClicked += GoToReturnProductView;
			startViewModel.OnHelpButtonClicked += GoToHelpDocumentView;
			startViewModel.OnBannerClicked += BannerClicked;
			startViewModel.OnBuyMoviesButtonClicked += GoToForSaleMovieBrowseView;
			startViewModel.OnBuyGamesButtonClicked += GoToForSaleGamesBrowseView;
			startViewModel.OnSignInButtonClicked += ProcessSignInSignOutCommand;
			startViewModel.OnSignInButtonTooltipClicked += SignInButtonTooltipClicked;
			ConfigureBurnInTimers(startViewModel);
			ConfigureStartViewIdleTimer();
			ConfigureAdaIdleTimer();
			ConfigureBanner(startViewModel);
			ConfigureMarketingAdTimer();
			ConfigureTTS(startViewModel);
			if (BaseViewController.ApplicationState.IsABEOn)
			{
				TurnAdaOn();
			}
			else
			{
				TurnAdaOff();
			}
			ConfigureCarousel(startViewModel);
			startViewModel.UseTestScreenSaver = false;
			startViewModel.ScreenSaverModel.IsAnimationOn = false;
			if (IsTestCarouselSelected && !startViewModel.IsReturnOnlyMode)
			{
				if (StartViewRentAndBuyABTestVersion == "D" || StartViewRentAndBuyABTestVersion == "E")
				{
					startViewModel.UseRentAndBuyTestStartView = true;
					startViewModel.UseTestStartView = false;
				}
				else
				{
					startViewModel.UseRentAndBuyTestStartView = false;
					startViewModel.UseTestStartView = true;
				}
				startViewModel.UseClassicStartView = false;
				startViewModel.CarouselModel.IsAnimationOn = false;
			}
			else
			{
				startViewModel.UseRentAndBuyTestStartView = false;
				startViewModel.UseTestStartView = false;
				startViewModel.UseClassicStartView = true;
				startViewModel.CarouselModel.IsAnimationOn = true;
			}
			startViewUserControl.DataContext = startViewModel;
			CommandManager.InvalidateRequerySuggested();
			startViewModel?.AdImpression?.Start();
			if (startViewModel?.AdImpression?.Ad is IVistarAd)
			{
				BaseViewController.RecordAdImpression(startViewModel?.AdImpression);
			}
			if (OrchestrationService.IsRentalSessionActive())
			{
				OrchestrationService.ResumeIdleTimer("idle_timer");
			}
		}
		_cardReadUserInteractionStarted = false;
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		if (instance != null && instance.ZeroTouchEnabled)
		{
			ZeroTouchCardRead();
		}
	}

	public static void OnCardReaderConnected()
	{
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		if (instance != null && instance.ZeroTouchEnabled)
		{
			ZeroTouchCardRead();
		}
	}

	private static void ZeroTouchCardRead()
	{
		string text = OrchestrationService.PeekView();
		if (text != "start_view")
		{
			BaseViewController.Log($"ZeroTouch Skipped - Current View is Not Start View: {text}");
			return;
		}
		ISession instance = Redbox.Rental.Services.Session.Session.Instance;
		if (instance != null && instance.HasReceivedUserInteractionThatStartsTimedSession)
		{
			BaseViewController.Log($"ZeroTouch Skipped - Current session already has user interaction {Redbox.Rental.Services.Session.Session.Instance?.SessionStartTriggerAction}");
			return;
		}
		ISession instance2 = Redbox.Rental.Services.Session.Session.Instance;
		if (instance2 != null && instance2.IsInAddProductFlow)
		{
			BaseViewController.Log("ZeroTouch Skipped - User is in add product flow");
			return;
		}
		ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service != null)
		{
			try
			{
				StopZeroTouchDisconnectedCardReaderTimer();
				if (Interlocked.CompareExchange(ref _startingCardReadFlag, 1, 0) == 1)
				{
					LogHelper.Instance.Log("StartViewController card read is already being started - exiting in method");
				}
				else
				{
					_cardReadUserInteractionStarted = false;
					service.StopCardRead(CancelReason.RestartingCardRead);
					IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
					DeviceInputType deviceInputType = (UseContactless ? DeviceInputType.CS : DeviceInputType.S);
					if (service is ClassicTrackDataService)
					{
						deviceInputType = DeviceInputType.M;
					}
					CardReadAttempt cardReadAttempt = new CardReadAttempt
					{
						DeviceInputType = deviceInputType,
						StartDateTime = DateTime.Now
					};
					Redbox.Rental.Services.Session.Session.Instance.CardReadAttemptCollection.Add(cardReadAttempt);
					VasMode vasMode = VasMode.PayOnly;
					if (VasEnabled && deviceInputType.HasFlag(DeviceInputType.C))
					{
						ISession instance3 = Redbox.Rental.Services.Session.Session.Instance;
						bool? obj;
						if (instance3 == null)
						{
							obj = null;
						}
						else
						{
							IPersonalizationSession personalizationSession = instance3.PersonalizationSession;
							obj = ((personalizationSession != null) ? new bool?(!personalizationSession.IsLoggedIn) : ((bool?)null));
						}
						if (obj ?? true)
						{
							vasMode = VasMode.VasAndPay;
						}
					}
					CardReadRequestInfo request = new CardReadRequestInfo
					{
						Timeout = (service2?.ZeroTouchReadTimeout ?? 600000),
						IsZeroTouchRead = true,
						InputType = deviceInputType,
						VasMode = vasMode,
						SessionId = (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ShoppingSessionId ?? default(Guid))
					};
					service.StartCardRead(request, delegate(IReadCardJob readCardJob)
					{
						LogHelper.Instance.Log($"Card read Starting - RequestId: {readCardJob.RequestId}");
						cardReadAttempt.CardReadJobCommandRequestId = readCardJob.RequestId;
					}, OnCardReadUserInteractionStarted, OnCardReadComplete);
					_cardReadComplete.Reset();
				}
				return;
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("Exception occurred in ZeroTouchCardRead!", e);
				return;
			}
			finally
			{
				_startingCardReadFlag = 0;
				Task.Factory.StartNew(delegate
				{
					StartViewModel startViewModelFromView = GetStartViewModelFromView();
					FrameworkElement startViewUserControl = GetStartViewUserControl();
					if (startViewModelFromView != null && startViewUserControl != null)
					{
						SetCultureSpecificText(startViewModelFromView, startViewUserControl, hasInstockGames: false);
					}
				}, CancellationToken.None, TaskCreationOptions.None, BaseViewController.GetViewService()?.UITaskScheduler);
			}
		}
		LogHelper.Instance.Log("TrackDataService not Connected - Creating ZeroTouchDisconnectedCardReaderTimer");
		CreateAndStartZeroTouchDisconnectedCardReaderTimer();
	}

	private static void CreateAndStartZeroTouchDisconnectedCardReaderTimer()
	{
		ITimer timer = TimerService.GetTimer(Constants.ZeroTouchDisconnectedCardReaderTimerName);
		if (timer == null)
		{
			timer = TimerService.CreateTimer(Constants.ZeroTouchDisconnectedCardReaderTimerName, null, 15000, delegate
			{
				ZeroTouchCardRead();
			});
		}
		timer?.Start();
	}

	private static void StopZeroTouchDisconnectedCardReaderTimer()
	{
		OrchestrationService.StopTimer(Constants.ZeroTouchDisconnectedCardReaderTimerName);
	}

	private static void OnCardReadComplete(CardReadRequestInfo request, CardReadCompleteDetails readDetails)
	{
		BaseViewController.Log($"StartViewController.OnCardReadComplete - RequestId {readDetails?.RequestId}");
		_cardReadComplete.Set();
		ITrackData trackData = readDetails?.TrackData;
		if (trackData != null && trackData.ReadStatus != ResponseStatus.Timeout)
		{
			ResponseStatus? readStatus = trackData.ReadStatus;
			ResponseStatus responseStatus = ResponseStatus.Errored;
			if (readStatus.GetValueOrDefault() != responseStatus || !readStatus.HasValue || (!trackData.Errors.Any((Redbox.KioskEngine.ComponentModel.Error x) => string.Compare(x.Description, "TimeOut", ignoreCase: true) == 0) && !trackData.Errors.Any((Redbox.KioskEngine.ComponentModel.Error x) => string.Compare(x.Description, "READERDISCONNECT", ignoreCase: true) == 0)))
			{
				if (trackData != null && trackData.ReadStatus == ResponseStatus.Cancelled)
				{
					BaseViewController.Log("ZeroTouch Card Read Was Cancelled");
					return;
				}
				Redbox.Rental.Services.Session.Session.Instance.TrackData = trackData;
				Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode = true;
				OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ZeroTouch));
				BaseViewController.AnalyticsService?.AddSessionProperty("IsZeroTouch", Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode);
				CardFunctions.CardReadComplete(SwipeViewType.ZeroTouchReservationPickup, trackData, readDetails?.CardRemoved ?? false, readDetails?.RequestId, delegate
				{
					LogHelper.Instance.Log("CONTINUE");
					OnContinueCardReadComplete(trackData);
				}, delegate
				{
					LogHelper.Instance.Log("ERROR");
					OnErrorCardReadComplete(trackData);
				});
				return;
			}
		}
		BaseViewController.Log($"OnCardReadComplete Start New Read - TrackData.ReadStatus: {trackData?.ReadStatus}");
		ZeroTouchCardRead();
	}

	private static void ContinueAfterLogin()
	{
		BaseViewController.Log("After Login Continue");
		ServiceLocator.Instance.GetService<IViewService>().Show();
	}

	private static void OnContinueCardReadComplete(ITrackData trackData)
	{
		LogHelper.Instance.Log("StartViewController.OnContinueCardReadComplete BEGIN");
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		IList<IReservation> list = null;
		if (service != null && service.ZeroTouchPickupEnabled)
		{
			list = ReservationFunctions.GetAccountReservations();
		}
		if (service.ZeroTouchGoogleTwoTap && trackData.WalletTypeIsGoogleAndHasPayAndHasVas())
		{
			Task.Factory.StartNew(delegate
			{
				MessagePopupViewController.ShowMessagePopupViewGoogleWorkaroundError(delegate
				{
					OrchestrationService.ViewGoBackSafe("ShowMessagePopupViewGoogleWorkaroundError time out action", popOnly: false);
					Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode = false;
					PrepareToLeaveStartView();
					ServiceLocator.Instance.GetService<IViewService>()?.Push("browse_view", BrowseFunctions.GetBrowseViewparameters());
					SignInViewController.MobileSignInZeroTouchSwipeAction();
				}, delegate
				{
					OrchestrationService.ViewGoBackSafe("ShowMessagePopupViewGoogleWorkaroundError continue action", popOnly: false);
					Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode = false;
					PrepareToLeaveStartView();
					ServiceLocator.Instance.GetService<IViewService>()?.Push("browse_view", BrowseFunctions.GetBrowseViewparameters());
					SignInViewController.MobileSignInZeroTouchSwipeAction();
				});
			}, CancellationToken.None, TaskCreationOptions.None, BaseViewController.GetViewService()?.UITaskScheduler);
			ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.ReportAuthorizeResult(authorizeSuccessful: true);
		}
		else if (trackData.HasVas && !string.IsNullOrEmpty(trackData.VasIdentifier))
		{
			Task.Factory.StartNew(delegate
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode = false;
				PrepareToLeaveStartView();
				ServiceLocator.Instance.GetService<IViewService>()?.Push("browse_view", BrowseFunctions.GetBrowseViewparameters());
				PersonalizationFunctions.StartLoginProcess(null, null, null, null, trackData.VasIdentifier, trackData.WalletType, delegate(LoginType loginType, ILoginResult loginResult)
				{
					AfterLoginAction(loginResult, ContinueAfterLogin);
				});
			}, CancellationToken.None, TaskCreationOptions.None, BaseViewController.GetViewService()?.UITaskScheduler);
			ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.ReportAuthorizeResult(authorizeSuccessful: true);
		}
		else if (trackData.CardSourceTypeIsMobileAndHasPayAndNotVas())
		{
			Task.Factory.StartNew(delegate
			{
				MessagePopupViewController.ShowMessagePopupViewMobilePaymentCardNoPassFoundError(delegate
				{
					OrchestrationService.ViewGoBackSafe("ShowMessagePopupViewMobilePaymentCardNoPassFoundError time out action", popOnly: false);
					OnErrorCardReadComplete(trackData);
				}, delegate
				{
					OrchestrationService.ViewGoBackSafe("ShowMessagePopupViewMobilePaymentCardNoPassFoundError continue action", popOnly: false);
					OnErrorCardReadComplete(trackData);
				});
			}, CancellationToken.None, TaskCreationOptions.None, BaseViewController.GetViewService()?.UITaskScheduler);
		}
		else if (list != null && list.Any())
		{
			BaseViewController.AnalyticsService?.AddSessionProperty("SessionType", AnalyticsConstants.SessionType.Pickup.ToString());
			Task.Factory.StartNew(delegate
			{
				PrepareToLeaveStartView();
				Redbox.Rental.Services.Session.Session.Instance.IsInReservationFlow = true;
				ReservationFunctions.InvokeZeroTouchContinueReservation();
			}, CancellationToken.None, TaskCreationOptions.None, BaseViewController.GetViewService()?.UITaskScheduler);
		}
		else if (service != null && service.ZeroTouchReturnEnabled)
		{
			BaseViewController.AnalyticsService?.AddSessionProperty("SessionType", AnalyticsConstants.SessionType.Return.ToString());
			Task.Factory.StartNew(delegate
			{
				GoToReturnProductView(isZeroTouch: true);
			}, CancellationToken.None, TaskCreationOptions.None, BaseViewController.GetViewService()?.UITaskScheduler);
			ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.ReportAuthorizeResult(authorizeSuccessful: true);
		}
		else
		{
			LogHelper.Instance.Log("No ZeroTouch Modes Qualify, Closing Session.");
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("ZeroTouchReturnNotEnabled");
		}
	}

	private static void OnErrorCardReadComplete(ITrackData trackData)
	{
		LogHelper.Instance.Log("StartViewController.OnErrorCardReadComplete BEGIN");
		ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("ZeroTouchCardReadError");
	}

	private static void AfterLoginAction(ILoginResult loginResult, Action continueAction)
	{
		if (loginResult != null && loginResult.Success)
		{
			OrchestrationService.ViewShow("sign_in_thanks_view", new SignInThanksViewParameters
			{
				DelaySeconds = 3,
				EmailAddress = loginResult.PersonalizationSession.LoginEmailAddress,
				PopOnlyOnViewGoBack = true,
				ContinueAction = delegate
				{
					continueAction?.Invoke();
					OrchestrationService.UnwindToSafe("browse_view", discard: false, "Login process finished - pop signin view");
					if (PersonalizationFunctions.IsSignedIn)
					{
						OrchestrationService.StopIdleTimer("idle_timer");
						PersonalizationFunctions.ShowRecommendedTitlesIfNeeded(loginResult);
					}
				}
			});
		}
		else
		{
			PersonalizationFunctions.ShowPerksPassLoginError(loginResult);
		}
	}

	private static void OnCardReadUserInteractionStarted(Guid cardReadJobCommandRequestId)
	{
		LogHelper.Instance.Log("Zero touch card read started.");
		ICardReadAttempt cardReadAttempt = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection.GetByCardReadJobCommandRequestId(cardReadJobCommandRequestId);
		if (cardReadAttempt != null && !cardReadAttempt.AttemptedCancel)
		{
			_cardReadUserInteractionStarted = true;
		}
	}

	private static StartViewModel GetStartViewModel()
	{
		FrameworkElement startViewUserControl = GetStartViewUserControl();
		if (startViewUserControl != null)
		{
			return startViewUserControl.Dispatcher.Invoke(() => startViewUserControl.DataContext) as StartViewModel;
		}
		return null;
	}

	private static void ConfigureTTS(StartViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => StartViewTextToSpeech.GetSpeechControl(GetStartViewModel);
		}
	}

	private static FrameworkElement GetStartViewUserControl()
	{
		IViewFrameInstance obj = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetWPFGridChild(obj?.Id);
	}

	private static void ConfigureButtonsAndText(StartViewModel startViewModel, FrameworkElement startViewUserControl)
	{
		if (startViewModel == null)
		{
			startViewModel = GetStartViewModelFromView();
		}
		if (startViewUserControl == null)
		{
			startViewUserControl = GetStartViewUserControl();
		}
		if (startViewModel != null && startViewUserControl != null)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			bool flag = (service?.GetInStockGameCount() ?? 0) > 0;
			DateTime? returnOnlyModeDate = service.GetStore().ReturnOnlyModeDate;
			bool flag2 = (startViewModel.IsReturnOnlyMode = returnOnlyModeDate.HasValue && returnOnlyModeDate.Value < DateTime.Now);
			SetCultureSpecificText(startViewModel, startViewUserControl, flag);
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			bool flag4 = service2.CanSellUsedMovies && service2.PurchasePrice > 0m;
			bool flag5 = StartViewRentAndBuyABTestVersion == "G" || StartViewRentAndBuyABTestVersion == "H";
			startViewModel.RentMoviesGamesTextVisibility = (flag5 ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.RentAndBuyMoviesGamesTextVisibility = ((!flag5) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.BuyMoviesOrGamesButtonVisibility = ((!flag4 || flag2 || flag5) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.RentAndBuyMoviesButtonTextVisibility = ((startViewModel.BuyMoviesOrGamesButtonVisibility != Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.BuyGamesButtonVisibility = ((!flag4 || !service2.CanSellUsedGames || flag2 || flag5 || Redbox.Rental.Services.Configuration.Configuration.Instance.EndGames) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.RentAndBuyGamesButtonTextVisibility = ((startViewModel.BuyGamesButtonVisibility != Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.RentOrBuyGamesButtonVisibility = (((Redbox.Rental.Services.Configuration.Configuration.Instance.EndGameRentals || Redbox.Rental.Services.Configuration.Configuration.Instance.EndGames) | flag2) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.SecondaryButtonsColumn = ((startViewModel.BuyMoviesOrGamesButtonVisibility != Visibility.Visible && startViewModel.BuyGamesButtonVisibility != Visibility.Visible) ? 1 : 2);
			startViewModel.SecondaryButtonsMargin = ((startViewModel.BuyMoviesOrGamesButtonVisibility != Visibility.Visible && startViewModel.BuyGamesButtonVisibility != Visibility.Visible) ? new Thickness(0.0, 0.0, 115.0, 0.0) : new Thickness(0.0, 0.0, 15.0, 0.0));
			List<IKioskProduct> products = null;
			Redbox.KioskEngine.ComponentModel.ErrorList source = service?.GetProducts(BrowseViewFilter.ForSale, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, null, null, out products);
			bool buyGamesEnabled = !source.Any() && products.Any((IKioskProduct x) => ((ITitleProduct)x).TitleFamily == TitleFamily.Games);
			bool buyMoviesEnabled = !source.Any() && products.Any((IKioskProduct x) => ((ITitleProduct)x).TitleFamily == TitleFamily.Movies);
			startViewModel.RentGamesEnabled = flag;
			startViewModel.BuyGamesEnabled = buyGamesEnabled;
			startViewModel.BuyMoviesEnabled = buyMoviesEnabled;
			ServiceLocator.Instance.GetService<IMacroService>()["Start:BuyMoviesOrGames"] = "Movies" + (service2.CanSellUsedGames ? " and Games" : string.Empty);
			SetSignInButtonVisibility(startViewModel);
			if (IsDarkMode)
			{
				SetDarkMode(startViewModel, startViewUserControl);
			}
			else
			{
				SetLightMode(startViewModel, startViewUserControl);
			}
		}
	}

	private static void SetSignInButtonVisibility(StartViewModel startViewModel)
	{
		IPersonalizationService service = ServiceLocator.Instance.GetService<IPersonalizationService>();
		bool flag = !startViewModel.IsReturnOnlyMode && (service?.IsEarlyIdEnabled ?? false);
		bool flag2 = IsTestCarouselSelected && !(StartViewRentAndBuyABTestVersion == "D") && !(StartViewRentAndBuyABTestVersion == "E");
		bool flag3 = flag && (!BaseViewController.ApplicationState.IsADAAccessible || !flag2);
		startViewModel.SignInVisibility = ((!flag3) ? Visibility.Collapsed : Visibility.Visible);
		startViewModel.SignInButtonTooltipVisibility = ((!flag3 || PersonalizationFunctions.IsSignedIn) ? Visibility.Collapsed : Visibility.Visible);
		startViewModel.AdaSignInVisibility = ((!flag || !(BaseViewController.ApplicationState.IsADAAccessible & flag2)) ? Visibility.Collapsed : Visibility.Visible);
		startViewModel.SignInButtonEnabled = flag;
	}

	private static void SetLightMode(StartViewModel startViewModel, FrameworkElement startViewUserControl)
	{
		startViewModel.NonBurnInBackgroundBrush = System.Windows.Media.Brushes.White;
		startViewModel.AdaTurnedOnLabelBackgroundBrush = System.Windows.Media.Brushes.White;
		startViewModel.AdaTurnedOnLabelForegroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(72, 27, 84));
		startViewModel.HandicapButtonStyle = WPFFunctions.GetStyle(startViewUserControl, "white_warmpurple_border_button_style");
		startViewModel.HandicapButtonIconOffForegroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(121, 45, 141));
		startViewModel.HandicapButtonIconOnForegroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(121, 45, 141));
		startViewModel.EllipseFillBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(121, 45, 141));
		startViewModel.TextBlockForegroundBrush = System.Windows.Media.Brushes.White;
		startViewModel.EspanolButtonStyle = WPFFunctions.GetStyle(startViewUserControl, "white_warmpurple_border_button_style");
		startViewModel.HelpButtonStyle = WPFFunctions.GetStyle(startViewUserControl, "white_warmpurple_border_button_style");
	}

	private static void SetDarkMode(StartViewModel startViewModel, FrameworkElement startViewUserControl)
	{
		startViewModel.NonBurnInBackgroundBrush = System.Windows.Media.Brushes.Black;
		startViewModel.AdaTurnedOnLabelBackgroundBrush = System.Windows.Media.Brushes.Black;
		startViewModel.AdaTurnedOnLabelForegroundBrush = System.Windows.Media.Brushes.White;
		startViewModel.HandicapButtonStyle = WPFFunctions.GetStyle(startViewUserControl, "black_with_white_border_button_style");
		startViewModel.HandicapButtonIconOffForegroundBrush = System.Windows.Media.Brushes.White;
		startViewModel.HandicapButtonIconOnForegroundBrush = System.Windows.Media.Brushes.White;
		startViewModel.EllipseFillBrush = System.Windows.Media.Brushes.White;
		startViewModel.TextBlockForegroundBrush = System.Windows.Media.Brushes.Black;
		startViewModel.EspanolButtonStyle = WPFFunctions.GetStyle(startViewUserControl, "black_with_white_border_button_style");
		startViewModel.HelpButtonStyle = WPFFunctions.GetStyle(startViewUserControl, "black_with_white_border_button_style");
	}

	private static void ConfigureScreenSaver(StartViewModel startViewModel)
	{
		MarketingScreenSaverModel marketingScreenSaverModel = new MarketingScreenSaverModel();
		marketingScreenSaverModel.StartButtonText = Resources.screen_saver_start_button_text;
		marketingScreenSaverModel.BrowseCommand = DynamicRouting.RegisterRoutedCommand<PerksPopupInfo>(new DynamicRoutedCommand(), delegate
		{
			GoToStartViewFromScreenSaver();
		});
		marketingScreenSaverModel.ImageModels = ConfigureScreenSaverImages();
		marketingScreenSaverModel.WelcomeScreenTimingABTestVersion = WelcomeScreenTimingABTestVersion;
		marketingScreenSaverModel.OnLeaveLastImage = (Action)Delegate.Combine(marketingScreenSaverModel.OnLeaveLastImage, new Action(GoToStartViewFromScreenSaverLastImage));
		startViewModel.ScreenSaverModel = marketingScreenSaverModel;
	}

	private static ObservableCollection<ClickableImageModel> ConfigureScreenSaverImages()
	{
		ObservableCollection<ClickableImageModel> observableCollection = new ObservableCollection<ClickableImageModel>();
		if (WelcomeScreenTimingABTestVersion == "B")
		{
			observableCollection.Add(new ClickableImageModel
			{
				ImageUrl = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/COR_7206.2_May_KioskWelcome_Dog.jpg",
				ClickAction = delegate
				{
					GoToTitleDetailsViewFromScreenSaver(215522L);
				}
			});
			observableCollection.Add(new ClickableImageModel
			{
				ImageUrl = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/COR_7206.1_May_KioskWelcome_Uncharted.jpg",
				ClickAction = delegate
				{
					GoToTitleDetailsViewFromScreenSaver(215468L);
				}
			});
			observableCollection.Add(new ClickableImageModel
			{
				ImageUrl = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/COR-7131.1_KioskWelcome_SpidermanNWH.jpg",
				ClickAction = delegate
				{
					GoToTitleDetailsViewFromScreenSaver(215139L);
				}
			});
		}
		return observableCollection;
	}

	public static bool IsProductInCarousel(long productId)
	{
		bool result = false;
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView?.CarouselModel?.CarouselDisplayProductModels != null)
		{
			foreach (DisplayProductModel carouselDisplayProductModel in startViewModelFromView.CarouselModel.CarouselDisplayProductModels)
			{
				if (!(carouselDisplayProductModel.Data is IKioskProduct kioskProduct))
				{
					continue;
				}
				switch (kioskProduct.KioskProductType)
				{
				case KioskProductType.Title:
					if ((kioskProduct as ITitleProduct).ProductId == productId)
					{
						result = true;
					}
					break;
				case KioskProductType.MDV:
					if (kioskProduct is MDVProduct mDVProduct && mDVProduct.TitleProducts.Any((ITitleProduct product) => product.ProductId == productId))
					{
						result = true;
					}
					break;
				case KioskProductType.TitleRollup:
					if (kioskProduct is TitleRollupProduct titleRollupProduct && titleRollupProduct.KioskProducts.Any((ITitleProduct product) => product.ProductId == productId))
					{
						result = true;
					}
					break;
				}
			}
		}
		return result;
	}

	public static void ReloadCarousel(bool force)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null && ((startViewModelFromView.CarouselModel.CarouselDisplayProductModels.Count() < Constants.MaximumCarouselItems) | force))
		{
			LogHelper.Instance.Log("Reloading Carousel");
			BaseViewController.ApplicationState.NeedToReloadCarousel = false;
			ConfigureButtonsAndText(startViewModelFromView, null);
			ConfigureCarousel(GetStartViewModelFromView());
		}
	}

	public static void LeaveStartView(IViewFrameInstance viewFrameInstance)
	{
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<StartViewModel>(viewFrameInstance.Id)?.AdImpression);
		(IdleTimerService?.GetIdleTimer(Constants.BurnInIdleTimerName))?.Stop();
		(TimerService?.GetTimer(Constants.BurnInViewDurationTimerName))?.Stop();
		(IdleTimerService?.CreateIdleTimer(Constants.StartIdleTimerName))?.Stop();
		(IdleTimerService?.GetIdleTimer(Constants.AdaIdleTimerName))?.Stop();
		ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (!_cardReadUserInteractionStarted)
		{
			service?.StopCardRead(CancelReason.LeavingStartView);
			_cardReadComplete.WaitOne(3000);
		}
		StopZeroTouchDisconnectedCardReaderTimer();
	}

	private static void ConfigureMarketingAdTimer()
	{
		(TimerService?.CreateTimer(Constants.MarketingAdTimerName, Constants.MarketingAdTimerDuration, Constants.MarketingAdTimerDuration, delegate
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			if (service.Peek() == service.GetStartOverViewName())
			{
				IMarketingAdService service2 = ServiceLocator.Instance.GetService<IMarketingAdService>();
				if (service2 != null && service2.CheckForNewActiveCampaign())
				{
					LogHelper.Instance.Log("activating new marketing ads campaign");
					service2?.ActivateCampaigns();
					ConfigureBanner(GetStartViewModelFromView());
				}
			}
		}))?.Start();
	}

	private static void CloseTitleRollupOverlay()
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			CloseTitleRollupOverlay(startViewModelFromView);
		}
	}

	private static void CloseTitleRollupOverlay(StartViewModel startViewModel)
	{
		if (startViewModel.CarouselModel.TitleRollupOverlayModel.Visibility == Visibility.Visible)
		{
			startViewModel.CarouselModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
			startViewModel.CarouselModel.IsAnimationOn = IsClassicCarouselSelected;
			startViewModel.CarouselTestModel.IsAnimationOn = IsTestCarouselSelected;
		}
	}

	private static void BannerClicked()
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView == null)
		{
			return;
		}
		CloseTitleRollupOverlay(startViewModelFromView);
		switch (startViewModelFromView.BannerType)
		{
		case StartViewBannerType.bluray:
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.PageNumber = 1;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = BrowseSelectedSort.Bluray;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.Bluray;
			PrepareToLeaveStartView();
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.BluRayBannerClicked));
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartBannerBluRays");
			OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			break;
		case StartViewBannerType.game:
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = TitleFamily.Games;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = null;
			PrepareToLeaveStartView();
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.GamesBannerClicked));
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartBannerGames");
			OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			break;
		case StartViewBannerType.collections:
		{
			ICollectionProduct collection = null;
			if (startViewModelFromView.BannerCollectionId != "ALL")
			{
				long result = 0L;
				long.TryParse(startViewModelFromView.BannerCollectionId, out result);
				IDataService service = ServiceLocator.Instance.GetService<IDataService>();
				if (service != null)
				{
					service.GetCollection(result, out collection);
					if (collection == null)
					{
						LogHelper.Instance.Log("collection banner click: invalid collection id {0}.  Showing all collections instead.", startViewModelFromView.BannerCollectionId);
					}
				}
			}
			PrepareToLeaveStartView();
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.CollectionsBannerClicked));
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartBannerCollections");
			ShowCollectionBrowseView(collection);
			break;
		}
		case StartViewBannerType.none:
		case StartViewBannerType.ad:
			break;
		}
	}

	private static void ConfigureBanner(StartViewModel startViewModel)
	{
		BaseViewController.RecordAdImpression(startViewModel?.AdImpression);
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		bool flag = service?.ShowBlurayBrowse ?? false;
		bool flag2 = service?.ShowGameBanner ?? false;
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		string text = null;
		StartViewBannerType startViewBannerType = StartViewBannerType.none;
		BitmapImage bitmapImage = null;
		bool flag3 = ABTestVersion != "C" && ABTestVersion != "D" && StartViewAdABTestVersion != "B";
		bool flag4 = StartViewAdABTestVersion == "C";
		bool flag5 = StartViewRentAndBuyABTestVersion != "A" && StartViewRentAndBuyABTestVersion != "F" && StartViewRentAndBuyABTestVersion != "G" && StartViewRentAndBuyABTestVersion != "H";
		bool flag6 = flag3 | flag4 | flag5;
		if ((!BaseViewController.ApplicationState.IsADAAccessible && AdService != null) & flag6)
		{
			AdLocation adLocation = ((!(StartViewAdABTestVersion == "C")) ? AdLocation.Welcome : AdLocation.TitleDetail);
			IAd ad = AdService?.GetScreenAd(adLocation);
			if (ad != null)
			{
				startViewBannerType = StartViewBannerType.ad;
				bitmapImage = ad?.Image;
				startViewModel.AdImpression = ((ad != null) ? new AdImpression
				{
					Ad = ad,
					AdLocation = AdLocation.Welcome
				} : null);
			}
		}
		if (bitmapImage == null)
		{
			Image image = service2.GetBitmap("banner_image");
			if (image != null)
			{
				IResource resource = service2.GetResource("banner_image");
				if (resource != null)
				{
					IAspect aspect = resource.GetAspect("content");
					if (aspect != null)
					{
						text = (aspect.Properties.ContainsKey("collectionid") ? aspect.Properties["collectionid"].ToUpper() : null);
						if (text == string.Empty)
						{
							text = null;
						}
					}
				}
				if (string.IsNullOrEmpty(text) || OrchestrationService.RentalIsCollectionsEnabled())
				{
					startViewBannerType = StartViewBannerType.collections;
				}
			}
			if (startViewBannerType == StartViewBannerType.none)
			{
				if (flag)
				{
					startViewBannerType = StartViewBannerType.bluray;
					image = service2.GetBitmap("image_bluray_banner");
				}
				else if (flag2)
				{
					startViewBannerType = StartViewBannerType.game;
					image = service2.GetBitmap("image_game_banner");
				}
				else
				{
					image = null;
				}
			}
			if (image != null)
			{
				bitmapImage = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
			}
		}
		if (startViewModel != null)
		{
			startViewModel.BannerCollectionId = text;
			startViewModel.BannerType = startViewBannerType;
			startViewModel.BannerImage = bitmapImage;
			startViewModel.BannerVisibility = ((!(((bitmapImage != null) & flag3) | flag4 | flag5)) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void ConfigureAdaIdleTimer()
	{
		(IdleTimerService?.CreateIdleTimer(Constants.AdaIdleTimerName)).Initialize(Constants.AdaIdleTimerDuration, delegate
		{
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service != null && !service.TTSEnabled)
			{
				TurnAdaOff();
				ConfigureCarousel(GetStartViewModelFromView());
			}
		});
	}

	private static void ConfigureStartViewIdleTimer()
	{
		IIdleTimer startIdleTimer = IdleTimerService?.CreateIdleTimer(Constants.StartIdleTimerName);
		startIdleTimer?.Initialize(Constants.StartIdleTimerDuration, delegate
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			if (service?.Peek() == service?.GetStartOverViewName())
			{
				OrchestrationService.InvokeStartIdleTimerCallback();
				LogHelper.Instance.Log("Current Culture: {0} Default Culture: {1}", OrchestrationService.GetCulture(), OrchestrationService.GetDefaultCulture());
				if (OrchestrationService.GetCulture() != OrchestrationService.GetDefaultCulture())
				{
					ToggleLocalizationCulture();
				}
			}
			startIdleTimer?.Reset();
		});
		startIdleTimer?.Start();
	}

	private static void ConfigureBurnInTimers(StartViewModel startViewModel)
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		IIdleTimer burnInIdleTimer = IdleTimerService?.CreateIdleTimer(Constants.BurnInIdleTimerName);
		int timeout = ((!IsTestScreenSaverSelected || startViewModel.IsReturnOnlyMode) ? Redbox.Rental.Services.Configuration.Configuration.Instance.BurnInTimeout : Redbox.Rental.Services.Configuration.Configuration.Instance.ScreenSaverTimeout);
		burnInIdleTimer.Initialize(timeout, delegate
		{
			burnInIdleTimer.Stop();
			startViewModel.BurnInVisibility = Visibility.Collapsed;
			startViewModel.UseTestStartView = false;
			startViewModel.UseRentAndBuyTestStartView = false;
			if (IsTestScreenSaverSelected && !startViewModel.IsReturnOnlyMode)
			{
				startViewModel.UseTestScreenSaver = true;
				startViewModel.ScreenSaverModel.IsAnimationOn = true;
				startViewModel.UseClassicStartView = false;
			}
			else
			{
				startViewModel.UseTestScreenSaver = false;
				startViewModel.UseClassicStartView = true;
				startViewModel.CarouselModel.IsAnimationOn = true;
				timerService.GetTimer(Constants.BurnInViewDurationTimerName)?.Start();
			}
		});
		burnInIdleTimer.Start();
		Action turnOffBurnInView = delegate
		{
			FrameworkElement startViewUserControl = GetStartViewUserControl();
			if (startViewUserControl != null)
			{
				if (IsDarkMode)
				{
					SetDarkMode(startViewModel, startViewUserControl);
				}
				else
				{
					SetLightMode(startViewModel, startViewUserControl);
				}
			}
			startViewModel.BurnInVisibility = Visibility.Visible;
			startViewModel.UseTestScreenSaver = false;
			if (IsTestCarouselSelected && !startViewModel.IsReturnOnlyMode)
			{
				startViewModel.UseClassicStartView = false;
				if (StartViewRentAndBuyABTestVersion == "D" || StartViewRentAndBuyABTestVersion == "E")
				{
					startViewModel.UseRentAndBuyTestStartView = true;
					startViewModel.UseTestStartView = false;
				}
				else
				{
					startViewModel.UseRentAndBuyTestStartView = false;
					startViewModel.UseTestStartView = true;
				}
				startViewModel.CarouselModel.IsAnimationOn = false;
			}
			else
			{
				startViewModel.UseClassicStartView = true;
				startViewModel.UseTestStartView = false;
				startViewModel.UseRentAndBuyTestStartView = false;
				startViewModel.CarouselModel.IsAnimationOn = true;
			}
			timerService.GetTimer(Constants.BurnInViewDurationTimerName)?.Stop();
			burnInIdleTimer.Start();
		};
		Action value = delegate
		{
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.TurnOffBurnInViewButtonClicked));
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartStartBrowsing");
			turnOffBurnInView();
		};
		timerService.CreateTimer(Constants.BurnInViewDurationTimerName, Redbox.Rental.Services.Configuration.Configuration.Instance.BurnInViewDuration, Redbox.Rental.Services.Configuration.Configuration.Instance.BurnInViewDuration, delegate
		{
			turnOffBurnInView();
		});
		startViewModel.OnExitBurnInView += value;
	}

	private static void ToggleAdaMode()
	{
		CloseTitleRollupOverlay();
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled)
		{
			if (BaseViewController.ApplicationState.IsABEOn)
			{
				TurnAdaOff();
			}
			else
			{
				TurnAdaOn();
			}
			ConfigureBanner(GetStartViewModelFromView());
			ConfigureCarousel(GetStartViewModelFromView());
		}
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ADAModeButtonClicked));
		BaseViewController.AnalyticsService?.AddButtonPressEvent(BaseViewController.ApplicationState.IsABEOn ? "StartADAOn" : "StartADAOff");
	}

	private static void TurnAdaOff()
	{
		SetAdaMode(adaEnabled: false);
		IdleTimerService?.GetIdleTimer(Constants.AdaIdleTimerName)?.Stop();
	}

	private static void TurnAdaOn()
	{
		SetAdaMode(adaEnabled: true);
		IdleTimerService?.GetIdleTimer(Constants.AdaIdleTimerName)?.Start();
	}

	private static void SetAdaMode(bool adaEnabled)
	{
		BaseViewController.ApplicationState.IsADAAccessible = adaEnabled;
		LogHelper.Instance.Log("ADA mode turned {0}.", BaseViewController.ApplicationState.IsADAAccessible ? "On" : "Off");
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			startViewModelFromView.AdaTurnedOnLabelVisibility = ((!adaEnabled) ? Visibility.Collapsed : Visibility.Visible);
			SetSignInButtonVisibility(startViewModelFromView);
		}
	}

	private static StartViewModel GetStartViewModelFromView()
	{
		return GetStartViewUserControl()?.DataContext as StartViewModel;
	}

	private static void ToggleLanguage()
	{
		CloseTitleRollupOverlay();
		LogHelper.Instance.Log("language_toggle, setting language_switch=true");
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.LanguageButtonClicked));
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StartLanguageToggle");
		LogHelper.Instance.Log("Function:switch_language()");
		ToggleLocalizationCulture();
	}

	private static void ConfigureCarousel(StartViewModel startViewModel)
	{
		CarouselModel carouselModel = new CarouselModel();
		carouselModel.OnDisplayProductModelSelected += CarouselModel_OnDisplayProductModelSelected;
		carouselModel.OnAddDisplayProductModel += CarouselModel_OnAddDisplayProductModel;
		carouselModel.OnTitleRollupOverlayCancel += CarouselModel_OnTitleRollupOverlayCancel;
		carouselModel.OnTitleRollupOverlayTimeout += CarouselModel_OnTitleRollupOverlayTimeout;
		carouselModel.OnTitleRollupOverlayAddDVD += CarouselModel_OnTitleRollupOverlayAddDVDProduct;
		carouselModel.OnTitleRollupOverlayAddBluray += CarouselModel_OnTitleRollupOverlayAddBlurayProduct;
		carouselModel.OnTitleRollupOverlayAdd4kUhd += CarouselModel_OnTitleRollupOverlayAdd4kUhdProduct;
		carouselModel.OnTitleRollupOverlayVisibilityChange += CarouselModel_OnTitleRollupOverlayVisibilityChange;
		carouselModel.ShowAddButton = !(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.IsFull ?? true);
		OrchestrationService.RentalRefreshIfNeeded();
		List<IKioskProduct> list = ((!startViewModel.IsReturnOnlyMode) ? OrchestrationService.RentalGetCarouselProducts() : new List<IKioskProduct>());
		List<DisplayProductModel> list2 = new List<DisplayProductModel>();
		ServiceLocator.Instance.GetService<IBitmapCacheService>();
		foreach (IKioskProduct item in list)
		{
			DisplayProductModel displayProductModel = new DisplayProductModel
			{
				Data = item
			};
			displayProductModel.ConfigureForCarousel();
			displayProductModel.AddButtonLabelText = Resources.carousel_add_button_label;
			displayProductModel.Image = OrchestrationService.GetProductBitmapImage(item);
			if (item.KioskProductType == KioskProductType.Collection && item is ICollectionProduct)
			{
				displayProductModel.AddButtonLabelText = Resources.carousel_add_item_to_cart_button_for_collection;
			}
			list2.Add(displayProductModel);
		}
		carouselModel.CarouselDisplayProductModels = list2;
		CarouselTestModel carouselTestModel = new CarouselTestModel();
		carouselTestModel.OnDisplayProductModelSelected += CarouselModel_OnDisplayProductModelSelected;
		carouselTestModel.OnAddDisplayProductModel += CarouselModel_OnAddDisplayProductModel;
		carouselTestModel.CarouselDisplayProductModels = list2;
		carouselModel.IsAnimationOn = IsClassicCarouselSelected && !startViewModel.IsReturnOnlyMode;
		carouselTestModel.IsAnimationOn = IsTestCarouselSelected && !startViewModel.IsReturnOnlyMode;
		carouselTestModel.IsStartViewAdABTest = StartViewAdABTestVersion == "C" || (StartViewRentAndBuyABTestVersion != "A" && StartViewRentAndBuyABTestVersion != "F" && StartViewRentAndBuyABTestVersion != "G" && StartViewRentAndBuyABTestVersion != "H");
		if (startViewModel != null)
		{
			startViewModel.CarouselModel = carouselModel;
			startViewModel.CarouselTestModel = carouselTestModel;
			startViewModel.CarouselGridRowHeight = ((StartViewAdABTestVersion == "C" || (StartViewRentAndBuyABTestVersion != "A" && StartViewRentAndBuyABTestVersion != "F" && StartViewRentAndBuyABTestVersion != "G" && StartViewRentAndBuyABTestVersion != "H")) ? 340 : 438);
			bool flag = (startViewModel?.CarouselModel?.CarouselDisplayProductModels?.Count ?? 0) >= Constants.MinimumCarouselItems;
			startViewModel.LoadingTitlesImageVisibility = ((startViewModel.IsReturnOnlyMode | flag) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayVisibilityChange(Visibility newVisibility)
	{
		if (newVisibility == Visibility.Visible)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = true;
			}
			IKioskProduct centerCarouselProduct = GetStartViewModelFromView()?.CarouselModel?.TitleRollupOverlayModel?.BrowseItemModel?.Data as IKioskProduct;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.CenterCarouselProduct = centerCarouselProduct;
		}
		else
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.CenterCarouselProduct = null;
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayAddBlurayProduct(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray));
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayAdd4kUhdProduct(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType._4KUHD));
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayAddDVDProduct(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD));
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayTimeout(IBrowseItemModel browseItemModel, object parameter)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
			}
			startViewModelFromView.CarouselModel.IsAnimationOn = IsClassicCarouselSelected;
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayCancel(IBrowseItemModel browseItemModel, object parameter)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.IsInAddProductFlow = false;
			}
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.TitleRollupOverlayCancelClicked));
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartCarouselCancelAddItem");
			startViewModelFromView.CarouselModel.IsAnimationOn = IsClassicCarouselSelected;
		}
	}

	private static void CarouselModel_OnAddDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct))
		{
			return;
		}
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			switch (kioskProduct.KioskProductType)
			{
			case KioskProductType.Title:
			case KioskProductType.MDV:
				startViewModelFromView.CarouselModel.IsAnimationOn = false;
				startViewModelFromView.CarouselTestModel.IsAnimationOn = false;
				AddProductToCart(kioskProduct as ITitleProduct);
				break;
			case KioskProductType.TitleRollup:
				startViewModelFromView.CarouselModel.IsAnimationOn = false;
				startViewModelFromView.CarouselTestModel.IsAnimationOn = false;
				TitleRollupUtilities.ConfigureTitleRollupOverlay(browseItemModel, startViewModelFromView.CarouselModel.TitleRollupOverlayModel, showDigitalCodeInfo: false, isPurchase: false);
				break;
			case KioskProductType.Collection:
				startViewModelFromView.CarouselModel.IsAnimationOn = false;
				startViewModelFromView.CarouselTestModel.IsAnimationOn = false;
				OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.CollectionsCarouselClicked));
				BaseViewController.AnalyticsService?.AddButtonPressEvent("StartAddToCart");
				PrepareToLeaveStartView();
				ShowCollectionBrowseView(kioskProduct);
				break;
			case KioskProductType.None:
				break;
			}
		}
	}

	private static void AddProductToCart(ITitleProduct titleProduct)
	{
		if (titleProduct != null)
		{
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.AddProductCarouselButtonClicked));
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartAddToCart");
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductAddedFromCarousel = true;
			SetBrowseFiltersBasedOnSelectedProduct(titleProduct);
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			if ((titleProduct.HasMultiNightPrice || (titleProduct.IsSellableAndInStock && (titleProduct.IsMovie || titleProduct.IsGame))) && !flag)
			{
				BrowseFunctions.AddMultiNightPricedProductToCart(titleProduct, "start_view");
			}
			else
			{
				BrowseFunctions.AddProductToCart(titleProduct, isPurchase: false);
			}
		}
	}

	private static void SetBrowseFiltersBasedOnSelectedProduct(ITitleProduct titleProduct)
	{
		if (titleProduct != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = titleProduct.TitleFamily;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = null;
		}
	}

	private static void CarouselModel_OnDisplayProductModelSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct))
		{
			return;
		}
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ProductCarouselClicked));
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StartCarouselArt");
		switch (kioskProduct.KioskProductType)
		{
		case KioskProductType.Title:
		case KioskProductType.MDV:
		case KioskProductType.TitleRollup:
		{
			if (!(kioskProduct is ITitleProduct browseFiltersBasedOnSelectedProduct))
			{
				break;
			}
			StartViewModel startViewModelFromView = GetStartViewModelFromView();
			if (startViewModelFromView != null)
			{
				startViewModelFromView.CarouselModel.IsAnimationOn = false;
				startViewModelFromView.CarouselTestModel.IsAnimationOn = false;
				SetBrowseFiltersBasedOnSelectedProduct(browseFiltersBasedOnSelectedProduct);
				OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = true;
				IViewService service = ServiceLocator.Instance.GetService<IViewService>();
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductDetailShownFromView = service.Peek();
				PrepareToLeaveStartView();
				IAnalyticsService service2 = ServiceLocator.Instance.GetService<IAnalyticsService>();
				if ((ITitleProduct)kioskProduct != null)
				{
					service2?.AddData(ProductData.ToProductData((ITitleProduct)kioskProduct));
				}
				OrchestrationService.ViewShow("product_detail_view");
			}
			break;
		}
		case KioskProductType.Collection:
			PrepareToLeaveStartView();
			ShowCollectionBrowseView(kioskProduct);
			break;
		case KioskProductType.None:
			break;
		}
	}

	private static void ShowCollectionBrowseView(IKioskProduct kioskProduct)
	{
		ICollectionProduct collectionProduct = kioskProduct as ICollectionProduct;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.CarouselCollectionsSelected = true;
		OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.PushBrowseStateSet();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseCollections = collectionProduct == null || collectionProduct.Id == 0;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseCollectionProduct = ((collectionProduct == null || collectionProduct.Id == 0L) ? null : collectionProduct);
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseCollectionProduct;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = TitleFamily.Movies;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.All;
		OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
	}

	private static void ToggleLocalizationCulture()
	{
		string text = null;
		text = ((!(OrchestrationService.GetCulture() == OrchestrationService.GetDefaultCulture())) ? OrchestrationService.GetDefaultCulture() : OrchestrationService.GetOtherCulture());
		LogHelper.Instance.Log("Toggling Culture to {0}", text);
		PrepareToLeaveStartView();
		ServiceLocator.Instance.GetService<IConfiguration>()?.ResetCachedIsDefaultCulture();
		OrchestrationService.SetCulture(text);
		OrchestrationService.InvokeLocalizationReloadCallback();
	}

	private static void SetCultureSpecificText(StartViewModel startViewModel, FrameworkElement startViewUserControl, bool hasInstockGames)
	{
		OrchestrationService.SetCurrentUICulture();
		ServiceLocator.Instance.GetService<IStoreManager>();
		if (IsTestCarouselSelected)
		{
			bool flag = StartViewRentAndBuyABTestVersion == "D" || StartViewRentAndBuyABTestVersion == "E";
			startViewModel.AllMoviesButtonTextLine1 = (flag ? Resources.start_view_rent_and_buy_movies_button : Resources.movie_button_rent_label);
			startViewModel.AllMoviesButtonTextLine2 = (flag ? Resources.start_view_rent_and_buy_button : Resources.Alt_StartView_Movies);
			startViewModel.PickUpButtonText = Resources.pickup;
			startViewModel.ReturnButtonText = Resources.return_button_label;
			startViewModel.ToggleLanguageButtonText = (flag ? Resources.start_view_rent_and_buy_toggle_language_button : Resources.toggle_language_button_label);
			startViewModel.BurnInViewLabelText = Resources.burn_in_view_label;
			startViewModel.BuyMoviesOrGamesButtonTextLine1 = Resources.buy_movies_or_games_line1;
			startViewModel.BuyMoviesOrGamesButtonTextLine2 = Resources.buy_movies_or_games_line2;
			startViewModel.BuyGamesButtonTextLine1 = Resources.buy_games_line1;
			startViewModel.BuyGamesButtonTextLine2 = Resources.buy_games_line2;
			startViewModel.GamesButtonTextLine1 = (flag ? Resources.start_view_rent_and_buy_games_button : Resources.games_button_rent_label);
			startViewModel.GamesButtonTextLine2 = (flag ? Resources.start_view_rent_and_buy_button : Resources.games_button_games_label);
			startViewModel.AdaTurnedOnLabelText = Resources.ada_turned_on_label_text;
		}
		else
		{
			startViewModel.AllMoviesButtonTextLine1 = Resources.movie_button_rent_label;
			startViewModel.AllMoviesButtonTextLine2 = Resources.Alt_StartView_Movies;
			startViewModel.PickUpButtonText = Resources.pickup_uppercase;
			startViewModel.ReturnButtonText = Resources.return_button_label;
			startViewModel.ToggleLanguageButtonText = Resources.toggle_language_button_label;
			startViewModel.BurnInViewLabelText = Resources.burn_in_view_label;
			startViewModel.BuyMoviesOrGamesButtonTextLine1 = Resources.buy_movies_button_text;
			startViewModel.BuyGamesButtonTextLine1 = Resources.buy_games_button_text;
			startViewModel.BuyMoviesOrGamesButtonTextLine1Style = (OrchestrationService.IsCultureSpanish() ? WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_extrabold_16") : WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_extrabold_20"));
			startViewModel.BuyGamesButtonTextLine1Style = (OrchestrationService.IsCultureSpanish() ? WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_extrabold_16") : WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_extrabold_20"));
			startViewModel.AllMoviesButtonTextLine1Style = WPFFunctions.GetStyle(startViewUserControl, OrchestrationService.IsCultureSpanish() ? "font_montserrat_extrabold_16" : "font_montserrat_extrabold_20");
			startViewModel.AllMoviesButtonTextLine2Style = WPFFunctions.GetStyle(startViewUserControl, OrchestrationService.IsCultureSpanish() ? "font_montserrat_extrabold_20" : "font_montserrat_extrabold_24");
			startViewModel.GamesButtonTextLine1Style = WPFFunctions.GetStyle(startViewUserControl, OrchestrationService.IsCultureSpanish() ? "font_montserrat_regular_16" : "font_montserrat_regular_20");
			startViewModel.GamesButtonTextLine2Style = WPFFunctions.GetStyle(startViewUserControl, OrchestrationService.IsCultureSpanish() ? "font_montserrat_extrabold_20" : "font_montserrat_extrabold_24");
			startViewModel.GamesButtonTextLine1 = Resources.games_button_rent_label;
			startViewModel.GamesButtonTextLine2 = Resources.games_button_games_label;
			startViewModel.AdaTurnedOnLabelText = Resources.ada_turned_on_label_text;
		}
		startViewModel.ToggleSignInButtonText = (PersonalizationFunctions.IsSignedIn ? Resources.sign_out_button : Resources.button_sign_in);
		startViewModel.SignUpButtonText = (PersonalizationFunctions.IsSignedIn ? null : Resources.button_sign_up);
		startViewModel.SignInUpButtonSeparator = (PersonalizationFunctions.IsSignedIn ? null : Resources.button_sign_in_sign_up_separator);
		startViewModel.SignInButtonTooltipText = Resources.sign_in_button_tooltip_text;
		startViewModel.AdaTurnedOnLabelVisibility = ((!BaseViewController.ApplicationState.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
	}

	private static void PrepareToLeaveStartView()
	{
		CloseTitleRollupOverlay();
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null && startViewModelFromView.CarouselModel != null)
		{
			startViewModelFromView.CarouselModel.IsAnimationOn = false;
		}
		if (startViewModelFromView != null && startViewModelFromView.CarouselTestModel != null)
		{
			startViewModelFromView.CarouselTestModel.IsAnimationOn = false;
		}
		if (startViewModelFromView?.ScreenSaverModel != null)
		{
			startViewModelFromView.ScreenSaverModel.IsAnimationOn = false;
		}
		if (BaseViewController.ApplicationState.IsABEOn)
		{
			BaseViewController.ApplicationState.IsADAAccessible = true;
		}
	}

	private static void GoToMovieBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("all_movies_button", TitleFamily.Movies, "StartMovies");
	}

	private static void GoToRentAndBuyMoviesBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("rent_and_buy_movies_button", TitleFamily.Movies, "StartRentAndBuyMovies");
	}

	private static void GoToStartViewFromScreenSaver()
	{
		BaseViewController.ProcessWPFHit();
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowStartViewFromScreenSaver));
		PrepareToLeaveStartView();
		ResetToStartView("start_browsing_button");
	}

	private static void GoToStartViewFromScreenSaverLastImage()
	{
		BaseViewController.ProcessWPFHit();
		PrepareToLeaveStartView();
		OrchestrationService.ResetCurrentView();
	}

	private static void GoToForSaleMovieBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("buy_movies_button", TitleFamily.Movies, "StartBuyMovies");
	}

	private static void GoTo99CentsMoviesBrowseViewFromScreenSaver()
	{
		BaseViewController.ProcessWPFHit();
		PrepareToLeaveStartView();
		GoToBrowseView("screen_saver_99_cents_movies", TitleFamily.Movies, "StartScreenSaver99CentsMovies");
	}

	private static void GoToForSaleMovieBrowseViewFromScreenSaver()
	{
		BaseViewController.ProcessWPFHit();
		PrepareToLeaveStartView();
		GoToBrowseView("screen_saver_buy_movies", TitleFamily.Movies, "StartScreenSaverBuyMovies");
	}

	private static void GoToTitleDetailsViewFromScreenSaver(long productGroupId)
	{
		BaseViewController.ProcessWPFHit();
		PrepareToLeaveStartView();
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowBrowseView));
		IKioskProduct kioskProduct = null;
		ServiceLocator.Instance.GetService<IDataService>()?.GetTitleRollupProduct(productGroupId, out kioskProduct);
		ITitleRollupProduct titleRollupProduct = kioskProduct as ITitleRollupProduct;
		if (titleRollupProduct != null)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartScreenSaverTitleDetails")?.AddData(ProductData.ToProductData(titleRollupProduct));
		}
		else
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartScreenSaverTitleDetails")?.AddData(ProductData.ToProductData(productGroupId));
		}
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.SetDefaultBrowseStateSet();
		OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
		if (titleRollupProduct != null && titleRollupProduct.IsRentableAndInStock)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
			OrchestrationService.ViewShow("product_detail_view");
		}
	}

	private static void GoToForSaleGamesBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("buy_games_button", TitleFamily.Games, "StartBuyGames");
	}

	private static void GoToGameView()
	{
		PrepareToLeaveStartView();
		_ = Redbox.Rental.Services.Configuration.Configuration.Instance;
		GoToBrowseView("games_button", TitleFamily.Games, "StartGames");
	}

	private static void GoToRentAndBuyGamesBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("rent_and_buy_games_button", TitleFamily.Games, "StartRentAndBuyGames");
	}

	private static void GoToBrowseView(string pressedButtonName, TitleFamily titleFamily, string analyticsButtonName)
	{
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowBrowseView));
		BaseViewController.AnalyticsService?.AddButtonPressEvent(analyticsButtonName);
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.SetDefaultBrowseStateSet();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = titleFamily;
		switch (pressedButtonName)
		{
		case "buy_movies_button":
		case "buy_games_button":
		case "screen_saver_buy_movies":
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.ForSale;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
			break;
		case "screen_saver_99_cents_movies":
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = BrowseSelectedSort._99Cents;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.PageNumber = 1;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowsePriceRangeFilter = new BrowsePriceRangeParameter
			{
				PriceRangeLow = 0.99m,
				PriceRangeHigh = 0.99m
			};
			break;
		}
		OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
	}

	private static void ResetToStartView(string pressedButtonName)
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StartPressedButton", pressedButtonName);
		OrchestrationService.ResetCurrentView();
	}

	private static void GoToPickupReservationSwipeView()
	{
		PrepareToLeaveStartView();
		Redbox.Rental.Services.Session.Session.Instance.IsInReservationFlow = true;
		OrchestrationService.ExecuteScript("pickup_reserved_product");
	}

	private static void GoToReturnProductView()
	{
		GoToReturnProductView(isZeroTouch: false);
	}

	private static void GoToReturnProductView(bool isZeroTouch)
	{
		LockHelper.TryLockObject(_lockObject, delegate
		{
			if ((!(Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.InReturnFlow)) ?? false)
			{
				StartViewModel startViewModelFromView = GetStartViewModelFromView();
				if (isZeroTouch || (startViewModelFromView != null && startViewModelFromView.ReturnButtonEnabled))
				{
					Redbox.Rental.Services.Session.Session.Instance.BrowseState.InReturnFlow = true;
					if (startViewModelFromView != null)
					{
						startViewModelFromView.ReturnButtonEnabled = false;
					}
					PrepareToLeaveStartView();
					if (!isZeroTouch)
					{
						OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowReturnView), AnalyticsSessionType.Return);
						BaseViewController.AnalyticsService?.AddButtonPressEvent("StartReturn");
					}
					OrchestrationService.ExecuteScript("return_product");
				}
				else
				{
					LogHelper.Instance.Log("Return button not processed because return button was disabled.");
				}
			}
			else
			{
				LogHelper.Instance.Log("Return button not processed because session is already in return mode.");
			}
		}, "StartReturnProcess");
	}

	private static void GoToHelpDocumentView()
	{
		PrepareToLeaveStartView();
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowHelpView));
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StartHelp");
		HelpViewController.ShowFullHelpView(allowFMA: true, restartTimer: true);
	}

	private static void ShowReserveOnlinePopupView()
	{
		BaseViewController.ProcessWPFHit();
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowReserveOnlinePopupView));
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StartScreenSaverReserveOnlineLearnMore");
		BulletListPopupParameters parameter = new BulletListPopupParameters
		{
			ViewAnalyticsName = "screen_saver_reserve_online"
		};
		OrchestrationService.ViewShow("reserve_online_popup_view", parameter);
		PrepareToLeaveStartView();
	}

	private static void ShowPerksInfoPopupView()
	{
		BaseViewController.ProcessWPFHit();
		OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowPerksInfoPopupView));
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StartScreenSaverRedboxPerksLearnMore");
		BulletListPopupParameters parameter = new BulletListPopupParameters
		{
			ViewAnalyticsName = "screen_saver_redbox_perks"
		};
		OrchestrationService.ViewShow("perks_info_popup_view", parameter);
		PrepareToLeaveStartView();
	}

	private static bool IsCurrentViewValid(bool logError = true)
	{
		return OrchestrationService.IsCurrentViewValid("start_view");
	}

	private static void ProcessSignInSignOutCommand()
	{
		if (IsCurrentViewValid())
		{
			PrepareToLeaveStartView();
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.SignOut));
			IAnalyticsService analyticsService = BaseViewController.AnalyticsService;
			if (analyticsService != null)
			{
				IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession;
				analyticsService.AddButtonPressEvent((personalizationSession != null && personalizationSession.IsLoggedIn) ? "StartSignOut" : "StartSignIn");
			}
			IPersonalizationSession personalizationSession2 = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession;
			if (personalizationSession2 != null && personalizationSession2.IsLoggedIn)
			{
				PersonalizationFunctions.ShowLogoutDialog();
				return;
			}
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.SetDefaultBrowseStateSet();
			BaseViewController.AnalyticsService?.AddButtonPressEvent("StartSignInHome");
			service.Push("browse_view", BrowseFunctions.GetBrowseViewparameters());
			PersonalizationFunctions.ShowRegularSignInSignUpDialog();
		}
	}

	private static void SignInButtonTooltipClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StartSignInTooltip");
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			startViewModelFromView.SignInButtonTooltipVisibility = Visibility.Collapsed;
		}
	}
}
