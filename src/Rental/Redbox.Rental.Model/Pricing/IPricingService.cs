using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Pricing;

public interface IPricingService
{
	IPricingSet CurrentPriceSet { get; }

	bool IsPricingEngineEnabled { get; }

	event PricingSetChange OnPricingSetChange;

	void Start();

	void Stop();

	void Reload();

	void UpdatePricingInstance();

	decimal GetAdjustedPrice(decimal price, RentalShoppingCartItemAction action, TitleType titleType);

	decimal? GetPriceSetting(TitleFamily titleFamily, TitleType titleType, string settingName, RentalShoppingCartItemAction action);

	IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool useAdjustedPrices);

	IPricingRecord GetOneDayProductPricingRecord(ITitleProduct titleProduct);

	IPricingRecord GetOneDayProductPricingRecord(long productId);

	IList<IPricingRecord> GetProductPricingRecords(ITitleProduct titleProduct);

	IList<IPricingRecord> GetProductPricingRecords(long productId);

	IPricingRecord GetPurchasePricing(ITitleProduct titleProduct);

	decimal GetRentalTaxRate();

	decimal GetPurchaseTaxRate();

	decimal GetDigitalPurchaseTaxRate();

	decimal GetEmptyCasePrice();

	void ClearOnPricingSetChange();
}
