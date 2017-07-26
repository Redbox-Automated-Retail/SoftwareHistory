using System;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.Session;

public class BrowseState : IBrowseState
{
	private string _browseSelectedSort;

	public string ItemAddedFromView { get; set; }

	public string ProductDetailShownFromView { get; set; }

	public IKioskProduct BrowseSelectionSelectedProduct
	{
		set
		{
			BrowseSelectionSelectedProductLuaTable = value?.ToDictionary()?.ToLuaTable();
		}
	}

	public LuaTable BrowseSelectionSelectedProductLuaTable { get; set; }

	public IKioskProduct CenterCarouselProduct { get; set; }

	public LuaTable CenterCarouselProductLuaTable => CenterCarouselProduct?.ToDictionary()?.ToLuaTable();

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

	public BrowseViewFilter BrowseMode { get; set; }

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

	public TitleFamily TitleFamily { get; set; }

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

	public Genres? Genre { get; set; }

	public int PageNumber { get; set; }

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

	public TitleType TitleType { get; set; }

	public string SelectedAlphaGroupName { get; set; }

	public string BrowseSelectedSort
	{
		get
		{
			return _browseSelectedSort;
		}
		set
		{
			switch (value)
			{
			case "alpha":
			case "Newest":
			case "for-sale":
			case "coming-soon":
			case "Blu-ray":
			case "Xbox 360":
			case "Xbox One":
			case "PS3":
			case "PS4":
			case "Wii":
			case "Wii U":
			case "Top 20":
			case "Action":
			case "Comedy":
			case "Drama":
			case "Family":
			case "Horror":
				_browseSelectedSort = value;
				break;
			default:
				throw new ArgumentException("Invalid value for BrowseSelectedSort: {0}", value);
			}
		}
	}
}
