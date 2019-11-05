using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public class AnalyticsShoppingCartItem
{
	public long ProductId { get; set; }

	public string TivoQueryId { get; set; }

	public string OfferCode { get; set; }

	public List<string> Tags { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public RentalShoppingCartItemAction Action { get; set; }

	public decimal Price { get; set; }

	public decimal Quantity { get; set; }

	public decimal TaxRate { get; set; }

	public bool Removed { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public LineItemStatus Status { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public TitleFamily ProductFamily { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public TitleType ProductType { get; set; }

	public string Barcode { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public ItemSourceType SourceType { get; set; }

	public string AddSequence { get; set; }

	public long? ProductPricingId { get; set; }

	public string ExtraNight { get; set; }

	public int? InitialDays { get; set; }

	public decimal? DefaultInitialNightPrice { get; set; }

	public decimal? DefaultExtraNightPrice { get; set; }

	public static AnalyticsShoppingCartItem ToShoppingCartItem(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		return new AnalyticsShoppingCartItem
		{
			ProductId = rentalShoppingCartItem.ProductId,
			Action = rentalShoppingCartItem.Action,
			Price = rentalShoppingCartItem.Price,
			Quantity = rentalShoppingCartItem.Quantity,
			TaxRate = rentalShoppingCartItem.TaxRate,
			Status = rentalShoppingCartItem.Status,
			ProductFamily = rentalShoppingCartItem.TitleFamily,
			ProductType = rentalShoppingCartItem.TitleType,
			Barcode = rentalShoppingCartItem.Barcode,
			SourceType = rentalShoppingCartItem.SourceType,
			ExtraNight = ((rentalShoppingCartItem.Action != RentalShoppingCartItemAction.purchase) ? rentalShoppingCartItem.PricingRecord.ExtraNight.ToString() : null),
			InitialDays = ((rentalShoppingCartItem.Action != RentalShoppingCartItemAction.purchase) ? rentalShoppingCartItem.PricingRecord.InitialDays : ((int?)null)),
			DefaultInitialNightPrice = ((rentalShoppingCartItem.Action != RentalShoppingCartItemAction.purchase) ? rentalShoppingCartItem.PricingRecord.DefaultInitialNightPrice : ((decimal?)null)),
			DefaultExtraNightPrice = ((rentalShoppingCartItem.Action != RentalShoppingCartItemAction.purchase) ? rentalShoppingCartItem.PricingRecord.DefaultExtraNightPrice : ((decimal?)null)),
			TivoQueryId = rentalShoppingCartItem.TivoQueryId,
			OfferCode = rentalShoppingCartItem.OfferCode,
			Tags = rentalShoppingCartItem.Tags
		};
	}
}
