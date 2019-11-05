using DeviceService.ComponentModel;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ITrackData
{
	ErrorList Errors { get; set; }

	string FirstSix { get; set; }

	string LastFour { get; set; }

	string FirstName { get; set; }

	string LastName { get; set; }

	Redbox.Core.CardType? CardType { get; set; }

	string ExpiryYear { get; set; }

	string ExpiryMonth { get; set; }

	string CardHashId { get; }

	string ReservationHashId { get; }

	CardSourceType CardSourceType { get; set; }

	FallbackStatusAction FallbackStatusAction { get; set; }

	FallbackType? FallbackReason { get; set; }

	FallbackType? LastFallbackReason { get; set; }

	ResponseStatus? ReadStatus { get; set; }

	bool IsInTechnicalFallback { get; set; }

	bool NextCardReadIsInTechnicalFallback { get; set; }

	bool CardHasChip { get; set; }

	bool HasValidData();

	void LockData();
}
