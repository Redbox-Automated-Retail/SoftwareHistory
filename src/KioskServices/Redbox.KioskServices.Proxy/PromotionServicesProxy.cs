using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model.Session;

namespace Redbox.KioskServices.Proxy;

public class PromotionServicesProxy : IPromotionServices
{
	public class KioskEventsMessage
	{
		public Guid MessageId { get; set; }

		public string MessageType { get; set; }

		public long KioskId { get; set; }

		public string EngineVersion { get; set; }

		public string BundleVersion { get; set; }

		public string KioskEventsData { get; set; }
	}

	private int m_working;

	public static PromotionServicesProxy Instance => Singleton<PromotionServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "LiveCall", value);
		}
	}

	public string[] PromoCodes
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "PromoCodes", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "PromoCodes", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Promotion Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Promotion Services", "SimulateCommunicationFailure", value);
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

	public void KioskOptIn(string storeNumber, List<IOptInItem> optInItems, RemoteServiceCallback callback)
	{
		ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to Remote IPromotionServices.KioskOptIn, URL: {0} ", KioskServicesProxy.Instance.Url);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
				IEnumerable<IOptInItem> enumerable = (optInItems ?? new List<IOptInItem>()).Where((IOptInItem x) => !string.IsNullOrEmpty(x.OptInMethodData));
				errorList = (enumerable.Any() ? service2.Enqueue(200, "KioskOptIn", new
				{
					MessageType = "KioskOptIn",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					RequestDate = DateTime.Now,
					OptInItems = enumerable
				}) : new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("U001", "No Valid Optins To Send", (optInItems?.Count() ?? 0) + " items were missing OptInMethodData") });
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServices.KioskOptIn.", e);
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

	public void MarketingAdImpressions(long kioskId, List<MarketingAdImpressionData> impressions)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				Interlocked.Increment(ref m_working);
				MarketingAdImpressionMessage marketingAdImpressionMessage = ToMarketingAdImpressionMessage(kioskId, impressions);
				if (marketingAdImpressionMessage != null)
				{
					LogHelper.Instance.Log("Enqueue call to remote IPromotionServices.MarketingAdImpressions, URL: {0}", KioskServicesProxy.Instance.Url);
					ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "MarketingAdImpression", marketingAdImpressionMessage);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PromotionServicesProxy.MarketingAdImpressions.", e);
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		});
	}

	public void SendKioskEvents(long kioskId, List<KioskEventData> events)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				Interlocked.Increment(ref m_working);
				KioskEventsMessage kioskEventsMessage = ToKioskEventsMessage(kioskId, events);
				if (kioskEventsMessage != null)
				{
					LogHelper.Instance.Log("Enqueue call to remote HealthServicesProxy.SendKioskEvents, URL: {0}", KioskServicesProxy.Instance.Url);
					ServiceLocator.Instance.GetService<IQueueService>().EnqueueKioskEvent(kioskEventsMessage);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in HealthServicesProxy.SendKioskEvents.", e);
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		});
	}

	private KioskEventsMessage ToKioskEventsMessage(long kioskId, List<KioskEventData> events)
	{
		KioskEventsMessage kioskEventsMessage = new KioskEventsMessage();
		try
		{
			kioskEventsMessage.MessageId = Guid.NewGuid();
			kioskEventsMessage.MessageType = "KioskEvents";
			kioskEventsMessage.KioskId = kioskId;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			kioskEventsMessage.EngineVersion = service["EngineVersion"];
			kioskEventsMessage.BundleVersion = service["ProductVersion"];
			kioskEventsMessage.KioskEventsData = CompressString(events.ToJson());
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in HealthServicesProxy.ToKioskEventsMessage.", e);
			kioskEventsMessage = null;
		}
		return kioskEventsMessage;
	}

	private static string CompressString(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		memoryStream.Position = 0L;
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Read(array, 0, array.Length);
		byte[] array2 = new byte[array.Length + 4];
		Buffer.BlockCopy(array, 0, array2, 4, array.Length);
		Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, array2, 0, 4);
		return Convert.ToBase64String(array2);
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
						remoteServiceResult.Properties["rent_qty"] = (dictionary.ContainsKey("RentQty") ? dictionary["RentQty"] : ((object)0));
						remoteServiceResult.Properties["rent_format"] = (dictionary.ContainsKey("RentFormat") ? dictionary["RentFormat"] : null);
						remoteServiceResult.Properties["get_qty"] = (dictionary.ContainsKey("GetQty") ? dictionary["GetQty"] : ((object)0));
						remoteServiceResult.Properties["get_format"] = (dictionary.ContainsKey("GetFormat") ? dictionary["GetFormat"] : null);
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CampaignTitles") ? (dictionary["CampaignTitles"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							ArrayList value = dictionary2["Titles"] as ArrayList;
							Dictionary<string, object> value2 = new Dictionary<string, object>
							{
								["include"] = (bool)dictionary2["Include"],
								["titles"] = value
							};
							remoteServiceResult.Properties["campaign_titles"] = value2;
						}
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						remoteServiceResult.Properties["product_type_id"] = (dictionary.ContainsKey("ProductTypeId") ? dictionary["ProductTypeId"] : null);
						remoteServiceResult.Properties["format_ids"] = (dictionary.ContainsKey("FormatIds") ? (dictionary["FormatIds"] as ArrayList) : null);
						remoteServiceResult.Properties["action_code"] = (dictionary.ContainsKey("ActionType") ? dictionary["ActionType"] : PromoActionCode.Rental.ToString());
						if (!remoteServiceResult.Success)
						{
							ArrayList arrayList = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList != null && arrayList.Count > 0)
							{
								if (arrayList[0] is Dictionary<string, object> dictionary3)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(PromotionValidationResponse?), (string)dictionary3["Code"]);
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
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
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
				int num = new Random().Next(1, 5);
				new AutoResetEvent(initialState: false).WaitOne(num * 1000);
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
					remoteServiceResult.Properties["rent_qty"] = 0;
					remoteServiceResult.Properties["rent_format"] = null;
					remoteServiceResult.Properties["get_qty"] = 0;
					remoteServiceResult.Properties["get_format"] = null;
					remoteServiceResult.Properties["campaign_titles"] = null;
					remoteServiceResult.Properties["product_type_id"] = null;
					remoteServiceResult.Properties["format_ids"] = null;
					remoteServiceResult.Properties["action_code"] = PromoActionCode.Rental.ToString();
				}
				else
				{
					remoteServiceResult.Success = false;
					remoteServiceResult.Properties["amount"] = 0;
					remoteServiceResult.Properties["promotion_intent"] = PromotionIntentCode.FixedValue.ToString();
					remoteServiceResult.Properties["response"] = PromotionValidationResponse.CodeNotValid.ToString();
					remoteServiceResult.Properties["rent_qty"] = 0;
					remoteServiceResult.Properties["rent_format"] = null;
					remoteServiceResult.Properties["get_qty"] = 0;
					remoteServiceResult.Properties["get_format"] = null;
					remoteServiceResult.Properties["campaign_titles"] = null;
					remoteServiceResult.Properties["product_type_id"] = null;
					remoteServiceResult.Properties["format_ids"] = null;
					remoteServiceResult.Properties["action_code"] = PromoActionCode.Rental.ToString();
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
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult?.ToObfuscatedString());
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
		for (int i = 0; i < promoCodes.Length; i++)
		{
			string[] array = promoCodes[i].Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
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

	private MarketingAdImpressionMessage ToMarketingAdImpressionMessage(long kioskId, List<MarketingAdImpressionData> impressions)
	{
		MarketingAdImpressionMessage marketingAdImpressionMessage = new MarketingAdImpressionMessage();
		try
		{
			marketingAdImpressionMessage.MessageId = Guid.NewGuid();
			marketingAdImpressionMessage.MessageType = "MarketingAdImpression";
			marketingAdImpressionMessage.KioskId = kioskId;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			marketingAdImpressionMessage.EngineVersion = service["EngineVersion"];
			marketingAdImpressionMessage.BundleVersion = service["ProductVersion"];
			marketingAdImpressionMessage.UseMessageControl = true;
			List<AdCampaign> list = new List<AdCampaign>();
			foreach (Guid campaignId in impressions.Select((MarketingAdImpressionData i) => i.CampaignId).Distinct())
			{
				IEnumerable<MarketingAdImpressionData> enumerable = impressions.Where((MarketingAdImpressionData c) => campaignId == c.CampaignId);
				List<AdImpression> list2 = new List<AdImpression>();
				foreach (MarketingAdImpressionData item in enumerable)
				{
					list2.Add(new AdImpression
					{
						ImpressionStart = item.ImpressionStartDate,
						ImpressionEnd = item.ImpressionEndDate,
						LocationId = item.AdLocation,
						Count = item.Count
					});
				}
				MarketingAdImpressionData marketingAdImpressionData = enumerable.First();
				list.Add(new AdCampaign
				{
					CampaignId = marketingAdImpressionData.CampaignId,
					Name = marketingAdImpressionData.Name,
					StartDate = marketingAdImpressionData.StartDate,
					EndDate = marketingAdImpressionData.EndDate,
					StartTime = marketingAdImpressionData.StartTime,
					EndTime = marketingAdImpressionData.EndTime,
					DaysOfWeek = marketingAdImpressionData.DaysOfWeek,
					Impressions = list2
				});
			}
			marketingAdImpressionMessage.MarketingAdCampaigns = list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in PromotionServicesProxy.ToMarketingAdImpressionMessage.", e);
			marketingAdImpressionMessage = null;
		}
		return marketingAdImpressionMessage;
	}
}
