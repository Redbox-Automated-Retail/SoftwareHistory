using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class TitleRollupProduct : KioskProduct, ITitleProduct, IKioskProduct
{
	protected List<ITitleProduct> _kioskProducts = new List<ITitleProduct>();

	private Dictionary<long, ITitleProduct> _allTitleProducts;

	private Dictionary<long, MDVProduct> _allMDVProducts;

	private IProfileDataInstance _profile;

	private IKioskProduct _primaryProduct;

	private ITitleProduct _primaryTitle;

	public List<ITitleProduct> KioskProducts => _kioskProducts;

	private ProductGroup _productGroup { get; set; }

	public IKioskProduct PrimaryProduct => _primaryProduct;

	public ITitleProduct PrimaryTitle => _primaryTitle;

	public override string ProductName => _productGroup.ProductGroupName;

	public override string ProductDescription
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Description;
			}
			return string.Empty;
		}
	}

	public override bool IsValidProduct
	{
		get
		{
			if (_isValidProduct)
			{
				return _kioskProducts.All((ITitleProduct tp) => tp.IsValidProduct);
			}
			return false;
		}
		set
		{
			_isValidProduct = value;
		}
	}

	public long ProductGroupId => _productGroup.ProductGroupId;

	public string ProductGroupName => _productGroup.ProductGroupName;

	public long ProductId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ProductId;
			}
			return 0L;
		}
	}

	public int Stars
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Stars;
			}
			return 0;
		}
	}

	public string RunningTime
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.RunningTime;
			}
			return string.Empty;
		}
	}

	public List<string> Starring
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Starring;
			}
			return new List<string>();
		}
	}

	public int ProductTypeId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ProductTypeId;
			}
			return 0;
		}
	}

	public TitleType TitleType
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.TitleType;
			}
			return TitleType.DVD;
		}
	}

	public TitleFamily TitleFamily => TitleType.ToFamily();

	public long? RatingId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.RatingId;
			}
			return 0L;
		}
	}

	public string LongName
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.LongName;
			}
			return string.Empty;
		}
	}

	public string ImageFile
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ImageFile;
			}
			return string.Empty;
		}
	}

	public string Description
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Description;
			}
			return string.Empty;
		}
	}

	public List<long> Genres
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Genres;
			}
			return new List<long>();
		}
	}

	public DateTime? ReleaseDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ReleaseDate;
			}
			return DateTime.Now;
		}
	}

	public decimal BoxOfficeGross
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.BoxOfficeGross;
			}
			return 0m;
		}
	}

	public List<string> Directors
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.Directors;
			}
			return new List<string>();
		}
	}

	public int ComingSoonDays
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ComingSoonDays;
			}
			return 0;
		}
	}

	public DateTime? NationalStreetDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.NationalStreetDate;
			}
			return DateTime.Now;
		}
	}

	public bool ClosedCaptioned
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.ClosedCaptioned;
			}
			return false;
		}
	}

	public DateTime MerchandiseDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.MerchandiseDate;
			}
			return DateTime.Now.AddYears(10);
		}
	}

	public DateTime? DoNotRentDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.DoNotRentDate;
			}
			return DateTime.Now;
		}
	}

	public bool SellThru => _kioskProducts.Any((ITitleProduct kp) => kp.SellThru);

	public bool SellThruNew => _kioskProducts.Any((ITitleProduct kp) => kp.SellThruNew);

	public DateTime? SellThruDate => _kioskProducts.Min((ITitleProduct kp) => kp.SellThruDate);

	public decimal? SellThruPrice => _kioskProducts.Max((ITitleProduct kp) => kp.SellThruPrice);

	public long? SubPlatformId
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SubPlatformId;
			}
			return 0L;
		}
	}

	public string NumberOfPlayersText
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.NumberOfPlayersText;
			}
			return string.Empty;
		}
	}

	public int? DiscNumber
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.DiscNumber;
			}
			return 0;
		}
	}

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
			return _kioskProducts.Max((ITitleProduct kp) => kp.GamePurchasePrice);
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
			return _kioskProducts.Max((ITitleProduct kp) => kp.MoviePurchasePrice);
		}
	}

	public bool DoNotRent => _kioskProducts.All((ITitleProduct kp) => kp.DoNotRent);

	public bool CanBeSold => _kioskProducts.Any((ITitleProduct kp) => kp.CanBeSold);

	public bool IsAvailable => _kioskProducts.Any((ITitleProduct kp) => kp.IsAvailable);

	public bool ComingSoon => _kioskProducts.All((ITitleProduct kp) => kp.ComingSoon);

	public bool InStock => _kioskProducts.Any((ITitleProduct kp) => kp.InStock);

	public bool ShowOutOfStock => _kioskProducts.All((ITitleProduct kp) => kp.ShowOutOfStock);

	public string SortAlpha
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortAlpha;
			}
			return string.Empty;
		}
	}

	public DateTime SortMerchandise
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortMerchandise;
			}
			return DateTime.Now;
		}
	}

	public DateTime SortDate
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortDate;
			}
			return DateTime.Now;
		}
	}

	public decimal SortBoxOfficeGross
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortBoxOfficeGross;
			}
			return 0m;
		}
	}

	public string SortName
	{
		get
		{
			if (_primaryTitle != null)
			{
				return _primaryTitle.SortName;
			}
			return string.Empty;
		}
	}

	public long TitleRollupProductGroupId { get; set; }

	public TitleRollupProduct(ProductGroup productGroup, Dictionary<long, ITitleProduct> titleProducts, Dictionary<long, MDVProduct> mdvProducts, IProfileDataInstance profile, Store store)
		: base(store)
	{
		base.KioskProductType = KioskProductType.TitleRollup;
		_productGroup = productGroup;
		_allTitleProducts = titleProducts;
		_allMDVProducts = mdvProducts;
		_profile = profile;
		LoadKioskProducts();
	}

	public override string ToString()
	{
		return $"TitleRollupProduct {ProductId} {ProductName}";
	}

	private void LoadKioskProducts()
	{
		try
		{
			_productGroup.ProductIds.ForEach(delegate(long id)
			{
				if (_allTitleProducts.ContainsKey(id))
				{
					ITitleProduct title = _allTitleProducts[id];
					MDVProduct mDVProduct = (from item in _allMDVProducts
						where item.Value.TitleProducts.Contains(title)
						select item.Value).FirstOrDefault();
					if (mDVProduct != null)
					{
						if (!_kioskProducts.Contains(mDVProduct))
						{
							mDVProduct.TitleRollupProductGroupId = _productGroup.ProductGroupId;
							_kioskProducts.Add(mDVProduct);
						}
					}
					else
					{
						title.TitleRollupProductGroupId = _productGroup.ProductGroupId;
						_kioskProducts.Add(title);
					}
					if (title.IsMovie && title.TitleType == TitleType.DVD)
					{
						_primaryProduct = title;
					}
					if (title.IsGame && title.TitleType == TitleType.Xbox360)
					{
						_primaryProduct = title;
					}
				}
				else
				{
					LogHelper.Instance.Log("TitleRollupProduct.LoadKioskProducts - ProductGroupId {0} has a product id {1} that does not exist.", _productGroup.ProductGroupId, id);
				}
			});
			if (_primaryProduct == null)
			{
				if (_kioskProducts.Count() == 0)
				{
					IsValidProduct = false;
				}
				else
				{
					_primaryProduct = _kioskProducts.First();
				}
			}
		}
		finally
		{
			if (_primaryProduct != null)
			{
				if (_primaryProduct is MDVProduct)
				{
					_primaryTitle = (_primaryProduct as MDVProduct).PrimaryTitle;
				}
				else if (_primaryProduct is TitleProduct)
				{
					_primaryTitle = _primaryProduct as TitleProduct;
				}
			}
		}
	}
}
