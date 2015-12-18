using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

internal class AuthenticationServicesProxy : IAuthenticationServices
{
	private static readonly byte[] m_keyValue = new Guid("{02CAD7C4-E0F5-4b1b-BD04-808B840A61D2}").ToByteArray();

	private static readonly byte[] m_initialVector = new Guid("{AD186FCD-2ACD-41a8-958D-6CA7DAEB1DE5}").ToByteArray();

	private int m_working;

	public static AuthenticationServicesProxy Instance => Singleton<AuthenticationServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "LiveCall", value);
		}
	}

	public string[] Users
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "Users", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "Users", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool EnableLocalAccountCaching
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "EnableLocalAccountCaching", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "EnableLocalAccountCaching", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Authentication Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Authentication Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	public void Authenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformLiveCall(storeNumber, userName, password, useLdapAuthentication, completeCallback);
		}
		else
		{
			SimulateCall(storeNumber, userName, password, useLdapAuthentication, completeCallback);
		}
	}

	private AuthenticationServicesProxy()
	{
	}

	private void PerformLiveCall(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Live call to remote IAuthenticationServices.Authenticate, URL: {0}", KioskServicesProxy.Instance.Url);
				if (!StoreServicesProxy.Instance.IsStandAlone)
				{
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
					{
						MessageType = "Authenticate",
						MessageId = Guid.NewGuid(),
						KioskId = int.Parse(storeNumber),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						UseNTAuthentication = useLdapAuthentication,
						Password = Encrypt(password),
						Username = userName
					});
					HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
					remoteServiceResult.Properties["response"] = AuthenticationResponse.Valid.ToString();
					remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("ServerName"))
					{
						remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
					}
					if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
					{
						ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
						if (arrayList != null && arrayList.Count > 0)
						{
							if (arrayList[0] is Dictionary<string, object> dictionary2)
							{
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthenticationResponse?), (string)dictionary2["Code"]);
								remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : AuthenticationResponse.Error.ToString());
							}
						}
						else
						{
							remoteServiceResult.Properties["response"] = AuthenticationResponse.Error.ToString();
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to authenticate.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = ((!useLdapAuthentication) ? AuthenticationResponse.AuthenticationFailed.ToString() : AuthenticationResponse.Valid.ToString());
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in AuthenticationServicesProxy.Authenticate.", e);
				remoteServiceResult.Properties["response"] = AuthenticationResponse.Error.ToString();
				remoteServiceResult.Properties["is_online"] = false;
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
						Message = "Invoking completeCallback from AuthenticationServicesProxy.Authenticate."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void SimulateCall(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Simulating call to IAuthenticationServices.Authenticate.");
			LogHelper.Instance.LogTo("UserLoginLog", "Authentication Simulated (Live Call is disabled)");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				int num = new Random().Next(1, 5);
				new AutoResetEvent(initialState: false).WaitOne(num * 1000);
				remoteServiceResult.Properties["store_number"] = storeNumber;
				if (SimulateServerBusyError)
				{
					remoteServiceResult.Success = false;
					RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
					{
						Type = InstructionType.WaitAbsolute
					};
					remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
					remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
					remoteServiceResult.Properties["response"] = "Server Busy";
					remoteServiceResult.Properties["is_online"] = false;
				}
				else if (SimulateCommunicationsFailure)
				{
					remoteServiceResult.Success = false;
					RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
					{
						Type = InstructionType.WaitForSuccessfulPing
					};
					remoteServiceResult.InnerInstructions.Add(item);
					remoteServiceResult.Properties["response"] = "Communications Failure";
					remoteServiceResult.Properties["is_online"] = false;
				}
				else
				{
					GetUserResponse(userName, out var response);
					remoteServiceResult.Success = response == AuthenticationResponse.Valid;
					remoteServiceResult.Properties["response"] = response.ToString();
					remoteServiceResult.Properties["user_name"] = userName;
					remoteServiceResult.Properties["password"] = password;
					remoteServiceResult.Properties["use_ldap"] = useLdapAuthentication;
					remoteServiceResult.Properties["is_online"] = true;
				}
				remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in AuthenticationServicesProxy.Authenticate.", e);
				remoteServiceResult.Properties["response"] = AuthenticationResponse.Error.ToString();
				remoteServiceResult.Properties["is_online"] = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult);
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from AuthenticationServicesProxy.Authenticate."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void GetUserResponse(string userName, out AuthenticationResponse response)
	{
		response = AuthenticationResponse.AuthenticationFailed;
		if (Users == null)
		{
			return;
		}
		string[] users = Users;
		for (int i = 0; i < users.Length; i++)
		{
			string[] array = users[i].Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 2 && string.Compare(array[0], userName, ignoreCase: true) == 0)
			{
				response = Enum<AuthenticationResponse>.ParseIgnoringCase(array[1], AuthenticationResponse.Valid);
				break;
			}
		}
	}

	private static string Encrypt(string data)
	{
		byte[] bytes = Encoding.Default.GetBytes(data);
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		return Convert.ToBase64String(memoryStream.ToArray());
	}
}
