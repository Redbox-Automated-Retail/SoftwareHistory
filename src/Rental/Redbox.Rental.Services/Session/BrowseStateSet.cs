using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class BrowseStateSet : IBrowseStateSet
{
	private string _browseSelectedSort;

	public bool IsNonBrowseView { get; set; }

	public BrowseViewFilter BrowseMode { get; set; }

	public TitleFamily TitleFamily { get; set; }

	public Genres? Genre { get; set; }

	public int PageNumber { get; set; }

	public TitleType TitleType { get; set; }

	public bool BrowseCollections { get; set; }

	public ICollectionProduct BrowseCollectionProduct { get; set; }

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
			case "Digital Code":
			case "Xbox 360":
			case "Xbox One":
			case "PS3":
			case "PS4":
			case "Wii":
			case "Wii U":
			case "Nintendo Switch":
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

	public List<IBrowseItemModel> BrowseItemModels { get; set; }

	public List<IKioskProduct> BrowseProductsShown { get; set; }

	public List<IBrowseItemModel> TTSBrowseItemModelsShown { get; set; }
}
