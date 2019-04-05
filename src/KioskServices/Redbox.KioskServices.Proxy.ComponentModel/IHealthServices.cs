using System;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IHealthServices
{
	bool IsOnline { get; set; }

	bool LiveCall { get; set; }

	int PingPeriod { get; set; }

	string SmtpServer { get; set; }

	string Recipients { get; set; }

	void Ping(string storeNumber);

	void SendAlert(string storeNumber, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action = null, bool skipQueue = false);

	void SendAuditEvent(string storeNumber, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback);

	void SendKioskFunctionCheck(string storeNumber, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack);

	void StartPingTimer();

	void StopPingTimer();

	void EnqueueStatistic(string name, string value);

	void EnqueueProperty(string name, string value);

	bool CanSwitch();
}
