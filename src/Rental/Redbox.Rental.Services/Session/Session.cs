using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class Session : ISession
{
	public static ISession Instance { get; set; }

	public IBrowseState BrowseState { get; set; }

	public IHelpState HelpState { get; set; }

	public IPersonalizationSession PersonalizationSession { get; set; }

	public Session()
	{
		BrowseState = new BrowseState();
		HelpState = new HelpState();
		ServiceLocator.Instance.GetService<ITrackDataService>()?.RefreshCertificate();
	}
}
