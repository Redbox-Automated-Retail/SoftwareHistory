using System;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

public class PersonalizationServicesProxySimulator : IPersonalizationServices
{
	private int m_working;

	public static PersonalizationServicesProxySimulator Instance => Singleton<PersonalizationServicesProxySimulator>.Instance;

	public KioskLoginResponseErrors ResponseCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "KioskLoginResponseError", "Error");
			return (KioskLoginResponseErrors)Enum.Parse(typeof(KioskLoginResponseErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "KioskLoginResponseError", value);
		}
	}

	public UpdateAccountResponseErrors UpdateAccountResponseCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "UpdateAccountResponseError", "Error");
			return (UpdateAccountResponseErrors)Enum.Parse(typeof(UpdateAccountResponseErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "UpdateAccountResponseError", value);
		}
	}

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LiveCall", value);
		}
	}

	public bool SimulateSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "SimulateSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "SimulateSuccess", value);
		}
	}

	public string CustomerProfileNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "CustomerProfileNumber", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "CustomerProfileNumber", value);
		}
	}

	public string FirstName
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "FirstName", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "FirstName", value);
		}
	}

	public string LoginEmail
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoginEmail", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoginEmail", value);
		}
	}

	public string PhoneNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PhoneNumber", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PhoneNumber", value);
		}
	}

	public string PostalCode
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PostalCode", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PostalCode", value);
		}
	}

	public DateTime? BirthDate
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "BirthDate", DateTime.MinValue);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "BirthDate", value);
		}
	}

	public int LoyaltyPointBalance
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoyaltyPointBalance", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoyaltyPointBalance", value);
		}
	}

	public string LoyaltyTier
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoyaltyTier", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoyaltyTier", value);
		}
	}

	public int LoyaltyTierCounter
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoyaltyTierCounter", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoyaltyTierCounter", value);
		}
	}

	public int PointsExpiring
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PointsExpiring", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PointsExpiring", value);
		}
	}

	public DateTime? PointsExpirationDate
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PointsExpirationDate", DateTime.MinValue);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PointsExpirationDate", value);
		}
	}

	private PersonalizationServicesProxySimulator()
	{
	}

	public void Login(int kioskId, string emailAddress, SecureString password, string phoneNumber, SecureString pin, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Simulate call to remote IPersonalization.Login");
				remoteServiceResult.Success = SimulateSuccess;
				remoteServiceResult.Properties["CustomerProfileNumber"] = CustomerProfileNumber;
				remoteServiceResult.Properties["FirstName"] = FirstName;
				remoteServiceResult.Properties["LoginEmail"] = LoginEmail;
				remoteServiceResult.Properties["PhoneNumber"] = PhoneNumber;
				remoteServiceResult.Properties["PostalCode"] = PostalCode;
				remoteServiceResult.Properties["BirthDate"] = BirthDate;
				remoteServiceResult.Properties["LoyaltyPointBalance"] = LoyaltyPointBalance;
				remoteServiceResult.Properties["LoyaltyTier"] = LoyaltyTier;
				remoteServiceResult.Properties["LoyaltyTierCounter"] = LoyaltyTierCounter;
				remoteServiceResult.Properties["PointsExpiring"] = PointsExpiring;
				remoteServiceResult.Properties["PointsExpirationDate"] = PointsExpirationDate;
				remoteServiceResult.Properties["server_name"] = "SimulatedCall";
				if (!SimulateSuccess)
				{
					remoteServiceResult.Properties["response"] = ResponseCode;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.PerformSimulatedLogin.", e);
				remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.Error;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult);
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformSimulatedLogin."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void UpdateAccount(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Simulate call to remote IPersonalization.UpdateAccount");
				remoteServiceResult.Success = SimulateSuccess;
				remoteServiceResult.Properties["MobilePhoneNumber"] = phoneNumber;
				remoteServiceResult.Properties["server_name"] = "SimulatedCall";
				if (!SimulateSuccess)
				{
					remoteServiceResult.Properties["response"] = UpdateAccountResponseCode;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.PerformSimulatedUpdateAccount.", e);
				remoteServiceResult.Properties["response"] = UpdateAccountResponseErrors.Error;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult);
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformSimulatedUpdateAccount."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}
}
