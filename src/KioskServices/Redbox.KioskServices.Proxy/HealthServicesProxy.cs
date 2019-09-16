using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskServices.Proxy;

public class HealthServicesProxy : IHealthServices
{
	internal sealed class Statistic
	{
		public string Name { get; set; }

		public string Value { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
	private struct SYSTEMTIME
	{
		public ushort Year;

		public ushort Month;

		public ushort DayOfWeek;

		public ushort Day;

		public ushort Hour;

		public ushort Minute;

		public ushort Second;

		public ushort Milliseconds;
	}

	internal const int DefaultPingPeriod = 300000;

	internal const string DefaultSmtpServer = "corpsmtp01.rb.local";

	internal const string DefaultRecipients = "";

	private Timer m_pingTimer;

	private readonly object m_syncLock = new object();

	private readonly Queue<Statistic> m_statisticsQueue = new Queue<Statistic>();

	private readonly Queue<KeyValuePair<string, string>> m_propertiesQueue = new Queue<KeyValuePair<string, string>>();

	private int m_working;

	public static HealthServicesProxy Instance => Singleton<HealthServicesProxy>.Instance;

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
		return m_working == 0;
	}

	public void Ping(string storeNumber)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				if (!string.IsNullOrEmpty(storeNumber) && string.Compare(storeNumber, "unconfigured", ignoreCase: true) == 0)
				{
					IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
					if (service != null)
					{
						storeNumber = service.GetObject<string>("NullKioskId") ?? "1";
					}
				}
				if (LiveCall)
				{
					LogHelper.Instance.Log("Live call to remote IHealthServices.Ping, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
					var anon = new
					{
						MessageType = "KioskPing",
						MessageId = Guid.NewGuid(),
						KioskId = int.Parse(storeNumber),
						EngineVersion = service2["EngineVersion"],
						BundleVersion = service2["ProductVersion"],
						Versions = new Dictionary<string, string>(),
						Statistics = new Dictionary<string, string>(),
						Properties = new Dictionary<string, string>()
					};
					anon.Versions["Hal Service"] = service2["HALServiceVersion"] ?? "Not Found";
					anon.Versions["Hal Client"] = service2["HALVersion"] ?? "Not Found";
					anon.Versions["Kiosk Engine"] = service2["EngineVersion"];
					IDeviceServiceClientHelper service3 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
					anon.Versions["Device Service"] = service3?.DeviceServiceVersion?.ToString() ?? "Not Found";
					anon.Versions["Device Service Client"] = service3?.DeviceServiceClientVersion?.ToString() ?? "Not Found";
					IKernelExtensionService service4 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
					if (service4 != null)
					{
						foreach (IKernelExtension extension in service4.Extensions)
						{
							anon.Versions[extension.Name] = extension.Version;
						}
					}
					IResourceBundleService service5 = ServiceLocator.Instance.GetService<IResourceBundleService>();
					if (service5 != null)
					{
						foreach (IBundleSpecifier bundle in service5.Bundles)
						{
							anon.Versions[bundle.Name] = bundle.Version.ToString();
						}
					}
					lock (m_syncLock)
					{
						while (m_statisticsQueue.Count > 0)
						{
							Statistic statistic = m_statisticsQueue.Dequeue();
							anon.Statistics[statistic.Name] = statistic.Value;
						}
						while (m_propertiesQueue.Count > 0)
						{
							KeyValuePair<string, string> keyValuePair = m_propertiesQueue.Dequeue();
							anon.Properties[keyValuePair.Key] = keyValuePair.Value;
						}
					}
					IDictionary<string, object> response = KioskServicesProxy.Instance.ExecuteMessage(anon);
					UpdateOnlineStatus(response);
					if (StoreServicesProxy.Instance.SynchronizeTimeToServer)
					{
						IEngineApplication service6 = ServiceLocator.Instance.GetService<IEngineApplication>();
						if (service6 != null && service6.CanShutDown())
						{
							SynchronizeToServerTime(response);
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IHealthServices.Ping", LogEntryType.Info);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in HealthServicesProxy.Ping.", e);
				IsOnline = false;
			}
		});
	}

