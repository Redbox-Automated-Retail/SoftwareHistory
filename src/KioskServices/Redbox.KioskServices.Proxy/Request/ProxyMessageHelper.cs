using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.Requests;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskServices.Proxy.Request;

internal class ProxyMessageHelper
{
	public static CartMessage CreateCartMessage(string customerProfileNumber, string messageType)
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service?.CurrentRentalShoppingCart != null)
		{
			return CreateCartMessage(service.CurrentRentalShoppingCart.ShoppingSessionId, customerProfileNumber, messageType);
		}
		return null;
	}

	public static CartMessage CreateCartMessage(Guid sessionId, string customerProfileNumber, string messageType)
	{
		int result;
		return new CartMessage
		{
			MessageType = messageType,
			KioskId = (int.TryParse(StoreServicesProxy.Instance.StoreNumber, out result) ? result : 0),
			CustomerProfileNumber = customerProfileNumber,
			ShoppingCart = CreateShoppingCart(sessionId)
		};
	}

	public static ShoppingCart CreateShoppingCart(string sessionId)
	{
		return CreateShoppingCart(new Guid(sessionId));
	}

	public static ShoppingCart CreateShoppingCart(Guid id)
	{
		ShoppingCart shoppingCart = new ShoppingCart();
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id);
		if (rentalShoppingCart?.Items?.Any() ?? false)
		{
			(rentalShoppingCart?.Items?.Where((IRentalShoppingCartItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false))?.ForEach(delegate(IRentalShoppingCartItem x)
			{
				shoppingCart.Discounts.Add(new Discount
				{
					ProductId = x.ProductId,
					DiscountType = DiscountType.Loyalty,
					RedemptionPoints = x.LoyaltyPointsRecord.PointsToRedeem,
					Amount = (x.PricingRecord?.InitialNight ?? 0m)
				});
			});
			if (rentalShoppingCart.Promotion != null)
			{
				IPromotion promo = rentalShoppingCart.Promotion;
				promo?.PromotionItems?.ForEach(delegate(IPromotionItem x)
				{
					shoppingCart.Discounts.Add(new Discount
					{
						Amount = x.PromotionAmount,
						DiscountType = DiscountType.PromotionCode,
						PromotionCode = promo.Name,
						PromotionCodeValue = promo.OriginalPrice,
						PromotionIntentCode = promo.Intent,
						ProductId = x.RentalShoppingCartItem.ProductId
					});
				});
			}
			foreach (IGrouping<RentalShoppingCartItemAction, IRentalShoppingCartItem> item in from x in rentalShoppingCart.Items
				group x by x.Action)
			{
				LineItemGroupType groupType = ((item.Key == RentalShoppingCartItemAction.rental) ? LineItemGroupType.Rental : LineItemGroupType.Purchase);
				List<LineItem> list = new List<LineItem>();
				foreach (IRentalShoppingCartItem item2 in item)
				{
					IUpsellItem upsellItemByRentalShoppingCartItem = rentalShoppingCart.UpsellInfo.GetUpsellItemByRentalShoppingCartItem(item2);
					BlurayUpsell blurayUpsell = null;
					if (upsellItemByRentalShoppingCartItem != null)
					{
						blurayUpsell = new BlurayUpsell
						{
							ProductGroupId = (int)upsellItemByRentalShoppingCartItem.ProductGroupId,
							TitleId = item2.ProductId,
							Offer = upsellItemByRentalShoppingCartItem.OfferResponse.ToString(),
							TypeId = (int)upsellItemByRentalShoppingCartItem.TypeId
						};
					}
					IPricingRecord pricingRecord = item2?.PricingRecord;
					list.Add(new LineItem
					{
						Barcode = item2.Barcode,
						BlurayUpsell = blurayUpsell,
						SourceType = item2.SourceType,
						MultiDisc = item2.MultiDisc,
						ProductFamily = item2.TitleFamily.ToString(),
						ProductId = item2.ProductId,
						ProductType = item2.TitleType.GetDisplayName(),
						Pricing = new Pricing
						{
							DefaultExtraNight = pricingRecord?.DefaultExtraNightPrice,
							DefaultInitialNight = pricingRecord?.DefaultInitialNightPrice,
							Expiration = (pricingRecord?.ExpirationPrice ?? 0m),
							ExtraNight = (pricingRecord?.ExtraNight ?? 0m),
							InitialDays = pricingRecord?.InitialDays,
							InitialNight = (pricingRecord?.InitialNight ?? 0m),
							NonReturn = (pricingRecord?.NonReturn ?? 0m),
							NonReturnDays = (int)(pricingRecord?.NonReturnDays ?? 0m),
							PriceSetId = (pricingRecord?.PriceSetId ?? 0),
							ProductPriceId = (int)(pricingRecord?.ProductPricingId ?? 0),
							Purchase = (pricingRecord?.Purchase ?? 0m)
						},
						VendStatus = ConvertToVendStatus(item2.Status)
					});
				}
				ISubTotalGroup subTotalGroup = rentalShoppingCart.Totals.SubTotalGroups.FirstOrDefault((ISubTotalGroup y) => item.Key == y.Action);
				shoppingCart.Groups.Add(new LineItemGroup
				{
					GroupType = groupType,
					Items = list,
					Totals = new Totals
					{
						DiscountedSubtotal = subTotalGroup.SubTotal,
						Subtotal = subTotalGroup.Amount,
						GrandTotal = subTotalGroup.GrantTotal,
						TaxAmount = subTotalGroup.TaxAmount,
						TaxRate = subTotalGroup.TaxRate * 100m
					}
				});
			}
		}
		if (rentalShoppingCart.ServiceFee != null && shoppingCart.ServiceFee != null)
		{
			shoppingCart.ServiceFee.DefaultAmount = rentalShoppingCart.ServiceFee.DefaultAmount;
			shoppingCart.ServiceFee.ActualAmount = rentalShoppingCart.ServiceFee.ActualAmount;
			shoppingCart.ServiceFee.TaxRate = rentalShoppingCart.ServiceFee.TaxRate;
			shoppingCart.ServiceFee.TaxAmount = rentalShoppingCart.ServiceFee.TaxAmount;
		}
		return shoppingCart;
	}

	private static VendStatus ConvertToVendStatus(LineItemStatus status)
	{
		switch (status)
		{
		case LineItemStatus.Pending:
		case LineItemStatus.InventoryError:
			return VendStatus.EmptyOrStuck;
		case LineItemStatus.CustomerDisabled:
			return VendStatus.CustomerDisabled;
		case LineItemStatus.ItemNotTaken:
			return VendStatus.DiskNotTaken;
		case LineItemStatus.HardwareError:
			return VendStatus.HardwareError;
		case LineItemStatus.InventoryCapacity:
			return VendStatus.KioskFull;
		case LineItemStatus.Fullfilled:
			return VendStatus.Vended;
		default:
			return VendStatus.NotVended;
		}
	}
}
