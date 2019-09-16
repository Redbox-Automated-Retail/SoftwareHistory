using System;

namespace DeviceService.ComponentModel;

public interface IIUC285Notifier
{
	void SendCardReaderConnectedEvent();

	void SendDeviceServiceCanShutDownEvent();

	void SendDeviceServiceShutDownStartingEvent(ShutDownReason shutDownReason);

	void SendCardReaderDisconnectedEvent(Exception exception);

	void SetCommandQueueState(bool commandQueuePaused);
}
