using System;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.ViewDefinitions;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartViewFunctions
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	public static void ViewShoppingCart()
	{
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.IsShoppingCartFullMessagesOpen = false;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.ShowKMTInCartOffers && (PromotionService.Instance.NeedToShowFreeInCartMessage || PromotionService.Instance.NeedToShowRentTwoInCartMessage))
		{
			BrowseState.ItemAddedFromView = null;
			ShowInCartOfferMessage();
		}
		else if (rentalShoppingCart != null && rentalShoppingCart.UpsellInfo.GetDialogNeedsToBeShown(TitleType.DVD))
		{
			OrchestrationService.ViewShow("upsell_view", new ShowUpsellViewParameter
			{
				TitleType = TitleType.DVD
			});
		}
		else if (rentalShoppingCart != null && rentalShoppingCart.UpsellInfo.GetDialogNeedsToBeShown(TitleType.Bluray))
		{
			OrchestrationService.ViewShow("upsell_view", new ShowUpsellViewParameter
			{
				TitleType = TitleType.Bluray
			});
		}
		else
		{
			OrchestrationService.ViewShow("shopping_cart_view");
		}
	}

	public static void ShowInCartOfferMessage()
	{
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			rentalShoppingCart.InCartPromoOfferMade = true;
		}
		MessagePopupViewController.ShowInCartOfferMessage(delegate
		{
			if (PromotionService.Instance.IsFreeInCartOfferRunning)
			{
				PromotionService.Instance.SetFreeInCartPromo(rentalShoppingCart);
				rentalShoppingCart?.ValidateAndUpdatePromoCode();
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: false);
			}
			else if (PromotionService.Instance.IsRentTwoInCartOfferRunning && (PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0) == 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: false);
			}
			else if (PromotionService.Instance.IsRentTwoInCartOfferRunning && (PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0) > 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = false;
				PromotionService.Instance.SetRentTwoInCartPromo(rentalShoppingCart);
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: false);
			}
			else
			{
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: true);
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				CheckoutFunctions.BeginCheckout();
			}
		}, delegate
		{
			AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartPromoDecline");
			if (PromotionService.Instance.IsRentTwoInCartOfferRunning && (PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0) == 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				GetViewService()?.GoBackSafe("in_cart_offer_message_view:no_button", popOnly: false);
			}
			else
			{
				GetViewService()?.GoBackSafe("in_cart_offer_message_view:no_button.", popOnly: true);
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				CheckoutFunctions.BeginCheckout();
			}
		}, delegate
		{
			AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartPromoAccept");
			if (PromotionService.Instance.IsFreeInCartOfferRunning)
			{
				PromotionService.Instance.SetFreeInCartPromo(rentalShoppingCart);
				rentalShoppingCart?.ValidateAndUpdatePromoCode();
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage yes button.", popOnly: false);
			}
			else if (PromotionService.Instance.IsRentTwoInCartOfferRunning && (PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() ?? 0) > 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = false;
				PromotionService.Instance.SetRentTwoInCartPromo(rentalShoppingCart);
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage yes button.", popOnly: false);
			}
			else
			{
				IPromotionRule inCartPromoActionRule = PromotionService.Instance.GetInCartPromoActionRule();
				if (inCartPromoActionRule != null && (inCartPromoActionRule.OfferAction == PromotionRule.Constants.OfferActionBrowseFilter || inCartPromoActionRule.OfferAction == PromotionRule.Constants.OfferActionTitleMarketing))
				{
					BrowseState.TitleFamily = inCartPromoActionRule.TitleFamily;
					BrowseState.Genre = null;
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
					BrowseState.PageNumber = 1;
					BrowseState.TitleType = inCartPromoActionRule.TitleType;
					BrowseState.BrowseMode = BrowseViewFilter.All;
					BrowseState.BrowsePriceRangeFilter = null;
					BrowseState.BrowseStudio = null;
					if (inCartPromoActionRule.OfferAction == PromotionRule.Constants.OfferActionTitleMarketing)
					{
						rentalShoppingCart.InCartTitleMarketing = true;
					}
				}
				PromotionService.Instance.SetPromoToFixedPriceInCart(rentalShoppingCart, 0m);
				GetViewService()?.UnwindTo("browse_view", discard: true, "in-cart-offer-message-view:yes_button");
			}
		});
	}

	public static void RemoveItemFromShoppingCart(DisplayProductModel displayProductModel, object parameter)
	{
		IRentalShoppingCartItem rentalShoppingCartItem = displayProductModel?.Data as IRentalShoppingCartItem;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null || rentalShoppingCartItem == null)
		{
			return;
		}
		IRentalShoppingCartItem deleteRentalShoppingCartItem = rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == rentalShoppingCartItem.ProductId);
		bool flag = true;
		if (rentalShoppingCart.InCartTitleMarketing)
		{
			LogHelper.Instance.Log("Creating temp RentalShoppingCart to retest in cart offer conditions");
			IRentalShoppingCart rentalShoppingCart2 = CloneRentalShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
			bool isCartValid = PromotionService.Instance.ValidateRegularInCartPromo(rentalShoppingCart2).IsCartValid;
			if (rentalShoppingCart2 != null)
			{
				RentalShoppingCartService.RemoveRentalShoppingCart(rentalShoppingCart2.ShoppingSessionId);
			}
			if (!isCartValid)
			{
				flag = false;
				MessagePopupViewController.ShowInCartCancelWarning(delegate
				{
					LogHelper.Instance.Log("Title marketing remove item warning popup timed out");
					RemoveItemFromShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
					ClearInCart(rentalShoppingCart);
					GetViewService()?.UnwindToSafe("shopping_cart_view", discard: true, "ShowInCartCancelWarning Timed out.");
				}, delegate
				{
					LogHelper.Instance.Log("User cancelled remove item with back");
					AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartRemoveItemCancel");
					GetViewService()?.UnwindToSafe("shopping_cart_view", discard: true, "ShowInCartCancelWarning go back.");
				}, delegate
				{
					LogHelper.Instance.Log("User confirmed remove item warning popup with continue");
					AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartRemoveItemConfirm");
					RemoveItemFromShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
					ClearInCart(rentalShoppingCart);
					GetViewService()?.UnwindToSafe("shopping_cart_view", discard: true, "ShowInCartCancelWarning continue.");
				});
			}
		}
		if (flag && deleteRentalShoppingCartItem != null)
		{
			RemoveItemFromShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
			GetViewService()?.Show();
		}
	}

	private static IRentalShoppingCart CloneRentalShoppingCart(IRentalShoppingCart rentalShoppingCartToClone, IRentalShoppingCartItem excludeRentalShoppingCartItem)
	{
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService.CreateNew(Guid.NewGuid());
		foreach (IRentalShoppingCartItem item in rentalShoppingCartToClone.Items.Where((IRentalShoppingCartItem x) => excludeRentalShoppingCartItem == null || x.ProductId != (excludeRentalShoppingCartItem?.ProductId ?? 0)))
		{
			IRentalShoppingCartItem rentalShoppingCartItem = RentalShoppingCartService.CreateRentalShoppingCartItemFromProductId(item.ProductId);
			if (rentalShoppingCartItem != null)
			{
				rentalShoppingCartItem.SourceType = item.SourceType;
				rentalShoppingCartItem.Action = item.Action;
				if (rentalShoppingCartItem.SourceType == ItemSourceType.Reservation)
				{
					rentalShoppingCartItem.Barcode = item.Barcode;
				}
				rentalShoppingCartItem.Quantity = 1m;
				rentalShoppingCartItem.Status = LineItemStatus.Pending;
				rentalShoppingCart.AddItem(rentalShoppingCartItem);
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.AddItem. Unable to find product with id {0}", item.ProductId);
			}
		}
		return rentalShoppingCart;
	}

	private static void ClearInCart(IRentalShoppingCart rentalShoppingCart)
	{
		rentalShoppingCart.ClearPromoCode();
		rentalShoppingCart.InCartPromoOfferMade = true;
		rentalShoppingCart.InCartPromoOfferCancel = true;
		rentalShoppingCart.InCartTitleMarketing = false;
	}

	private static void RemoveItemFromShoppingCart(IRentalShoppingCart rentalShoppingCart, IRentalShoppingCartItem deleteRentalShoppingCartItem)
	{
		AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsRemoveItem")?.AddData(ProductData.ToProductData(deleteRentalShoppingCartItem));
		rentalShoppingCart.RemoveItem(deleteRentalShoppingCartItem, userRemoved: true);
	}

	private static IViewService GetViewService()
	{
		return ServiceLocator.Instance.GetService<IViewService>();
	}
}
