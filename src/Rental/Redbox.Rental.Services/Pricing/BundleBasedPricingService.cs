using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Pricing;

public class BundleBasedPricingService : IBundleBasedPricingService
{
	private Store _store;

	private string _pricingConfigName;

	private IPricingSet _currentPricingSet;

	public IPricingSet CurrentPriceSet => _currentPricingSet;

	private Store Store
	{
		get
		{
			if (_store == null)
			{
				_store = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
			}
			return _store;
		}
	}

	public bool Reload()
	{
		GetPricingConfigName();
		LogHelper.Instance.Log(">  Pricing Service Load...");
		IPricingSet pricingSet = GetPricingSet(_pricingConfigName);
		bool result = false;
		if (_currentPricingSet != null)
		{
			LogHelper.Instance.Log(">  Checking for price changes...");
			result = !pricingSet.IsEqual(_currentPricingSet);
		}
		_currentPricingSet = pricingSet;
		return result;
	}

	public IPricingRecord GetProductPricingRecord(long productId)
	{
		IPricingRecord result = null;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (productId != 0L)
		{
			service.GetProduct(productId, excludeTitleRollup: true, out var kioskProduct);
			if (kioskProduct != null)
			{
				if (kioskProduct is ITitleProduct titleProduct)
				{
					result = GetProductTypePricingRecord(titleProduct.TitleFamily, titleProduct.TitleType, useAdjustedPrices: true);
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in BundleBasedPricingService.GetProductPricingRecord.  Unable to find product with id {0}", productId);
			}
		}
		return result;
	}

	public IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool useAdjustedPrices)
	{
		IPricingRecord pricingRecord = null;
		IPricingRecord pricingRecord2 = CurrentPriceSet?.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == titleFamily && (x.TitleType == titleType || x.TitleType == TitleType.All));
		if (pricingRecord2 != null)
		{
			pricingRecord = pricingRecord2.Clone();
			if (useAdjustedPrices)
			{
				pricingRecord.InitialNight = GetAdjustedPrice(pricingRecord.InitialNight, RentalShoppingCartItemAction.rental, titleType);
				pricingRecord.ExtraNight = GetAdjustedPrice(pricingRecord.ExtraNight, RentalShoppingCartItemAction.rental, titleType);
				pricingRecord.ExpirationPrice = GetAdjustedPrice(pricingRecord.ExpirationPrice, RentalShoppingCartItemAction.rental, titleType);
			}
		}
		return pricingRecord;
	}

	public decimal GetAdjustedPrice(decimal price, RentalShoppingCartItemAction action, TitleType titleType)
	{
		decimal result = price;
		if (Store?.State == "MD")
		{
			decimal num = 0m;
			switch (action)
			{
			case RentalShoppingCartItemAction.rental:
				num = Store.RentalTaxRate;
				break;
			case RentalShoppingCartItemAction.purchase:
				num = ((titleType == TitleType.DigitalCode) ? Store.DigitalPurchaseTaxRate : Store.PurchaseTaxRate);
				break;
			}
			if (num != 0m)
			{
				result = Math.Round(price * (num / 100m + 1m), 2);
			}
		}
		return result;
	}

	private IPricingSet GetPricingSet(string pricingConfigName)
	{
		IPricingSet pricingSet = null;
		List<Dictionary<string, object>> resourceTable = OrchestrationService.GetResourceTable(pricingConfigName);
		if (resourceTable != null)
		{
			pricingSet = new PricingSet();
			foreach (Dictionary<string, object> item in resourceTable)
			{
				IPricingRecord pricingRecord = new PricingRecord();
				string stringValue = GetStringValue(item, "family");
				if (Enum.TryParse<TitleFamily>(stringValue, ignoreCase: true, out var result))
				{
					pricingRecord.TitleFamily = result;
					string text = GetStringValue(item, "type");
					if (text == "*")
					{
						text = "All";
					}
					text = text?.Replace("-", string.Empty);
					if (Enum.TryParse<TitleType>(text, ignoreCase: true, out var result2))
					{
						pricingRecord.TitleType = result2;
						pricingRecord.InitialNight = GetDecimalValue(item, "initial_night");
						pricingRecord.ExtraNight = GetDecimalValue(item, "extra_night");
						pricingRecord.ExpirationPrice = GetDecimalValue(item, "expiration_price");
						pricingRecord.NonReturnDays = GetDecimalValue(item, "non_return_days");
						pricingRecord.PromoValue = GetDecimalValue(item, "promo_value");
						if (Store?.State == "MD")
						{
							pricingRecord.NonReturn = Math.Round(pricingRecord.InitialNight * (Store.RentalTaxRate / 100m + 1m), 2) * pricingRecord.NonReturnDays;
						}
						else
						{
							pricingRecord.NonReturn = pricingRecord.InitialNight * pricingRecord.NonReturnDays;
						}
					}
					else
					{
						LogHelper.Instance.Log("Error in BundleBasedPricingService.GetPricingSet.  Unable to parse TitleType {0} in pricingConfigName {1}", text, pricingConfigName);
					}
				}
				else
				{
					LogHelper.Instance.Log("Error in BundleBasedPricingService.GetPricingSet.  Unable to parse TitleFamily {0} in pricingConfigName {1}", stringValue, pricingConfigName);
				}
				pricingSet.PriceRecords.Add(pricingRecord);
			}
			object priceSetId = OrchestrationService.GetResourceProperty(pricingConfigName, "price_set_id");
			pricingSet.PriceRecords.ForEach(delegate(IPricingRecord x)
			{
				int result3 = 0;
				x.PriceSetId = (int.TryParse(priceSetId?.ToString() ?? string.Empty, out result3) ? new int?(result3) : ((int?)null));
			});
			pricingSet.PriceSetId = priceSetId?.ToString();
		}
		return pricingSet;
	}

	private string GetStringValue(Dictionary<string, object> values, string key)
	{
		string result = null;
		if (values.TryGetValue(key, out var value))
		{
			result = value?.ToString() ?? null;
		}
		return result;
	}

	private decimal GetDecimalValue(Dictionary<string, object> values, string key)
	{
		decimal result = 0m;
		if (values.TryGetValue(key, out var value) && value != null)
		{
			decimal.TryParse(value.ToString(), out result);
		}
		return result;
	}

	private void GetPricingConfigName()
	{
		LogHelper.Instance.Log(">  Looking for pricing configuration...");
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		_pricingConfigName = null;
		service?.TryGetValue<string>("system", "General", "PricingTableName", out _pricingConfigName);
		if (string.IsNullOrEmpty(_pricingConfigName))
		{
			_pricingConfigName = "pricing";
			LogHelper.Instance.Log(">  Default to 'pricing' and standard pricing process...");
			return;
		}
		LogHelper.Instance.Log(">  Found pricing configuration '{0}'", _pricingConfigName);
		if (OrchestrationService.GetResourceTable(_pricingConfigName) == null)
		{
			LogHelper.Instance.Log(">  Pricing configuration file not found...");
			_pricingConfigName = "pricing";
			LogHelper.Instance.Log(">  Default to 'pricing' and standard pricing process...");
		}
	}
}
