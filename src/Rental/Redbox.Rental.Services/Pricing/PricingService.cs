using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Services.Pricing;

public class PricingService : IPricingService
{
	private Store _store;

	private IBundleBasedPricingService _bundleBasedPricingService;

	private IKioskPricingService _kioskPricingService;

	private IConfiguration _configuration;

	private IPricingData _pricingData;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	private Timer _changeTimer;

	private int _inRefresh;

	public bool IsPricingEngineEnabled
	{
		get
		{
			if (_kioskPricingService == null)
			{
				return false;
			}
			if (!_kioskPricingService.IsPricingEngineDataAvailable)
			{
				return false;
			}
			if (!_configuration.EnablePricingEngine)
			{
				return false;
			}
			return true;
		}
	}

	public IPricingSet CurrentPriceSet
	{
		get
		{
			if (IsPricingEngineEnabled)
			{
				IPricingSet currentPriceSet = _kioskPricingService.CurrentPriceSet;
				if (currentPriceSet != null)
				{
					return currentPriceSet;
				}
			}
			return _bundleBasedPricingService.CurrentPriceSet;
		}
	}

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

	public event PricingSetChange OnPricingSetChange;

	public PricingService(IKernelService kernelService, IStoreManager storeManager)
	{
		_bundleBasedPricingService = new BundleBasedPricingService();
		_pricingData = new PricingData(kernelService, storeManager);
		_kioskPricingService = new KioskPricingService(_pricingData, storeManager.KioskId, Redbox.Rental.Services.Configuration.Configuration.Instance);
		_configuration = Redbox.Rental.Services.Configuration.Configuration.Instance;
		if (_changeTimer == null)
		{
			_changeTimer = new Timer(delegate
			{
				OnTimer();
			});
		}
	}

	public void Start()
	{
		try
		{
			LogHelper.Instance.Log("PricingService.Start");
			int seconds = new Random().Next(5, 55);
			int minutes = 2;
			_changeTimer.Change(new TimeSpan(0, 1, seconds), new TimeSpan(0, minutes, seconds));
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("PricingService.Start - an exception occurred.", e);
		}
	}

	public void Stop()
	{
		try
		{
			LogHelper.Instance.Log("PricingService.Stop");
			_changeTimer.Change(-1, -1);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("PricingService.Stop - an exception occurred.", e);
		}
	}

	private void OnTimer()
	{
		if (Interlocked.CompareExchange(ref _inRefresh, 1, 0) == 1)
		{
			LogHelper.Instance.Log("PricingService.Refresh - exiting already in OnTimer");
			return;
		}
		try
		{
			_pricingData.LoadStaged();
		}
		finally
		{
			_inRefresh = 0;
		}
	}

	public void ClearOnPricingSetChange()
	{
		OnPricingSetChange = null;
	}

	public void Reload()
	{
		LogHelper.Instance.Log("PricingService.Reload");
		try
		{
			IBundleBasedPricingService bundleBasedPricingService = _bundleBasedPricingService;
			if (bundleBasedPricingService != null && bundleBasedPricingService.Reload() && OnPricingSetChange != null)
			{
				OnPricingSetChange();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingService.Reload - Unhandled exception occurred", e);
		}
	}

	public void UpdatePricingInstance()
	{
		LogHelper.Instance.Log("PricingService.UpdatePricingInstance");
		try
		{
			if (_kioskPricingService != null)
			{
				_kioskPricingService.UpdatePricingInstance();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("PricingService.UpdatePricingInstance - Unhandled exception occurred", e);
		}
	}

	public IPricingRecord GetPurchasePricing(ITitleProduct titleProduct)
	{
		if (IsPricingEngineEnabled)
		{
			return _kioskPricingService.GetPurchasePricing(titleProduct);
		}
		return null;
	}

	public IPricingRecord GetOneDayProductPricingRecord(ITitleProduct titleProduct)
	{
		return GetOneDayProductPricingRecord(titleProduct.ProductId);
	}

	public IPricingRecord GetOneDayProductPricingRecord(long productId)
	{
		IPricingRecord pricingRecord = null;
		if (IsPricingEngineEnabled)
		{
			pricingRecord = _kioskPricingService.GetOneDayProductPricingRecord(productId);
		}
		if (pricingRecord == null)
		{
			pricingRecord = _bundleBasedPricingService.GetProductPricingRecord(productId);
		}
		return pricingRecord;
	}

	public IList<IPricingRecord> GetProductPricingRecords(ITitleProduct titleProduct)
	{
		return GetProductPricingRecords(titleProduct?.ProductId ?? 0);
	}

	public IList<IPricingRecord> GetProductPricingRecords(long productId)
	{
		IList<IPricingRecord> list = null;
		if (IsPricingEngineEnabled)
		{
			list = _kioskPricingService.GetProductPricingRecords(productId);
		}
		if (list == null)
		{
			list = new List<IPricingRecord>();
			list.Add(_bundleBasedPricingService.GetProductPricingRecord(productId));
		}
		return list;
	}

	public IPricingRecord GetProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType, bool useAdjustedPrices)
	{
		IPricingRecord pricingRecord = null;
		if (IsPricingEngineEnabled)
		{
			pricingRecord = _kioskPricingService.GetProductTypePricingRecord(titleFamily, titleType, useAdjustedPrices);
		}
		if (pricingRecord == null)
		{
			pricingRecord = _bundleBasedPricingService.GetProductTypePricingRecord(titleFamily, titleType, useAdjustedPrices);
		}
		return pricingRecord;
	}

	public IPricingRecord GetActiveProductTypePricingRecord(TitleFamily titleFamily, TitleType titleType)
	{
		if (IsPricingEngineEnabled)
		{
			return _kioskPricingService.GetActiveProductTypePricingRecord(titleFamily, titleType);
		}
		return null;
	}

	public decimal? GetPriceSetting(TitleFamily titleFamily, TitleType titleType, string settingName, RentalShoppingCartItemAction action)
	{
		decimal? result = null;
		IPricingRecord productTypePricingRecord = GetProductTypePricingRecord(titleFamily, titleType, useAdjustedPrices: false);
		if (productTypePricingRecord != null)
		{
			switch (settingName)
			{
			case "initial_night":
				result = productTypePricingRecord.InitialNight;
				break;
			case "extra_night":
				result = productTypePricingRecord.ExtraNight;
				break;
			case "expiration_price":
				result = productTypePricingRecord.ExpirationPrice;
				break;
			case "promo_value":
				result = productTypePricingRecord.PromoValue;
				break;
			}
			if (result.HasValue && result.HasValue)
			{
				result = GetAdjustedPrice(result.Value, action, titleType);
			}
		}
		return result;
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

	public decimal GetRentalTaxRate()
	{
		decimal result = 0m;
		if (Store != null && Store.State != "MD" && Store.State != "SC")
		{
			return Store.RentalTaxRate;
		}
		return result;
	}

	public decimal GetPurchaseTaxRate()
	{
		decimal result = 0m;
		if (Store != null && Store.State != "MD" && Store.State != "SC")
		{
			return Store.PurchaseTaxRate;
		}
		return result;
	}

	public decimal GetDigitalPurchaseTaxRate()
	{
		decimal result = 0m;
		if (Store != null && Store.State != "MD" && Store.State != "SC")
		{
			return Store.DigitalPurchaseTaxRate;
		}
		return result;
	}

	public decimal GetEmptyCasePrice()
	{
		return GetPriceSetting(TitleFamily.Movies, TitleType.DVD, "initial_night", RentalShoppingCartItemAction.purchase) ?? 0m;
	}
}
