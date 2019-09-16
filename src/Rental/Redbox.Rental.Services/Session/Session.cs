using System.Collections.Generic;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;
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

	public ITrackData TrackData { get; set; }

	public IUserInfo UserInfo { get; set; }

	public IPromoOfferState PromoOfferState { get; set; }

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

	public ICardReadAttemptCollection CardReadAttemptCollection { get; }

	public bool LastCardReadAuthorizeSuccess
	{
		get
		{
			bool result = false;
			ICardReadAttempt cardReadAttempt = CardReadAttemptCollection?.GetMostRecent();
			if (cardReadAttempt != null)
			{
				result = cardReadAttempt?.AuthorizeResult?.Success ?? false;
			}
			return result;
		}
	}

	public bool LastCardReadAttemptSuccess
	{
		get
		{
			bool result = false;
			ICardReadAttempt cardReadAttempt = CardReadAttemptCollection?.GetMostRecent();
			if (cardReadAttempt != null)
			{
				result = cardReadAttempt.CardReadAttemptStatus == CardReadAttemptStatus.Success;
			}
			return result;
		}
	}

	public int CardReadTechnicalFallbackCount { get; set; }

	public FallbackType? LastFallbackType { get; set; }

	public bool HasReceivedUserInteraction { get; set; }

	public string SessionStartTriggerAction { get; set; }

	public bool WaitingForCardReaderConnection { get; set; }

	public IPreauthorizeResult PreauthorizeResult { get; set; }

	public Session()
	{
		BrowseState = new BrowseState();
		HelpState = new HelpState();
		UserInfo = new UserInfo();
		ServiceLocator.Instance.GetService<ITrackDataEncryptionService>()?.RefreshCertificate();
		ValidatedReservations = null;
		CardReadAttemptCollection = new CardReadAttemptCollection();
	}

	public void ClearTrackData()
	{
		LogHelper.Instance.Log("Clearing track data for session");
		TrackData = null;
		if (CardReadAttemptCollection == null)
		{
			return;
		}
		foreach (ICardReadAttempt item in CardReadAttemptCollection)
		{
			item.TrackData = null;
		}
	}
}
