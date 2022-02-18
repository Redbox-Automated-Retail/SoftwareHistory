using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.Proxy;

internal abstract class BaseHealthServicesBroadcast : IHealthServicesBroadcast
{
	private int m_working;

	protected HealthServicesProxy Caller { get; private set; }

	protected string KioskClientServiceBaseUrl => ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceBaseUrl;

	public abstract void Ping(Guid messageId, int kioskId, Dictionary<string, string> versions, Dictionary<string, string> statistics, Dictionary<string, string> properties);

	public abstract void SendAlert(Guid messageId, int kioskId, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action, bool skipQueue);

	public abstract void SendAuditEvent(Guid messageId, int kioskId, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback);

	public abstract void SendKioskEvents(Guid messageId, long kioskId, DateTime createdOn, IList<KioskEvent> events);

	public abstract void SendKioskFunctionCheck(Guid messageId, int kioskId, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack);

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	protected BaseHealthServicesBroadcast(HealthServicesProxy caller)
	{
		Caller = caller;
	}

	protected void WorkingDecrement()
	{
		Interlocked.Decrement(ref m_working);
	}

	protected void WorkingIncrement()
	{
		Interlocked.Increment(ref m_working);
	}
}
