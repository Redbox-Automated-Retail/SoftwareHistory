using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Session;

public interface IBrowseState
{
	bool CollectionsPopupShown { get; set; }

	int PaddleClickCount { get; set; }

	string BrowseFilterMode { get; set; }

	BrowseViewFilter BrowseMode { get; set; }

	string BrowseSelectedSort { get; set; }
}
