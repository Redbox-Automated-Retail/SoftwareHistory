using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class BrowseState : IBrowseState
{
	public bool CollectionsPopupShown { get; set; }

	public int PaddleClickCount { get; set; }
}
