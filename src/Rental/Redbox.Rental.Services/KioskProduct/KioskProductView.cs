using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.KioskData;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;

namespace Redbox.Rental.Services.KioskProduct;

public class KioskProductView
{
	private Store _store;

	private Dictionary<long, ITitleProduct> _titleProducts = new Dictionary<long, ITitleProduct>();

	private Dictionary<long, MDVProduct> _mdvProducts = new Dictionary<long, MDVProduct>();

	private Dictionary<long, TitleRollupProduct> _titleRollupProducts = new Dictionary<long, TitleRollupProduct>();

	private Dictionary<long, ICollectionProduct> _collectionProducts = new Dictionary<long, ICollectionProduct>();

	private List<ITitleProduct> _titlesWithNoRollup = new List<ITitleProduct>();

	private List<MDVProduct> _mdvTitlesWithNoRollup = new List<MDVProduct>();

	private List<ITitleProduct> _gamesProducts = new List<ITitleProduct>();

	private List<ITitleProduct> _movieProducts = new List<ITitleProduct>();

	public IProfileDataInstance ProfileDataInstance { get; set; }

	public ICacheDataInstance CacheDataInstance { get; set; }

	public IKioskDataInstance KioskDataInstance { get; set; }

	public Dictionary<long, ITitleProduct> TitleProducts => _titleProducts;

	public Dictionary<long, MDVProduct> MDVProducts => _mdvProducts;

	public Dictionary<long, TitleRollupProduct> TitleRollupProducts => _titleRollupProducts;

	public Dictionary<long, ICollectionProduct> CollectionProducts => _collectionProducts;

	public KioskProductView(Store store, IProfileDataInstance profileDataInstance, ICacheDataInstance cacheDataInstance, IKioskDataInstance kioskDataInstance)
	{
		_store = store;
		ProfileDataInstance = profileDataInstance;
		CacheDataInstance = cacheDataInstance;
		KioskDataInstance = kioskDataInstance;
		LoadView();
	}

	public void LoadView()
	{
		LogHelper.Instance.Log("KioskProductView.LoadView start");
		LoadTitleProducts();
		LoadMDVProducts();
		LoadTitleRollupProducts();
		LoadCollectionProducts();
		LoadTitlesWithNoRollupOrMDV();
		LoadMDVTitlesWithNoRollup();
		LoadMovies();
		LoadGames();
		LogHelper.Instance.Log("KioskProductView.LoadView complete");
	}

	public void LoadView(TitleCache titleCache)
	{
		TitleProduct titleProduct = AddTitleProduct(titleCache);
		if (titleProduct != null)
		{
			AddMDVProduct(titleProduct);
			AddTitleRollupProduct(titleProduct);
			LoadTitlesWithNoRollupOrMDV();
			LoadMDVTitlesWithNoRollup();
			LoadMovies();
			LoadGames();
		}
	}

