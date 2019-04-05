using System;

namespace DeviceService.ComponentModel;

public interface IIUC285Notifier
{
	void SendCardReaderConnectedEvent();

	void SendCardReaderDisconnectedEvent(Exception exception);
}
