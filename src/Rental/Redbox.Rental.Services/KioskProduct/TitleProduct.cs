using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class TitleProduct : KioskProduct, ITitleProduct, IKioskProduct
{
	private Title _title;

	private TitleCache _titleCache;

	private TitleType _titleType;

	private IProfileDataInstance _profile;

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

	public decimal GamePurchasePrice
	{
		get
		{
			if (!IsGame)
			{
				return 0m;
			}
			return GetAdjustedPurchasePrice(SellThruPrice ?? 0m);
		}
	}

	public decimal MoviePurchasePrice
	{
		get
		{
			if (!IsMovie)
			{
				return 0m;
			}
			return GetAdjustedPurchasePrice(_store.PurchasePrice);
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

	public bool CanBeSold => _titleCache.CanBeSold;

	public bool IsAvailable => DateTime.Now > MerchandiseDate;

	public bool ComingSoon
	{
		get
		{
			if (ProductId == 1139)
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

	public string SortAlpha => _title.SortName.ToLower();

	public DateTime SortMerchandise => _title.MerchandiseDate ?? DateTime.Now;

	public DateTime SortDate => _title.SortDate ?? DateTime.Now;

	public decimal SortBoxOfficeGross => _title.BoxOfficeGross;

	public string SortName => _title.SortName.ToLower();

	public TitleProduct(Title title, TitleCache titleCache, TitleType titleType, IProfileDataInstance profile, Store store)
		: base(store)
	{
		base.KioskProductType = KioskProductType.Title;
		_title = title;
		_titleCache = titleCache;
		_titleType = titleType;
		_profile = profile;
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
		if (_store.State == "MD" || _store.State == "SC")
		{
			decimal purchaseTaxRate = _store.PurchaseTaxRate;
			if (purchaseTaxRate > 0m)
			{
				return Math.Round(price * (purchaseTaxRate / 100m + 1m), 2);
			}
		}
		return price;
	}
}
