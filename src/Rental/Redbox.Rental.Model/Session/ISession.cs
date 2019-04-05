using System.Collections.Generic;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Model.Session;

public interface ISession
{
	IBrowseState BrowseState { get; set; }

	IHelpState HelpState { get; set; }

	IPersonalizationSession PersonalizationSession { get; set; }

	IUserInfo UserInfo { get; set; }

	string CustomerProfileNumber { get; }

	List<IReservation> ValidatedReservations { get; set; }
}
