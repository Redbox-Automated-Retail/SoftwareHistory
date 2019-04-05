using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class Session : ISession
{
	public static ISession Instance { get; set; }

	public IBrowseState BrowseState { get; set; }

	public IHelpState HelpState { get; set; }

	public IPersonalizationSession PersonalizationSession { get; set; }

	public IUserInfo UserInfo { get; set; }

	public string CustomerProfileNumber
	{
		get
		{
			string text = null;
			if (PersonalizationSession != null && !string.IsNullOrEmpty(PersonalizationSession.CustomerProfileNumber))
			{
				return PersonalizationSession.CustomerProfileNumber;
			}
			return UserInfo?.CustomerProfileNumber;
		}
	}

	public List<IReservation> ValidatedReservations { get; set; }

	public Session()
	{
		BrowseState = new BrowseState();
		HelpState = new HelpState();
		UserInfo = new UserInfo();
		ServiceLocator.Instance.GetService<ITrackDataService>()?.RefreshCertificate();
		ValidatedReservations = null;
	}
}
