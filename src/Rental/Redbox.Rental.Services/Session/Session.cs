using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class Session : ISession
{
	public static ISession Instance { get; set; }

	public IBrowseState BrowseState { get; set; }

	public Session()
	{
		BrowseState = new BrowseState();
	}
}
