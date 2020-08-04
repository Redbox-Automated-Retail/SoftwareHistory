using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SwipeViewController : BaseViewController
{
	private delegate void ShowMessage(Action timeout, Action goBack);

	private static Action _cancelAction;

	private static Action _continueAction;

	private static SwipeViewType _swipeViewType;

	private static readonly List<SwipeViewType> _regularCheckoutTypes = new List<SwipeViewType>
	{
		SwipeViewType.RegularCheckOut,
		SwipeViewType.EncryptedSwipeRegularCheckOut,
		SwipeViewType.EMVInsertRegularCheckout,
		SwipeViewType.EMVInsertTapRegularCheckout,
		SwipeViewType.EMVInsertTapHideMobileRegularCheckout,
		SwipeViewType.EMVTapRegularCheckout
	};

	private static readonly List<SwipeViewType> _reservationPickupTypes = new List<SwipeViewType>
	{
		SwipeViewType.ReservationPickup,
		SwipeViewType.EncryptedSwipeReservationPickup,
		SwipeViewType.EMVInsertReservationPickup,
		SwipeViewType.EMVInsertTapReservationPickup,
		SwipeViewType.EMVTapReservationPickup
	};

	private static bool ChipEnabled
	{
		get
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			if (service != null && service.ChipEnabled)
			{
				return service2?.SupportsEMV ?? false;
			}
			return false;
		}
	}

	private static bool ContactlessEnabled
	{
		get
		{
			bool flag = false;
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			flag = service != null && service.ContactlessEnabled && (service2?.SupportsEMV ?? false);
			if (ServiceLocator.Instance.GetService<IEMVReaderHealth>().ContactlessOffDueToDisconnectThresholdReached)
			{
				flag = false;
			}
			return flag;
		}
	}

	private static string ShoppingCartSessionId
	{
		get
		{
			string result = null;
			IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			if (service != null)
			{
				IShoppingSession currentSession = service.GetCurrentSession();
				if (currentSession != null)
				{
					result = currentSession.Id.ToString();
				}
			}
			return result;
		}
	}

	public static void SetSwipeViewParameters(Action cancelAction, Action continueAction, string stringSwipeViewType)
	{
		SwipeViewType result = SwipeViewType.RegularCheckOut;
		if (Enum.TryParse<SwipeViewType>(stringSwipeViewType, out result))
		{
			SetSwipeViewParameters2(cancelAction, continueAction, result);
			return;
		}
		throw new ArgumentException("Invalid stringSwipeViewType parameter value: {0}", stringSwipeViewType);
	}

	public static void SetSwipeViewParameters2(Action cancelAction, Action continueAction, SwipeViewType swipeViewType)
	{
		_cancelAction = cancelAction;
		_continueAction = continueAction;
		_swipeViewType = swipeViewType;
	}

	private static void AdjustSwipeViewType()
	{
		if (!(ServiceLocator.Instance.GetService<ITrackDataService>() is IDeviceServiceTrackDataService deviceServiceTrackDataService))
		{
			return;
		}
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		if (_regularCheckoutTypes.Contains(_swipeViewType))
		{
			if (ChipEnabled && !deviceServiceTrackDataService.IsInTechnicalFallback)
			{
				if (ContactlessEnabled)
				{
					if (service != null && service.HideMobilePayOption)
					{
						_swipeViewType = SwipeViewType.EMVInsertTapHideMobileRegularCheckout;
					}
					else
					{
						_swipeViewType = SwipeViewType.EMVInsertTapRegularCheckout;
					}
				}
				else
				{
					_swipeViewType = SwipeViewType.EMVInsertRegularCheckout;
				}
			}
			else if (ContactlessEnabled && !deviceServiceTrackDataService.IsInTechnicalFallback)
			{
				_swipeViewType = SwipeViewType.EMVTapRegularCheckout;
			}
			else
			{
				_swipeViewType = SwipeViewType.EncryptedSwipeRegularCheckOut;
			}
		}
		else
		{
			if (!_reservationPickupTypes.Contains(_swipeViewType))
			{
				return;
			}
			if (ChipEnabled && !deviceServiceTrackDataService.IsInTechnicalFallback)
			{
				if (ContactlessEnabled)
				{
					_swipeViewType = SwipeViewType.EMVInsertTapReservationPickup;
				}
				else
				{
					_swipeViewType = SwipeViewType.EMVInsertReservationPickup;
				}
			}
			else
			{
				_swipeViewType = SwipeViewType.EncryptedSwipeReservationPickup;
			}
		}
	}

	public static void ConfigureSwipeView()
	{
		if (!(BaseViewController.GetViewUserControl() is SwipeViewUserControl swipeViewUserControl))
		{
			return;
		}
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		OrchestrationService.StopIdleTimer("idle_timer");
		ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service2 == null)
		{
			LogHelper.Instance.Log("TrackDataService is null.  Attempting to detect card reader.");
			service?.DetectCardReaderAndRegisterTrackDataService();
			service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		}
		if (service2 == null)
		{
			LogHelper.Instance.Log(string.Format("Error in ConfigureSwipeView.  TrackDataService is null.  Showing {0}.", "please_wait_view"));
			Task.Run(delegate
			{
				CheckoutFunctions.ShowPleaseWaitBecauseOfDisconnectedCardReader();
			});
			return;
		}
		AdjustSwipeViewType();
		swipeViewUserControl.SwipeType = _swipeViewType;
		SwipeViewModel swipeViewModel = new SwipeViewModel();
		swipeViewModel.CardReadAttempt = new CardReadAttempt
		{
			StartDateTime = DateTime.Now,
			CardReadAttemptStatus = CardReadAttemptStatus.notStarted
		};
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.CardReadAttemptCollection.Add(swipeViewModel.CardReadAttempt);
		}
		ConfigureTextAndActions(swipeViewModel);
		swipeViewUserControl.DataContext = swipeViewModel;
		swipeViewModel?.AdImpression?.Start();
		BaseViewController.AnalyticsService?.AddData(new SwipeViewData
		{
			SwipeViewType = _swipeViewType.ToString(),
			TitleText = swipeViewModel.TitleText,
			BackButtonText = swipeViewModel.BackButtonText,
			MessageText = swipeViewModel.MessageText,
			ChipNotEnabledText = swipeViewModel.ChipNotEnabledText
		});
		swipeViewModel.OnGetSpeechControl = delegate
		{
			ISpeechControl result = new SpeechControl
			{
				Name = "tts_swipe_view"
			};
			AddSpeechPartTitle(result, swipeViewModel);
			AddSpeechPartMessage(result, swipeViewModel);
			result.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.SpeechControl = result;
			});
			return result;
		};
		int num = 50000;
		int timeout = num + 10000;
		swipeViewUserControl.Timeout = num;
		if (service2 != null)
		{
			service2.ClearCardReadHandlers();
			service2.OnCardReadComplete += OnCardReadCompleteHandler;
			service2.OnCardReadStarted += OnCardReadStartedHandler;
			DeviceInputType deviceInputType = DeviceInputType.M;
			if (service2 is IDeviceServiceTrackDataService)
			{
				if (ChipEnabled && ContactlessEnabled && _reservationPickupTypes.Contains(_swipeViewType))
				{
					deviceInputType = DeviceInputType.MCS;
				}
				else if (ChipEnabled && (!ContactlessEnabled || _reservationPickupTypes.Contains(_swipeViewType)))
				{
					deviceInputType = DeviceInputType.MS;
				}
				else if (ChipEnabled && ContactlessEnabled && _regularCheckoutTypes.Contains(_swipeViewType))
				{
					deviceInputType = DeviceInputType.MCS;
				}
				else if (!ChipEnabled && ContactlessEnabled && _regularCheckoutTypes.Contains(_swipeViewType))
				{
					deviceInputType = DeviceInputType.MC;
				}
			}
			swipeViewModel.CardReadAttempt.DeviceInputType = deviceInputType;
			swipeViewModel.CardReadAttempt.CardReadAttemptStatus = CardReadAttemptStatus.Active;
			service2.StartCardRead(timeout, isZeroTouchRead: false, deviceInputType, delegate(IReadCardJob cardReadJob)
			{
				if (cardReadJob != null && swipeViewModel?.CardReadAttempt != null)
				{
					swipeViewModel.CardReadAttempt.CardReadJobCommandRequestId = cardReadJob.RequestId;
				}
			}, BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ShoppingSessionId ?? default(Guid));
		}
		else
		{
			LogHelper.Instance.Log("Error in SwipeViewController.ConfigiureSwipeView. TrackDataService is null.");
		}
	}

	private static void OnCardReadStartedHandler(Guid cardReadJobCommandRequestId)
	{
		LogHelper.Instance.Log("SwipeViewController card read started.");
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service?.UITaskScheduler != null)
		{
			Task.Factory.StartNew(delegate
			{
				StopTimer();
				HandleWPFHit();
			}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
		}
	}

	private static void ConfigureTextAndActions(SwipeViewModel swipeViewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		if (swipeViewModel == null)
		{
			return;
		}
		swipeViewModel.GridBackground = WPFFunctions.GetBrush(BaseViewController.GetViewUserControl(), (_swipeViewType == SwipeViewType.EMVInsertTapReservationPickup) ? "RubineToDarkPurpleGradientBrush" : "RubineRedBrush");
		IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
		swipeViewModel.ApplePayImageVisibility = ((service != null && service.HideApplePayImage) ? Visibility.Collapsed : Visibility.Visible);
		swipeViewModel.GooglePayImageVisibility = ((service != null && service.HideGooglePayImage) ? Visibility.Collapsed : Visibility.Visible);
		IDeviceServiceTrackDataService deviceServiceTrackDataService = ServiceLocator.Instance.GetService<ITrackDataService>() as IDeviceServiceTrackDataService;
		swipeViewModel.ChipNotEnabledText = Resources.chip_not_enabled_text;
		swipeViewModel.BackButtonText = Resources.back_button.ToUpper();
		swipeViewModel.BackButtonVisibility = Visibility.Visible;
		swipeViewModel.ImageStackPanelVisibility = Visibility.Collapsed;
		swipeViewModel.OnBackButtonClicked = OnBackButtonPressed;
		swipeViewModel.OnTimeout = SwipeTimerTimeout;
		bool flag = false;
		switch (_swipeViewType)
		{
		case SwipeViewType.ReservationPickup:
			swipeViewModel.TitleText = Resources.reservation_swipe_title;
			swipeViewModel.MessageText = Resources.reservation_swipe_message;
			flag = true;
			break;
		case SwipeViewType.EncryptedSwipeReservationPickup:
			swipeViewModel.TitleText = Resources.reservation_emv_swipe_title;
			if (deviceServiceTrackDataService == null || deviceServiceTrackDataService.IsInTechnicalFallback)
			{
				swipeViewModel.MessageText = null;
				swipeViewModel.ImageStackPanelVisibility = Visibility.Collapsed;
			}
			else
			{
				swipeViewModel.MessageText = Resources.reservation_checkout_emv_swipe_message;
			}
			flag = true;
			break;
		case SwipeViewType.EMVInsertReservationPickup:
			swipeViewModel.TitleText = Resources.reservation_emv_insert_title;
			swipeViewModel.MessageText = null;
			flag = true;
			break;
		case SwipeViewType.EMVInsertTapReservationPickup:
			swipeViewModel.TitleText = Resources.reservation_emv_insert_tap_title;
			swipeViewModel.MessageText = Resources.reservation_emv_insert_tap_hide_mobile_message;
			swipeViewModel.ImageStackPanelVisibility = Visibility.Visible;
			flag = true;
			break;
		case SwipeViewType.EMVTapReservationPickup:
			swipeViewModel.TitleText = Resources.reservation_emv_tap_title;
			swipeViewModel.MessageText = null;
			flag = true;
			break;
		case SwipeViewType.EncryptedSwipeRegularCheckOut:
			swipeViewModel.TitleText = Resources.regular_checkout_emv_swipe_title;
			if (deviceServiceTrackDataService == null || deviceServiceTrackDataService.IsInTechnicalFallback)
			{
				if (swipeViewModel.TitleText.Contains("Swipe!"))
				{
					swipeViewModel.TitleText = swipeViewModel.TitleText.Replace("Swipe!", "\r\nSwipe!");
				}
				swipeViewModel.MessageText = null;
				swipeViewModel.ImageStackPanelVisibility = Visibility.Collapsed;
			}
			else
			{
				swipeViewModel.MessageText = Resources.regular_checkout_emv_swipe_message;
				swipeViewModel.ImageStackPanelVisibility = Visibility.Visible;
			}
			break;
		case SwipeViewType.EMVInsertRegularCheckout:
			swipeViewModel.TitleText = Resources.regular_checkout_emv_insert_title;
			swipeViewModel.MessageText = Resources.regular_checkout_emv_insert_message;
			swipeViewModel.ImageStackPanelVisibility = Visibility.Visible;
			break;
		case SwipeViewType.EMVInsertTapRegularCheckout:
			swipeViewModel.TitleText = Resources.regular_checkout_emv_insert_tap_title;
			swipeViewModel.MessageText = null;
			break;
		case SwipeViewType.EMVInsertTapHideMobileRegularCheckout:
			swipeViewModel.TitleText = Resources.regular_checkout_emv_insert_tap_hide_mobile_title;
			swipeViewModel.MessageText = Resources.regular_checkout_emv_insert_tap_hide_mobile_message;
			swipeViewModel.ImageStackPanelVisibility = Visibility.Visible;
			break;
		case SwipeViewType.EMVTapRegularCheckout:
			swipeViewModel.TitleText = Resources.regular_checkout_emv_tap_title;
			swipeViewModel.MessageText = null;
			break;
		default:
			swipeViewModel.TitleText = Resources.regular_checkout_swipe_title;
			swipeViewModel.MessageText = Resources.regular_checkout_swipe_message;
			break;
		}
		BitmapImage adImage = null;
		if (flag)
		{
			IAd ad = (BaseViewController.ApplicationState.IsADAAccessible ? null : ServiceLocator.Instance.GetService<IAdService>()?.GetScreenAd(AdLocation.Pickup));
			adImage = ad?.Image;
			swipeViewModel.AdImpression = ((ad != null) ? new AdImpression
			{
				AdLocation = AdLocation.Pickup,
				Ad = ad
			} : null);
		}
		swipeViewModel.AdImage = adImage;
	}

	private static void AddSpeechPartTitle(ISpeechControl result, SwipeViewModel swipeViewModel)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Title",
			Loop = false,
			StartPause = 500,
			EndPause = 1000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			AddBaseEvents(speechPart);
			BaseViewController.GetViewUserControl();
			speechPart.Texts.Add(new TextPart
			{
				Text = swipeViewModel?.TitleText
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartMessage(ISpeechControl result, SwipeViewModel swipeViewModel)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "Message",
			Loop = true,
			StartPause = 0,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			AddBaseEvents(speechPart);
			BaseViewController.GetViewUserControl();
			speechPart.Texts.Add(new TextPart
			{
				Text = swipeViewModel?.MessageText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The credit card swiper is located on your right."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "If you wish to go back, press the CANCEL key."
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private static void AddBaseEvents(ISpeechPart speechPart)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "CANCEL",
			Action = delegate
			{
				OnBackButtonPressed();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "MINUS",
			Action = delegate
			{
				OnBackButtonPressed();
			}
		});
	}

	public static void LeaveSwipeView(IViewFrameInstance viewFrameInstance)
	{
		ServiceLocator.Instance.GetService<ITrackDataService>()?.StopCardRead(CancelReason.LeavingSwipeView);
		ICardReadAttempt currentCardReadAttempt = GetCurrentCardReadAttempt();
		if (currentCardReadAttempt != null && currentCardReadAttempt.CardReadAttemptStatus == CardReadAttemptStatus.Active)
		{
			currentCardReadAttempt.CardReadAttemptStatus = CardReadAttemptStatus.TimedOut;
		}
		StopTimer();
		LogCardReadAttemptStatistics();
		BaseViewController.RecordAdImpression(BaseViewController.GetViewModel<SwipeViewModel>(viewFrameInstance.Id)?.AdImpression);
	}

	private static void LogCardReadAttemptStatistics()
	{
		ICardReadAttemptCollection cardReadAttemptCollection = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection;
		ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
		int? num = (ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession())?.CardReadTechnicalFallbackCount;
		if (cardReadAttemptCollection == null)
		{
			return;
		}
		LogHelper.Instance.Log($"CardReadAttempts: count: {cardReadAttemptCollection.Count}; success count: {cardReadAttemptCollection.SuccessCount}; cancelled count: {cardReadAttemptCollection.CancelledCount}; time out count: {cardReadAttemptCollection.TimedOutCount}; Error count: {cardReadAttemptCollection.ErrorCount}; Track Data Service Type: {service?.GetType()?.Name}; Current Chip read failure (fallback) count: {num}");
		foreach (ICardReadAttempt item in cardReadAttemptCollection.OrderBy((ICardReadAttempt x) => x.StartDateTime))
		{
			StringBuilder stringBuilder = new StringBuilder($"Card Read Attempt {cardReadAttemptCollection.IndexOf(item) + 1}: ");
			stringBuilder.Append($" Status: {item.CardReadAttemptStatus}, ");
			if (item.AttemptedCancel)
			{
				stringBuilder.Append($" AttemptedCancel: True, CancelReason: {item.CancelReason}, ");
			}
			stringBuilder.Append($" Start Time: {item.StartDateTime}, ");
			stringBuilder.Append($" Available Input Type(s): {item.DeviceInputType}, ");
			ITrackData trackData = item.TrackData;
			if (trackData != null)
			{
				stringBuilder.Append($" Track Data Type: {trackData.GetType().Name}, ");
				stringBuilder.Append($" Card Source Type: {trackData.CardSourceType}, ");
				if (trackData is IEncryptedTrackData encryptedTrackData)
				{
					stringBuilder.Append($" Has Valid Data: {encryptedTrackData.HasValidData()}, ");
				}
				if (trackData is IUnencryptedTrackData unencryptedTrackData)
				{
					stringBuilder.Append($" Has Valid Data: {unencryptedTrackData.HasValidData()}, ");
				}
				if (trackData.Errors.IsNotNullOrEmpty())
				{
					stringBuilder.Append(string.Format(" Error Codes: {0},", string.Join(",", trackData.Errors.Select((Redbox.KioskEngine.ComponentModel.Error x) => $"{x.Code} - {x.Description}"))));
				}
			}
			if (item?.AuthorizeResult != null)
			{
				stringBuilder.Append($" Authorize Result: Success={item?.AuthorizeResult.Success} Response={item?.AuthorizeResult.Response}");
			}
			LogHelper.Instance.Log(stringBuilder.ToString());
		}
	}

	private static ICardReadAttempt GetCurrentCardReadAttempt()
	{
		return Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection?.GetMostRecent();
	}

	private static void UpdateCardReadAttemptWithTrackDataResult(ITrackData trackData)
	{
		CardReadAttemptStatus cardReadAttemptStatus = CardReadAttemptStatus.Error;
		if (trackData != null && trackData.HasValidData())
		{
			cardReadAttemptStatus = CardReadAttemptStatus.Success;
		}
		UpdateCardReadAttempt(cardReadAttemptStatus, trackData);
	}

	private static void UpdateCardReadAttempt(CardReadAttemptStatus cardReadAttemptStatus, ITrackData trackData)
	{
		ICardReadAttempt currentCardReadAttempt = GetCurrentCardReadAttempt();
		if (currentCardReadAttempt != null)
		{
			if (trackData != null)
			{
				currentCardReadAttempt.TrackData = trackData;
			}
			currentCardReadAttempt.EndDateTime = DateTime.Now;
			currentCardReadAttempt.CardReadAttemptStatus = cardReadAttemptStatus;
		}
	}

	private static void OnCardReadCompleteHandler(ITrackData trackData, bool chipCardRemovedFromReader, Guid? requestId)
	{
		try
		{
			LogHelper.Instance.Log("Starting SwipeViewController.OnCardReadCompleteHandler");
			StopTimer();
			ICardReadAttempt currentCardReadAttempt = GetCurrentCardReadAttempt();
			if (requestId.HasValue)
			{
				bool num;
				if (currentCardReadAttempt == null)
				{
					num = !requestId.HasValue;
				}
				else
				{
					Guid cardReadJobCommandRequestId = currentCardReadAttempt.CardReadJobCommandRequestId;
					Guid? guid = requestId;
					if (!guid.HasValue)
					{
						goto IL_010d;
					}
					num = cardReadJobCommandRequestId == guid.GetValueOrDefault();
				}
				if (!num)
				{
					goto IL_010d;
				}
			}
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.TrackData = trackData;
			}
			IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
			if (viewService?.UITaskScheduler != null)
			{
				Task.Factory.StartNew(delegate
				{
					LogHelper.Instance.Log("Starting UI update in SwipeViewController.OnCardReadCompleteHandler");
					try
					{
						UpdateCardReadAttemptWithTrackDataResult(trackData);
						if (viewService?.Peek() == "swipe_view")
						{
							OrchestrationService.SetCurrentUICulture();
							CardReadEvent cardReadEvent = new CardReadEvent
							{
								CardReadValid = false,
								CardSourceType = trackData?.CardSourceType.ToString(),
								NextCardReadIsInTechnicalFallback = trackData?.NextCardReadIsInTechnicalFallback,
								IsInTechnicalFallback = trackData?.IsInTechnicalFallback,
								CardHasChip = trackData?.CardHasChip,
								FallbackStatusAction = trackData?.FallbackStatusAction.ToString(),
								HasName = (!string.IsNullOrWhiteSpace(trackData?.FirstName) || !string.IsNullOrWhiteSpace(trackData?.LastName))
							};
							if (trackData == null || !trackData.HasValidData())
							{
								cardReadEvent.CardReadValid = false;
								Action action = delegate
								{
									MessagePopupViewController.ReenableTimer();
									viewService.Push("idle_timeout_message_view");
									viewService.Show();
								};
								cardReadEvent.Action = "bad_swipe_action";
								Action action2 = delegate
								{
									GoBackSafe("bad_swipe_action");
								};
								ShowMessage showMessage = MessagePopupViewController.ShowMessagePopupViewCardNotRecognized;
								ITrackData trackData2 = trackData;
								if (trackData2 != null && trackData2.NextCardReadIsInTechnicalFallback)
								{
									cardReadEvent.Action = "technical_fallback";
									action2 = delegate
									{
										GoBackSafe("technical_fallback");
									};
									showMessage = MessagePopupViewController.ShowMessagePopupViewCardNotRecognizedTechnicalFallback;
								}
								else if (trackData.HasErrors())
								{
									if (trackData.HasSwipedCardIsChipEnabledError())
									{
										cardReadEvent.Action = "swiped_card_is_chip_enabled";
										action2 = delegate
										{
											GoBackSafe("swiped_card_is_chip_enabled");
										};
										showMessage = MessagePopupViewController.ShowMessagePopupViewSwipedCardIsChipEnabled;
									}
									else if (_swipeViewType.IsReservation() && trackData.HasReservationMobileDeviceError())
									{
										cardReadEvent.Action = "reservation_with_mobile_wallet";
										action2 = delegate
										{
											GoBackSafe("reservation_with_mobile_wallet");
										};
										showMessage = MessagePopupViewController.ShowMessagePopupViewReservationMobileWalletError;
									}
									else if (trackData.HasTamperedError())
									{
										cardReadEvent.Action = "tampered_card_reader";
										action2 = delegate
										{
											GoBackSafe("tampered_card_reader");
											ServiceLocator.Instance.GetService<IRedboxRental>()?.EnterMaintenanceModeBecauseOfTamperedCardReader();
										};
										action = action2;
										showMessage = ((!_swipeViewType.IsReservation()) ? new ShowMessage(MessagePopupViewController.ShowMessagePopupViewCheckoutCardReaderTampered) : new ShowMessage(MessagePopupViewController.ShowMessagePopupViewReservationCardReaderTampered));
									}
									else if (trackData.CardSourceTypeIsEMVContactOrQuickChip())
									{
										cardReadEvent.Action = "bad_chip_read_action";
										action2 = delegate
										{
											GoBackSafe("bad_chip_read_action");
										};
										showMessage = MessagePopupViewController.ShowMessagePopupViewBadChipReadError;
									}
									else if (trackData.CardSourceTypeIsEMVContactlessOrMobileOrMSDContactless())
									{
										cardReadEvent.Action = "bad_contactless_read_action";
										action2 = delegate
										{
											GoBackSafe("bad_contactless_read_action");
										};
										showMessage = MessagePopupViewController.ShowMessagePopupViewBadContactlessReadError;
									}
								}
								if (action != null && action2 != null && showMessage != null)
								{
									showMessage(action, action2);
									BaseViewController.AnalyticsService?.AddViewEvent(cardReadEvent);
								}
								else
								{
									LogHelper.Instance.Log("OnCardReadCompleteHandler missing action.");
								}
							}
							else
							{
								cardReadEvent.CardReadValid = true;
								cardReadEvent.Action = "continue";
								BaseViewController.AnalyticsService?.AddViewEvent(cardReadEvent);
								if (QuickChipWasUsedAndCardNotRemovedFromReader(trackData, chipCardRemovedFromReader))
								{
									OrchestrationService.ViewShow("remove_card_view", new RemoveCardViewParameters
									{
										IsQuickChip = true,
										ContinueAction = PerformContinue
									});
								}
								else
								{
									PerformContinue();
								}
							}
						}
						else
						{
							LogHelper.Instance.Log("SwipeViewController.OnCardReadCompleteHandler: current view is not swipe_view.  Card read data will not be processed.");
						}
					}
					catch (Exception e2)
					{
						LogHelper.Instance.Log("An unhandled exception was raised in SwipeViewController.OnCardReadCompleteHandler.", e2);
					}
				}, CancellationToken.None, TaskCreationOptions.None, viewService?.UITaskScheduler);
			}
			else
			{
				LogHelper.Instance.Log("ViewService UITaskScheduler is null in SwipeViewController.OnCardReadCompleteHandler");
			}
			return;
			IL_010d:
			LogHelper.Instance.Log($"ViewService Received a response for request Id {requestId} which is not the current read attempt.");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in SwipeViewController.OnCardReadCompleteHandler.", e);
		}
	}

	private static bool QuickChipWasUsedAndCardNotRemovedFromReader(ITrackData trackData, bool chipCardRemovedFromReader)
	{
		bool result = false;
		if (trackData is IEMVTrackData { CardSourceType: CardSourceType.QuickChip } && !chipCardRemovedFromReader)
		{
			result = true;
		}
		return result;
	}

	private static void PerformContinue()
	{
		if (Redbox.Rental.Services.Session.Session.Instance?.TrackData != null && _continueAction != null)
		{
			if (_swipeViewType == SwipeViewType.RegularCheckOut || _swipeViewType == SwipeViewType.EncryptedSwipeRegularCheckOut || _swipeViewType == SwipeViewType.EMVInsertRegularCheckout || _swipeViewType == SwipeViewType.EMVInsertTapRegularCheckout || _swipeViewType == SwipeViewType.EMVInsertTapHideMobileRegularCheckout || _swipeViewType == SwipeViewType.EMVTapRegularCheckout)
			{
				ServiceLocator.Instance.GetService<IAfterSwipeOfferService>().StartOfferFlow(delegate(bool c)
				{
					ContinueAfterOfferFlow(c);
				});
			}
			else
			{
				_continueAction();
			}
		}
		else
		{
			GoBackSafe("trackdatanotify");
		}
	}

	private static void ContinueAfterOfferFlow(bool performContinue)
	{
		if (performContinue)
		{
			_continueAction();
		}
	}

	private static void StopTimer()
	{
		(BaseViewController.GetViewUserControl() as SwipeViewUserControl)?.StopTimer();
	}

	private static void HandleWPFHit()
	{
		(BaseViewController.GetViewUserControl() as SwipeViewUserControl)?.HandleWPFHit();
	}

	private static void GoBackSafe(object content)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe(content, popOnly: false);
	}

	private static void OnBackButtonPressed()
	{
		if (BaseViewController.GetViewService().Peek() == "swipe_view")
		{
			ICardReadAttempt currentCardReadAttempt = GetCurrentCardReadAttempt();
			if (currentCardReadAttempt != null && currentCardReadAttempt.CardReadAttemptStatus == CardReadAttemptStatus.Active)
			{
				SwipeViewModel viewModel = BaseViewController.GetViewModel<SwipeViewModel>();
				if (viewModel != null)
				{
					viewModel.BackButtonVisibility = Visibility.Hidden;
				}
				BaseViewController.AnalyticsService?.AddButtonPressEvent("SwipeCardSwipeBack");
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				if (service != null)
				{
					service["MatureWarning:Header"] = string.Empty;
				}
				UpdateCardReadAttempt(CardReadAttemptStatus.Cancelled, null);
				if (_cancelAction != null)
				{
					_cancelAction();
				}
				else
				{
					GoBackSafe("swipe_view:back_button");
				}
			}
			else
			{
				LogHelper.Instance.Log($"Back button from swipe view not processed because the current card read attempt is {currentCardReadAttempt?.CardReadAttemptStatus} and not Active.");
			}
		}
		else
		{
			LogHelper.Instance.Log("Back button from swipe view not processed because the swipe view is not on top of the stack.");
		}
	}

	private static void SwipeTimerTimeout()
	{
		BaseViewController.AnalyticsService?.AddViewEvent(new CardReadEvent
		{
			CardReadValid = false,
			Action = "timeout"
		});
		UpdateCardReadAttempt(CardReadAttemptStatus.TimedOut, null);
		GoBackSafe("swipe_view:timeout");
	}
}
