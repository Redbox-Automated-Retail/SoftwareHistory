using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.Session;

public class BrowseState : IBrowseState
{
	private Stack<IBrowseStateSet> _browseStateSetStack = new Stack<IBrowseStateSet>();

	private IKioskProduct _browseSelectionSelectedProduct;

	private LuaTable _browseSelectionSelectedProductLuaTable;

	public string ItemAddedFromView { get; set; }

	public string ProductDetailShownFromView { get; set; }

	public IKioskProduct BrowseSelectionSelectedProduct
	{
		get
		{
			return _browseSelectionSelectedProduct;
		}
		set
		{
			_browseSelectionSelectedProduct = value;
			_browseSelectionSelectedProductLuaTable = value?.ToDictionary()?.ToLuaTable();
		}
	}

	public LuaTable BrowseSelectionSelectedProductLuaTable
	{
		get
		{
			return _browseSelectionSelectedProductLuaTable;
		}
		set
		{
			SetBrowseSelectionSelectedProductLuaTable(value);
		}
	}

	public IKioskProduct CenterCarouselProduct { get; set; }

	public LuaTable CenterCarouselProductLuaTable => CenterCarouselProduct?.ToDictionary()?.ToLuaTable();

	public List<IKioskProduct> BrowseProductsShown
	{
		get
		{
			return _browseStateSetStack.Peek().BrowseProductsShown;
		}
		set
		{
			_browseStateSetStack.Peek().BrowseProductsShown = value;
		}
	}

	public List<IBrowseItemModel> TTSBrowseItemModelsShown
	{
		get
		{
			return _browseStateSetStack.Peek().TTSBrowseItemModelsShown;
		}
		set
		{
			_browseStateSetStack.Peek().TTSBrowseItemModelsShown = value;
		}
	}

	public LuaTable BrowseProductsShownLuaTable => BrowseProductsShown?.ToDictionary()?.ToLuaTable();

	public bool CollectionsPopupShown { get; set; }

	public bool CarouselCollectionsSelected { get; set; }

	public bool ProductSelectedFromCarousel { get; set; }

	public bool ProductAddedFromCarousel { get; set; }

	public bool MovieMatureWarningShown { get; set; }

	public bool BluRayWarningShown { get; set; }

	public bool MatureGameWarningShown { get; set; }

	public bool IsShoppingCartFullMessagesOpen { get; set; }

	public bool ShoppingCartFullMessagesShown { get; set; }

	public int PaddleClickCount { get; set; }

	public string BrowseFilterMode
	{
		get
		{
			return BrowseMode.ToString();
		}
		set
		{
			BrowseMode = (BrowseViewFilter)Enum.Parse(typeof(BrowseViewFilter), value);
		}
	}

	public BrowseViewFilter BrowseMode
	{
		get
		{
			return _browseStateSetStack.Peek().BrowseMode;
		}
		set
		{
			_browseStateSetStack.Peek().BrowseMode = value;
		}
	}

	public string BrowseTitleFamily
	{
		get
		{
			return TitleFamily.ToString();
		}
		set
		{
			TitleFamily = value.ConvertToTitleFamily();
		}
	}

	public TitleFamily TitleFamily
	{
		get
		{
			return _browseStateSetStack.Peek().TitleFamily;
		}
		set
		{
			_browseStateSetStack.Peek().TitleFamily = value;
		}
	}

	public int? BrowseGenre
	{
		get
		{
			return (int?)Genre;
		}
		set
		{
			Genre = (Genres?)value;
		}
	}

	public Genres? Genre
	{
		get
		{
			return _browseStateSetStack.Peek().Genre;
		}
		set
		{
			_browseStateSetStack.Peek().Genre = value;
		}
	}

	public int PageNumber
	{
		get
		{
			return _browseStateSetStack.Peek().PageNumber;
		}
		set
		{
			_browseStateSetStack.Peek().PageNumber = value;
		}
	}

	public string BrowseTitleType
	{
		get
		{
			return TitleType.ToString();
		}
		set
		{
			TitleType = value.ConvertToTitleType();
		}
	}

	public TitleType TitleType
	{
		get
		{
			return _browseStateSetStack.Peek().TitleType;
		}
		set
		{
			_browseStateSetStack.Peek().TitleType = value;
		}
	}

	public bool BrowseCollections
	{
		get
		{
			return _browseStateSetStack.Peek().BrowseCollections;
		}
		set
		{
			_browseStateSetStack.Peek().BrowseCollections = value;
		}
	}

