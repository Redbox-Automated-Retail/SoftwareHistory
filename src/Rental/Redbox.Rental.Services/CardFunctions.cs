using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public class CardFunctions
{
	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	private static void UpdateCardReadAttempt(CardReadAttemptStatus cardReadAttemptStatus, ITrackData trackData)
	{
		ICardReadAttempt cardReadAttempt = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection?.GetMostRecent();
		if (cardReadAttempt != null)
		{
			if (trackData != null)
			{
				cardReadAttempt.TrackData = trackData;
			}
			cardReadAttempt.EndDateTime = DateTime.Now;
			cardReadAttempt.CardReadAttemptStatus = cardReadAttemptStatus;
		}
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

	private static bool QuickChipWasUsedAndCardNotRemovedFromReader(ITrackData trackData, bool chipCardRemovedFromReader)
	{
		bool result = false;
		if (trackData is IEMVTrackData { CardSourceType: CardSourceType.QuickChip } && !chipCardRemovedFromReader)
		{
			result = true;
		}
		return result;
	}

	public static void CardReadComplete(SwipeViewType swipeType, ITrackData trackData, bool chipCardRemovedFromReader, Guid? requestId, Action onContinue = null, Action onTimeout = null)
	{
		try
		{
			LogHelper.Instance.Log("Starting CardFunctions.CardReadComplete");
			ICardReadAttempt cardReadAttempt = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection?.GetMostRecent();
			if (requestId.HasValue)
			{
				bool num;
				if (cardReadAttempt == null)
				{
					num = !requestId.HasValue;
				}
				else
				{
					Guid cardReadJobCommandRequestId = cardReadAttempt.CardReadJobCommandRequestId;
					Guid? guid = requestId;
					if (!guid.HasValue)
					{
						goto IL_0137;
					}
					num = cardReadJobCommandRequestId == guid.GetValueOrDefault();
				}
				if (!num)
				{
					goto IL_0137;
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
					LogHelper.Instance.Log("Starting UI update in CardFunctions.CardReadComplete");
					try
					{
						UpdateCardReadAttemptWithTrackDataResult(trackData);
						if (IsCurrentViewTheCardReadView(swipeType, out var cardReadViewName))
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
								Action action2 = ((onTimeout != null) ? onTimeout : action);
								cardReadEvent.Action = "bad_swipe_action";
								Action action3 = delegate
								{
									OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, "bad_swipe_action");
								};
								Action<Action, Action> action4 = MessagePopupViewController.ShowMessagePopupViewCardNotRecognized;
								ITrackData trackData2 = trackData;
								if (trackData2 != null && trackData2.NextCardReadIsInTechnicalFallback)
								{
									cardReadEvent.Action = "technical_fallback";
									action3 = delegate
									{
										OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, "technical_fallback");
									};
									action4 = MessagePopupViewController.ShowMessagePopupViewCardNotRecognizedTechnicalFallback;
								}
								else if (trackData.HasErrors())
								{
									if (trackData.HasSwipedCardIsChipEnabledError())
									{
										cardReadEvent.Action = "swiped_card_is_chip_enabled";
										action3 = delegate
										{
											OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, "swiped_card_is_chip_enabled");
										};
										action4 = MessagePopupViewController.ShowMessagePopupViewSwipedCardIsChipEnabled;
									}
									else if (swipeType.IsReservation() && trackData.HasReservationMobileDeviceError())
									{
										cardReadEvent.Action = "reservation_with_mobile_wallet";
										action3 = delegate
										{
											OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, "reservation_with_mobile_wallet");
										};
										action4 = MessagePopupViewController.ShowMessagePopupViewReservationMobileWalletError;
									}
									else if (trackData.HasTamperedError())
									{
										cardReadEvent.Action = "tampered_card_reader";
										action3 = delegate
										{
											OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, "tampered_card_reader");
											ServiceLocator.Instance.GetService<IRedboxRental>()?.EnterMaintenanceModeBecauseOfTamperedCardReader();
										};
										action = action3;
										action4 = ((!swipeType.IsReservation()) ? new Action<Action, Action>(MessagePopupViewController.ShowMessagePopupViewCheckoutCardReaderTampered) : new Action<Action, Action>(MessagePopupViewController.ShowMessagePopupViewReservationCardReaderTampered));
									}
									else if (trackData.CardSourceTypeIsEMVContactOrQuickChip())
									{
										cardReadEvent.Action = "bad_chip_read_action";
										action3 = delegate
										{
											OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, "bad_chip_read_action");
										};
										action4 = MessagePopupViewController.ShowMessagePopupViewBadChipReadError;
									}
									else if (trackData.CardSourceTypeIsEMVContactlessOrMobileOrMSDContactless())
									{
										cardReadEvent.Action = "bad_contactless_read_action";
										action3 = delegate
										{
											OrchestrationService.UnwindToSafe(cardReadViewName, discard: true, "bad_contactless_read_action");
										};
										action4 = MessagePopupViewController.ShowMessagePopupViewBadContactlessReadError;
									}
								}
								if (action2 != null && action3 != null && action4 != null)
								{
									action4(action2, action3);
									AnalyticsService?.AddViewEvent(cardReadEvent);
								}
								else
								{
									LogHelper.Instance.Log("CardFunctions.CardReadComplete missing action.");
								}
							}
							else
							{
								cardReadEvent.CardReadValid = true;
								cardReadEvent.Action = "continue";
								AnalyticsService?.AddViewEvent(cardReadEvent);
								if (QuickChipWasUsedAndCardNotRemovedFromReader(trackData, chipCardRemovedFromReader))
								{
									OrchestrationService.ViewShow("remove_card_view", new RemoveCardViewParameters
									{
										IsQuickChip = true,
										ContinueAction = onContinue
									});
								}
								else
								{
									onContinue?.Invoke();
								}
							}
						}
						else
						{
							LogHelper.Instance.Log($"CardFunctions.CardReadComplete: current view is not {cardReadViewName}.  Card read data will not be processed.");
						}
					}
					catch (Exception e2)
					{
						LogHelper.Instance.Log("An unhandled exception was raised in CardFunctions.CardReadComplete.", e2);
					}
				}, CancellationToken.None, TaskCreationOptions.None, viewService?.UITaskScheduler);
			}
			else
			{
				LogHelper.Instance.Log("ViewService UITaskScheduler is null in CardFunctions.CardReadComplete");
			}
			return;
			IL_0137:
			LogHelper.Instance.Log($"ViewService Received a response for request Id {requestId} which is not the current read attempt.");
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in CardFunctions.CardReadComplete.", e);
		}
	}

	private static bool IsCurrentViewTheCardReadView(SwipeViewType swipeType, out string cardReadViewName)
	{
		cardReadViewName = ((swipeType == SwipeViewType.ZeroTouchReservationPickup) ? "start_view" : "swipe_view");
		bool result = false;
		string text = OrchestrationService.PeekView();
		if (swipeType != SwipeViewType.ZeroTouchReservationPickup && text == "swipe_view")
		{
			result = true;
		}
		else if (swipeType == SwipeViewType.ZeroTouchReservationPickup)
		{
			if (text == "start_view")
			{
				result = true;
			}
			else if (text == "please_wait_view" && OrchestrationService.PeekView(1) == "start_view")
			{
				result = true;
			}
		}
		return result;
	}
}
