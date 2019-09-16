using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.ViewDefinitions;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class CheckoutFunctions
{
	public class Constants
	{
		public const string CreditCardError = "CreditCardError";

		public const string CustomerDisabled = "CustomerDisabled";

		public const string EnforceAccertify = "EnforceAccertify";

		public const string Declined = "Declined";

		public const string GCDeclined = "GCDeclined";

		public const string GCRentalLimitExceeded = "GCRentalLimitExceeded";

		public const string GCInsufficientBalance = "GCInsufficientBalance";

		public const string NewCustomerOnly = "NewCustomerOnly";

		public const string CodeNotValidAtThisLocation = "CodeNotValidAtThisLocation";

		public const string CodeAlreadyUsed = "CodeAlreadyUsed";

		public const string CodeNotValid = "CodeNotValid";

		public const string AvsFailure = "AvsFailure";

		public const string RentalLimitExceeded = "RentalLimitExceeded";

		public const string GameLimitExceeded = "GameLimitExceeded";

		public const string OfflineGCDeclined = "OfflineGCDeclined";

		public const string OfflineItemsLimitZero = "OfflineItemsLimitZero";

		public const string OfflineGameRentalAndPurchaseLimitZero = "OfflineGameRentalAndPurchaseLimitZero";

		public const string OfflineGameRentalLimitZero = "OfflineGameRentalLimitZero";

		public const string OfflineGamePurchaseLimitZero = "OfflineGamePurchaseLimitZero";

		public const string OfflineGameRentalLimitExceeded = "OfflineGameRentalLimitExceeded";

		public const string OfflineGamePurchaseLimitExceeded = "OfflineGamePurchaseLimitExceeded";

		public const string OfflineItemsLimitExceeded = "OfflineItemsLimitExceeded";

		public const string OfflineSellThruNewRefused = "OfflineSellThruNewRefused";

		public const string OfflineCardBlocklisted = "OfflineCardBlocklisted";

		public const string NewCustomerRentalLimitExceeded = "NewCustomerRentalLimitExceeded";

		public const string Error = "Error";

		public const string PleaseWaitTimer = "PleaseWaitTimer";

		public const string OfflineFallbackDecline = "OfflineFallbackDecline";
	}

	public static void BeginCheckout()
	{
		LogHelper.Instance.Log("BeginCheckout");
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart currentRentalShoppingCart = service.CurrentRentalShoppingCart;
		if (currentRentalShoppingCart == null)
		{
			return;
		}
		List<string> list = service?.AssignBarcodesToVend(currentRentalShoppingCart);
		if (list != null && list.Count > 0)
		{
			bool flag = false;
			foreach (IRentalShoppingCartItem item in currentRentalShoppingCart.Items)
			{
				if (item.Action == RentalShoppingCartItemAction.rental)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				currentRentalShoppingCart.ClearPromoCode();
			}
			(ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(currentRentalShoppingCart.ShoppingSessionId.ToString()))?.AddEvent(ShoppingSessionEventType.Checkout, "User activated check out.");
			SwipeViewController.SetSwipeViewParameters2(SwipeViewCancel, SwipeViewContinue, SwipeViewType.RegularCheckOut);
			OrchestrationService.ExecuteScript("check_out");
		}
		else
		{
			currentRentalShoppingCart.Clear();
			OrchestrationService.SetCurrentUICulture();
			Action action = delegate
			{
				OrchestrationService.ViewGoBackSafe("no_barcodes_to_vend", popOnly: false);
			};
			MessagePopupViewController.ShowSimpleOkMessage(null, Resources.inventory_error, action, action);
		}
	}

	public static void SwipeViewCancel()
	{
		OrchestrationService.UnwindToSafe("shopping_cart_view", discard: false, "check_out:swipe_view_cancel_func");
	}

	public static void SwipeViewContinue()
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart rentalShoppingCart = service?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			service?.AddMultiDiscDiscsToRentalShoppingCart(rentalShoppingCart.ShoppingSessionId);
			OrchestrationService.InvokePrepositionVendCallback();
			if (rentalShoppingCart.Count > 0)
			{
				IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
				if (OrchestrationService.IsGiftCard())
				{
					if (service2 != null && service2.GiftCardsEnabled)
					{
						OrchestrationService.ViewGoBackSafe("gift card enabled. perform gc authorize", popOnly: true);
						Authorize(AuthorizeType.GiftCardAuthorize);
						return;
					}
					Action action = delegate
					{
						OrchestrationService.ViewGoBackSafe("gift card not enabled", popOnly: false);
					};
					MessagePopupViewController.ShowGiftCardNotEnabledMessage(action, action);
				}
				else if (service2 != null && service2.ShowAuthChangeMessage)
				{
					Preauthorize();
				}
				else
				{
					SetPreauthorizeValues(rentalShoppingCart.ShoppingSessionId, authAcceptFlag: false, 0, 0m, skipAuthRule: true);
					GetZipCodeIfRequired();
				}
			}
			else
			{
				OrchestrationService.UnwindToSafe("shopping_cart_view", discard: false, "check_out:INVENTORY ERROR no barcodes, cart cleared");
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in CheckoutFunctions.SwipeViewContinue.  RentalShoppingCart is null");
		}
	}

	private static void GetZipCodeIfRequired()
	{
		_ = (Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection?.GetMostRecent())?.TrackData;
		OrchestrationService.ViewShow("zip_code_view", new ZipCodeViewParameter
		{
			ContinueAction = ProcessZipCodeEntry
		});
	}

	private static void ProcessZipCodeEntry(string zipCode)
	{
		if (Redbox.Rental.Services.Session.Session.Instance.UserInfo != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.UserInfo.PostalCode = zipCode;
		}
		OrchestrationService.ViewGoBackSafe("called from CheckoutFunctions.ProcessZipCodeEntry", popOnly: true);
		Authorize(AuthorizeType.Authorize);
	}

	public static bool IsCurrentCardOnOfflineBadCardList()
	{
		bool result = false;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
		if (service != null && trackData != null && trackData.HasValidData())
		{
			string cardHashId = trackData.CardHashId;
			result = service.IsAccountOnOfflineBadCardList(cardHashId);
		}
		return result;
	}

	public static void UpdateAccountOnOfflineBadCardList()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
		if (service != null && trackData != null && trackData.HasValidData())
		{
			string cardHashId = trackData.CardHashId;
			service?.UpdateAccountOnOfflineBadCardList(cardHashId);
		}
	}

	public static void RemoveAccountFromOfflineBadCardList()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
		if (service != null && trackData != null && trackData.HasValidData())
		{
			string cardHashId = trackData.CardHashId;
			service?.RemoveAccountFromOfflineBadCardList(cardHashId);
		}
	}

	public static void ProcessAuthError(LuaTable properties, string shoppingCartId)
	{
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		int offlineItemsLimit = instance.OfflineItemsLimit;
		int offlineGameRentalLimit = instance.OfflineGameRentalLimit;
		int offlineGamePurchaseLimit = instance.OfflineGamePurchaseLimit;
		string text = (string)properties["response"];
		Action action = ShoppingCartViewFunctions.ViewShoppingCart;
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(new Guid(shoppingCartId));
		OrchestrationService.SetCurrentUICulture();
		if (ServiceLocator.Instance.GetService<IApplicationState>().EnableOfflineBadCardList && (bool)properties["is_online"] && !(bool)properties["stand_alone"])
		{
			switch (text)
			{
			case "GameLimitExceeded":
			case "RentalLimitExceeded":
			case "NewCustomerRentalLimitExceeded":
			case "Declined":
			case "CustomerDisabled":
			case "CreditCardError":
				UpdateAccountOnOfflineBadCardList();
				break;
			}
		}
		string empty = string.Empty;
		string empty2 = string.Empty;
		string got_it_button = Resources.got_it_button;
		string text2 = null;
		string offline_game_limit_exceeded_message_game = Resources.offline_game_limit_exceeded_message_game;
		switch (text)
		{
		case "OfflineItemsLimitZero":
			text2 = "OfflineItemsLimitZero";
			empty = Resources.oops;
			empty2 = Resources.offline_items_limit_zero_message;
			break;
		case "OfflineGameRentalAndPurchaseLimitZero":
			text2 = "OfflineGameRentalAndPurchaseLimitZero";
			empty = Resources.oops;
			empty2 = string.Format(Resources.offline_game_rental_and_purchase_limit_zero_message, offlineItemsLimit);
			break;
		case "OfflineGameRentalLimitZero":
			text2 = "OfflineGameRentalLimitZero";
			empty = Resources.oops;
			empty2 = string.Format(Resources.offline_game_rental_limit_zero_message, offlineItemsLimit);
			break;
		case "OfflineGamePurchaseLimitZero":
			text2 = "OfflineGamePurchaseLimitZero";
			empty = Resources.oops;
			empty2 = string.Format(Resources.offline_game_purchase_limit_zero_message, offlineItemsLimit);
			break;
		case "OfflineGameRentalLimitExceeded":
			text2 = "offlineGameRentalLimitExceeded";
			empty = Resources.offline_game_rental_limit_exceeded_message_header;
			offline_game_limit_exceeded_message_game = Resources.offline_game_limit_exceeded_message_game;
			if (offlineGameRentalLimit > 1)
			{
				offline_game_limit_exceeded_message_game = Resources.offline_game_limit_exceeded_message_games;
			}
			empty2 = string.Format(Resources.offline_game_rental_limit_exceeded_message, offlineItemsLimit, offlineGameRentalLimit, offline_game_limit_exceeded_message_game);
			break;
		case "OfflineGamePurchaseLimitExceeded":
			text2 = "offlineGamePurchaseLimitExceeded";
			empty = Resources.offline_game_purchase_limit_exceeded_message_header;
			offline_game_limit_exceeded_message_game = Resources.offline_game_limit_exceeded_message_game;
			if (offlineGamePurchaseLimit > 1)
			{
				offline_game_limit_exceeded_message_game = Resources.offline_game_limit_exceeded_message_games;
			}
			empty2 = string.Format(Resources.offline_game_purchase_limit_exceeded_message, offlineItemsLimit, offlineGamePurchaseLimit, offline_game_limit_exceeded_message_game);
			break;
		case "OfflineItemsLimitExceeded":
			text2 = "offlineItemsLimitExceeded";
			empty = Resources.offline_items_limit_exceeded_message_header;
			empty2 = string.Format(Resources.offline_items_limit_exceeded_message, offlineItemsLimit);
			break;
		case "GameLimitExceeded":
			empty = Resources.game_limit_exceeded_message_header;
			text2 = "gameLimitExceededOnline";
			empty2 = Resources.game_limit_exceeded_message;
			break;
		case "RentalLimitExceeded":
			empty = Resources.rental_limit_exceeded_message_header;
			text2 = "rentalLimitExceededOnline";
			empty2 = Resources.rental_limit_exceeded_message;
			break;
		case "NewCustomerRentalLimitExceeded":
			empty = Resources.rental_limit_exceeded_message_header;
			text2 = "newCustomerRentalLimitExceededOnline";
			empty2 = Resources.rental_limit_exceeded_new_customer_message;
			break;
		case "AvsFailure":
			text2 = "cardNotAuthorized";
			empty = null;
			empty2 = Resources.postal_code_mismatch_message;
			break;
		case "CodeNotValid":
			rentalShoppingCart.ClearPromoCode();
			text2 = "promoNotValid";
			empty = Resources.promo_code_not_valid_message_title;
			empty2 = Resources.promo_code_not_valid_message;
			break;
		case "CodeAlreadyUsed":
			if (PromotionService.Instance.IsRentTwoInCartOfferRunning)
			{
				rentalShoppingCart.InCartDoNotShow = true;
				rentalShoppingCart.InCartPromoOfferMade = false;
				rentalShoppingCart.InCartPromoOfferCancel = false;
			}
			rentalShoppingCart.ClearPromoCode();
			text2 = "promoAlreadyUsed";
			empty = Resources.promo_code_already_used_message_title;
			empty2 = Resources.promo_code_already_used_message;
			break;
		case "CodeNotValidAtThisLocation":
			rentalShoppingCart.ClearPromoCode();
			text2 = "promoInvalidLocation";
			empty = Resources.promo_code_error_title;
			empty2 = Resources.promo_code_not_valid_location_message;
			break;
		case "NewCustomerOnly":
			rentalShoppingCart.ClearPromoCode();
			empty = Resources.promo_code_transaction_error_title;
			empty2 = Resources.promo_code_new_customer_only_message;
			break;
		case "OfflineSellThruNewRefused":
			empty = null;
			empty2 = Resources.sell_thru_new_offline_denied;
			break;
		case "OfflineGCDeclined":
			text2 = "redboxCardKioskOffline";
			empty = Resources.gc_offline_declined_title;
			empty2 = Resources.gc_offline_declined;
			break;
		case "GCInsufficientBalance":
			text2 = "redboxCardInsufficientFunds";
			empty = Resources.gc_insufficient_balance_title;
			empty2 = string.Format(Resources.gc_insufficient_balance, properties["gift_card_balance"]);
			break;
		case "GCRentalLimitExceeded":
			text2 = "redboxCardRentalLimitExceeded";
			empty = Resources.gc_rental_limit_exceeded_title;
			empty2 = string.Format(Resources.gc_rental_limit_exceeded, properties["gift_card_max_discs"]);
			break;
		case "GCDeclined":
		case "Declined":
			empty = Resources.gc_declined_title;
			empty2 = Resources.gc_declined;
			break;
		case "EnforceAccertify":
			text2 = "accertifyRejectedGames";
			empty = Resources.accertify_enforce_accertify_header;
			empty2 = Resources.accertify_enforce_accertify_message;
			break;
		default:
			text2 = "cardNotAuthorized";
			empty = null;
			empty2 = Resources.authorization_failure_message;
			break;
		}
		List<string> list = new List<string> { "authorizationDecline" };
		if (!string.IsNullOrWhiteSpace(text2))
		{
			list.Add(text2);
		}
		MessagePopupViewController.ShowAuthErrorMessage(empty, empty2, got_it_button, action, action, list);
	}

	public static void ShowMultiDiscVendViewIfNeeded(Action continueCheckoutCallback)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && rentalShoppingCart.HasMultiDiscItem())
		{
			OrchestrationService.ViewShow("multi_disc_vend_view", continueCheckoutCallback);
		}
		else
		{
			continueCheckoutCallback?.Invoke();
		}
	}

	public static void ShowReservationDetailsView(Action continueReservationPickupCallback)
	{
		OrchestrationService.ViewShow("reservation_details_view", continueReservationPickupCallback);
	}

	public static bool CheckOfflineValidations(out string response)
	{
		response = null;
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		QueuedPendingCounts pendingCounts = QueueFunctions.GetPendingCounts();
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		ISession session = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		IRentalShoppingCart rentalShoppingCart = service?.CurrentRentalShoppingCart;
		int num = rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental && x.TitleFamily == TitleFamily.Games);
		int num2 = rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.purchase && x.TitleFamily == TitleFamily.Games);
		rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental && x.TitleFamily == TitleFamily.Movies);
		rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.purchase && x.TitleFamily == TitleFamily.Movies);
		int count = rentalShoppingCart.Count;
		IShoppingSession session2 = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(rentalShoppingCart?.ShoppingSessionId.ToString());
		bool flag = OrchestrationService.IsCardBlocklisted();
		session2?.ShoppingCart?.SetBlocklisted(flag);
		if (flag)
		{
			response = "OfflineCardBlocklisted";
		}
		else if (session?.TrackData != null && session.TrackData.IsInTechnicalFallback && session.TrackData.CardSourceType == CardSourceType.Swipe && session.TrackData.CardHasChip)
		{
			response = "OfflineFallbackDecline";
		}
		else if (OrchestrationService.IsGiftCard())
		{
			response = "OfflineGCDeclined";
		}
		else if (instance.OfflineItemsLimit == 0)
		{
			response = "OfflineItemsLimitZero";
		}
		else if (instance.OfflineGameRentalLimit == 0 && instance.OfflineGamePurchaseLimit == 0 && pendingCounts.GameRentalCount + num + pendingCounts.GamePurchaseCount + num2 > 0)
		{
			response = "OfflineGameRentalAndPurchaseLimitZero";
		}
		else if (instance.OfflineGameRentalLimit == 0 && pendingCounts.GameRentalCount + num > 0)
		{
			response = "OfflineGameRentalLimitZero";
		}
		else if (instance.OfflineGamePurchaseLimit == 0 && pendingCounts.GamePurchaseCount + num2 > 0)
		{
			response = "OfflineGamePurchaseLimitZero";
		}
		else if (pendingCounts.GameRentalCount + num > instance.OfflineGameRentalLimit)
		{
			response = "OfflineGameRentalLimitExceeded";
		}
		else if (pendingCounts.GamePurchaseCount + num2 > instance.OfflineGamePurchaseLimit)
		{
			response = "OfflineGamePurchaseLimitExceeded";
		}
		else if (pendingCounts.TotalCount + count > instance.OfflineItemsLimit)
		{
			response = "OfflineItemsLimitExceeded";
		}
		else if (ApplicationState.Instance.EnableOfflineBadCardList && !ApplicationState.Instance.IsStandAlone && IsCurrentCardOnOfflineBadCardList())
		{
			response = "Error";
		}
		else
		{
			IDataService service2 = ServiceLocator.Instance.GetService<IDataService>();
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				IKioskProduct kioskProduct = null;
				service2?.GetProduct(item.ProductId, excludeTitleRollup: true, out kioskProduct);
				if ((kioskProduct as ITitleProduct).SellThruNew)
				{
					response = "OfflineSellThruNewRefused";
					break;
				}
			}
		}
		return string.IsNullOrEmpty(response);
	}

	public static void ReportAuthorizeResultToDeviceService(bool authorizeSuccessful)
	{
		if (ServiceLocator.Instance.GetService<ITrackDataService>() is IDeviceServiceTrackDataService)
		{
			ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.ReportAuthorizeResult(authorizeSuccessful);
		}
	}

	public static void SetReadCardAttemptAuthorizeResult(IAuthorizeResult authorizeResult)
	{
		ICardReadAttempt cardReadAttempt = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection?.GetMostRecent();
		if (cardReadAttempt != null)
		{
			cardReadAttempt.AuthorizeResult = authorizeResult;
		}
	}

	public static void ShowPleaseWaitBecauseOfDisconnectedCardReader()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		Task.Factory.StartNew(delegate
		{
			LogHelper.Instance.Log(string.Format("Showing {0} while waiting for card reader reconnection.", "please_wait_view"));
			if (Redbox.Rental.Services.Session.Session.Instance != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.WaitingForCardReaderConnection = true;
			}
			OrchestrationService.ViewShow("please_wait_view", null);
			int value = ServiceLocator.Instance.GetService<IConfiguration>()?.DeviceServicePleaseWaitDuration ?? 90000;
			ServiceLocator.Instance.GetService<ITimerService>()?.CreateTimer("PleaseWaitTimer", value, value, delegate
			{
				RemovePleaseWaitTimer();
				LogHelper.Instance.Log(string.Format("{0} timed out.  Attempt to enter maintenance mode", "please_wait_view"));
				IRedboxRental service2 = ServiceLocator.Instance.GetService<IRedboxRental>();
				Redbox.Rental.Services.Session.Session.Instance.WaitingForCardReaderConnection = false;
				service2?.EnterMaintenanceModeBecauseOfDisconnectedCardReader();
			})?.Start();
		}, CancellationToken.None, TaskCreationOptions.None, service.UITaskScheduler).Wait();
	}

	private static void RemovePleaseWaitTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		ITimer timer = service.GetTimer("PleaseWaitTimer");
		if (timer != null)
		{
			timer?.Stop();
			service.RemoveTimer("PleaseWaitTimer");
		}
	}

	public static void PopPleaseWaitViewBecauseCardReaderReconnected()
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.WaitingForCardReaderConnection = false;
		}
		IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
		Task.Factory.StartNew(delegate
		{
			if (viewService?.Peek() == "please_wait_view")
			{
				RemovePleaseWaitTimer();
				viewService.GoBackSafe("PopPleaseWaitViewBecauseCardReaderReconnected", popOnly: false);
			}
		}, CancellationToken.None, TaskCreationOptions.None, viewService.UITaskScheduler).Wait();
	}

	private static void SetShoppingSessionPostalCode(Guid shoppingSessionId, string postalCode)
	{
		(ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(shoppingSessionId.ToString()))?.ShoppingCart?.SetPostalCode(postalCode);
	}

	public static void Preauthorize()
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart rentalShoppingCart = service?.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null)
		{
			return;
		}
		LogHelper.Instance.Log($"PreAuth for Session ID: {rentalShoppingCart.ShoppingSessionId}");
		ISession instance = Redbox.Rental.Services.Session.Session.Instance;
		if (instance != null && instance.UserInfo?.PostalCode != null)
		{
			SetShoppingSessionPostalCode(rentalShoppingCart.ShoppingSessionId, instance?.UserInfo?.PostalCode);
		}
		service?.AssignBarcodesToVend(rentalShoppingCart);
		foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
		{
			LogHelper.Instance.Log($"Session add Line Item: {item?.Barcode}  for  {item?.Action} Price: {item.Price} {item?.TitleFamily}-{item?.TitleType} {item?.SourceType} ");
		}
		OrchestrationService.ViewShow("general_message_view", new GeneralMessageViewModel
		{
			DisableIdleTimer = false,
			MessageText = Resources.authorization_message
		});
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddNode("authorizations", "preauth", rentalShoppingCart?.ShoppingSessionId.ToString());
		string customerProfileNumber = instance?.PersonalizationSession?.CustomerProfileNumber;
		ServiceLocator.Instance.GetService<IMerchantServices>()?.PreAuth(rentalShoppingCart.ShoppingSessionId, customerProfileNumber, OnPreauthorizeComplete);
	}

	private static void SetPreauthorizeValues(Guid sessionId, bool authAcceptFlag, int authRuleId, decimal authAmount, bool skipAuthRule)
	{
		ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId.ToString())?.ShoppingCart.SetVariableAuthValues(authAcceptFlag, authRuleId, authAmount, skipAuthRule);
	}

	public static void OnPreauthorizeComplete(IRemoteServiceResult result)
	{
		if (result != null)
		{
			IPreauthorizeResult preauthorizeResult = (result.Properties.ContainsKey("PreauthorizeResult") ? (result.Properties["PreauthorizeResult"] as IPreauthorizeResult) : null);
			if (preauthorizeResult != null)
			{
				IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
				if (rentalShoppingCart != null && rentalShoppingCart.ShoppingSessionId == preauthorizeResult.SessionId)
				{
					Redbox.Rental.Services.Session.Session.Instance.PreauthorizeResult = preauthorizeResult;
					Dictionary<string, string> attributes = new Dictionary<string, string>
					{
						{
							"online",
							preauthorizeResult.IsOnline.ToString()
						},
						{
							"timestamp",
							DateTime.Now.ToString("MM/dd/yyyy HH:mm tt")
						}
					};
					OrchestrationService.AnalyticsAddAttribute("preauth", attributes, rentalShoppingCart.ShoppingSessionId);
					AddEventAuthorization(rentalShoppingCart.ShoppingSessionId, "preauth", preauthorizeResult.IsOnline, null, null);
					SetPreauthorizeValues(rentalShoppingCart.ShoppingSessionId, preauthorizeResult.PromptRequired, preauthorizeResult.AuthRuleId, preauthorizeResult.AuthAmount, preauthorizeResult.SkipAuthRule);
					IViewService service = ServiceLocator.Instance.GetService<IViewService>();
					if (service?.UITaskScheduler != null)
					{
						Task.Factory.StartNew(delegate
						{
							GetZipCodeIfRequired();
						}, CancellationToken.None, TaskCreationOptions.None, service.UITaskScheduler);
					}
				}
				else
				{
					LogHelper.Instance.Log($"OnPreauthorizeComplete - preauthorize result not processed.  Session {preauthorizeResult.SessionId} is no longer current.");
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in OnPreauthorizeComplete - preauthorize result is missing.");
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in OnPreauthorizeComplete - result is null");
		}
	}

	public static void Authorize(AuthorizeType authorizeType)
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart rentalShoppingCart = service?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			LogHelper.Instance.Log($"{authorizeType} for Session ID: {rentalShoppingCart?.ShoppingSessionId}");
			ISession instance = Redbox.Rental.Services.Session.Session.Instance;
			if (authorizeType == AuthorizeType.Authorize && instance != null && instance.UserInfo?.PostalCode != null && rentalShoppingCart != null)
			{
				SetShoppingSessionPostalCode(rentalShoppingCart.ShoppingSessionId, instance?.UserInfo?.PostalCode);
			}
			service?.AssignBarcodesToVend(rentalShoppingCart);
			bool flag = false;
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				LogHelper.Instance.Log($"Session add Line Item: {item?.Barcode}  for  {item?.Action} Price: {item.Price} {item?.TitleFamily}-{item?.TitleType} {item?.SourceType} ");
				if (item.Status == LineItemStatus.InventoryError)
				{
					flag = true;
				}
			}
			OrchestrationService.ViewShow("general_message_view", new GeneralMessageViewModel
			{
				DisableIdleTimer = false,
				MessageText = Resources.authorization_message
			});
			ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddNode("authorizations", GetAuthorizeTypeText(authorizeType), rentalShoppingCart?.ShoppingSessionId.ToString());
			if (flag && rentalShoppingCart.Promotion != null && rentalShoppingCart.Promotion.Name == PromotionService.Instance.GetInCartPromoCode())
			{
				LogHelper.Instance.Log($"> Inventory Error: In-Cart promo code {rentalShoppingCart?.Promotion?.Name} cleared.");
				rentalShoppingCart.ClearPromoCode();
			}
			string customerProfileNumber = instance?.PersonalizationSession?.CustomerProfileNumber;
			ServiceLocator.Instance.GetService<IMerchantServices>()?.Authorize(rentalShoppingCart.ShoppingSessionId, authorizeType, customerProfileNumber, OnAuthorizeComplete);
		}
		else
		{
			LogHelper.Instance.Log("Error in CheckoutFunctions.Authorize.  RentalShoppingCart is null.");
		}
	}

	private static string GetAuthorizeTypeText(AuthorizeType authorizeType)
	{
		if (authorizeType != AuthorizeType.Authorize)
		{
			return "gc_authorize";
		}
		return "authorize";
	}

	private static void OnAuthorizeComplete(IRemoteServiceResult remoteServiceResult)
	{
		if (remoteServiceResult != null)
		{
			IAuthorizeResult authorizeResult = (remoteServiceResult.Properties.ContainsKey("AuthorizeResult") ? (remoteServiceResult.Properties["AuthorizeResult"] as IAuthorizeResult) : null);
			if (authorizeResult != null)
			{
				IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
				if (rentalShoppingCart != null && rentalShoppingCart.ShoppingSessionId == authorizeResult.SessionId)
				{
					IViewService service = ServiceLocator.Instance.GetService<IViewService>();
					if (service == null || !service.IsInMaintenanceMode())
					{
						bool num = authorizeResult.AuthorizeType == AuthorizeType.Authorize;
						bool flag = authorizeResult.AuthorizeType == AuthorizeType.GiftCardAuthorize;
						bool flag2 = (num && authorizeResult.Response == AuthorizeResponse.Approved.ToString()) || (flag && authorizeResult.Response == GCAuthorizeResponse.Approved.ToString());
						ReportAuthorizeResultToDeviceService(flag2);
						if (flag2)
						{
							OrchestrationService.ShoppingCartCommit(rentalShoppingCart.ShoppingSessionId.ToString(), "User checked out successfully.", shouldEndAndSave: false);
						}
						else if (remoteServiceResult?.Errors != null && remoteServiceResult.Errors.ContainsCode("(D003)"))
						{
							LogHelper.Instance.Log($"> Invalid Promo code {rentalShoppingCart?.Promotion?.Name} cleared.");
							rentalShoppingCart.ClearPromoCode();
						}
						Dictionary<string, string> attributes = new Dictionary<string, string>
						{
							{
								"approved",
								flag2.ToString()
							},
							{
								"online",
								authorizeResult.IsOnline.ToString()
							},
							{
								"timestamp",
								DateTime.Now.ToString("MM/dd/yyyy HH:mm tt")
							},
							{
								"response",
								authorizeResult.Response.ToString()
							}
						};
						OrchestrationService.AnalyticsAddAttribute(GetAuthorizeTypeText(authorizeResult.AuthorizeType), attributes, rentalShoppingCart.ShoppingSessionId);
						AddEventAuthorization(rentalShoppingCart.ShoppingSessionId, GetAuthorizeTypeText(authorizeResult.AuthorizeType), authorizeResult.IsOnline, flag2, null);
						AnalyticsAddAuthTransactionInfo(authorizeResult);
						AnalyticsAddAuthAccountInfo(authorizeResult);
						ProcessAuthorizeResult(authorizeResult);
					}
					else
					{
						LogHelper.Instance.Log($"OnAuthorizeComplete - authorize result not processed for session {authorizeResult.SessionId}.  Kiosk is in maintenanance mode");
					}
				}
				else
				{
					LogHelper.Instance.Log($"OnAuthorizeComplete - authorize result not processed.  Session {authorizeResult.SessionId} is no longer current.");
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in OnAuthorizeComplete - authorize result is missing.");
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in OnAuthorizeComplete - result is null");
		}
	}

	private static void ProcessAuthorizeResult(IAuthorizeResult authorizeResult)
	{
		LuaTable authorizeResultLuaTable = ConvertAuthorizeResultToLuaTable(authorizeResult);
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		Task.Factory.StartNew(delegate
		{
			OrchestrationService.InvokeProcessAuthorizeResultCallback(authorizeResultLuaTable);
		}, CancellationToken.None, TaskCreationOptions.None, service?.UITaskScheduler);
	}

	private static LuaTable ConvertAuthorizeResultToLuaTable(IAuthorizeResult authorizeResult)
	{
		return new Dictionary<string, object>
		{
			["success"] = authorizeResult.Success,
			["properties"] = new Dictionary<string, object>
			{
				["is_online"] = authorizeResult.IsOnline,
				["stand_alone"] = authorizeResult.StandAlone,
				["server_name"] = authorizeResult.ServerName,
				["response"] = authorizeResult.Response,
				["email"] = authorizeResult.Email,
				["reference_number"] = authorizeResult.ReferenceNumber,
				["has_profile"] = authorizeResult.HasProfile,
				["is_rbi"] = authorizeResult.IsRBI,
				["first_name"] = authorizeResult.FirstName,
				["last_name"] = authorizeResult.LastName,
				["confirmation_status"] = authorizeResult.ConfirmationStatus,
				["account_number"] = authorizeResult.AccountNumber,
				["customer_profile_number"] = authorizeResult.CustomerProfileNumber,
				["store_number"] = authorizeResult.StoreNumber,
				["gift_card_balance"] = authorizeResult.GiftCardBalance,
				["gift_card_max_discs"] = authorizeResult.GiftCardMaxDiscs
			}.ToLuaTable()
		}.ToLuaTable();
	}

	private static void AddEventAuthorization(Guid sessionId, string type, bool online, bool? approved, bool? pickupValid)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddData(new AuthorizationData
		{
			AuthorizationType = type,
			Online = online,
			Approved = approved,
			PickupValid = pickupValid
		});
	}

	private static void AnalyticsAddAuthTransactionInfo(IAuthorizeResult authorizeResult)
	{
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service != null && authorizeResult != null && service.SessionExists(authorizeResult.SessionId.ToString()))
		{
			service.AddAuthTransactionInfo(authorizeResult.CustomerProfileNumber, authorizeResult.AccountNumber, authorizeResult.ReferenceNumber, authorizeResult.SessionId.ToString());
		}
	}

	private static void AnalyticsAddAuthAccountInfo(IAuthorizeResult authorizeResult)
	{
		ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddAuthTransactionInfo(authorizeResult.CustomerProfileNumber, authorizeResult.AccountNumber, authorizeResult.ReferenceNumber, authorizeResult.SessionId.ToString());
	}
}
