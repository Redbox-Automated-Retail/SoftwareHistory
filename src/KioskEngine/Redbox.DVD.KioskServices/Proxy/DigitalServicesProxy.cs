using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Proxy;

internal class DigitalServicesProxy : IDigitalServices
{
	private sealed class CreditAllocation : ICreditAllocation
	{
		public string Barcode { get; set; }

		public decimal Amount { get; set; }

		public int NumberApplied { get; set; }
	}

	private int m_working;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "LiveCall", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "SimulateServerBusyFailure", value);
		}
	}

	public DigitalServicesResponse DigitalServicesResponse
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "DigitalServicesResponse", DigitalServicesResponse.Valid);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "DigitalServicesResponse", value);
		}
	}

	public bool HasProfile
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "HasProfile", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "HasProfile", value);
		}
	}

	public int TotalCreditsAvailable
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "TotalCreditsAvailable", 10);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "TotalCreditsAvailable", value);
		}
	}

	public string CPFirstName
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "CPFirstName", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "CPFirstName", value);
		}
	}

	public string CPLastName
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "CPLastName", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "CPLastName", value);
		}
	}

	public string EmailAddress
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "EmailAddress", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "EmailAddress", value);
		}
	}

	public bool HasCustomerProfile
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "HasCustomerProfile", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "HasCustomerProfile", value);
		}
	}

	public bool IsEligible
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "IsEligible", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "IsEligible", value);
		}
	}

	public bool CanCreateProfile
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Digital Services", "CanCreateProfile", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Digital Services", "CanCreateProfile", value);
		}
	}

	public static DigitalServicesProxy Instance => Singleton<DigitalServicesProxy>.Instance;

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	public void GetCustomerCredits(string sessionId, RemoteServiceCallback callback)
	{
		if (LiveCall)
		{
			PerformLiveCall(sessionId, callback);
		}
		else
		{
			SimulateCall(sessionId, callback);
		}
	}

	public void GetCustomer(string sessionId, string cardId, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					LogHelper.Instance.Log("Live call to remote IDigitalServices.GetCustomer, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					var message = new
					{
						MessageType = "GetCustomer",
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						KioskId = int.Parse(session.StoreNumber),
						CardId = cardId
					};
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("ServerName"))
					{
						remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
					}
					remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
					remoteServiceResult.Properties["is_eligible_free_trial"] = dictionary.ContainsKey("IsEligibleFreeTrial") && (bool)dictionary["IsEligibleFreeTrial"];
					remoteServiceResult.Properties["email"] = (dictionary.ContainsKey("Email") ? dictionary["Email"] : string.Empty);
					Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
					if (dictionary2 != null)
					{
						remoteServiceResult.Properties["has_customer_profile"] = (bool)dictionary2["HasCustomerProfile"];
						remoteServiceResult.Properties["cp_first_name"] = dictionary2["CPFirstName"];
						remoteServiceResult.Properties["cp_last_name"] = dictionary2["CPLastName"];
					}
					else
					{
						remoteServiceResult.Properties["has_customer_profile"] = dictionary.ContainsKey("HasCustomerProfile") && (bool)dictionary["HasCustomerProfile"];
						remoteServiceResult.Properties["cp_first_name"] = (dictionary.ContainsKey("CPFirstName") ? dictionary["CPFirstName"] : string.Empty);
						remoteServiceResult.Properties["cp_last_name"] = (dictionary.ContainsKey("CPLastName") ? dictionary["CPLastName"] : string.Empty);
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IDigitalServices.GetCustomer.");
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
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["is_eligible_free_trial"] = IsEligible;
						remoteServiceResult.Properties["email"] = EmailAddress;
						remoteServiceResult.Properties["has_customer_profile"] = HasCustomerProfile;
						remoteServiceResult.Properties["cp_first_name"] = CPFirstName;
						remoteServiceResult.Properties["cp_last_name"] = CPLastName;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DigitalServicesProxy.GetCustomer.", e);
				remoteServiceResult.Success = false;
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
						Message = "Invoking completeCallback from DigitalServicesProxy.GetCustomer."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void CreateRBIFreeTrial(string sessionId, string emailAddress, string status, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					LogHelper.Instance.Log("Live call to remote IDigitalServices.CreateRBIFreeTrial, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
					var message = new
					{
						MessageType = "RBiFreeTrialRequest",
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						KioskId = int.Parse(session.StoreNumber),
						UseMessageControl = true,
						RequestStatus = status,
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
								session.ShoppingCart.PostalCode ?? service["JVHardTake:KeyPad:Text"]
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
						Email = emailAddress,
						FreeTrialPackageId = 1
					};
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("ServerName"))
					{
						remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
					}
					remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
					remoteServiceResult.Properties["is_eligible_free_trial"] = dictionary.ContainsKey("IsEligibleFreeTrial") && (bool)dictionary["IsEligibleFreeTrial"];
					remoteServiceResult.Properties["can_create_profile"] = dictionary.ContainsKey("CanCreateProfile") && (bool)dictionary["CanCreateProfile"];
					Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
					if (dictionary2 != null)
					{
						remoteServiceResult.Properties["can_create_profile"] = (bool)dictionary2["CanCreateProfile"];
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IDigitalServices.CreateRBIFreeTrial.");
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
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["is_eligible_free_trial"] = IsEligible;
						remoteServiceResult.Properties["can_create_profile"] = CanCreateProfile;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DigitalServicesProxy.CreateRBIFreeTrial.", e);
				remoteServiceResult.Success = false;
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
						Message = "Invoking completeCallback from DigitalServicesProxy.CreateRBIFreeTrial."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private DigitalServicesProxy()
	{
	}

	private void PerformLiveCall(string sessionId, RemoteServiceCallback callback)
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
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqeue call to remote IDigitalServices.GetCustomerCredits");
				ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
				var anon = new
				{
					MessageType = "GetCustomerCredits",
					MessageId = Guid.NewGuid(),
					RequestDate = DateTime.Now,
					KioskId = int.Parse(session.StoreNumber),
					EngineVersion = macroService["EngineVersion"],
					BundleVersion = macroService["ProductVersion"],
					UseMessageControl = false,
					DiscountCode = session.ShoppingCart.DiscountCode,
					DiscountValue = session.ShoppingCart.DiscountValue,
					CreditCard = new Dictionary<string, object>
					{
						{
							"FirstName",
							session.ShoppingCart.FirstName
						},
						{
							"LastName",
							session.ShoppingCart.LastName
						},
						{
							"Track2",
							ExtractFromSecure(session.ShoppingCart.TrackData)
						},
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
					Contents = new Dictionary<string, object>(),
					TransactionDate = DateTime.Now
				};
				session.ShoppingCart.ClearTrackData();
				foreach (KeyValuePair<string, IShoppingCartLineItemGroup> group in session.ShoppingCart.Groups)
				{
					var anon2 = new
					{
						Items = new List<object>(),
						Totals = new
						{
							TaxRate = group.Value.Totals.TaxRate * 100m,
							SubTotal = group.Value.Totals.SubTotal,
							TaxAmount = group.Value.Totals.TaxAmount,
							GrandTotal = group.Value.Totals.GrandTotal,
							DiscountedSubtotal = group.Value.Totals.DiscountedSubtotal
						}
					};
					foreach (IShoppingCartLineItem item in group.Value.Items)
					{
						var anon3 = new
						{
							Barcode = item.Barcode,
							ProductFamily = item.ProductFamily,
							ProductType = item.ProductType,
							MultiDisc = item.MultiDisc,
							Prices = new Dictionary<string, decimal>()
						};
						foreach (KeyValuePair<string, decimal> price in item.Prices)
						{
							string key = price.Key.Replace("_", string.Empty);
							anon3.Prices[key] = price.Value;
						}
						anon2.Items.Add(anon3);
					}
					anon.Contents[group.Key] = anon2;
				}
				IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(anon);
				remoteServiceResult.Properties["response"] = DigitalServicesResponse.Valid.ToString();
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
							object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(DigitalServicesResponse?), (string)dictionary2["Code"]);
							remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : DigitalServicesResponse.Error.ToString());
						}
					}
					else
					{
						remoteServiceResult.Properties["response"] = DigitalServicesResponse.Error.ToString();
					}
				}
				Dictionary<string, object> dictionary3 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
				if (dictionary3 != null)
				{
					remoteServiceResult.Properties["has_profile"] = (bool)dictionary3["Valid"];
					remoteServiceResult.Properties["first_name"] = dictionary3["FirstName"];
				}
				Dictionary<string, object> dictionary4 = (dictionary.ContainsKey("CreditSummary") ? (dictionary["CreditSummary"] as Dictionary<string, object>) : null);
				if (dictionary4 != null)
				{
					remoteServiceResult.Properties["total_credits_applied"] = dictionary4["TotalCreditsApplied"];
					remoteServiceResult.Properties["total_credits_available"] = dictionary4["TotalCreditsAvailable"];
				}
				IKernelService service3 = ServiceLocator.Instance.GetService<IKernelService>();
				LuaTable luaTable = (LuaTable)service3.NewTable();
				int num = 0;
				if (dictionary.ContainsKey("CreditAllocations"))
				{
					foreach (IDictionary<string, object> item2 in (ArrayList)dictionary["CreditAllocations"])
					{
						LuaTable luaTable2 = (LuaTable)service3.NewTable();
						luaTable2["barcode"] = (item2.ContainsKey("Barcode") ? ((string)item2["Barcode"]) : string.Empty);
						luaTable2["amount"] = (item2.ContainsKey("Amount") ? ((decimal)item2["Amount"]) : 0m);
						luaTable2["number_applied"] = (item2.ContainsKey("NumberApplied") ? ((int)item2["NumberApplied"]) : 0);
						luaTable[num++] = luaTable2;
					}
				}
				remoteServiceResult.Properties["credit_allocations"] = luaTable;
				Dictionary<string, object> dictionary6 = (dictionary.ContainsKey("GiftCardSummary") ? (dictionary["GiftCardSummary"] as Dictionary<string, object>) : null);
				if (dictionary6 != null)
				{
					remoteServiceResult.Properties["gift_card_max_discs"] = dictionary6["MaxDiscs"];
					remoteServiceResult.Properties["gift_card_max_games"] = dictionary6["MaxGames"];
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DigitalServicesProxy.GetCustomerCredits.", e);
				remoteServiceResult.Properties["response"] = DigitalServicesResponse.Error.ToString();
				remoteServiceResult.Properties["is_online"] = false;
			}
			finally
			{
				try
				{
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking callback from DigitalServicesProxy.GetCustomerCredits."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void SimulateCall(string sessionId, RemoteServiceCallback callback)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IShoppingSession session = service.GetSession(sessionId);
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Simulating call to IDigitalServices.GetCustomerCredits.");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				int num = new Random().Next(1, 5);
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
					remoteServiceResult.Success = DigitalServicesResponse == DigitalServicesResponse.Valid;
					remoteServiceResult.Properties["response"] = DigitalServicesResponse.ToString();
					remoteServiceResult.Properties["is_online"] = true;
					if (DigitalServicesResponse == DigitalServicesResponse.Valid)
					{
						remoteServiceResult.Properties["has_profile"] = HasProfile;
						remoteServiceResult.Properties["first_name"] = session.ShoppingCart.FirstName;
						IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
						LuaTable luaTable = (LuaTable)service2.NewTable();
						int num2 = 0;
						foreach (KeyValuePair<string, IShoppingCartLineItemGroup> group in session.ShoppingCart.Groups)
						{
							foreach (IShoppingCartLineItem item2 in group.Value.Items)
							{
								if (num2 < TotalCreditsAvailable)
								{
									LuaTable luaTable2 = (LuaTable)service2.NewTable();
									luaTable2["barcode"] = item2.Barcode;
									luaTable2["amount"] = item2.Price;
									luaTable2["number_applied"] = 1;
									luaTable[num2++] = luaTable2;
								}
							}
						}
						remoteServiceResult.Properties["total_credits_applied"] = num2;
						remoteServiceResult.Properties["total_credits_available"] = TotalCreditsAvailable;
						remoteServiceResult.Properties["credit_allocations"] = luaTable;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DigitalServicesProxy.GetCustomerCredits.", e);
				remoteServiceResult.Properties["response"] = DigitalServicesResponse.Error.ToString();
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
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking callback from DigitalServicesProxy.GetCustomerCredits."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
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
