using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public class ProductData : AnalyticsData
{
	public string ViewName { get; set; }

	public long? ProductId { get; set; }

	public long? TitleRollupProductGroupId { get; set; }

	public int? ProductTypeId { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public TitleFamily? TitleFamily { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public TitleType? TitleType { get; set; }

	public ProductData()
	{
		base.DataType = "Product";
	}

	public static ProductData ToProductData(ITitleProduct titleProduct)
	{
		if (titleProduct == null)
		{
			return null;
		}
		return new ProductData
		{
			ProductId = titleProduct.ProductId,
			TitleRollupProductGroupId = titleProduct.TitleRollupProductGroupId,
			ProductTypeId = titleProduct.ProductTypeId,
			TitleFamily = titleProduct.TitleFamily,
			TitleType = titleProduct.TitleType
		};
	}

	public static ProductData ToProductData(IRentalShoppingCartItem rentalShoppingCartItem)
	{
		if (rentalShoppingCartItem == null)
		{
			return null;
		}
		return new ProductData
		{
			ProductId = rentalShoppingCartItem.ProductId,
			TitleFamily = rentalShoppingCartItem.TitleFamily,
			TitleType = rentalShoppingCartItem.TitleType
		};
	}
}
