using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.KioskServices.Proxy;

internal class KioskServicesHealthListener : BaseHealthServicesBroadcast
{
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

	public override void Ping(Guid messageId, int kioskId, Dictionary<string, string> versions, Dictionary<string, string> statistics, Dictionary<string, string> properties)
	{
		IKioskHealthConfiguration service = ServiceLocator.Instance.GetService<IKioskHealthConfiguration>();
		if (service != null && !service.KioskServicesPingEnabled)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				if (base.Caller.LiveCall)
				{
					LogHelper.Instance.Log("Live call for KioskServicesHealthListener.Ping, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
					var message = new
					{
						MessageType = "KioskPing",
						MessageId = messageId,
						KioskId = kioskId,
						EngineVersion = service2["EngineVersion"],
						BundleVersion = service2["ProductVersion"],
						Versions = versions,
						Statistics = statistics,
						Properties = properties
					};
					IDictionary<string, object> response = KioskServicesProxy.Instance.ExecuteMessage(message);
					base.Caller.UpdateFirstPingSent();
					base.Caller.UpdateOnlineStatus(response);
					SynchronizeToServerTime(response);
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to KioskServicesHealthListener.Ping", LogEntryType.Info);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in KioskServicesHealthListener.Ping.", e);
			}
		});
	}

	public override void SendAlert(Guid messageId, int kioskId, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action, bool skipQueue)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskAlertsToKioskServices)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				ErrorList errorList = null;
				try
				{
					WorkingIncrement();
					if (base.Caller.LiveCall)
					{
						LogHelper.Instance.Log("Enqueue call to remote KioskServicesHealthListener.SendAlert, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						var message2 = new
						{
							MessageType = "KioskAlert",
							MessageId = messageId,
							KioskId = kioskId,
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							SubType = subType,
							Detail = message,
							Type = alertType,
							Time = dateTime
						};
						if (skipQueue)
						{
							KioskServicesProxy.Instance.ExecuteMessage(message2);
						}
						else
						{
							errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(byte.MaxValue, "KioskAlert", message2);
						}
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to KioskServicesHealthListener.SendAlert", LogEntryType.Info);
						base.Caller.SendEmail($"Test Alert: {alertType}, Kiosk ID: {kioskId}, Date Time: {dateTime}", message);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskServicesHealthListener.SendAlert.", e);
				}
				finally
				{
					try
					{
						LogHelper.Instance.Log("Invoking action from KioskServicesHealthListener.SendAlert");
						action?.Invoke();
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
								Message = "Invoking callback from KioskServicesHealthListener.SendAlert."
							});
						}
					}
					finally
					{
						WorkingDecrement();
					}
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskServicesHealthListener.SendAlert - bypassing kiosk alert to on premise due to configuration flag being off");
		}
	}

	public override void SendAuditEvent(Guid messageId, int kioskId, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendAuditEventsToKioskServices)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				WorkingIncrement();
				ErrorList errorList = null;
				try
				{
					if (base.Caller.LiveCall)
					{
						LogHelper.Instance.Log("Enqueue call to remote IHealthServices.SendAuditEvent endpoints");
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(254, "AuditEvent", new
						{
							MessageType = "AuditEvent",
							MessageId = messageId,
							KioskId = kioskId,
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							EventType = eventType,
							LogDate = logDate,
							UserName = userName.CheckNotAvailable(),
							Message = message,
							Source = source
						});
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to IHealthServices.SendAuditEvent", LogEntryType.Info);
						base.Caller.SendEmail($"Test Audit Event: {eventType}, Kiosk ID: {kioskId}, Date Time: {logDate}, User Name: {userName}, Source: {source}", message);
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
						if (callback != null)
						{
							ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service2 != null)
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
					}
					finally
					{
						WorkingDecrement();
					}
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskServicesHealthListener.SendAuditEvent - bypassing kiosk audit event to on premise due to configuration flag being off");
		}
	}

	public override void SendKioskEvents(Guid messageId, long kioskId, DateTime notused, IList<KioskEvent> events)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskEventsToKioskServices)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				WorkingIncrement();
				try
				{
					KioskEventsPayload kioskEventsPayload = new KioskEventsPayload
					{
						MessageId = messageId,
						MessageType = "KioskEvents",
						KioskId = kioskId
					};
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					kioskEventsPayload.EngineVersion = service["EngineVersion"];
					kioskEventsPayload.BundleVersion = service["ProductVersion"];
					kioskEventsPayload.KioskEventsData = JsonConvert.SerializeObject(events).CompressString();
					LogHelper.Instance.Log("Enqueue call to remote KioskServicesHealthListener.SendKioskEvents, URL: {0}", KioskServicesProxy.Instance.Url);
					ServiceLocator.Instance.GetService<IQueueService>().EnqueueKioskEvent(kioskEventsPayload);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceHealthListener.SendKioskEvents.", e);
				}
				finally
				{
					WorkingDecrement();
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskServicesHealthListener.SendKioskEvents - bypassing kiosk events to on premise due to configuration flag being off");
		}
	}

	public override void SendKioskFunctionCheck(Guid messageId, int kioskId, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskFunctionChecksToKioskServices)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				ErrorList errorList = null;
				try
				{
					WorkingIncrement();
					if (base.Caller.LiveCall)
					{
						LogHelper.Instance.Log("Enqueue call to KioskServicesHealthListener.SendKioskFunctionCheck");
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						KioskFunctionCheckPayload message = new KioskFunctionCheckPayload
						{
							MessageId = messageId,
							KioskId = kioskId,
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							ReportTime = reportTime,
							UserName = userName.CheckNotAvailable(),
							VerticalSlot = verticalSlot,
							InitTest = initTest,
							VendDoor = vendDoor,
							TrackTest = trackTest,
							SnapDecode = snapDecode,
							TouchScreenDriver = touchScreenDriver,
							CameraDriver = cameraDriver
						};
						errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(254, "KioskFunctionCheck", message);
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to KioskServicesHealthListener.SendKioskFunctionCheck", LogEntryType.Info);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskServicesHealthListener.SendKioskFunctionCheck!", e);
				}
				finally
				{
					try
					{
						if (callback != null)
						{
							ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service2 != null)
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
									Message = "Invoking callback from KioskServicesHealthListener.SendKioskFunctionCheck."
								});
							}
							else
							{
								dotNetCallBack?.Invoke();
							}
						}
					}
					finally
					{
						WorkingDecrement();
					}
				}
			});
		}
		else
		{
			LogHelper.Instance.Log("KioskServicesHealthListener.SendKioskFunctionCheck - bypassing kiosk function check to on premise due to configuration flag being off");
		}
	}

	internal KioskServicesHealthListener(HealthServicesProxy caller)
		: base(caller)
	{
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);

	private static bool IsKioskTimeOutOfSync(IDictionary<string, object> response)
	{
		bool result = false;
		if (response != null && response.ContainsKey("ServerTime"))
		{
			try
			{
				DateTime value = response["ServerTime"].ToJson().ToObject<DateTime>();
				value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
				if (Math.Abs((DateTime.UtcNow - value).TotalMinutes) > 1.0)
				{
					result = true;
				}
			}
			catch (Exception arg)
			{
				LogHelper.Instance.LogError("E001", $"Exception in HealthServicesProxy.IsKioskTimeOutOfSync.  {arg}");
			}
		}
		return result;
	}

	private static void SynchronizeToServerTime(IDictionary<string, object> response)
	{
		if (!response.ContainsKey("ServerTime") || !ServiceLocator.Instance.GetService<IStoreServices>().SynchronizeTimeToServer)
		{
			return;
		}
		if (IsKioskTimeOutOfSync(response))
		{
			IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
			if (service == null || !service.CanShutDown())
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
		else
		{
			LogHelper.Instance.Log("Kiosk time is in sync with server time.");
		}
	}
}
