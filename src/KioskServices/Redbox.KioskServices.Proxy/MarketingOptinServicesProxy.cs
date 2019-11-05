using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Session;

namespace Redbox.KioskServices.Proxy;

public class MarketingOptinServicesProxy : IMarketingOptinServices
{
	private int m_working;

	public static MarketingOptinServicesProxy Instance => Singleton<MarketingOptinServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "LiveCall", value);
		}
	}

	public ConfirmationStatus ConfirmationStatusCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "ConfirmationStatusCode", "DoNotShow");
			return (ConfirmationStatus)Enum.Parse(typeof(ConfirmationStatus), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "ConfirmationStatusCode", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "SimulateCommunicationsFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "SimulateCommunicationsFailure", value);
		}
	}

	public string CustomerProfileNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "CustomerProfileNumber", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "CustomerProfileNumber", value);
		}
	}

	public string MobilePhoneNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "MobilePhoneNumber", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "MobilePhoneNumber", value);
		}
	}

	public bool PromptForPerks
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "PromptForPerks", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "PromptForPerks", value);
		}
	}

	public bool PromptForEarlyId
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "PromptForEarlyId", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "PromptForEarlyId", value);
		}
	}

	public bool TextClubEnabled
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Marketing Optin Services", "TextClubEnabled", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "TextClubEnabled", value);
		}
	}

	public bool? EmailVerified
	{
		get
		{
			bool? result = null;
			if (bool.TryParse(ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Remote Services\\Marketing Optin Services", "EmailVerified", null), out var result2))
			{
				result = result2;
			}
			return result;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Marketing Optin Services", "EmailVerified", value.ToString());
		}
	}

	public void SendEmailConfirm(string storeNumber, string emailAddress, string accountNumber, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			SendLiveCall(storeNumber, emailAddress, accountNumber, completeCallback);
		}
		else
		{
			SendSimulatedCall(storeNumber, emailAddress, accountNumber, completeCallback);
		}
	}

	public void SendSimulatedCall(string storeNumber, string emailAddress, string accountNumber, RemoteServiceCallback completeCallback)
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
					remoteServiceResult.Properties["email_verified"] = null;
				}
				else
				{
					remoteServiceResult.Success = true;
					remoteServiceResult.Properties["has_errors"] = false;
					remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatusCode.ToString();
					remoteServiceResult.Properties["customer_profile_number"] = CustomerProfileNumber;
					remoteServiceResult.Properties["mobile_phone_number"] = MobilePhoneNumber;
					remoteServiceResult.Properties["prompt_for_perks"] = PromptForPerks;
					remoteServiceResult.Properties["prompt_for_early_id"] = PromptForEarlyId;
					remoteServiceResult.Properties["text_club_enabled"] = TextClubEnabled;
					remoteServiceResult.Properties["email_verified"] = EmailVerified;
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
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
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

	public void SendLiveCall(string storeNumber, string emailAddress, string accountNumber, RemoteServiceCallback completeCallback)
	{
		ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
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
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				DateTime now = DateTime.Now;
				dictionary.Add("theDate", now.Ticks);
				dictionary.Add("dateTimeKind", "Unspecified");
				dictionary.Add("dateString", now.ToString("yyyy-mm-ddThh:mm:ss"));
				IDictionary<string, object> dictionary2 = KioskServicesProxy.Instance.ExecuteMessage(new
				{
					MessageType = "EmailConfirm",
					TransactionDate = dictionary,
					Email = emailAddress,
					AccountNumber = accountNumber,
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					UseMessageControl = false
				});
				remoteServiceResult.Success = dictionary2.ContainsKey("Success") && (bool)dictionary2["Success"];
				remoteServiceResult.Properties["confirmation_status"] = ((ConfirmationStatus)dictionary2["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString();
				remoteServiceResult.Properties["customer_profile_number"] = (dictionary2.ContainsKey("CustomerProfileNumber") ? ((string)dictionary2["CustomerProfileNumber"]) : string.Empty);
				remoteServiceResult.Properties["mobile_phone_number"] = (dictionary2.ContainsKey("MobilePhoneNumber") ? ((string)dictionary2["MobilePhoneNumber"]) : string.Empty);
				remoteServiceResult.Properties["prompt_for_perks"] = dictionary2.ContainsKey("PromptForPerks") && (bool)dictionary2["PromptForPerks"];
				remoteServiceResult.Properties["prompt_for_early_id"] = dictionary2.ContainsKey("PromptForEarlyId") && (bool)dictionary2["PromptForEarlyId"];
				remoteServiceResult.Properties["text_club_enabled"] = dictionary2.ContainsKey("TextClubEnabled") && (bool)dictionary2["TextClubEnabled"];
				remoteServiceResult.Properties["email_verified"] = (dictionary2.ContainsKey("EmailVerified") ? ((bool?)dictionary2["EmailVerified"]) : ((bool?)null));
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
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
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
