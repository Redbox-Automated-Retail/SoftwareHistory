using System.Collections.Generic;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class BrowseState : IBrowseState
{
	private Stack<IBrowseStateSet> _browseStateSetStack = new Stack<IBrowseStateSet>();

	private IKioskProduct _browseSelectionSelectedProduct;

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
		}
	}

	public IKioskProduct CenterCarouselProduct { get; set; }

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

	public bool CollectionsPopupShown { get; set; }

	public bool CarouselCollectionsSelected { get; set; }

	public bool ProductSelectedFromCarousel { get; set; }

	public bool ProductAddedFromCarousel { get; set; }

	public bool MovieMatureWarningShown { get; set; }

	public bool _4kWarningShown { get; set; }

	public bool SubscriptionRemoved { get; set; }

	public bool MatureGameWarningShown { get; set; }

	public bool IsShoppingCartFullMessagesOpen { get; set; }

	public bool ShoppingCartFullMessagesShown { get; set; }

	public bool ReturnVisitIneligibleCartWarningShown { get; set; }

	public ISignupFlowState SignupFlowState { get; private set; }

	public int PaddleClickCount { get; set; }

	public BrowsePriceRangeParameter BrowsePriceRangeFilter { get; set; }

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

	public string BrowseStudio { get; set; }

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

	public BrowseSelectedSort BrowseSelectedSort
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

	public bool InReturnFlow { get; set; }

	public bool NeedMarketingConfirm { get; set; }

	public bool EnableBrowseMessagePopup { get; set; }

	public bool EnableStarzOffer { get; set; }

	public int BrowseMessagePopupDuration { get; set; }

	public bool ShowBrowseMessagePopup { get; set; }

	public bool ShowingBrowseMessagePopup { get; set; }

	public bool IsStarzTermsAcceptanceCheckMarkChecked { get; set; }

	public bool ShowRedboxPlusTooltip { get; set; }

	public bool IsRedboxPlusTermsAcceptanceCheckMarkChecked { get; set; }

	public BrowseState()
	{
		PushBrowseStateSet();
		SignupFlowState = new SignupFlowState();
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
		BrowseSelectedSort = BrowseSelectedSort.Newest;
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
		BrowseSelectedSort = BrowseSelectedSort.Newest;
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
		BrowseSelectedSort = BrowseSelectedSort.Newest;
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
		BrowseSelectedSort = BrowseSelectedSort.Newest;
		SelectedAlphaGroupName = null;
		Genre = null;
		BrowsePriceRangeFilter = null;
		BrowseStudio = null;
	}
}
