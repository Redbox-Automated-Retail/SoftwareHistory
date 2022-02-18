using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskServices.Proxy;

public class HealthServicesProxy : IHealthServices
{
	private const int DefaultPingPeriod = 300000;

	private const string DefaultSmtpServer = "corpsmtp01.rb.local";

	private const string DefaultRecipients = "";

	private bool m_wasFirstPingSent;

	private Timer m_pingTimer;

	private readonly object m_syncLock = new object();

	private readonly Dictionary<string, string> m_statistics = new Dictionary<string, string>();

	private readonly Dictionary<string, string> m_properties = new Dictionary<string, string>();

	private readonly Collection<IHealthServicesBroadcast> m_listeners;

	public static IHealthServices Instance => Singleton<HealthServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "LiveCall", value);
		}
	}

	public int PingPeriod
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "PingPeriod", 300000);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "PingPeriod", value);
			StopPingTimer();
			StartPingTimer();
		}
	}

	public bool IsOnline
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "IsOnline", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "IsOnline", value);
		}
	}

	public bool IsNotOnline => !IsOnline;

	public bool IsServerBusy
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "IsServerBusy", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "IsServerBusy", value);
		}
	}

	public string SmtpServer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "SmtpServer", "corpsmtp01.rb.local");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "SmtpServer", value);
		}
	}

	public string Recipients
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "Recipients", "");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "Recipients", value);
		}
	}

	public string RemoteServicesStatus
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "RemoteServicesStatus", "Normal");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "RemoteServicesStatus", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Health Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Health Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool CanSwitch()
	{
		bool result = true;
		m_listeners.ForEach(delegate(IHealthServicesBroadcast l)
		{
			result &= l.CanSwitch();
		});
		if (result)
		{
			ResetMinutesInMaintenanceMode();
		}
		return result;
	}

	public void Ping(string storeNumber)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		Dictionary<string, string> versions = CollectVersions();
		Dictionary<string, string> statistics;
		Dictionary<string, string> properties;
		lock (m_syncLock)
		{
			statistics = m_statistics.ShallowCopy();
			m_statistics.Clear();
			properties = m_properties.ShallowCopy();
			m_properties.Clear();
		}
		m_listeners.ForEach(delegate(IHealthServicesBroadcast l)
		{
			l.Ping(messageId, kioskId, versions, statistics, properties);
		});
	}

	public void SendAlert(string storeNumber, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action = null, bool skipQueue = false)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		m_listeners.ForEach(delegate(IHealthServicesBroadcast l)
		{
			l.SendAlert(messageId, kioskId, alertType, subType, message, dateTime, callback, action, skipQueue);
		});
	}

	public void SendAuditEvent(string storeNumber, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		m_listeners.ForEach(delegate(IHealthServicesBroadcast l)
		{
			l.SendAuditEvent(messageId, kioskId, eventType, logDate, userName, message, source, callback);
		});
	}

	public void SendKioskFunctionCheck(string storeNumber, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack)
	{
		Guid messageId = Guid.NewGuid();
		int kioskId = VerifyStoreNumber(storeNumber);
		m_listeners.ForEach(delegate(IHealthServicesBroadcast l)
		{
			l.SendKioskFunctionCheck(messageId, kioskId, userName, reportTime, verticalSlot, initTest, vendDoor, trackTest, snapDecode, touchScreenDriver, cameraDriver, callback, dotNetCallBack);
		});
	}

	public void SendKioskEvents(long kioskId, List<KioskEvent> events)
	{
		Guid messageId = Guid.NewGuid();
		m_listeners.ForEach(delegate(IHealthServicesBroadcast l)
		{
			l.SendKioskEvents(messageId, kioskId, DateTime.Now, events);
		});
	}

	public void StopPingTimer()
	{
		if (m_pingTimer != null)
		{
			m_pingTimer.Dispose();
			m_pingTimer = null;
		}
	}

	public void StartPingTimer()
	{
		if (m_pingTimer != null)
		{
			return;
		}
		m_pingTimer = new Timer(delegate
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			try
			{
				if (m_pingTimer != null)
				{
					m_pingTimer.Change(-1, -1);
				}
				Ping(service.GetValue<string>("Store", "ID"));
			}
			finally
			{
				if (m_pingTimer != null)
				{
					m_pingTimer.Change(PingPeriod, PingPeriod);
				}
			}
		}, null, 0, PingPeriod);
	}

	public void EnqueueStatistic(string name, string value)
	{
		lock (m_syncLock)
		{
			if (m_statistics.ContainsKey(name))
			{
				m_statistics[name] = value;
			}
			else
			{
				m_statistics.Add(name, value);
			}
		}
	}

	public void EnqueueProperty(string name, string value)
	{
		lock (m_syncLock)
		{
			m_properties.Add(name, value);
		}
	}

	public void SendEmail(string subject, string message)
	{
		if (!string.IsNullOrEmpty(Recipients))
		{
			MailMessage message2 = new MailMessage("deddie@redbox.com", Recipients, subject, message);
			new SmtpClient(SmtpServer).Send(message2);
		}
		else
		{
			LogHelper.Instance.Log("> recipient list for simulated SendAlert was empty, email was not sent.", LogEntryType.Info);
		}
	}

	public void UpdateOnlineStatus(IDictionary<string, object> response)
	{
		bool flag = response.ContainsKey("ServerBusy") && (bool)response["ServerBusy"];
		IsServerBusy = flag;
		IsOnline = response.ContainsKey("IsOnline") && (bool)response["IsOnline"];
		RemoteServicesStatus = (flag ? "Busy" : "Normal");
	}

	public void UpdateOnlineStatus(bool isServerBusy, bool isOnline)
	{
		IsServerBusy = IsServerBusy;
		IsOnline = isOnline;
		RemoteServicesStatus = (isServerBusy ? "Busy" : "Normal");
	}

	internal void UpdateFirstPingSent()
	{
		m_wasFirstPingSent = true;
	}

	private HealthServicesProxy()
	{
		m_listeners = new Collection<IHealthServicesBroadcast>
		{
			new KioskServicesHealthListener(this),
			new KioskClientServiceHealthListener(this)
		};
	}

	public void ResetMinutesInMaintenanceMode()
	{
		string lastShownView = ServiceLocator.Instance.GetService<IViewService>().GetLastShownView();
		if (!string.IsNullOrEmpty(lastShownView) && lastShownView.Equals("maintenance_view"))
		{
			EnqueueStatistic("MinutesInMaintenance", "0");
		}
	}

	private Dictionary<string, string> CollectVersions()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!m_wasFirstPingSent)
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			dictionary["Hal Service"] = service["HALServiceVersion"] ?? "Not Found";
			dictionary["Hal Client"] = service["HALVersion"] ?? "Not Found";
			dictionary["Kiosk Engine"] = service["EngineVersion"];
			IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			dictionary["Device Service"] = service2?.DeviceServiceVersion?.ToString() ?? "Not Found";
			dictionary["Device Service Client"] = service2?.DeviceServiceClientVersion?.ToString() ?? "Not Found";
			foreach (IBundleSpecifier bundle in ServiceLocator.Instance.GetService<IResourceBundleService>().Bundles)
			{
				dictionary[bundle.Name] = bundle.Version.ToString();
			}
		}
		return dictionary;
	}

	private int VerifyStoreNumber(string number)
	{
		if (!string.IsNullOrEmpty(number) && string.Compare(number, "unconfigured", ignoreCase: true) == 0)
		{
			IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
			if (service != null)
			{
				number = service.GetObject<string>("NullKioskId") ?? "1";
			}
		}
		return int.Parse(number);
	}
}
