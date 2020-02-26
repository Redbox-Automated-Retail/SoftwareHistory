using System.Collections.Generic;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Promotion;

public class Promotion : IPromotion
{
	public string Name { get; set; }

	public decimal OriginalPrice { get; set; }

	public decimal Price { get; set; }

	public int Quantity { get; set; }

	public PromotionIntentCode Intent { get; set; }

	public decimal OriginalPercent { get; set; }

	public decimal Percent { get; set; }

	public byte RentQuantity { get; set; }

	public PromotionRentFormat? RentFormat { get; set; }

	public byte GetQuantity { get; set; }

	public PromotionGetFormat? GetFormat { get; set; }

	public ICampaignTitles CampaignTitles { get; set; }

	public int? ProductTypeId { get; set; }

	public List<int> FormatIds { get; set; }

	public IList<IRentalShoppingCartItem> QualifyingItems { get; set; }

	public IList<IPromotionItem> PromotionItems { get; set; }

	public bool IsProductSpecific
	{
		get
		{
			if (Intent != PromotionIntentCode.FirstNightFreeItem)
			{
				if (Intent == PromotionIntentCode.FixedValue)
				{
					ICampaignTitles campaignTitles = CampaignTitles;
					if (campaignTitles != null && campaignTitles.Titles?.Count > 0)
					{
						goto IL_0061;
					}
				}
				return Intent == PromotionIntentCode.MultiNightItem;
			}
			goto IL_0061;
			IL_0061:
			return true;
		}
	}

	public bool IsValidatedPostVend { get; set; }

	public DiscountType DiscountType { get; set; }

	public bool IsExternallyValidated => Intent == PromotionIntentCode.MultiNightItem;

	public PromotionActionCode ActionCode { get; set; }

	public RentalShoppingCartItemAction CartItemAction
	{
		get
		{
			if (ActionCode != PromotionActionCode.Purchase)
			{
				return RentalShoppingCartItemAction.rental;
			}
			return RentalShoppingCartItemAction.purchase;
		}
	}

	public Promotion()
	{
		QualifyingItems = new List<IRentalShoppingCartItem>();
		PromotionItems = new List<IPromotionItem>();
		IsValidatedPostVend = false;
		DiscountType = DiscountType.PromotionCode;
	}
}
