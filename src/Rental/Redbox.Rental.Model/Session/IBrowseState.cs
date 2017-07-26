using Redbox.Lua;
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

	bool MatureGameWarningShown { get; set; }

	bool IsShoppingCartFullMessagesOpen { get; set; }

	bool ShoppingCartFullMessagesShown { get; set; }

	int PaddleClickCount { get; set; }

	string BrowseFilterMode { get; set; }

	BrowseViewFilter BrowseMode { get; set; }

	string BrowseSelectedSort { get; set; }

	string SelectedAlphaGroupName { get; set; }

	string BrowseTitleFamily { get; set; }

	TitleFamily TitleFamily { get; set; }

	string BrowseTitleType { get; set; }

	TitleType TitleType { get; set; }

	int PageNumber { get; set; }

	int? BrowseGenre { get; set; }

	Genres? Genre { get; set; }

	IKioskProduct CenterCarouselProduct { get; set; }

	LuaTable CenterCarouselProductLuaTable { get; }

	IKioskProduct BrowseSelectionSelectedProduct { set; }

	LuaTable BrowseSelectionSelectedProductLuaTable { get; set; }

	string ProductDetailShownFromView { get; set; }

	string ItemAddedFromView { get; set; }
}
