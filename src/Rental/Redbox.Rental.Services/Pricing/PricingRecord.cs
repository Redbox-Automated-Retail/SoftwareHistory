using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Services.Pricing;

public class PricingRecord : IPricingRecord
{
	public class Constants
	{
		public const string PriceSetId = "price_set_id";

		public const string Family = "family";

		public const string Type = "type";

		public const string InitialNight = "initial_night";

		public const string ExtraNight = "extra_night";

		public const string ExpirationlPrice = "expiration_price";

		public const string NonReturn = "non_return";

		public const string NonReturnDays = "non_return_days";

		public const string PromoValue = "promo_value";

		public const string Purchase = "purchase";
	}

	public TitleFamily TitleFamily { get; set; }

	public TitleType TitleType { get; set; }

	public decimal InitialNight { get; set; }

	public decimal ExtraNight { get; set; }

	public decimal ExpirationPrice { get; set; }

	public decimal NonReturn { get; set; }

	public decimal NonReturnDays { get; set; }

	public decimal PromoValue { get; set; }

	public decimal Purchase { get; set; }

	public bool IsEqual(IPricingRecord pricingRecord)
	{
		return CompareTo(pricingRecord) == 0;
	}

	public int CompareTo(IPricingRecord pricingRecord)
	{
		int num = TitleFamily.CompareTo(pricingRecord.TitleFamily);
		if (num == 0)
		{
			num = TitleType.CompareTo(pricingRecord.TitleType);
		}
		if (num == 0)
		{
			num = InitialNight.CompareTo(pricingRecord.InitialNight);
		}
		if (num == 0)
		{
			num = ExtraNight.CompareTo(pricingRecord.ExtraNight);
		}
		if (num == 0)
		{
			num = ExpirationPrice.CompareTo(pricingRecord.ExpirationPrice);
		}
		if (num == 0)
		{
			num = NonReturn.CompareTo(pricingRecord.NonReturn);
		}
		if (num == 0)
		{
			num = NonReturnDays.CompareTo(pricingRecord.NonReturnDays);
		}
		if (num == 0)
		{
			num = PromoValue.CompareTo(pricingRecord.PromoValue);
		}
		if (num == 0)
		{
			num = Purchase.CompareTo(pricingRecord.Purchase);
		}
		return num;
	}

	public IPricingRecord Clone()
	{
		return new PricingRecord
		{
			TitleFamily = TitleFamily,
			TitleType = TitleType,
			InitialNight = InitialNight,
			ExtraNight = ExtraNight,
			ExpirationPrice = ExpirationPrice,
			NonReturn = NonReturn,
			NonReturnDays = NonReturnDays,
			PromoValue = PromoValue,
			Purchase = Purchase
		};
	}

	public Dictionary<string, object> ToDictionary()
	{
		return new Dictionary<string, object>
		{
			{
				"family",
				TitleFamily.ToString().ToLower()
			},
			{
				"type",
				TitleType.ToString().ToLower().Replace("bluray", "blu-ray")
					.Replace("all", "*")
			},
			{ "initial_night", InitialNight },
			{ "extra_night", ExtraNight },
			{ "expiration_price", ExpirationPrice },
			{ "non_return", NonReturn },
			{ "non_return_days", NonReturnDays },
			{ "promo_value", PromoValue },
			{ "purchase", Purchase },
			{
				"price_set_id",
				ServiceLocator.Instance.GetService<IPricingService>()?.CurrentPriceSet?.PriceSetId ?? "1"
			}
		};
	}
}
