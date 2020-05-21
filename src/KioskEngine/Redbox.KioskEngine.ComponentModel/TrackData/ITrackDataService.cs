using System;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ITrackDataService
{
	bool IsOlderReader { get; }

	bool IsNewReader { get; }

	event CardReadCompleteHandler OnCardReadComplete;

	event CardReadStartedEventHandler OnCardReadStarted;

	void StartCardRead(int timeout, bool isZeroTouchRead, DeviceInputType deviceInputType, Action<IReadCardJob> cardReadStartedAction, Guid sessionId);

	void StopCardRead(CancelReason cancelReason);

	void StopAllSessionCardReads(Guid sessionId, CancelReason cancelReason);

	void ClearCardReadHandlers();
}
