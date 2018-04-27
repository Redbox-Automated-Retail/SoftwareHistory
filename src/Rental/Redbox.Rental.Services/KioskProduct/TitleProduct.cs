using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Pricing;

namespace Redbox.Rental.Services.KioskProduct;

public class TitleProduct : KioskProduct, ITitleProduct, IKioskProduct
{
	private Title _title;

	private TitleCache _titleCache;

	private TitleType _titleType;

	private IProfileDataInstance _profile;

	private IStoreManager _storeManager;

	private IPricingService _pricingService;

	public Dictionary<string, BarcodeProduct> BarCodeProducts => _titleCache.BarcodeProducts;

	public override string ProductName => _title.LongName;

	public override string ProductDescription => _title.Description;

	public long ProductId => _title.ProductId;

	public int Stars => _title.Stars;

	public string RunningTime => _title.RunningTime;

	public List<string> Starring => _title.Starring;

	public int ProductTypeId => _title.ProductTypeId;

	public TitleType TitleType => GetTitleTypeFromProductTypeId(_title.ProductTypeId);

	public TitleFamily TitleFamily => TitleType.ToFamily();

	public long? RatingId => _title.RatingId;

	public string LongName => _title.LongName;

	public string ImageFile => _title.ImageFile;

	public string Description => _title.Description;

	public List<long> Genres => _title.Genres;

	public DateTime? ReleaseDate => _title.ReleaseDate;

	public decimal BoxOfficeGross => _title.BoxOfficeGross;

	public List<string> Directors => _title.Directors;

	public int ComingSoonDays => _title.ComingSoonDays;

	public DateTime? NationalStreetDate => _title.NationalStreetDate;

	public bool ClosedCaptioned => _title.ClosedCaptioned;

	public DateTime MerchandiseDate => _title.MerchandiseDate ?? DateTime.Now.AddYears(10);

	public DateTime? DoNotRentDate => _title.DoNotRentDate;

	public bool SellThru => _title.SellThru;

	public bool SellThruNew => _title.SellThruNew;

	public DateTime? SellThruDate => _title.SellThruDate;

	public decimal? SellThruPrice => _title.SellThruPrice;

	public long? SubPlatformId => _title.SubPlatformId;

	public string NumberOfPlayersText => _title.NumberOfPlayersText;

	public int? DiscNumber => _title.DiscNumber;

	public bool IsGame => TitleFamily == TitleFamily.Games;

	public bool IsMovie => TitleFamily == TitleFamily.Movies;

	public bool IsEmptyCase => ProductId == 1139;

	public bool IsSellable
	{
		get
		{
			if (IsEmptyCase)
			{
				return true;
			}
			if (PricingService.IsPricingEngineEnabled)
			{
				if (PriceEngineIsSellable)
				{
					return PurchasePrice > 0m;
				}
				return false;
			}
			return NoPriceEngineIsSellable;
		}
	}

	private bool NoPriceEngineIsSellable
	{
		get
		{
			if (DoNotRent || !IsValidProduct || !SellThru || !IsAvailable)
			{
				return false;
			}
			if (IsGame)
			{
				if (!_storeManager.CanSellUsedGames)
				{
					return false;
				}
				if (PurchasePrice <= 0m)
				{
					return false;
				}
				if (!SellThruDate.HasValue)
				{
					return false;
				}
				if (SellThruDate > DateTime.Now)
				{
					return false;
				}
			}
			if (IsMovie)
			{
				if (!_storeManager.CanSellUsedMovies)
				{
					return false;
				}
				if (PurchasePrice <= 0m)
				{
					return false;
				}
				if (!SellThruDate.HasValue)
				{
					if (TitleType == TitleType.DigitalCode)
					{
						return true;
					}
					IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
					if (service == null)
					{
						return false;
					}
					if ((DateTime.Now - MerchandiseDate).TotalDays < (double)service.GetSellThruDays())
					{
						return false;
					}
				}
				if (SellThruDate.HasValue && SellThruDate > DateTime.Now)
				{
					return false;
				}
			}
			return true;
		}
	}

