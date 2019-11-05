using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Promotion;

public class CustomerOffer
{
	public long OfferId { get; set; }

	public decimal Amount { get; set; }

	public OfferDiscountType DiscountType { get; set; }

	public string PromotionCode { get; set; }

	public DateTime? ExpirationDate { get; set; }

	public bool IncludeTitles { get; set; }

	public List<long> TitleIds { get; set; }

	public bool IncludeFormats { get; set; }

	public List<int> FormatIds { get; set; }

	public TitleFamily? TitleFamily { get; set; }

	public List<ITitleProduct> ApplicableProducts { get; set; }

	public TimeSpan? IncludeOlderThan { get; set; }

	public OfferScopeType OfferScopeType { get; set; }

	public CustomerSegmentType CustomerSegmentType { get; set; }

	public long? StoredOfferId { get; set; }

	public List<OfferProduct> OfferProducts { get; set; }

	public string OfferDiscountCode { get; set; }
}
