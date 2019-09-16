using System;
using DeviceService.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class CardReadAttempt : ICardReadAttempt
{
	public Guid CardReadJobCommandRequestId { get; set; }

	public DateTime? StartDateTime { get; set; }

	public DateTime? EndDateTime { get; set; }

	public DeviceInputType DeviceInputType { get; set; }

	public TimeSpan Duration
	{
		get
		{
			TimeSpan result = TimeSpan.FromSeconds(0.0);
			if (StartDateTime.HasValue && EndDateTime.HasValue)
			{
				result = StartDateTime.Value - EndDateTime.Value;
			}
			return result;
		}
	}

	public ITrackData TrackData { get; set; }

	public CardReadAttemptStatus CardReadAttemptStatus { get; set; }

	public bool AttemptedCancel { get; set; }

	public IAuthorizeResult AuthorizeResult { get; set; }
}
