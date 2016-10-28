using System;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class BrowseState : IBrowseState
{
	private string _browseSelectedSort;

	public bool CollectionsPopupShown { get; set; }

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
