using System.Collections.Generic;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Session;

public interface IBrowseStateSet
{
	bool IsNonBrowseView { get; set; }

	BrowseViewFilter BrowseMode { get; set; }

	BrowseSelectedSort BrowseSelectedSort { get; set; }

	string SelectedAlphaGroupName { get; set; }

	TitleFamily TitleFamily { get; set; }

	TitleType TitleType { get; set; }

	bool BrowseCollections { get; set; }

	ICollectionProduct BrowseCollectionProduct { get; set; }

	int PageNumber { get; set; }

	Genres? Genre { get; set; }

	List<IBrowseItemModel> BrowseItemModels { get; set; }

	List<IKioskProduct> BrowseProductsShown { get; set; }

	List<IBrowseItemModel> TTSBrowseItemModelsShown { get; set; }
}
