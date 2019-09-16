using System.Collections.Generic;
using DeviceService.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Reservation;

namespace Redbox.Rental.Model.Session;

public interface ISession
{
	IBrowseState BrowseState { get; set; }

	IHelpState HelpState { get; set; }

	IPersonalizationSession PersonalizationSession { get; set; }

	IUserInfo UserInfo { get; set; }

	ITrackData TrackData { get; set; }

	string CustomerProfileNumber { get; }

	List<IReservation> ValidatedReservations { get; set; }

	ICardReadAttemptCollection CardReadAttemptCollection { get; }

	bool LastCardReadAuthorizeSuccess { get; }

	bool LastCardReadAttemptSuccess { get; }

	int CardReadTechnicalFallbackCount { get; set; }

	bool HasReceivedUserInteraction { get; set; }

	string SessionStartTriggerAction { get; set; }

	bool WaitingForCardReaderConnection { get; set; }

	IPreauthorizeResult PreauthorizeResult { get; set; }

	IPromoOfferState PromoOfferState { get; set; }

	FallbackType? LastFallbackType { get; set; }

	void ClearTrackData();
}
