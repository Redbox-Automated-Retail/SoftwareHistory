using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

internal class PromotionServicesProxy : IPromotionServices
{
	private int m_working;

	public static PromotionServicesProxy Instance => Singleton<PromotionServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Promotion Services", "LiveCall", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Promotion Services", "LiveCall", value);
		}
	}

	public string[] PromoCodes
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Promotion Services", "PromoCodes", new string[0]);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Promotion Services", "PromoCodes", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Promotion Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Promotion Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Promotion Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Promotion Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	public void ValidatePromotionCode(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformLiveCall(storeNumber, promotionCode, completeCallback);
		}
		else
		{
			SimulatedCall(storeNumber, promotionCode, completeCallback);
		}
	}

	public void KioskOptIn(string storeNumber, List<IOptInItem> optInItems, bool sendCardHash, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to Remote IPromotionServices.KioskOptIn, URL: {0} ", KioskServicesProxy.Instance.Url);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
				string cardHash = string.Empty;
				ITrackDataService service3 = ServiceLocator.Instance.GetService<ITrackDataService>();
				string text = service3.GenerateCardHashId();
				if (sendCardHash && text != null)
				{
					cardHash = text;
				}
				errorList = service2.Enqueue(200, "KioskOptIn", new
				{
					MessageType = "KioskOptIn",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					RequestDate = DateTime.Now,
					CardHash = cardHash,
					OptInItems = optInItems
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServices.KioskOptIn.", e);
			}
			finally
			{
				try
				{
					ICallbackService service4 = ServiceLocator.Instance.GetService<ICallbackService>();
					if (callback != null && service4 != null)
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
						service4.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from PromotionServices.KioskOptIn."
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

	public IOptInItem NewOptInItem()
	{
		return new OptInItem();
	}

	private PromotionServicesProxy()
	{
	}

	private void PerformLiveCall(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["store"] = storeNumber;
			remoteServiceResult.Properties["code"] = promotionCode;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (HealthServicesProxy.Instance.IsOnline && !StoreServicesProxy.Instance.IsStandAlone)
				{
					LogHelper.Instance.Log("Live call to remote IPromotionServices.ValidatePromotionCode, URL: {0}", KioskServicesProxy.Instance.Url);
					if (!int.TryParse(storeNumber, out var result))
					{
						LogHelper.Instance.Log($"The passed storeNumber '{storeNumber}' cannot be converted into an Int32.", LogEntryType.Error);
						remoteServiceResult.Success = false;
						remoteServiceResult.Properties["response"] = PromotionValidationResponse.Error.ToString();
					}
					else
					{
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							KioskId = result,
							Code = promotionCode,
							MessageId = Guid.NewGuid(),
							MessageType = "DiscountCodeValidation",
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						remoteServiceResult.Properties["response"] = PromotionValidationResponse.Valid.ToString();
						remoteServiceResult.Properties["amount"] = (dictionary.ContainsKey("Amount") ? dictionary["Amount"] : ((object)1));
						remoteServiceResult.Properties["promotion_intent"] = (dictionary.ContainsKey("PromotionIntentCode") ? dictionary["PromotionIntentCode"] : PromotionIntentCode.FixedValue.ToString());
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						if (!remoteServiceResult.Success)
						{
							ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList != null && arrayList.Count > 0)
							{
								if (arrayList[0] is Dictionary<string, object> dictionary2)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(PromotionValidationResponse?), (string)dictionary2["Code"]);
									remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : PromotionValidationResponse.Error.ToString());
								}
							}
							else
							{
								remoteServiceResult.Properties["response"] = PromotionValidationResponse.Error.ToString();
							}
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("IsOnline is false or in stand alone mode, not issuing realtime call to ValidatePromotionCode.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = PromotionValidationResponse.Error.ToString();
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServicesProxy.ValidatePromotionCode.", e);
				remoteServiceResult.Properties["response"] = PromotionValidationResponse.Error.ToString();
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
						Message = "Invoking completeCallback from PromotionServicesProxy.ValidatePromotionCode."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void SimulatedCall(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			LogHelper.Instance.Log("Simulating call to IPromotionServices.ValidatePromotionCode.");
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				remoteServiceResult.Properties["store"] = storeNumber;
				remoteServiceResult.Properties["code"] = promotionCode;
				Random random = new Random();
				int num = random.Next(1, 5);
				AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
				autoResetEvent.WaitOne(num * 1000);
				PromotionValidationResponse response;
				PromotionIntentCode intent;
				decimal amount;
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
				}
				else if (DoesCodeMatch(promotionCode, out response, out intent, out amount))
				{
					remoteServiceResult.Success = response == PromotionValidationResponse.Valid;
					remoteServiceResult.Properties["amount"] = amount;
					remoteServiceResult.Properties["promotion_intent"] = intent.ToString();
					remoteServiceResult.Properties["response"] = response.ToString();
				}
				else
				{
					remoteServiceResult.Success = false;
					remoteServiceResult.Properties["amount"] = 0;
					remoteServiceResult.Properties["promotion_intent"] = PromotionIntentCode.FixedValue.ToString();
					remoteServiceResult.Properties["response"] = PromotionValidationResponse.CodeNotValid.ToString();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServicesProxy.ValidatePromotionCode.", e);
				remoteServiceResult.Properties["response"] = PromotionValidationResponse.Error.ToString();
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
						Message = "Invoking completeCallback from PromotionServicesProxy.ValidatePromotionCode."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private bool DoesCodeMatch(string promotionCode, out PromotionValidationResponse response, out PromotionIntentCode intent, out decimal amount)
	{
		amount = 0m;
		response = PromotionValidationResponse.CodeNotValid;
		intent = PromotionIntentCode.FixedValue;
		if (PromoCodes == null)
		{
			return false;
		}
		string[] promoCodes = PromoCodes;
		foreach (string text in promoCodes)
		{
			string[] array = text.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 4 && string.Compare(array[0], promotionCode, ignoreCase: true) == 0)
			{
				decimal.TryParse(array[1], out amount);
				intent = Enum<PromotionIntentCode>.ParseIgnoringCase(array[2], PromotionIntentCode.FixedValue);
				response = Enum<PromotionValidationResponse>.ParseIgnoringCase(array[3], PromotionValidationResponse.Valid);
				return true;
			}
		}
		return false;
	}
}