	private bool PriceEngineIsSellable
	{
		get
		{
			if (IsEmptyCase)
			{
				return true;
			}
			if (DoNotRent || !IsValidProduct || !SellThru || !IsAvailable)
			{
				return false;
			}
			if (IsGame && !_storeManager.CanSellUsedGames)
			{
				return false;
			}
			if (IsMovie && !_storeManager.CanSellUsedMovies)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsRentable
	{
		get
		{
			if (IsEmptyCase)
			{
				return true;
			}
			if (DoNotRent || !IsValidProduct)
			{
				return false;
			}
			if (TitleType == TitleType.DigitalCode)
			{
				return false;
			}
			return IsAvailable;
		}
	}

	public bool IsSellableAndInStock
	{
		get
		{
			if (IsSellable)
			{
				return InStock;
			}
			return false;
		}
	}

	public bool IsRentableAndInStock
	{
		get
		{
			if (IsRentable)
			{
				return InStock;
			}
			return false;
		}
	}

	public decimal PurchasePrice
	{
		get
		{
			if (IsEmptyCase)
			{
				return PricingService.GetEmptyCasePrice();
			}
			if (PricingService.IsPricingEngineEnabled)
			{
				if (PriceEngineIsSellable)
				{
					return PurchasePriceRecord?.Purchase ?? 0m;
				}
				return 0m;
			}
			return NoPricingEnginePurchasePrice;
		}
	}

	private decimal NoPricingEnginePurchasePrice
	{
		get
		{
			if (IsEmptyCase)
			{
				return PricingService.GetEmptyCasePrice();
			}
			if (IsGame)
			{
				if (!IsGame)
				{
					return 0m;
				}
				return GetAdjustedPurchasePrice(SellThruPrice ?? 0m);
			}
			if (!IsMovie)
			{
				return 0m;
			}
			if (TitleType == TitleType.DigitalCode)
			{
				if (!SellThruPrice.HasValue)
				{
					return 0m;
				}
				return GetAdjustedPurchasePrice(SellThruPrice.Value);
			}
			return GetAdjustedPurchasePrice(SellThruPrice ?? _store.PurchasePrice);
		}
	}

	public IPricingRecord PurchasePriceRecord
	{
		get
		{
			if (IsEmptyCase)
			{
				return new PricingRecord
				{
					Purchase = PricingService.GetEmptyCasePrice()
				};
			}
			if (PricingService.IsPricingEngineEnabled)
			{
				return PricingService.GetPurchasePricing(this);
			}
			return new PricingRecord
			{
				Purchase = NoPricingEnginePurchasePrice
			};
		}
	}

	public IList<IPricingRecord> RentalPriceRecords
	{
		get
		{
			if (PricingService.IsPricingEngineEnabled)
			{
				return PricingService.GetProductPricingRecords(this);
			}
			return new List<IPricingRecord> { PricingService.GetOneDayProductPricingRecord(this) };
		}
	}

	private IConfiguration Config => Redbox.Rental.Services.Configuration.Configuration.Instance;

	private IPricingService PricingService
	{
		get
		{
			if (_pricingService == null)
			{
				_pricingService = ServiceLocator.Instance.GetService<IPricingService>();
			}
			return _pricingService;
		}
	}

	public long TitleRollupProductGroupId { get; set; }

	public bool DoNotRent
	{
		get
		{
			if (!_title.DoNotRentDate.HasValue)
			{
				return false;
			}
			return _title.DoNotRentDate <= DateTime.Now;
		}
	}

	public bool IsAvailable
	{
		get
		{
			if (TitleType == TitleType.DigitalCode)
			{
				return true;
			}
			return DateTime.Now > MerchandiseDate;
		}
	}

	public bool ComingSoon
	{
		get
		{
			if (DoNotRent || !IsValidProduct)
			{
				return false;
			}
			if (ProductId == 1139)
			{
				return false;
			}
			if (TitleType == TitleType.DigitalCode || TitleType == TitleType.NintendoSwitch)
			{
				return false;
			}
			if (IsAvailable)
			{
				return false;
			}
			if (InStock)
			{
				return true;
			}
			return (MerchandiseDate - DateTime.Now).Days <= ComingSoonDays;
		}
	}

	public bool InStock => _titleCache.OnHandCount > 0;

	public bool ShowOutOfStock
	{
		get
		{
			if (InStock)
			{
				return false;
			}
			if (TitleType == TitleType.DigitalCode)
			{
				return false;
			}
			if (!_titleCache.LastDateOutOfStock.HasValue)
			{
				return false;
			}
			if (_titleCache.ThinnedOut)
			{
				return false;
			}
			return (DateTime.Now.Date - _titleCache.LastDateOutOfStock.Value).Days < 25;
		}
	}

	public bool HasBeenInStock
	{
		get
		{
			if (!_titleCache.LastDateOutOfStock.HasValue)
			{
				return false;
			}
			return true;
		}
	}

	public bool HasDeal => (ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(_title.ProductId))?.Any((IPricingRecord pr) => pr?.IsDeal ?? false) ?? false;

	public bool HasMultiNightPrice => (ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(_title.ProductId))?.Any((IPricingRecord pr) => pr?.IsMultiNightPrice ?? false) ?? false;

	public string SortAlpha => _title.SortName.ToLower();

	public DateTime SortMerchandise => _title.MerchandiseDate ?? DateTime.Now;

	public DateTime SortDate => _title.SortDate ?? DateTime.Now;

	public decimal SortBoxOfficeGross => _title.BoxOfficeGross;

	public string SortName => _title.SortName.ToLower();

	public TitleProduct(Title title, TitleCache titleCache, TitleType titleType, IProfileDataInstance profile, Store store, IStoreManager storeManager)
		: base(store)
	{
		base.KioskProductType = KioskProductType.Title;
		_title = title;
		_titleCache = titleCache;
		_titleType = titleType;
		_profile = profile;
		_storeManager = storeManager;
	}

	public override string ToString()
	{
		return $"TitleProduct {ProductId} {ProductName}";
	}

	public static TitleType GetTitleTypeFromProductTypeId(int productTypeId)
	{
		TitleType result = TitleType.DVD;
		if (Enum.IsDefined(typeof(TitleType), productTypeId))
		{
			result = (TitleType)productTypeId;
		}
		return result;
	}

	private decimal GetAdjustedPurchasePrice(decimal price)
	{
		if (_store == null || string.IsNullOrEmpty(_store.State))
		{
			return price;
		}
		if (_store.State == "MD")
		{
			decimal num = ((TitleType == TitleType.DigitalCode) ? _store.DigitalPurchaseTaxRate : _store.PurchaseTaxRate);
			if (num > 0m)
			{
				return Math.Round(price * (num / 100m + 1m), 2);
			}
		}
		return price;
	}
}
