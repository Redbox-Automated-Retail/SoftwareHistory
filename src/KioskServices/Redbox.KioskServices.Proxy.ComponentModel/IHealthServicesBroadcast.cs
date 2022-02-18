using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IHealthServicesBroadcast
{
	bool CanSwitch();

	void Ping(Guid messageId, int kioskId, Dictionary<string, string> versions, Dictionary<string, string> statistics, Dictionary<string, string> properties);

	void SendAlert(Guid messageId, int kioskId, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action, bool skipQueue);

	void SendAuditEvent(Guid messageId, int kioskId, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback);

	void SendKioskEvents(Guid messageId, long kioskId, DateTime createdOn, IList<KioskEvent> events);

	void SendKioskFunctionCheck(Guid messageId, int kioskId, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack);
}