	public ICollectionProduct BrowseCollectionProduct
	{
		get
		{
			return _browseStateSetStack.Peek().BrowseCollectionProduct;
		}
		set
		{
			_browseStateSetStack.Peek().BrowseCollectionProduct = value;
		}
	}

	public string SelectedAlphaGroupName
	{
		get
		{
			return _browseStateSetStack.Peek().SelectedAlphaGroupName;
		}
		set
		{
			_browseStateSetStack.Peek().SelectedAlphaGroupName = value;
		}
	}

	public string BrowseSelectedSort
	{
		get
		{
			return _browseStateSetStack.Peek().BrowseSelectedSort;
		}
		set
		{
			_browseStateSetStack.Peek().BrowseSelectedSort = value;
		}
	}

	public List<IBrowseItemModel> BrowseItemModels
	{
		get
		{
			return _browseStateSetStack.Peek().BrowseItemModels;
		}
		set
		{
			_browseStateSetStack.Peek().BrowseItemModels = value;
		}
	}

	public bool IsNonBrowseView
	{
		get
		{
			return _browseStateSetStack.Peek().IsNonBrowseView;
		}
		set
		{
			_browseStateSetStack.Peek().IsNonBrowseView = value;
		}
	}

	public BrowseState()
	{
		PushBrowseStateSet();
	}

	private void SetBrowseSelectionSelectedProductLuaTable(LuaTable value)
	{
		_browseSelectionSelectedProductLuaTable = value;
		IKioskProduct kioskProduct = null;
		IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
		long result = 0L;
		if (_browseSelectionSelectedProductLuaTable != null && long.TryParse(_browseSelectionSelectedProductLuaTable["product_id"].ToString(), out result))
		{
			int result2 = 0;
			if (int.TryParse(_browseSelectionSelectedProductLuaTable["kiosk_product_type"].ToString(), out result2) && Enum.TryParse<KioskProductType>(result2.ToString(), out var result3))
			{
				bool excludeTitleRollup = result3 != KioskProductType.TitleRollup;
				dataService.GetProduct(result, excludeTitleRollup, out kioskProduct);
			}
		}
		_browseSelectionSelectedProduct = kioskProduct;
	}

	public void PushBrowseStateSet()
	{
		_browseStateSetStack.Push(new BrowseStateSet());
	}

	public IBrowseStateSet PopBrowseStateSet()
	{
		IBrowseStateSet result = null;
		if (_browseStateSetStack.Count > 1)
		{
			result = _browseStateSetStack.Pop();
		}
		return result;
	}

	public void PushCollectionsBrowseStateSet()
	{
		PushBrowseStateSet();
		IsNonBrowseView = false;
		BrowseCollections = true;
		BrowseCollectionProduct = null;
		PageNumber = 1;
		TitleFamily = TitleFamily.Movies;
		TitleType = TitleType.All;
		BrowseMode = BrowseViewFilter.All;
		BrowseSelectedSort = "Newest";
		SelectedAlphaGroupName = null;
		Genre = null;
	}

	public void PushCollectionBrowseStateSet(ICollectionProduct collectionProduct)
	{
		PushBrowseStateSet();
		IsNonBrowseView = false;
		BrowseCollections = false;
		BrowseCollectionProduct = collectionProduct;
		PageNumber = 1;
		TitleFamily = TitleFamily.Movies;
		TitleType = TitleType.All;
		BrowseMode = BrowseViewFilter.All;
		BrowseSelectedSort = "Newest";
		SelectedAlphaGroupName = null;
		Genre = null;
	}

	public void PushNonBrowseViewBrowseStateSet()
	{
		PushBrowseStateSet();
		IsNonBrowseView = true;
		BrowseCollections = false;
		BrowseCollectionProduct = null;
		PageNumber = 1;
		TitleFamily = TitleFamily.Movies;
		TitleType = TitleType.All;
		BrowseMode = BrowseViewFilter.All;
		BrowseSelectedSort = "Newest";
		SelectedAlphaGroupName = null;
		Genre = null;
	}

	public void UnwindBrowseStateSetStack()
	{
		while (_browseStateSetStack.Count > 1)
		{
			_browseStateSetStack.Pop();
		}
	}

	public void SetDefaultBrowseStateSet()
	{
		IsNonBrowseView = false;
		BrowseCollections = false;
		BrowseCollectionProduct = null;
		PageNumber = 1;
		TitleFamily = TitleFamily.Movies;
		TitleType = TitleType.All;
		BrowseMode = BrowseViewFilter.All;
		BrowseSelectedSort = "Newest";
		SelectedAlphaGroupName = null;
		Genre = null;
	}
}
