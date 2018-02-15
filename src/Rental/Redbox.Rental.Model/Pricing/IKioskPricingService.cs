using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Pricing;

public interface IKioskPricingService
{
	IPricingSet CurrentPriceSet { get; }

	bool IsPricingEngineDataAvailable { get; }

	void UpdatePricingInstance(bool force = false);

	IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool useAdjustedPrices);

	IPricingRecord GetOneDayProductPricingRecord(long productId);

	List<IPricingRecord> GetProductPricingRecords(long productId);

	IPricingRecord GetPurchasePricing(ITitleProduct titleProduct);
}
