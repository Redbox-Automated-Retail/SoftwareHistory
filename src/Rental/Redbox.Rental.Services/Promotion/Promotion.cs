using System.Collections.Generic;
using Redbox.Rental.Model;
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

	public IList<IRentalShoppingCartItem> QualifyingItems { get; set; }

	public IList<IPromotionItem> PromotionItems { get; set; }

	public Promotion()
	{
		QualifyingItems = new List<IRentalShoppingCartItem>();
		PromotionItems = new List<IPromotionItem>();
	}
}
