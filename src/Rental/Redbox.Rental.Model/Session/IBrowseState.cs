namespace Redbox.Rental.Model.Session;

public interface IBrowseState
{
	bool CollectionsPopupShown { get; set; }

	int PaddleClickCount { get; set; }
}
