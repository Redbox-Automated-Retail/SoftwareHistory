using System;

namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IHealthServices
{
	bool IsOnline { get; set; }

	bool LiveCall { get; set; }

	int PingPeriod { get; set; }

	string SmtpServer { get; set; }

	string Recipients { get; set; }

	void Ping(string storeNumber);

	void SendAlert(string storeNumber, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback);

	void SendAuditEvent(string storeNumber, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback);

	void StartPingTimer();

	void StopPingTimer();

	void EnqueueStatistic(string name, string value);

	bool CanSwitch();
}
