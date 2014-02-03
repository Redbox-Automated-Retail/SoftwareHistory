using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

internal class MarketingOptinServicesProxy : IMarketingOptinServices
{
	private int m_working;

	public static MarketingOptinServicesProxy Instance => Singleton<MarketingOptinServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Marketing Optin Services", "LiveCall", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Marketing Optin Services", "LiveCall", value);
		}
	}

	public ConfirmationStatus ConfirmationStatusCode
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			string value = service.GetValue("Remote Services\\Marketing Optin Services", "ConfirmationStatusCode", "DoNotShow");
			return (ConfirmationStatus)Enum.Parse(typeof(ConfirmationStatus), value, ignoreCase: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Marketing Optin Services", "ConfirmationStatusCode", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Marketing Optin Services", "SimulateCommunicationsFailure", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Marketing Optin Services", "SimulateCommunicationsFailure", value);
		}
	}

	public void SendEmailConfirm(string storeNumber, string emailAddress, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			SendLiveCall(storeNumber, emailAddress, completeCallback);
		}
		else
		{
			SendSimulatedCall(storeNumber, emailAddress, completeCallback);
		}
	}

	public void SendSimulatedCall(string storeNumber, string emailAddress, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Simulated call to KioskServices.MarketingOptinServices.");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (SimulateCommunicationsFailure)
				{
					remoteServiceResult.Properties["has_errors"] = false;
					remoteServiceResult.Success = false;
					RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
					{
						Type = InstructionType.WaitForSuccessfulPing
					};
					remoteServiceResult.InnerInstructions.Add(item);
					remoteServiceResult.Properties["server_name"] = "Simulated";
					remoteServiceResult.Properties["server_busy"] = true;
					remoteServiceResult.Properties["is_online"] = false;
					remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
				}
				else
				{
					remoteServiceResult.Success = true;
					remoteServiceResult.Properties["has_errors"] = false;
					remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatusCode.ToString();
					remoteServiceResult.Properties["server_name"] = "Simulated";
					remoteServiceResult.Properties["server_busy"] = false;
					remoteServiceResult.Properties["is_online"] = true;
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					DateTime now = DateTime.Now;
					dictionary.Add("theDate", now.Ticks);
					dictionary.Add("dateTimeKind", "Utc");
					dictionary.Add("dateString", now.ToString("yyyy-mm-ddThh:mm:ss"));
					remoteServiceResult.Properties["server_time"] = dictionary;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MarketingOptinServicesProxy.SimulatedCall.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["server_name"] = "Catch";
				remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult);
					ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
					service.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MarketingOptinServicesProxy.SimulatedCall."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void SendLiveCall(string storeNumber, string emailAddress, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Live call to MerchantServices.MarketingOptinServices.");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				DateTime now = DateTime.Now;
				dictionary.Add("theDate", now.Ticks);
				dictionary.Add("dateTimeKind", "Unspecified");
				dictionary.Add("dateString", now.ToString("yyyy-mm-ddThh:mm:ss"));
				IDictionary<string, object> dictionary2 = KioskServicesProxy.Instance.ExecuteMessage(new
				{
					MessageType = "EmailConfirm",
					TransactionDate = dictionary,
					CardId = service2.GenerateCardHashId(),
					Email = emailAddress,
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					UseMessageControl = false
				});
				remoteServiceResult.Success = dictionary2.ContainsKey("Success") && (bool)dictionary2["Success"];
				remoteServiceResult.Properties["confirmation_status"] = ((ConfirmationStatus)dictionary2["ConfirmationStatus"]).ToString();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MarketingOptinServicesProxy.PerformLiveCall.", e);
				remoteServiceResult.Properties["IsOnline"] = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service3 = ServiceLocator.Instance.GetService<ICallbackService>();
					service3.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MarketingOptinServicesProxy.PerformLiveCall."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	private MarketingOptinServicesProxy()
	{
	}
}
