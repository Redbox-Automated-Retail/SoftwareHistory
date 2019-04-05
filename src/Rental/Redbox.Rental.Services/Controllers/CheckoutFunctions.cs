using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.UI.Properties;

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
	}

	public static bool IsCurrentCardOnOfflineBadCardList()
	{
		bool result = false;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service != null && service2 != null)
		{
			string hashId = service2.GenerateCardHashId();
			result = service.IsAccountOnOfflineBadCardList(hashId);
		}
		return result;
	}

	public static void UpdateAccountOnOfflineBadCardList()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service != null && service2 != null)
		{
			string hashId = service2.GenerateCardHashId();
			service?.UpdateAccountOnOfflineBadCardList(hashId);
		}
	}

	public static void RemoveAccountFromOfflineBadCardList()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service != null && service2 != null)
		{
			string hashId = service2.GenerateCardHashId();
			service?.RemoveAccountFromOfflineBadCardList(hashId);
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
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		int num = rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental && x.TitleFamily == TitleFamily.Games);
		int num2 = rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.purchase && x.TitleFamily == TitleFamily.Games);
		rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.rental && x.TitleFamily == TitleFamily.Movies);
		rentalShoppingCart.Items.Count((IRentalShoppingCartItem x) => x.Action == RentalShoppingCartItemAction.purchase && x.TitleFamily == TitleFamily.Movies);
		int count = rentalShoppingCart.Count;
		IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(rentalShoppingCart?.ShoppingSessionId.ToString());
		bool flag = OrchestrationService.IsCardBlocklisted();
		session?.ShoppingCart?.SetBlocklisted(flag);
		if (flag)
		{
			response = "OfflineCardBlocklisted";
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
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				IKioskProduct kioskProduct = null;
				service?.GetProduct(item.ProductId, excludeTitleRollup: true, out kioskProduct);
				if ((kioskProduct as ITitleProduct).SellThruNew)
				{
					response = "OfflineSellThruNewRefused";
					break;
				}
			}
		}
		return string.IsNullOrEmpty(response);
	}
}
