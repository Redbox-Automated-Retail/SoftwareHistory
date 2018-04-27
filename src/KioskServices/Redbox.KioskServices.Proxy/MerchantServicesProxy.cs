using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.KioskServices.Proxy.Request;

namespace Redbox.KioskServices.Proxy;

public class MerchantServicesProxy : IMerchantServices
{
	private int m_working;

	public static MerchantServicesProxy Instance => Singleton<MerchantServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "LiveCall", value);
		}
	}

	public bool ResvAuthLiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Auth Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Auth Services", "LiveCall", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool ResvAuthSimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Auth Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Auth Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool ResvAuthSimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Auth Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Auth Services", "SimulateCommunicationFailure", value);
		}
	}

	public AuthorizeResponse AuthorizeResponse
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "AuthorizeResponse", AuthorizeResponse.Approved);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "AuthorizeResponse", value);
		}
	}

	public AuthorizeResponse ResvAuthAuthorizeResponse
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Auth Services", "AuthorizeResponse", AuthorizeResponse.Approved);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Auth Services", "AuthorizeResponse", value);
		}
	}

	public bool EnableOfflineBadCardList
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "EnableOfflineBadCardList", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "EnableOfflineBadCardList", value);
		}
	}

	public bool HasCustomerProfile
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "HasCustomerProfile", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "HasCustomerProfile", value);
		}
	}

	public bool IsRBI
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "IsRBI", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "IsRBI", value);
		}
	}

	public string AccountNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "AccountNumber", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "AccountNumber", value);
		}
	}

	public string TransactionId
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "TransactionId", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "TransactionId", value);
		}
	}

	public string CPAccountNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "CPAccountNumber", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "CPAccountNumber", value);
		}
	}

	public string CPFirstName
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "CPFirstName", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "CPFirstName", value);
		}
	}

	public string CPLastName
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "CPLastName", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "CPLastName", value);
		}
	}

	public string EmailAddress
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "EmailAddress", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "EmailAddress", value);
		}
	}

	public ConfirmationStatus ConfirmationStatus
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "ConfirmationStatus", ConfirmationStatus.DoNotShow);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "ConfirmationStatus", value);
		}
	}

	public string[] UserOffers
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "UserOffers", new List<string>().ToArray());
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "UserOffers", value);
		}
	}

	public bool GCLiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "GCLiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "GCLiveCall", value);
		}
	}

	public bool GCSimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "GCSimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "GCSimulateCommunicationFailure", value);
		}
	}

	public bool GCSimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "GCSimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "GCSimulateServerBusyFailure", value);
		}
	}

	public GCAuthorizeResponse GCAuthorizeResponse
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "GCAuthorizeResponse", GCAuthorizeResponse.Approved);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "GCAuthorizeResponse", value);
		}
	}

	public bool Exists
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "Exists", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "Exists", value);
		}
	}

	public GiftCardStatus GCStatusType
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "GCStatusType", GiftCardStatus.None);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "GCStatusType", value);
		}
	}

	public string Balance
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "Balance", "0.00");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "Balance", value);
		}
	}

	public int MaxDiscs
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "MaxDiscs", 2);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "MaxDiscs", value);
		}
	}

	public int MaxGames
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "MaxGames", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "MaxGames", value);
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reconcile(string sessionId, string customerProfileNumber, RemoteServiceCallback callback)
	{
		IMacroService macroService = ServiceLocator.Instance.GetService<IMacroService>();
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IShoppingSession session = service.GetSession(sessionId);
		if (session == null)
		{
			LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IMerchantServices.Reconcile, URL: {0}", KioskServicesProxy.Instance.Url);
				ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
				bool isOffline = session.ShoppingCart.Type == ShoppingCartType.Offline;
				var message = new
				{
					MessageType = "Reconcile",
					MessageId = Guid.NewGuid(),
					IsOffline = isOffline,
					Blocklisted = session.ShoppingCart.Blocklisted,
					Email = session.ShoppingCart.Email,
					EngineVersion = macroService["EngineVersion"],
					BundleVersion = macroService["ProductVersion"],
					KioskId = int.Parse(session.StoreNumber),
					ReferenceNumber = session.ShoppingCart.ReferenceNumber,
					CreditCard = new Dictionary<string, object>
					{
						{
							"KeyId",
							service2.GetKeyId()
						},
						{
							"Number",
							service2.GetNumber()
						},
						{
							"CardType",
							service2.GetCardType()
						},
						{
							"CardId",
							service2.GenerateCardHashId()
						},
						{ "LastFour", "9999" },
						{
							"LastName",
							session.ShoppingCart.LastName
						},
						{
							"FirstName",
							session.ShoppingCart.FirstName
						},
						{
							"PostalCode",
							session.ShoppingCart.PostalCode
						},
						{
							"ExpirationYear",
							session.ShoppingCart.ExpiryYear
						},
						{
							"ExpirationMonth",
							session.ShoppingCart.ExpiryMonth
						}
					},
					TransactionDate = DateTime.Now,
					PlayPassPromptAccepted = session.ShoppingCart.PlayPassPromptAccepted,
					PlayPassPointsEarned = session.ShoppingCart.PlayPassPointsEarned,
					PropertyBag = session.ShoppingCart.PropertyBag,
					ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
					CustomerProfileNumber = customerProfileNumber
				};
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "Reconcile", message);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.Reconcile.", e);
			}
			finally
			{
				try
				{
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
							Message = "Invoking callback from MerchantServicesProxy.Reconcile."
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

	public void PreAuth(string sessionId, string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			try
			{
				Interlocked.Increment(ref m_working);
				if (session != null)
				{
					if (LiveCall)
					{
						if (!StoreServicesProxy.Instance.IsStandAlone)
						{
							LogHelper.Instance.Log("Live call to remote IMerchantServices.PreAuth, URL: {0}", KioskServicesProxy.Instance.Url);
							IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
							ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
							var message = new
							{
								MessageType = "PreAuth",
								MessageId = Guid.NewGuid(),
								EngineVersion = service["EngineVersion"],
								BundleVersion = service["ProductVersion"],
								KioskId = int.Parse(session.StoreNumber),
								CreditCard = new Dictionary<string, object>
								{
									{
										"KeyId",
										service2.GetKeyId()
									},
									{
										"Number",
										service2.GetNumber()
									},
									{
										"CardType",
										service2.GetCardType()
									},
									{
										"CardId",
										service2.GenerateCardHashId()
									},
									{
										"Track2",
										ExtractFromSecure(session.ShoppingCart.TrackData)
									},
									{ "LastFour", "9999" },
									{
										"LastName",
										session.ShoppingCart.LastName
									},
									{
										"FirstName",
										session.ShoppingCart.FirstName
									},
									{
										"PostalCode",
										session.ShoppingCart.PostalCode
									},
									{
										"ExpirationYear",
										session.ShoppingCart.ExpiryYear
									},
									{
										"ExpirationMonth",
										session.ShoppingCart.ExpiryMonth
									}
								},
								TransactionDate = DateTime.Now,
								UseCredits = session.ShoppingCart.UseCredits.ToString(),
								ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
								CustomerProfileNumber = customerProfileNumber
							};
							session.ShoppingCart.ClearTrackData();
							IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
							remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
							if (dictionary.ContainsKey("ServerName"))
							{
								remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
							}
							remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
							remoteServiceResult.Properties["prompt_required"] = dictionary.ContainsKey("PromptRequired") && (bool)dictionary["PromptRequired"];
							remoteServiceResult.Properties["auth_rule_id"] = (dictionary.ContainsKey("AuthRuleId") ? dictionary["AuthRuleId"] : ((object)0));
							remoteServiceResult.Properties["auth_amount"] = (dictionary.ContainsKey("AuthAmount") ? dictionary["AuthAmount"].ToString() : "0");
							remoteServiceResult.Properties["auth_days"] = (dictionary.ContainsKey("AuthDays") ? dictionary["AuthDays"] : ((object)0));
							remoteServiceResult.Properties["skip_auth_rule"] = !dictionary.ContainsKey("SkipAuthRule") || (bool)dictionary["SkipAuthRule"];
						}
						else
						{
							LogHelper.Instance.Log("In stand alone mode, skipping realtime call to PreAuth.", LogEntryType.Info);
							remoteServiceResult.Success = true;
							remoteServiceResult.Properties["is_online"] = false;
							remoteServiceResult.Properties["prompt_required"] = false;
							remoteServiceResult.Properties["auth_rule_id"] = 0;
							remoteServiceResult.Properties["auth_amount"] = "0";
							remoteServiceResult.Properties["auth_days"] = 0;
							remoteServiceResult.Properties["skip_auth_rule"] = true;
							new AutoResetEvent(initialState: false).WaitOne(1000);
						}
					}
					else
					{
						LogHelper.Instance.Log("Simulating call to IMerchantServices.PreAuth.");
						int num = new Random().Next(1, 3);
						new AutoResetEvent(initialState: false).WaitOne(num * 1000);
						if (SimulateServerBusyError)
						{
							remoteServiceResult.Success = false;
							RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
							{
								Type = InstructionType.WaitAbsolute
							};
							remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
							remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
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
							remoteServiceResult.Properties["is_online"] = false;
						}
						else
						{
							remoteServiceResult.Success = true;
							remoteServiceResult.Properties["is_online"] = false;
						}
						remoteServiceResult.Properties["prompt_required"] = false;
						remoteServiceResult.Properties["auth_rule_id"] = 0;
						remoteServiceResult.Properties["auth_amount"] = "0";
						remoteServiceResult.Properties["auth_days"] = 0;
						remoteServiceResult.Properties["skip_auth_rule"] = true;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.PreAuth.", e);
				remoteServiceResult.Success = false;
				remoteServiceResult.Properties["is_online"] = false;
				remoteServiceResult.Properties["prompt_required"] = false;
				remoteServiceResult.Properties["auth_rule_id"] = 0;
				remoteServiceResult.Properties["auth_amount"] = "0";
				remoteServiceResult.Properties["auth_days"] = 0;
				remoteServiceResult.Properties["skip_auth_rule"] = true;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MerchantServicesProxy.PreAuth."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void Authorize(string sessionId, string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			_ = ServiceLocator.Instance.GetService<IPersonalizationServices>()?.CustomerProfileNumber;
			try
			{
				Interlocked.Increment(ref m_working);
				if (session == null)
				{
					remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
				}
				else if (LiveCall)
				{
					if (!StoreServicesProxy.Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IMerchantServices.Authorize, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
						var message = new
						{
							MessageType = "Authorize",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(session.StoreNumber),
							CreditCard = new Dictionary<string, object>
							{
								{
									"KeyId",
									service2.GetKeyId()
								},
								{
									"Number",
									service2.GetNumber()
								},
								{
									"CardType",
									service2.GetCardType()
								},
								{
									"CardId",
									service2.GenerateCardHashId()
								},
								{
									"Track2",
									ExtractFromSecure(session.ShoppingCart.TrackData)
								},
								{ "LastFour", "9999" },
								{
									"LastName",
									session.ShoppingCart.LastName
								},
								{
									"FirstName",
									session.ShoppingCart.FirstName
								},
								{
									"PostalCode",
									session.ShoppingCart.PostalCode
								},
								{
									"ExpirationYear",
									session.ShoppingCart.ExpiryYear
								},
								{
									"ExpirationMonth",
									session.ShoppingCart.ExpiryMonth
								}
							},
							TransactionDate = DateTime.Now,
							UseCredits = session.ShoppingCart.UseCredits.ToString(),
							AuthAcceptFlag = session.ShoppingCart.AuthAcceptFlag,
							AuthRuleId = session.ShoppingCart.AuthRuleId,
							AuthAmount = session.ShoppingCart.AuthAmount,
							SkipAuthRule = session.ShoppingCart.SkipAuthRule,
							PropertyBag = session.ShoppingCart.PropertyBag,
							ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
							CustomerProfileNumber = customerProfileNumber
						};
						session.ShoppingCart.ClearTrackData();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						if (remoteServiceResult.Success && dictionary.ContainsKey("ReferenceNumber"))
						{
							session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(dictionary["ReferenceNumber"]));
						}
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
						remoteServiceResult.Properties["email"] = (dictionary.ContainsKey("Email") ? dictionary["Email"] : string.Empty);
						remoteServiceResult.Properties["reference_number"] = (dictionary.ContainsKey("ReferenceNumber") ? dictionary["ReferenceNumber"] : null);
						remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
						remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							remoteServiceResult.Properties["has_profile"] = (bool)dictionary2["Valid"];
							remoteServiceResult.Properties["first_name"] = dictionary2["FirstName"];
							remoteServiceResult.Properties["is_rbi"] = dictionary2.ContainsKey("IsRBI") && (bool)dictionary2["IsRBI"];
						}
						Dictionary<string, object> dictionary3 = (dictionary.ContainsKey("CreditSummary") ? (dictionary["CreditSummary"] as Dictionary<string, object>) : null);
						if (dictionary3 != null)
						{
							remoteServiceResult.Properties["total_credits_applied"] = dictionary3["TotalCreditsApplied"];
							remoteServiceResult.Properties["total_credits_available"] = dictionary3["TotalCreditsAvailable"];
						}
						remoteServiceResult.Properties["confirmation_status"] = (dictionary.ContainsKey("ConfirmationStatus") ? ((ConfirmationStatus)dictionary["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString() : ConfirmationStatus.DoNotShow.ToString());
						ArrayList arrayList = (dictionary.ContainsKey("Offers") ? (dictionary["Offers"] as ArrayList) : null);
						Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
						if (arrayList != null)
						{
							int num = 1;
							foreach (Dictionary<string, object> item2 in arrayList)
							{
								dictionary4.Add("offer_" + num, item2["Name"]);
								num++;
							}
						}
						remoteServiceResult.Properties["offers"] = dictionary4;
						remoteServiceResult.Properties["play_pass_prompt"] = dictionary.ContainsKey("PlayPassPrompt") && (bool)dictionary["PlayPassPrompt"];
						remoteServiceResult.Properties["play_pass_points_earned"] = (dictionary.ContainsKey("PlayPassPointsEarned") ? dictionary["PlayPassPointsEarned"] : ((object)0));
						remoteServiceResult.Properties["account_number"] = (dictionary.ContainsKey("AccountNumber") ? dictionary["AccountNumber"] : string.Empty);
						remoteServiceResult.Properties["customer_profile_number"] = (dictionary.ContainsKey("CustomerProfileNumber") ? dictionary["CustomerProfileNumber"] : string.Empty);
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList2 = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList2 != null && arrayList2.Count > 0)
							{
								if (arrayList2[0] is Dictionary<string, object> dictionary6)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), (string)dictionary6["Code"]);
									remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : AuthorizeResponse.Error.ToString());
								}
							}
							else
							{
								remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
							}
						}
					}
					else
					{
						LogHelper.Instance.Log("In stand alone mode, skipping realtime call to Authorize.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
						remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						new AutoResetEvent(initialState: false).WaitOne(1000);
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IMerchantServices.Authorize.");
					int num2 = new Random().Next(1, 3);
					new AutoResetEvent(initialState: false).WaitOne(num2 * 1000);
					remoteServiceResult.Properties["store_number"] = session.StoreNumber;
					remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
					remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
					if (SimulateServerBusyError)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
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
						remoteServiceResult.Success = AuthorizeResponse == AuthorizeResponse.Approved;
						remoteServiceResult.Properties["response"] = AuthorizeResponse.ToString();
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["has_profile"] = HasCustomerProfile;
						remoteServiceResult.Properties["email"] = EmailAddress;
						remoteServiceResult.Properties["first_name"] = CPFirstName;
						remoteServiceResult.Properties["last_name"] = CPLastName;
						remoteServiceResult.Properties["is_rbi"] = IsRBI;
						remoteServiceResult.Properties["total_credits_applied"] = 1;
						if (AuthorizeResponse == AuthorizeResponse.CheckIfCustomerWantsToUseCredits && HasCustomerProfile)
						{
							remoteServiceResult.Properties["total_credits_available"] = DigitalServicesProxy.Instance.TotalCreditsAvailable;
						}
						remoteServiceResult.Properties["reference_number"] = TransactionId;
						remoteServiceResult.Properties["account_number"] = AccountNumber;
						remoteServiceResult.Properties["customer_profile_number"] = CPAccountNumber;
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
						int num3 = 1;
						string[] userOffers = UserOffers;
						foreach (string value in userOffers)
						{
							dictionary7.Add("offer_" + num3, value);
							num3++;
						}
						remoteServiceResult.Properties["offers"] = dictionary7;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.Authorize.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
				remoteServiceResult.Properties["is_online"] = false;
				remoteServiceResult.Properties["first_name"] = ((session != null) ? session.ShoppingCart.FirstName : "N/A");
				remoteServiceResult.Properties["last_name"] = ((session != null) ? session.ShoppingCart.LastName : "N/A");
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MerchantServicesProxy.Authorize."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void GCAuthorize(string sessionId, string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			try
			{
				Interlocked.Increment(ref m_working);
				if (session == null)
				{
					remoteServiceResult.Properties["response"] = GCAuthorizeResponse.Error.ToString();
				}
				else if (GCLiveCall)
				{
					if (!StoreServicesProxy.Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IMerchantServices.GCAuthorize, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
						var message = new
						{
							MessageType = "GCAuthorize",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(session.StoreNumber),
							CreditCard = new Dictionary<string, object>
							{
								{
									"KeyId",
									service2.GetKeyId()
								},
								{
									"Number",
									service2.GetNumber()
								},
								{
									"CardType",
									service2.GetCardType()
								},
								{
									"CardId",
									service2.GenerateCardHashId()
								},
								{
									"Track2",
									ExtractFromSecure(session.ShoppingCart.TrackData)
								},
								{ "LastFour", "9999" },
								{
									"LastName",
									session.ShoppingCart.LastName
								},
								{
									"FirstName",
									session.ShoppingCart.FirstName
								},
								{
									"ExpirationYear",
									session.ShoppingCart.ExpiryYear
								},
								{
									"ExpirationMonth",
									session.ShoppingCart.ExpiryMonth
								}
							},
							TransactionDate = DateTime.Now,
							UseCredits = session.ShoppingCart.UseCredits.ToString(),
							ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
							CustomerProfileNumber = customerProfileNumber
						};
						session.ShoppingCart.ClearTrackData();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						if (remoteServiceResult.Success && dictionary.ContainsKey("ReferenceNumber"))
						{
							session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(dictionary["ReferenceNumber"]));
						}
						remoteServiceResult.Properties["response"] = GCAuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
						remoteServiceResult.Properties["email"] = (dictionary.ContainsKey("Email") ? dictionary["Email"] : string.Empty);
						remoteServiceResult.Properties["reference_number"] = (dictionary.ContainsKey("ReferenceNumber") ? dictionary["ReferenceNumber"] : null);
						remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
						remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							remoteServiceResult.Properties["has_profile"] = (bool)dictionary2["Valid"];
							remoteServiceResult.Properties["first_name"] = dictionary2["FirstName"];
							remoteServiceResult.Properties["is_rbi"] = dictionary2.ContainsKey("IsRBI") && (bool)dictionary2["IsRBI"];
						}
						Dictionary<string, object> dictionary3 = (dictionary.ContainsKey("CreditSummary") ? (dictionary["CreditSummary"] as Dictionary<string, object>) : null);
						if (dictionary3 != null)
						{
							remoteServiceResult.Properties["total_credits_applied"] = dictionary3["TotalCreditsApplied"];
							remoteServiceResult.Properties["total_credits_available"] = dictionary3["TotalCreditsAvailable"];
						}
						remoteServiceResult.Properties["confirmation_status"] = (dictionary.ContainsKey("ConfirmationStatus") ? ((ConfirmationStatus)dictionary["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString() : ConfirmationStatus.DoNotShow.ToString());
						ArrayList arrayList = (dictionary.ContainsKey("Offers") ? (dictionary["Offers"] as ArrayList) : null);
						Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
						if (arrayList != null)
						{
							int num = 1;
							foreach (Dictionary<string, object> item2 in arrayList)
							{
								dictionary4.Add("offer_" + num, item2["Name"]);
								num++;
							}
						}
						remoteServiceResult.Properties["offers"] = dictionary4;
						Dictionary<string, object> dictionary6 = (dictionary.ContainsKey("GiftCardSummary") ? (dictionary["GiftCardSummary"] as Dictionary<string, object>) : null);
						if (dictionary6 != null)
						{
							remoteServiceResult.Properties["gift_card_exists"] = (bool)dictionary6["Exists"];
							remoteServiceResult.Properties["gift_card_status"] = ((GiftCardStatus)dictionary6["Status"]/*cast due to constrained. prefix*/).ToString();
							remoteServiceResult.Properties["gift_card_balance"] = dictionary6["Balance"].ToString();
							remoteServiceResult.Properties["gift_card_max_discs"] = dictionary6["MaxDiscs"];
							remoteServiceResult.Properties["gift_card_max_games"] = dictionary6["MaxGames"];
						}
						remoteServiceResult.Properties["account_number"] = (dictionary.ContainsKey("AccountNumber") ? dictionary["AccountNumber"] : string.Empty);
						remoteServiceResult.Properties["customer_profile_number"] = (dictionary.ContainsKey("CustomerProfileNumber") ? dictionary["CustomerProfileNumber"] : string.Empty);
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList2 = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList2 != null && arrayList2.Count > 0)
							{
								if (arrayList2[0] is Dictionary<string, object> dictionary7)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(GCAuthorizeResponse?), (string)dictionary7["Code"]);
									remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : GCAuthorizeResponse.Error.ToString());
								}
							}
							else
							{
								remoteServiceResult.Properties["response"] = GCAuthorizeResponse.Error.ToString();
							}
						}
					}
					else
					{
						LogHelper.Instance.Log("In stand alone mode, skipping realtime call to GCAuthorize.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["response"] = GCAuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
						remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						new AutoResetEvent(initialState: false).WaitOne(1000);
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IMerchantServices.GCAuthorize.");
					int num2 = new Random().Next(1, 3);
					new AutoResetEvent(initialState: false).WaitOne(num2 * 1000);
					remoteServiceResult.Properties["store_number"] = session.StoreNumber;
					remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
					remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
					if (GCSimulateServerBusyError)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = GCAuthorizeResponse.Error.ToString();
						remoteServiceResult.Properties["is_online"] = false;
					}
					else if (GCSimulateCommunicationsFailure)
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
						remoteServiceResult.Success = GCAuthorizeResponse == GCAuthorizeResponse.Approved;
						remoteServiceResult.Properties["response"] = GCAuthorizeResponse.ToString();
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["has_profile"] = HasCustomerProfile;
						remoteServiceResult.Properties["email"] = EmailAddress;
						remoteServiceResult.Properties["first_name"] = CPFirstName;
						remoteServiceResult.Properties["last_name"] = CPLastName;
						remoteServiceResult.Properties["is_rbi"] = IsRBI;
						remoteServiceResult.Properties["reference_number"] = TransactionId;
						remoteServiceResult.Properties["account_number"] = AccountNumber;
						remoteServiceResult.Properties["customer_profile_number"] = CPAccountNumber;
						remoteServiceResult.Properties["total_credits_applied"] = 0;
						if (GCAuthorizeResponse == GCAuthorizeResponse.CheckIfCustomerWantsToUseCredits && HasCustomerProfile)
						{
							remoteServiceResult.Properties["total_credits_available"] = DigitalServicesProxy.Instance.TotalCreditsAvailable;
						}
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						Dictionary<string, object> dictionary8 = new Dictionary<string, object>();
						int num3 = 1;
						string[] userOffers = UserOffers;
						foreach (string value in userOffers)
						{
							dictionary8.Add("offer_" + num3, value);
							num3++;
						}
						remoteServiceResult.Properties["offers"] = dictionary8;
						remoteServiceResult.Properties["gift_card_exists"] = Exists;
						remoteServiceResult.Properties["gift_card_status"] = GCStatusType.ToString();
						remoteServiceResult.Properties["gift_card_balance"] = Balance;
						remoteServiceResult.Properties["gift_card_max_discs"] = MaxDiscs;
						remoteServiceResult.Properties["gift_card_max_games"] = MaxGames;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.GCAuthorize.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["response"] = GCAuthorizeResponse.Approved.ToString();
				remoteServiceResult.Properties["is_online"] = false;
				remoteServiceResult.Properties["first_name"] = ((session != null) ? session.ShoppingCart.FirstName : "N/A");
				remoteServiceResult.Properties["last_name"] = ((session != null) ? session.ShoppingCart.LastName : "N/A");
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MerchantServicesProxy.GCAuthorize."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void ReservationAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			try
			{
				Interlocked.Increment(ref m_working);
				if (session == null || session.ShoppingCart.ReferenceNumber.HasValue)
				{
					remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
				}
				else if (ResvAuthLiveCall)
				{
					if (!StoreServicesProxy.Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IMerchantServices.ReservationAuthorize, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
						var message = new
						{
							MessageType = "ResvAuth",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(session.StoreNumber),
							CreditCard = new Dictionary<string, object>
							{
								{
									"KeyId",
									service2.GetKeyId()
								},
								{
									"Number",
									service2.GetNumber()
								},
								{
									"CardType",
									service2.GetCardType()
								},
								{
									"CardId",
									service2.GenerateCardHashId()
								},
								{
									"Track2",
									ExtractFromSecure(session.ShoppingCart.TrackData)
								},
								{ "LastFour", "9999" },
								{
									"LastName",
									session.ShoppingCart.LastName
								},
								{
									"FirstName",
									session.ShoppingCart.FirstName
								},
								{
									"PostalCode",
									session.ShoppingCart.PostalCode
								},
								{
									"ExpirationYear",
									session.ShoppingCart.ExpiryYear
								},
								{
									"ExpirationMonth",
									session.ShoppingCart.ExpiryMonth
								}
							},
							TransactionDate = DateTime.Now,
							UseCredits = session.ShoppingCart.UseCredits.ToString(),
							ReferenceNumber = referenceNumber,
							ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
							CustomerProfileNumber = customerProfileNumber
						};
						session.ShoppingCart.ClearTrackData();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						if (remoteServiceResult.Success && dictionary.ContainsKey("ReferenceNumber"))
						{
							session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(dictionary["ReferenceNumber"]));
						}
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
						remoteServiceResult.Properties["email"] = (dictionary.ContainsKey("Email") ? dictionary["Email"] : string.Empty);
						remoteServiceResult.Properties["reference_number"] = (dictionary.ContainsKey("ReferenceNumber") ? dictionary["ReferenceNumber"] : null);
						remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
						remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							remoteServiceResult.Properties["has_profile"] = (bool)dictionary2["Valid"];
							remoteServiceResult.Properties["first_name"] = dictionary2["FirstName"];
						}
						Dictionary<string, object> dictionary3 = (dictionary.ContainsKey("CreditSummary") ? (dictionary["CreditSummary"] as Dictionary<string, object>) : null);
						if (dictionary3 != null)
						{
							remoteServiceResult.Properties["total_credits_applied"] = dictionary3["TotalCreditsApplied"];
							remoteServiceResult.Properties["total_credits_available"] = dictionary3["TotalCreditsAvailable"];
						}
						remoteServiceResult.Properties["confirmation_status"] = (dictionary.ContainsKey("ConfirmationStatus") ? ((ConfirmationStatus)dictionary["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString() : ConfirmationStatus.DoNotShow.ToString());
						ArrayList arrayList = (dictionary.ContainsKey("Offers") ? (dictionary["Offers"] as ArrayList) : null);
						Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
						if (arrayList != null)
						{
							int num = 1;
							foreach (Dictionary<string, object> item2 in arrayList)
							{
								dictionary4.Add("offer_" + num, item2["Name"]);
								num++;
							}
						}
						remoteServiceResult.Properties["offers"] = dictionary4;
						remoteServiceResult.Properties["account_number"] = (dictionary.ContainsKey("AccountNumber") ? dictionary["AccountNumber"] : string.Empty);
						remoteServiceResult.Properties["customer_profile_number"] = (dictionary.ContainsKey("CustomerProfileNumber") ? dictionary["CustomerProfileNumber"] : string.Empty);
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList2 = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList2 != null && arrayList2.Count > 0)
							{
								if (arrayList2[0] is Dictionary<string, object> dictionary6)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), (string)dictionary6["Code"]);
									remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : AuthorizeResponse.Error.ToString());
								}
							}
							else
							{
								remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
							}
						}
					}
					else
					{
						LogHelper.Instance.Log("In stand alone mode, skipping realtime call to ReservationAuthorize.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
						remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						new AutoResetEvent(initialState: false).WaitOne(1000);
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IMerchantServices.ReservationAuthorize.");
					int num2 = new Random().Next(1, 3);
					new AutoResetEvent(initialState: false).WaitOne(num2 * 1000);
					remoteServiceResult.Properties["store_number"] = session.StoreNumber;
					remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
					remoteServiceResult.Properties["last_name"] = session.ShoppingCart.LastName;
					if (ResvAuthSimulateServerBusyError)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
						remoteServiceResult.Properties["is_online"] = false;
					}
					else if (ResvAuthSimulateCommunicationsFailure)
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
						remoteServiceResult.Success = ResvAuthAuthorizeResponse == AuthorizeResponse.Approved;
						remoteServiceResult.Properties["response"] = ResvAuthAuthorizeResponse.ToString();
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["has_profile"] = HasCustomerProfile;
						remoteServiceResult.Properties["email"] = EmailAddress;
						remoteServiceResult.Properties["first_name"] = CPFirstName;
						remoteServiceResult.Properties["last_name"] = CPLastName;
						remoteServiceResult.Properties["reference_number"] = TransactionId;
						remoteServiceResult.Properties["account_number"] = AccountNumber;
						remoteServiceResult.Properties["customer_profile_number"] = CPAccountNumber;
						remoteServiceResult.Properties["total_credits_applied"] = 0;
						if (ResvAuthAuthorizeResponse == AuthorizeResponse.CheckIfCustomerWantsToUseCredits && HasCustomerProfile)
						{
							remoteServiceResult.Properties["total_credits_available"] = DigitalServicesProxy.Instance.TotalCreditsAvailable;
						}
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
						int num3 = 1;
						string[] userOffers = UserOffers;
						foreach (string value in userOffers)
						{
							dictionary7.Add("offer_" + num3, value);
							num3++;
						}
						remoteServiceResult.Properties["offers"] = dictionary7;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.ReservationAuthorize.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
				remoteServiceResult.Properties["is_online"] = false;
				remoteServiceResult.Properties["first_name"] = ((session != null) ? session.ShoppingCart.FirstName : "N/A");
				remoteServiceResult.Properties["last_name"] = ((session != null) ? session.ShoppingCart.LastName : "N/A");
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone || !LiveCall;
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MerchantServicesProxy.ReservationAuthorize."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void ValidatePickup(string sessionId, List<long> referenceNumbers, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			try
			{
				Interlocked.Increment(ref m_working);
				if (session == null)
				{
					remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
				}
				else if (LiveCall)
				{
					if (!StoreServicesProxy.Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IMerchantServices.ValidatePickup, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
						var message = new
						{
							MessageType = "PickupValidation",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(StoreServicesProxy.Instance.StoreNumber),
							CreditCard = new Dictionary<string, object>
							{
								{
									"KeyId",
									service2.GetKeyId()
								},
								{
									"Number",
									service2.GetNumber()
								},
								{
									"CardType",
									service2.GetCardType()
								},
								{
									"CardId",
									service2.GenerateCardHashId()
								},
								{
									"Track2",
									ExtractFromSecure(session.ShoppingCart.TrackData)
								},
								{ "LastFour", "9999" },
								{
									"LastName",
									session.ShoppingCart.LastName
								},
								{
									"FirstName",
									session.ShoppingCart.FirstName
								},
								{
									"PostalCode",
									session.ShoppingCart.PostalCode
								},
								{
									"ExpirationYear",
									session.ShoppingCart.ExpiryYear
								},
								{
									"ExpirationMonth",
									session.ShoppingCart.ExpiryMonth
								}
							},
							TransactionDate = DateTime.Now,
							ReferenceNumbers = referenceNumbers
						};
						session.ShoppingCart.ClearTrackData();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList != null && arrayList.Count > 0)
							{
								if (arrayList[0] is Dictionary<string, object> dictionary2)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), (string)dictionary2["Code"]);
									remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : AuthorizeResponse.Error.ToString());
								}
							}
							else
							{
								remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
							}
						}
					}
					else
					{
						LogHelper.Instance.Log("In stand alone mode, skipping realtime call to ValidatePickup.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = false;
						new AutoResetEvent(initialState: false).WaitOne(1000);
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IMerchantServices.ValidatePickup.");
					int num = new Random().Next(1, 3);
					new AutoResetEvent(initialState: false).WaitOne(num * 1000);
					remoteServiceResult.Properties["store_number"] = session.StoreNumber;
					if (SimulateServerBusyError)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
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
						remoteServiceResult.Properties["response"] = "Communication Failure";
						remoteServiceResult.Properties["is_online"] = false;
					}
					else
					{
						remoteServiceResult.Success = AuthorizeResponse == AuthorizeResponse.Approved;
						remoteServiceResult.Properties["response"] = AuthorizeResponse.ToString();
						remoteServiceResult.Properties["is_online"] = true;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.ValidatePickup.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
				remoteServiceResult.Properties["is_online"] = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone || !LiveCall;
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from MerchantServicesProxy.ValidatePickup."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void ModResvPickup(string storeNumber, string emailAddress, long referenceNumber, string sessionId, bool modifiedCart, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IMerchantServices.ModResvPickup, URL: {0}", KioskServicesProxy.Instance.Url);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				if (ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId) == null)
				{
					LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
				}
				else
				{
					var message = new
					{
						MessageType = "ModResvPickup",
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						Email = emailAddress,
						KioskId = int.Parse(storeNumber),
						ReferenceNumber = referenceNumber,
						Items = new Dictionary<string, object>(),
						PickupDate = DateTime.Now,
						ModifiedCart = modifiedCart,
						ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId)
					};
					errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "ModResvPickup", message);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.ModResvPickup.", e);
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
							Message = "Invoking callback from MerchantServicesProxy.ModResvPickup."
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

	private MerchantServicesProxy()
	{
	}

	private string ExtractFromSecure(SecureString value)
	{
		try
		{
			return Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(value));
		}
		catch
		{
			return string.Empty;
		}
	}
}
