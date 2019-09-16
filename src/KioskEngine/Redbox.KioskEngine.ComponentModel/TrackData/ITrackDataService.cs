using System;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public interface ITrackDataService
{
	event CardReadCompleteHandler OnCardReadComplete;

	event CardReadStartedEventHandler OnCardReadStarted;

	void StartCardRead(int timeout, DeviceInputType deviceInputType, Action<IReadCardJob> cardReadStartedAction);

	void StopCardRead();

	void ClearCardReadHandlers();
}
