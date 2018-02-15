using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Model.Session;

public interface ISession
{
	IBrowseState BrowseState { get; set; }

	IHelpState HelpState { get; set; }

	IPersonalizationSession PersonalizationSession { get; set; }
}