	public List<IKioskProduct> GetProducts(BrowseViewFilter browseFilter, TitleFamily titleFamily, TitleType titleType, BrowseSort browseSort, long? genreFilter, List<long> include, List<long> exclude, bool isADAView)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		if (titleType == TitleType.Bluray)
		{
			list.AddRange(from item in FilterToBlueray(_movieProducts)
				where ApplyMainFilter(item, browseFilter)
				select (item));
		}
		else
		{
			if (titleFamily == TitleFamily.All || titleFamily == TitleFamily.Movies)
			{
				list.AddRange(from item in _movieProducts
					where ApplyMainFilter(item, browseFilter)
					select (item));
			}
			if (titleFamily == TitleFamily.All || titleFamily == TitleFamily.Games)
			{
				list.AddRange(from item in _gamesProducts
					where ApplyMainFilter(item, browseFilter)
					select (item));
			}
			if (titleType != TitleType.All)
			{
				list = list.Where((ITitleProduct item) => ApplyTitleTypeFilter(item, titleType)).ToList();
			}
		}
		if (genreFilter.HasValue)
		{
			list = list.Where((ITitleProduct item) => ApplyGenreFilter(item, genreFilter.Value)).ToList();
		}
		if ((include != null && include.Count > 0) || (exclude != null && exclude.Count > 0))
		{
			list = FilterInCarts(list, include, exclude);
		}
		if (isADAView)
		{
			list = FilterToTitles(list);
		}
		list = ApplyMainFilter(list, browseFilter);
		return ((IEnumerable<ITitleProduct>)list.OrderList(browseSort)).Select((Func<ITitleProduct, IKioskProduct>)((ITitleProduct item) => item)).ToList();
	}

	public List<IKioskProduct> GetCarouselProducts(bool showCollections)
	{
		List<IKioskProduct> resultList = new List<IKioskProduct>();
		int num = 10;
		(from x in _movieProducts.OrderList(BrowseSort.SortDate)
			where IsCarouselViewable(x, Genres.HitMovies) && !resultList.Contains(x)
			select x).Take(num)?.ForEach(resultList.Add);
		(from x in _movieProducts.OrderList(BrowseSort.SortDate)
			where IsCarouselViewable(x, Genres.Top20Movies) && !resultList.Contains(x)
			select x).Take(num - resultList.Count)?.ForEach(resultList.Add);
		(from x in _movieProducts.OrderList(BrowseSort.SortDate)
			where IsCarouselViewable(x, Genres.ActionAndAdventure) && !resultList.Contains(x)
			select x).Take(num - resultList.Count)?.ForEach(resultList.Add);
		if (showCollections)
		{
			resultList.Insert(5, new CollectionProduct(new Collection
			{
				Name = "Default",
				Id = 0L
			}, null, null, null, null, _store)
			{
				ImageFile = "image_collections_carousel"
			});
		}
		ITitleProduct titleProduct = _gamesProducts.OrderList(BrowseSort.MerchandiseDate).FirstOrDefault((ITitleProduct x) => IsCarouselViewable(x, null) && ApplyGameRatingFilter(x, Ratings.T) && !resultList.Contains(x));
		if (GetInStockGameCount() >= 3 && titleProduct != null && resultList.Count >= 5)
		{
			resultList.Insert(6, titleProduct);
			if (resultList.Count > num)
			{
				resultList.RemoveAt(resultList.Count - 1);
			}
		}
		return resultList;
	}

	public List<IKioskProduct> GetCollectionProducts()
	{
		List<IKioskProduct> list = new List<IKioskProduct>();
		(from pair in _collectionProducts
			select pair.Value into c
			where c.IsValidProduct
			select c).ToList().ForEach(list.Add);
		return list;
	}

	public ICollectionProduct GetCollection(long collectionId)
	{
		if (!_collectionProducts.ContainsKey(collectionId))
		{
			return null;
		}
		return _collectionProducts[collectionId];
	}

	public int GetInStockGameCount()
	{
		if (_gamesProducts == null)
		{
			return 0;
		}
		return _gamesProducts.Count((ITitleProduct x) => IsRentable(x));
	}

	private bool IsCarouselViewable(ITitleProduct titleProduct, Genres? genre)
	{
		if (!IsRentable(titleProduct))
		{
			return false;
		}
		if (titleProduct.ProductId == 1139)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(titleProduct.ImageFile) && !titleProduct.SellThruNew)
		{
			if (genre.HasValue)
			{
				if (titleProduct.Genres != null)
				{
					return titleProduct.Genres.Contains((long)genre.Value);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool IsRentable(ITitleProduct kioskProduct)
	{
		if (kioskProduct != null && kioskProduct.IsValidProduct && kioskProduct.IsAvailable && kioskProduct.InStock)
		{
			return !kioskProduct.DoNotRent;
		}
		return false;
	}

	private bool IsSellable(IKioskProduct kioskProduct)
	{
		if (!(kioskProduct is ITitleProduct))
		{
			return false;
		}
		if (kioskProduct is MDVProduct)
		{
			return false;
		}
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		if (titleProduct.IsMovie && titleProduct.MoviePurchasePrice <= 0m)
		{
			return false;
		}
		if (titleProduct.IsGame && titleProduct.GamePurchasePrice <= 0m)
		{
			return false;
		}
		if (titleProduct.IsGame)
		{
			if (!titleProduct.SellThruDate.HasValue)
			{
				return false;
			}
			if (titleProduct.SellThruDate > DateTime.Now)
			{
				return false;
			}
		}
		if (titleProduct.SellThru)
		{
			return titleProduct.CanBeSold;
		}
		return false;
	}

	public IKioskProduct GetTitleRollupProduct(long productGroupId)
	{
		return (from item in _titleRollupProducts
			where item.Value.ProductGroupId == productGroupId
			select item.Value).FirstOrDefault();
	}

	private List<ITitleProduct> FilterInCarts(List<ITitleProduct> list, List<long> include, List<long> exclude)
	{
		if (include != null || exclude != null)
		{
			list = FilterToTitles(list);
			if (include != null && include.Count() > 0)
			{
				list = list.Where((ITitleProduct item) => item.HasTitle(include)).ToList();
			}
			else if (exclude != null && exclude.Count() > 0)
			{
				list = list.Where((ITitleProduct item) => !item.HasTitle(exclude)).ToList();
			}
		}
		return list;
	}

	private List<ITitleProduct> FilterToTitles(List<ITitleProduct> kioskProducts)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		kioskProducts.ForEach(delegate(ITitleProduct item)
		{
			if (item is TitleRollupProduct)
			{
				list.AddRange(FilterToTitles(((TitleRollupProduct)item).KioskProducts));
			}
			if (item is MDVProduct || item is TitleProduct)
			{
				list.Add(item);
			}
		});
		return list;
	}

	private List<ITitleProduct> FilterToBlueray(List<ITitleProduct> kioskProducts)
	{
		List<ITitleProduct> list = new List<ITitleProduct>();
		kioskProducts.ForEach(delegate(ITitleProduct item)
		{
			if (item is TitleRollupProduct)
			{
				list.AddRange(FilterToBlueray(((TitleRollupProduct)item).KioskProducts));
			}
			if (item is MDVProduct && ((MDVProduct)item).TitleType == TitleType.Bluray)
			{
				list.Add(item);
			}
			if (item is TitleProduct && ((TitleProduct)item).TitleType == TitleType.Bluray)
			{
				list.Add(item);
			}
		});
		return list;
	}

	private bool ApplyTitleTypeFilter(ITitleProduct titleProduct, TitleType titleType)
	{
		return titleProduct.TitleType == titleType;
	}

	private bool ApplyGenreFilter(ITitleProduct titleProduct, long genreFilter)
	{
		return titleProduct.Genres.Contains(genreFilter);
	}

	private List<ITitleProduct> ApplyMainFilter(List<ITitleProduct> list, BrowseViewFilter browseFilter)
	{
		return list.Where((ITitleProduct kioskProduct) => ApplyMainFilter(kioskProduct, browseFilter)).ToList();
	}

	public bool ApplyMainFilter(ITitleProduct titleProduct, BrowseViewFilter browseFilter)
	{
		if (titleProduct.IsValidProduct && (titleProduct.IsAvailable || titleProduct.ComingSoon) && !titleProduct.DoNotRent && (titleProduct.InStock || titleProduct.ShowOutOfStock || titleProduct.ComingSoon))
		{
			return ApplyBrowseFilter(titleProduct, browseFilter);
		}
		return false;
	}

	private bool ApplyBrowseFilter(ITitleProduct titleProduct, BrowseViewFilter browseFilter)
	{
		if ((browseFilter != BrowseViewFilter.All || !titleProduct.IsAvailable) && (browseFilter != BrowseViewFilter.ComingSoon || !titleProduct.ComingSoon))
		{
			if (browseFilter == BrowseViewFilter.ForSale)
			{
				return IsSellable(titleProduct);
			}
			return false;
		}
		return true;
	}

	private bool ApplyGameRatingFilter(ITitleProduct titleProduct, Ratings rating)
	{
		return (titleProduct.RatingId ?? 999) <= (long)rating;
	}

	private void LoadTitleProducts()
	{
		_titleProducts.Clear();
		CacheDataInstance.Titles.ToList().ForEach(delegate(KeyValuePair<long, TitleCache> pair)
		{
			AddTitleProduct(pair.Value);
		});
	}

	private TitleProduct AddTitleProduct(TitleCache titleCache)
	{
		if (!ProfileDataInstance.Titles.TryGetValue(titleCache.ProductId, out var value))
		{
			LogHelper.Instance.Log("KioskProductView.AddTitleProduct - Profile title {0} not found", titleCache.ProductId);
			return null;
		}
		return AddTitleProduct(value, titleCache, (TitleType)value.ProductTypeId, ProfileDataInstance);
	}

	private TitleProduct AddTitleProduct(Title title, TitleCache titleCache, TitleType titleType, IProfileDataInstance profile)
	{
		if (_titleProducts.ContainsKey(titleCache.ProductId))
		{
			LogHelper.Instance.Log("KioskProductView.AddTitleProduct - Title product {0} already exists", titleCache.ProductId);
			return null;
		}
		TitleProduct titleProduct = new TitleProduct(title, titleCache, (TitleType)title.ProductTypeId, ProfileDataInstance, _store);
		_titleProducts[titleProduct.ProductId] = titleProduct;
		return titleProduct;
	}

	private void AddMissingProductIds(List<long> Ids)
	{
		AddMissingProductIds(Ids, force: false);
	}

	private void AddMissingProductIds(List<long> Ids, bool force)
	{
		if (!force && !Ids.Any((long id) => _titleProducts.ContainsKey(id)))
		{
			return;
		}
		Ids.ForEach(delegate(long id)
		{
			if (!_titleProducts.ContainsKey(id))
			{
				if (!ProfileDataInstance.Titles.TryGetValue(id, out var value))
				{
					LogHelper.Instance.Log("KioskProductView.AddMissingProductIds - Product id {0} not found", id);
				}
				else
				{
					LogHelper.Instance.Log("KioskProductView.AddMissingProductIds - ProductId {0}", id);
					TitleCache titleCache = CacheDataInstance.AddTitleCache(id);
					AddTitleProduct(value, titleCache, (TitleType)value.ProductTypeId, ProfileDataInstance);
				}
			}
		});
	}

	private void LoadMDVProducts()
	{
		_mdvProducts.Clear();
		ProfileDataInstance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 3).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			if (pair.Value.ProductIds.Any((long id) => _titleProducts.ContainsKey(id)))
			{
				AddMissingProductIds(pair.Value.ProductIds);
				MDVProduct mDVProduct = new MDVProduct(pair.Value, _titleProducts, ProfileDataInstance, _store);
				_mdvProducts[mDVProduct.ProductGroupId] = mDVProduct;
			}
		});
	}

	private MDVProduct AddMDVProduct(TitleProduct titleProduct)
	{
		ProductGroup productGroup = (from pair in ProfileDataInstance.ProductGroups
			where pair.Value.ProductGroupTypeId == 3 && pair.Value.ProductIds.Contains(titleProduct.ProductId)
			select pair.Value).FirstOrDefault();
		if (productGroup == null)
		{
			return null;
		}
		if (_mdvProducts.ContainsKey(productGroup.ProductGroupId))
		{
			LogHelper.Instance.Log("KioskProductView.AddMDVProduct - MDV product {0} already exists", productGroup.ProductGroupId);
			return null;
		}
		AddMissingProductIds(productGroup.ProductIds);
		MDVProduct mDVProduct = new MDVProduct(productGroup, _titleProducts, ProfileDataInstance, _store);
		_mdvProducts[mDVProduct.ProductGroupId] = mDVProduct;
		return mDVProduct;
	}

	private void LoadTitleRollupProducts()
	{
		_titleRollupProducts.Clear();
		ProfileDataInstance.ProductGroups.Where((KeyValuePair<long, ProductGroup> pair) => pair.Value.ProductGroupTypeId == 1).ForEach(delegate(KeyValuePair<long, ProductGroup> pair)
		{
			if (pair.Value.ProductIds.Any((long id) => _titleProducts.ContainsKey(id)))
			{
				AddMissingProductIds(pair.Value.ProductIds);
				TitleRollupProduct titleRollupProduct = new TitleRollupProduct(pair.Value, _titleProducts, _mdvProducts, ProfileDataInstance, _store);
				_titleRollupProducts[titleRollupProduct.ProductGroupId] = titleRollupProduct;
			}
		});
	}

	private TitleRollupProduct AddTitleRollupProduct(TitleProduct titleProduct)
	{
		ProductGroup productGroup = (from pair in ProfileDataInstance.ProductGroups
			where pair.Value.ProductGroupTypeId == 1 && pair.Value.ProductIds.Contains(titleProduct.ProductId)
			select pair.Value).FirstOrDefault();
		if (productGroup == null)
		{
			return null;
		}
		if (_titleRollupProducts.ContainsKey(productGroup.ProductGroupId))
		{
			LogHelper.Instance.Log("KioskProductView.AddTitleRollupProduct - Title Rollup product {0} already exists", productGroup.ProductGroupId);
			return null;
		}
		AddMissingProductIds(productGroup.ProductIds);
		TitleRollupProduct titleRollupProduct = new TitleRollupProduct(productGroup, _titleProducts, _mdvProducts, ProfileDataInstance, _store);
		_titleRollupProducts[titleRollupProduct.ProductGroupId] = titleRollupProduct;
		return titleRollupProduct;
	}

	private void LoadCollectionProducts()
	{
		_collectionProducts.Clear();
		KioskDataInstance.Collections.ForEach(delegate(Collection c)
		{
			AddMissingProductIds(c.Titles, force: true);
			CollectionProduct value = new CollectionProduct(c, _titleProducts, _mdvProducts, _titleRollupProducts, ProfileDataInstance, _store);
			_collectionProducts[c.Id] = value;
		});
	}

	private void LoadTitlesWithNoRollupOrMDV()
	{
		_titlesWithNoRollup.Clear();
		List<ITitleProduct> second = (from item in _titleRollupProducts
			from title in item.Value.KioskProducts
			where title != null
			select title).ToList();
		List<ITitleProduct> second2 = (from item in _mdvProducts
			from title in item.Value.TitleProducts
			select title).ToList();
		_titleProducts.Select((KeyValuePair<long, ITitleProduct> item) => item.Value).ToList().Except(second)
			.Except(second2)
			.ToList()
			.ForEach(delegate(ITitleProduct item)
			{
				if (item.IsMovie)
				{
					LogHelper.Instance.Log($"KioskProductView.LoadTitlesWithNoRollup - Title has no rollup id:{item.ProductId} name:{item.ProductName}", LogEntryType.Debug);
				}
				_titlesWithNoRollup.Add(item);
			});
	}

	private void LoadMDVTitlesWithNoRollup()
	{
		_mdvTitlesWithNoRollup.Clear();
		List<MDVProduct> second = (from item in _titleRollupProducts
			from title in item.Value.KioskProducts
			where title is MDVProduct
			select (MDVProduct)title).ToList();
		_mdvProducts.Select((KeyValuePair<long, MDVProduct> item) => item.Value).ToList().Except(second)
			.ToList()
			.ForEach(delegate(MDVProduct item)
			{
				if (item.IsMovie)
				{
					LogHelper.Instance.Log($"KioskProductView.LoadMDVTitlesWithNoRollup - MDV Title has no rollup productgroupid:{item.ProductGroupId} name:{item.ProductName}", LogEntryType.Debug);
				}
				_mdvTitlesWithNoRollup.Add(item);
			});
	}

	private void LoadGames()
	{
		_gamesProducts.Clear();
		Dictionary<TitleType, bool> allowedGameTypes = GetEnabledGameTypes();
		_gamesProducts.AddRange((from a in _titleRollupProducts
			select a.Value into item
			where item.IsGame && allowedGameTypes.ContainsKey(item.TitleType) && allowedGameTypes[item.TitleType]
			select item).Select((Func<TitleRollupProduct, ITitleProduct>)((TitleRollupProduct item) => item)));
		_gamesProducts.AddRange(_mdvTitlesWithNoRollup.Where((MDVProduct item) => item.IsGame && allowedGameTypes.ContainsKey(item.TitleType) && allowedGameTypes[item.TitleType]).Select((Func<MDVProduct, ITitleProduct>)((MDVProduct item) => item)));
		_gamesProducts.AddRange(from item in _titlesWithNoRollup
			where item.IsGame && allowedGameTypes.ContainsKey(item.TitleType) && allowedGameTypes[item.TitleType]
			select (item));
	}

	private Dictionary<TitleType, bool> GetEnabledGameTypes()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		return new Dictionary<TitleType, bool>
		{
			{
				TitleType.Xbox360,
				GetGameConfig(service, "GameEnableXbox360")
			},
			{
				TitleType.XboxOne,
				GetGameConfig(service, "GameEnableXboxOne")
			},
			{
				TitleType.PS3,
				GetGameConfig(service, "GameEnablePS3")
			},
			{
				TitleType.PS4,
				GetGameConfig(service, "GameEnablePS4")
			},
			{
				TitleType.Wii,
				GetGameConfig(service, "GameEnableWii")
			},
			{
				TitleType.WiiU,
				GetGameConfig(service, "GameEnableWiiU")
			}
		};
	}

	private bool GetGameConfig(IConfigurationService configService, string settingName)
	{
		if (configService == null)
		{
			return false;
		}
		if (configService.TryGetValue<bool>("system", "Games", settingName, out var value))
		{
			return value;
		}
		return false;
	}

	private void LoadMovies()
	{
		_movieProducts.Clear();
		_movieProducts.AddRange((from a in _titleRollupProducts
			select a.Value into item
			where item.IsMovie
			select item).Select((Func<TitleRollupProduct, ITitleProduct>)((TitleRollupProduct item) => item)));
		_movieProducts.AddRange(_mdvTitlesWithNoRollup.Where((MDVProduct item) => item.IsMovie).Select((Func<MDVProduct, ITitleProduct>)((MDVProduct item) => item)));
		_movieProducts.AddRange(from item in _titlesWithNoRollup
			where item.IsMovie
			select (item));
	}
}
