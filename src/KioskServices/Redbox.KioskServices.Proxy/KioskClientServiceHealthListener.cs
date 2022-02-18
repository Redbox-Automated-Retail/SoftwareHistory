using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.KioskServices.Proxy;

internal class KioskClientServiceHealthListener : BaseHealthServicesBroadcast
{
	public override void Ping(Guid messageId, int kioskId, Dictionary<string, string> versions, Dictionary<string, string> statistics, Dictionary<string, string> properties)
	{
		ThreadPool.QueueUserWorkItem(async delegate
		{
			WorkingIncrement();
			try
			{
				if (base.Caller.LiveCall)
				{
					LogHelper.Instance.Log($"Live call for KioskClientServiceHealthListener.Ping, original message id: {messageId}");
					await Task.WhenAll(SendKioskVersions(kioskId, versions), SendKioskStatistics(kioskId, statistics), SendKioskProperties(kioskId, properties));
					base.Caller.UpdateFirstPingSent();
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to KioskClientServiceHealthListener.Ping", LogEntryType.Info);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceHealthListener.Ping.", e);
			}
			finally
			{
				WorkingDecrement();
			}
		});
	}

	public override void SendAlert(Guid messageId, int kioskId, string alertType, string subType, string message, DateTime dateTime, RemoteServiceCallback callback, Action action, bool skipQueue)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskAlertsToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				ErrorList errors = null;
				try
				{
					WorkingIncrement();
					if (base.Caller.LiveCall)
					{
						LogHelper.Instance.Log($"Enqueue call to KioskClientService.SendAlert, message id: {messageId}");
						BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskAlert(messageId, kioskId, alertType.ToAlertTypeNumeric(), subType, message, dateTime);
						if (baseResponse.Success)
						{
							LogHelper.Instance.Log("Call to KioskClientService.SendAlert successful");
						}
						else
						{
							LogHelper.Instance.Log($"Call to KioskClientService.SendAlert failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
							errors = baseResponse.Errors.ToKEComponentModelErrors();
						}
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to KioskClientService.SendAlert", LogEntryType.Info);
						base.Caller.SendEmail($"Test Alert: {alertType}, Kiosk ID: {kioskId}, Date Time: {dateTime}", message);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServicesHealthListener.SendAlert.", e);
				}
				finally
				{
					try
					{
						LogHelper.Instance.Log("Invoking action from KioskClientServicesHealthListener.SendAlert");
						action?.Invoke();
						if (callback != null)
						{
							ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service != null)
							{
								RemoteServiceResult remoteServiceResult = new RemoteServiceResult
								{
									Success = true
								};
								if (errors != null)
								{
									foreach (Redbox.KioskEngine.ComponentModel.Error item in errors)
									{
										remoteServiceResult.AddError(item.Code, item.Description, item.Details);
									}
									remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
								}
								service.EnqueueCallback(new RemoteServiceCallbackEntry
								{
									Callback = callback,
									Result = remoteServiceResult,
									Message = "Invoking callback from KioskServicesHealthListener.SendAlert."
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
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendAlert - bypassing kiosk alert to cloud due to configuration flag being off");
		}
	}

	public override void SendAuditEvent(Guid messageId, int kioskId, string eventType, DateTime logDate, string userName, string message, string source, RemoteServiceCallback callback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendAuditEventsToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				WorkingIncrement();
				ErrorList errors = null;
				try
				{
					if (base.Caller.LiveCall)
					{
						LogHelper.Instance.Log("Enqueue call to remote KioskClientServiceHealthListener.SendAuditEvent endpoint");
						BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.AuditEvent(messageId, kioskId, eventType, logDate, userName, message, source);
						if (baseResponse.Success)
						{
							LogHelper.Instance.Log("Call to KioskClientService.AuditEvent successful");
						}
						else
						{
							LogHelper.Instance.Log($"Call to KioskClientService.AuditEvent failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
							errors = baseResponse.Errors.ToKEComponentModelErrors();
						}
					}
					else
					{
						LogHelper.Instance.Log("Simulating enqueue to KioskClientServiceHealthListener.SendAuditEvent", LogEntryType.Info);
						base.Caller.SendEmail($"Test Audit Event: {eventType}, Kiosk ID: {kioskId}, Date Time: {logDate}, User Name: {userName}, Source: {source}", message);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceHealthListener.SendAuditEvent.", e);
				}
				finally
				{
					try
					{
						if (callback != null)
						{
							ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service != null)
							{
								RemoteServiceResult remoteServiceResult = new RemoteServiceResult
								{
									Success = true
								};
								if (errors != null)
								{
									remoteServiceResult.Errors.AddRange(errors);
									remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
								}
								service.EnqueueCallback(new RemoteServiceCallbackEntry
								{
									Callback = callback,
									Result = remoteServiceResult,
									Message = "Invoking callback from KioskClientServiceHealthListener.SendAuditEvent."
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
			LogHelper.Instance.Log("KioskClientServiceHealthListener.SendAuditEvent - bypassing kiosk audit event to cloud due to configuration flag being off");
		}
	}

	public override void SendKioskEvents(Guid messageId, long kioskId, DateTime createdOn, IList<KioskEvent> events)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskEventsToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				WorkingIncrement();
				try
				{
					LogHelper.Instance.Log("Enqueue call to remote KioskClientServiceHealthListener.SendKioskEvents");
					BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskEvents(messageId, kioskId, createdOn, events);
					if (baseResponse.Success)
					{
						LogHelper.Instance.Log("Call to KioskClientService.KioskEvents successful");
					}
					else
					{
						LogHelper.Instance.Log($"Call to KioskClientService.KioskEvents failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
					}
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
			LogHelper.Instance.Log("KioskClientServiceHealthListener.SendKioskEvents - bypassing kiosk events to cloud due to configuration flag being off");
		}
	}

	public override void SendKioskFunctionCheck(Guid messageId, int kioskId, string userName, DateTime reportTime, string verticalSlot, string initTest, string vendDoor, string trackTest, string snapDecode, string touchScreenDriver, string cameraDriver, RemoteServiceCallback callback, Action dotNetCallBack)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskFunctionChecksToKioskDataService)
		{
			ThreadPool.QueueUserWorkItem(async delegate
			{
				ErrorList errors = null;
				try
				{
					WorkingIncrement();
					if (base.Caller.LiveCall)
					{
						LogHelper.Instance.Log("Enqueue call to KioskServicesHealthListener.SendKioskFunctionCheck");
						BaseResponse baseResponse = await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskFunctionCheck(messageId, kioskId, userName, reportTime, verticalSlot, initTest, vendDoor, trackTest, snapDecode, touchScreenDriver, cameraDriver);
						if (baseResponse.Success)
						{
							LogHelper.Instance.Log("Call to KioskClientService.KioskFunctionCheck successful");
						}
						else
						{
							LogHelper.Instance.Log($"Call to KioskClientService.KioskFunctionCheck failed, errors: {JsonConvert.SerializeObject(baseResponse.Errors)}");
							errors = baseResponse.Errors.ToKEComponentModelErrors();
						}
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
							ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
							if (service != null)
							{
								RemoteServiceResult remoteServiceResult = new RemoteServiceResult
								{
									Success = true
								};
								if (errors != null)
								{
									remoteServiceResult.Errors.AddRange(errors);
									remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
								}
								service.EnqueueCallback(new RemoteServiceCallbackEntry
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
			LogHelper.Instance.Log("KioskClientServiceHealthListener.SendKioskFunctionCheck - bypassing kiosk events to cloud due to configuration flag being off");
		}
	}

	internal KioskClientServiceHealthListener(HealthServicesProxy caller)
		: base(caller)
	{
	}

	private async Task SendKioskProperties(int kioskId, IDictionary<string, string> properties)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskPropertiesToKioskDataService)
		{
			if (properties.Any())
			{
				await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskProperties(kioskId, properties);
			}
			else
			{
				LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskProperties() - bypassing live call due to no properties information to send up");
			}
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskProperties() - bypassing live call due to configuration flag being off");
		}
	}

	private async Task SendKioskStatistics(int kioskId, IDictionary<string, string> statistics)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskStatisticsToKioskDataService)
		{
			if (statistics.Any())
			{
				await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskStatistics(kioskId, statistics);
			}
			else
			{
				LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskStatistics() - bypassing live call due to no statistics information to send up");
			}
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskStatistics() - bypassing live call due to configuration flag being off");
		}
	}

	private async Task SendKioskVersions(int kioskId, IDictionary<string, string> versions)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskVersionsToKioskDataService)
		{
			if (versions.Any())
			{
				await ServiceLocator.Instance.GetService<IKioskClientService>().Data.KioskVersions(kioskId, versions);
			}
			else
			{
				LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskVersions() - bypassing live call due to no version information to send up");
			}
		}
		else
		{
			LogHelper.Instance.Log("KioskClientServicesHealthListener.SendKioskVersions() - bypassing live call due to configuration flag being off");
		}
	}
}