	public void SendAlert(string storeNumber, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action = null, bool skipQueue = false)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			Redbox.KioskEngine.ComponentModel.ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					LogHelper.Instance.Log("Enqueue call to remote IHealthServices.SendAlert, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
					if (skipQueue)
					{
						KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "KioskAlert",
							MessageId = Guid.NewGuid(),
							KioskId = int.Parse(storeNumber),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							SubType = subType,
							Detail = message,
							Type = alertType,
							Time = dateTime
						});
					}
					else
					{
						errorList = service2.Enqueue(byte.MaxValue, "KioskAlert", new
						{
							MessageType = "KioskAlert",
							MessageId = Guid.NewGuid(),
							KioskId = int.Parse(storeNumber),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							SubType = subType,
							Detail = message,
							Type = alertType,
							Time = dateTime
						});
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating enqueue to IHealthServices.SendAlert", LogEntryType.Info);
					if (Recipients != null && Recipients != string.Empty)
					{
						MailMessage message2 = new MailMessage("deddie@redbox.com", Recipients, $"Test Alert: {alertType}, Kiosk ID: {storeNumber}, Date Time: {dateTime}", message);
						new SmtpClient(SmtpServer).Send(message2);
					}
					else
					{
						LogHelper.Instance.Log("> recipient list for simulated SendAlert was empty, email was not sent.", LogEntryType.Info);
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in HealthServicesProxy.SendAlert.", e);
			}
			finally
			{
				try
				{
					LogHelper.Instance.Log("Invoking action from HealthServicesProxy.SendAlert");
					action?.Invoke();
					ICallbackService service3 = ServiceLocator.Instance.GetService<ICallbackService>();
					if (callback != null && service3 != null)
					{
						RemoteServiceResult remoteServiceResult = new RemoteServiceResult
						{
							Success = true
						};
						if (errorList != null)
						{
							remoteServiceResult.Errors.AddRange(errorList);
							remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
						}
						service3.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from HealthServicesProxy.SendAlert."
						});
					}
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void SendAuditEvent(string storeNumber, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			Redbox.KioskEngine.ComponentModel.ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					LogHelper.Instance.Log("Enqueue call to remote IHealthServices.SendAuditEvent, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(254, "AuditEvent", new
					{
						MessageType = "AuditEvent",
						MessageId = Guid.NewGuid(),
						KioskId = int.Parse(storeNumber),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						EventType = eventType,
						LogDate = logDate,
						UserName = (string.IsNullOrEmpty(userName) ? "N/A" : userName),
						Message = message,
						Source = source
					});
				}
				else
				{
					LogHelper.Instance.Log("Simulating enqueue to IHealthServices.SendAuditEvent", LogEntryType.Info);
					MailMessage message2 = new MailMessage("deddie@redbox.com", Recipients, $"Test Audit Event: {eventType}, Kiosk ID: {storeNumber}, Date Time: {logDate}, User Name: {userName}, Source: {source}", message);
					new SmtpClient(SmtpServer).Send(message2);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in HealthServicesProxy.SendAuditEvent.", e);
			}
			finally
			{
				try
				{
					ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
					if (callback != null && service2 != null)
					{
						RemoteServiceResult remoteServiceResult = new RemoteServiceResult
						{
							Success = true
						};
						if (errorList != null)
						{
							remoteServiceResult.Errors.AddRange(errorList);
							remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
						}
						service2.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from HealthServicesProxy.SendAlert."
						});
					}
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void SendKioskFunctionCheck(string storeNumber, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			Redbox.KioskEngine.ComponentModel.ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					LogHelper.Instance.Log("Enqueue call to remote IHealthServices.SendKioskFunctionCheck, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(254, "KioskFunctionCheck", new
					{
						MessageType = "KioskFunctionCheck",
						MessageId = Guid.NewGuid(),
						KioskId = int.Parse(storeNumber),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						ReportTime = reportTime,
						UserName = (string.IsNullOrEmpty(userName) ? "N/A" : userName),
						VerticalSlot = verticalSlot,
						InitTest = initTest,
						VendDoor = vendDoor,
						TrackTest = trackTest,
						SnapDecode = snapDecode,
						TouchScreenDriver = touchScreenDriver,
						CameraDriver = cameraDriver
					});
				}
				else
				{
					LogHelper.Instance.Log("Simulating enqueue to IHealthServices.SendKioskFunctionCheck.", LogEntryType.Info);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in HealthServicesProxy.SendKioskFunctionCheck.", e);
			}
			finally
			{
				try
				{
					ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
					if (callback != null && service2 != null)
					{
						RemoteServiceResult remoteServiceResult = new RemoteServiceResult
						{
							Success = true
						};
						if (errorList != null)
						{
							remoteServiceResult.Errors.AddRange(errorList);
							remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
						}
						service2.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from HealthServicesProxy.SendKioskFunctionCheck."
						});
					}
					else if (dotNetCallBack != null)
					{
						dotNetCallBack();
					}
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
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
			m_statisticsQueue.Enqueue(new Statistic
			{
				Name = name,
				Value = value
			});
		}
	}

	public void EnqueueProperty(string name, string value)
	{
		lock (m_syncLock)
		{
			m_propertiesQueue.Enqueue(new KeyValuePair<string, string>(name, value));
		}
	}

	public void UpdateOnlineStatus(IDictionary<string, object> response)
	{
		bool flag = response.ContainsKey("ServerBusy") && (bool)response["ServerBusy"];
		IsServerBusy = flag;
		IsOnline = response.ContainsKey("IsOnline") && (bool)response["IsOnline"];
		RemoteServicesStatus = (flag ? "Busy" : "Normal");
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);

	private HealthServicesProxy()
	{
	}

	private static void SynchronizeToServerTime(IDictionary<string, object> response)
	{
		if (!response.ContainsKey("ServerTime"))
		{
			return;
		}
		DateTime value = response["ServerTime"].ToJson().ToObject<DateTime>();
		value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
		if (!(Math.Abs((DateTime.UtcNow - value).TotalMinutes) <= 1.0))
		{
			DateTime dateTime = value.ToLocalTime();
			LogHelper.Instance.Log("...There's more than a one minute variance between the server and client, setting local time to match server: {0}", dateTime);
			SYSTEMTIME lpSystemTime = new SYSTEMTIME
			{
				Hour = (ushort)dateTime.Hour,
				Minute = (ushort)dateTime.Minute,
				Second = (ushort)dateTime.Second,
				Milliseconds = (ushort)dateTime.Millisecond,
				Month = (ushort)dateTime.Month,
				Day = (ushort)dateTime.Day,
				Year = (ushort)dateTime.Year,
				DayOfWeek = (ushort)dateTime.DayOfWeek
			};
			if (!SetLocalTime(ref lpSystemTime))
			{
				Win32Exception e = new Win32Exception();
				LogHelper.Instance.Log("Unable to set the local time.", e);
			}
		}
	}
}
