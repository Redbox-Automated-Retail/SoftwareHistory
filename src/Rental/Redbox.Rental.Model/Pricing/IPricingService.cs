using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Pricing;

public interface IPricingService
{
	IPricingSet CurrentPriceSet { get; }

	event PricingSetChange OnPricingSetChange;

	void Reload();

	decimal GetAdjustedPrice(decimal price, RentalShoppingCartItemAction action, TitleType titleType);

	decimal? GetPriceSetting(TitleFamily titleFamily, TitleType titleType, string settingName, RentalShoppingCartItemAction action);

	IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool forRecommendationOnPickupFlow);

	IPricingRecord GetProductPricingRecord(long productId);

	decimal GetRentalTaxRate();

	decimal GetPurchaseTaxRate();

	decimal GetDigitalPurchaseTaxRate();

	decimal GetEmptyCasePrice();

	void ClearOnPricingSetChange();
}
