using System.Collections.Generic;
using Redbox.Lua;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Session;

public interface IBrowseState
{
	bool CollectionsPopupShown { get; set; }

	bool CarouselCollectionsSelected { get; set; }

	bool ProductSelectedFromCarousel { get; set; }

	bool ProductAddedFromCarousel { get; set; }

	bool MovieMatureWarningShown { get; set; }

	bool BluRayWarningShown { get; set; }

	bool _4kWarningShown { get; set; }

	bool MatureGameWarningShown { get; set; }

	bool IsShoppingCartFullMessagesOpen { get; set; }

	bool ShoppingCartFullMessagesShown { get; set; }

	int PaddleClickCount { get; set; }

	string BrowseFilterMode { get; set; }

	BrowseViewFilter BrowseMode { get; set; }

	BrowseSelectedSort BrowseSelectedSort { get; set; }

	string SelectedAlphaGroupName { get; set; }

	string BrowseTitleFamily { get; set; }

	TitleFamily TitleFamily { get; set; }

	string BrowseTitleType { get; set; }

	TitleType TitleType { get; set; }

	bool BrowseCollections { get; set; }

	ICollectionProduct BrowseCollectionProduct { get; set; }

	int PageNumber { get; set; }

	int? BrowseGenre { get; set; }

	Genres? Genre { get; set; }

	List<IBrowseItemModel> BrowseItemModels { get; set; }

	IKioskProduct CenterCarouselProduct { get; set; }

	LuaTable CenterCarouselProductLuaTable { get; }

	IKioskProduct BrowseSelectionSelectedProduct { get; set; }

	LuaTable BrowseSelectionSelectedProductLuaTable { get; set; }

	string ProductDetailShownFromView { get; set; }

	string ItemAddedFromView { get; set; }

	bool IsNonBrowseView { get; set; }

	List<IKioskProduct> BrowseProductsShown { get; set; }

	List<IBrowseItemModel> TTSBrowseItemModelsShown { get; set; }

	LuaTable BrowseProductsShownLuaTable { get; }

	bool InReturnFlow { get; set; }

	void PushBrowseStateSet();

	IBrowseStateSet PopBrowseStateSet();

	void UnwindBrowseStateSetStack();

	void PushCollectionsBrowseStateSet();

	void PushCollectionBrowseStateSet(ICollectionProduct collectionProduct);

	void PushNonBrowseViewBrowseStateSet();

	void SetDefaultBrowseStateSet();
}
