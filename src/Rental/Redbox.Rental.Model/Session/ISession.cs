namespace Redbox.Rental.Model.Session;

public interface ISession
{
	IBrowseState BrowseState { get; set; }

	IHelpState HelpState { get; set; }
}
