using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.Local;

namespace Redbox.KioskServices.Proxy;

internal class InventoryServicesProxy : IInventoryServices
{
	private int m_working;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Inventory Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Inventory Services", "LiveCall", value);
		}
	}

	public bool EnableOutOfStockDisplay
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Inventory Services", "EnableOutOfStockDisplay", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Inventory Services", "EnableOutOfStockDisplay", value);
		}
	}

	public bool EnableSellThryOfNew
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Inventory Services", "EnableSellThruOfNew", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Inventory Services", "EnableSellThruOfnew", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Inventory Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Inventory Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Inventory Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Inventory Services", "SimulateCommunicationFailure", value);
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	public void Deactivate()
	{
	}

	public void Return(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, RemoteServiceCallback callback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendReturnToKioskClientService)
		{
			KioskClientServiceReturn(storeNumber, returnType, barcode, deck, slot, returnTime, failedSecurityRead, fileName, callback);
		}
		else
		{
			KioskServicesReturn(storeNumber, returnType, barcode, deck, slot, returnTime, failedSecurityRead, fileName, callback);
		}
	}

	private async void KioskClientServiceReturn(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, RemoteServiceCallback callback)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			IReturnResult returnResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.ReturnAsync(storeNumber, returnType, barcode, returnTime, deck, slot, failedSecurityRead, fileName);
			if (returnResult == null || !returnResult.Success)
			{
				errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", "Non 200 level result returned from the ReturnAsync call..", "") };
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.KioskClientServiceReturn.", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
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
							Message = "Invoking callback from InventoryServicesProxy.Return."
						});
					}
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
	}

	private void KioskServicesReturn(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, RemoteServiceCallback callback)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string bundleVersion = service["ProductVersion"];
		string engineVersion = service["EngineVersion"];
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.Return, URL: {0}", KioskServicesProxy.Instance.Url);
				if (!DateTime.TryParse(returnTime, null, DateTimeStyles.AssumeLocal, out var result))
				{
					LogHelper.Instance.Log("Invalid date time passed to Return : {0}.", returnTime);
					result = DateTime.Now;
				}
				IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
				ReturnMessage returnMessage = new ReturnMessage
				{
					MessageType = "Return",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = engineVersion,
					BundleVersion = bundleVersion,
					ReturnDate = result,
					ReturnType = returnType,
					Barcode = barcode,
					Deck = deck,
					Slot = slot,
					FailedSecurityRead = failedSecurityRead
				};
				object obj = null;
				obj = (string.IsNullOrEmpty(barcode) ? ((object)returnMessage) : ((object)new ReturnMessageWithClientData
				{
					ClientData = new ReturnMessageClientData
					{
						Barcode = barcode,
						FileName = fileName
					},
					ReturnMessage = returnMessage
				}));
				errorList = service2.Enqueue(200, "Return", obj);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.KioskServicesReturn.", e);
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
							Message = "Invoking callback from InventoryServicesProxy.Return."
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

	public void Snapshot(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendInventorySnapshotToKioskClientService)
		{
			KioskClientServiceSnapshot(storeNumber, items, callback);
		}
		else
		{
			KioskServicesSnapshot(storeNumber, items, callback);
		}
	}

	private void KioskServicesSnapshot(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			Guid messageId = Guid.NewGuid();
			int newMessageId = 0;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("InventoryServicesProxy.KioskServicesSnapshot - Enqueue soft sync call to kiosk services, URL: {0}", KioskServicesProxy.Instance.Url);
				List<RemovedQueueItem> removedQueueItems = ServiceLocator.Instance.GetService<IDataService>().GetRemovedQueueItems();
				if (removedQueueItems.Any())
				{
					LogHelper.Instance.Log(string.Format("InventoryServicesProxy.KioskServicesSnapshot - appending the following removed barcodes: {0}", string.Join(",", removedQueueItems.Select((RemovedQueueItem ri) => ri.Barcode))));
				}
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				List<IInventoryItem> list = new List<IInventoryItem>(items);
				list.AddRange(removedQueueItems.ToInventoryItems());
				var message = new
				{
					MessageType = "SoftSync",
					MessageId = messageId,
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					Discs = list,
					RemovedQueueItems = removedQueueItems
				};
				errorList = ServiceLocator.Instance.GetService<IQueueService>().EnqueueOnlyOne(200, "SoftSync", message, out newMessageId);
				if (errorList.Count > 0)
				{
					LogHelper.Instance.Log($"InventoryServicesProxy.KioskServicesSnapshot - KS call to snapshot failed!  Errors returned: {errorList.ToJson()}");
				}
				else
				{
					LogHelper.Instance.Log("InventoryServicesProxy.KioskServicesSnapshot - KS snapshot call successful");
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryServicesProxy.KioskServicesSnapshot - An unhandled exception was raised!", e);
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
							Message = "Invoking callback from InventoryServicesProxy.KioskServicesSnapshot."
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

	private async void KioskClientServiceSnapshot(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceSnapshot - Enqueue soft sync call to kiosk client service, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
			List<RemovedQueueItem> removedItems = ServiceLocator.Instance.GetService<IDataService>().GetRemovedQueueItems();
			LogHelper.Instance.Log(string.Format("InventoryServicesProxy.KioskClientServiceSnapshot - appending the following removed barcodes: {0}", string.Join(",", removedItems.Select((RemovedQueueItem ri) => ri.Barcode))));
			List<IInventoryItem> list = new List<IInventoryItem>(items);
			list.AddRange(removedItems.ToInventoryItems());
			IKioskClientServiceResult kioskClientServiceResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Inventory.SnapshotAsync(storeNumber, list);
			if (kioskClientServiceResult == null || !kioskClientServiceResult.Success)
			{
				LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceSnapshot - KCS call to snapshot failed!");
				if (kioskClientServiceResult.Error != null)
				{
					LogHelper.Instance.Log($"InventoryServicesProxy.KioskClientServiceSnapshot - error returned: {kioskClientServiceResult.Error.Code}, {kioskClientServiceResult.Error.Description}");
				}
				return;
			}
			LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceSnapshot - KCS snapshot call successful");
			if (removedItems.Any())
			{
				List<Task> processingTasks = new List<Task>();
				removedItems.ForEach(delegate(RemovedQueueItem i)
				{
					processingTasks.Add(Delete(i));
				});
				await Task.WhenAll(processingTasks);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceSnapshot - An unhandled exception was raised!", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
		}
		finally
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (callback != null && service != null)
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
						Message = "Invoking callback from InventoryServicesProxy.KioskClientServiceSnapshot."
					});
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
		static async Task Delete(RemovedQueueItem item)
		{
			await Task.Run(delegate
			{
				LogHelper.Instance.Log($"SoftSyncMessageProcessorService.Delete - deleting removedqueue record for barcode: {item.Barcode} with hash: {item.HashId}");
				try
				{
					ServiceLocator.Instance.GetService<IDataService>().DeleteRemovedQueueItem(item);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"SoftSyncMessageProcessorService.Delete - exception trying to delete removedqueue record for barcode: {item.Barcode}!", e);
					string storeNumber2 = (ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0).ToString();
					ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(storeNumber2, "TitleEmpty", "RemovedQueueDeleteFail", $"Unable to delete RemovedQueue record for hash: {item.HashId}, barcode: {item.Barcode}", DateTime.Now, null);
				}
			});
		}
	}

	public void SnapshotImmediate(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendInventorySnapshotToKioskClientService)
		{
			KioskClientServiceSnapshot(storeNumber, items, callback);
		}
		else
		{
			KioskServicesSnapshotImmediate(storeNumber, items, callback);
		}
	}

	private void KioskServicesSnapshotImmediate(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			Guid messageId = Guid.NewGuid();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Live call to remote IInventoryServices.SnapshotImmediate, URL: {0}", KioskServicesProxy.Instance.Url);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				var message = new
				{
					MessageType = "SoftSync",
					MessageId = messageId,
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					Discs = new List<IInventoryItem>(items)
				};
				IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
				ServiceLocator.Instance.GetService<IHealthServices>().UpdateOnlineStatus(dictionary);
				remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.SnapshotImmediate", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
					if (callback != null)
					{
						service2?.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from InventoryServicesProxy.SnapshotImmediate"
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

	public IInventoryItem CreateInventoryItem(string barcode, int? deck, int? slot, InventoryItemStatus status)
	{
		return new InventoryItem
		{
			Barcode = barcode,
			Deck = deck,
			Slot = slot,
			Status = status
		};
	}

	public async void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, RemoteServiceCallback completeCallback, long? transactionId = null, long? discId = null, DateTime? returnDate = null)
	{
		Dictionary<string, string> properties = new Dictionary<string, string> { { "FraudType", "Paper" } };
		if (discId.HasValue)
		{
			properties.Add("DiscId", discId.Value.ToString());
		}
		if (returnDate.HasValue)
		{
			properties.Add("ReturnDate", returnDate.Value.ToString("o"));
		}
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendDiscFraudToKioskClientService)
		{
			if (!DateTime.TryParseExact(readTime, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
			{
				result = DateTime.Now;
			}
			await KioskClientServiceUpdateFraudStatus(storeNumber, barcode, result, completeCallback, properties);
		}
		else
		{
			KioskServicesUpdateFraudStatus(storeNumber, barcode, isFraud, readTime, completeCallback, transactionId, properties);
		}
	}

	private void KioskServicesUpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, RemoteServiceCallback completeCallback, long? transactionId = null, Dictionary<string, string> properties = null)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueuing DiscFraud api call to KioskServices");
				if (!DateTime.TryParseExact(readTime, "yyyyMMddHHmmss", null, DateTimeStyles.None, out var result))
				{
					result = DateTime.Now;
				}
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
				var message = new
				{
					MessageType = "DiscFraud",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					Barcode = barcode,
					IsFraud = bool.Parse(isFraud),
					TransactionDate = result,
					TransactionId = transactionId,
					PropertyBag = properties
				};
				errorList = service2.Enqueue(60, "DiscFraud", message);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.UpdateFraudStatus.", e);
			}
			finally
			{
				try
				{
					ICallbackService service3 = ServiceLocator.Instance.GetService<ICallbackService>();
					if (errorList != null)
					{
						remoteServiceResult.Errors.AddRange(errorList);
						remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
					}
					service3.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from InventoryServicesProxy.UpdateFraudStatus."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private async Task KioskClientServiceUpdateFraudStatus(string storeNumber, string barcode, DateTime readDate, RemoteServiceCallback callback, Dictionary<string, string> properties = null)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscFraudStatus - Enqueue call to kiosk client service, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
			IKioskClientServiceResult kioskClientServiceResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Inventory.DiscFraudAsync(storeNumber, barcode, readDate, properties);
			if (kioskClientServiceResult == null || !kioskClientServiceResult.Success)
			{
				LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscFraudStatus - KCS call to disc action failed!");
				if (kioskClientServiceResult.Error != null)
				{
					LogHelper.Instance.Log($"InventoryServicesProxy.KioskClientServiceDiscFraudStatus - error returned: {kioskClientServiceResult.Error.Code}, {kioskClientServiceResult.Error.Description}");
				}
			}
			else
			{
				LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscFraudStatus - KCS disc action call successful");
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscFraudService - An unhandled exception was raised!", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
		}
		finally
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (callback != null && service != null)
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
						Message = "Invoking callback from InventoryServicesProxy.KioskClientServiceDiscFraudService."
					});
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
	}

	public void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> actionList, RemoteServiceCallback completeCallback)
	{
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendDiscActionToKioskClientService)
		{
			KioskClientServiceDiscAction(storeNumber, username, jobName, jobTimeStamp, actionList, completeCallback);
		}
		else
		{
			KioskServicesDiscAction(storeNumber, username, jobName, jobTimeStamp, actionList, completeCallback);
		}
	}

	private void KioskServicesDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> actionList, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.FmaDiscAction, URL: {0}", KioskServicesProxy.Instance.Url);
				DateTime dateTime = UnLUATimmestamp(jobTimeStamp);
				string value = $"{dateTime:yyyy-MM-ddTHH:mm:ss}";
				Dictionary<string, object> actionDate = new Dictionary<string, object>
				{
					{ "theDate", dateTime.Ticks },
					{ "dateTimeKind", "Local" },
					{ "dateString", value }
				};
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
				{
					MessageType = "DiscAction",
					UserName = username,
					JobName = jobName,
					ActionDate = actionDate,
					Actions = actionList,
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					UserMessageControl = false
				});
				remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.KioskServicesDiscAction.", e);
			}
			finally
			{
				try
				{
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from InventoryServicesProxy.KioskServicesDiscAction."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private async void KioskClientServiceDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> actionList, RemoteServiceCallback callback)
	{
		ErrorList errors = null;
		try
		{
			Interlocked.Increment(ref m_working);
			LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscAction - Enqueue call to kiosk client service, URL: {0}", ServiceLocator.Instance.GetService<IConfiguration>().KioskClientServiceBaseUrl);
			DateTime jobTimestamp = UnLUATimmestamp(jobTimeStamp);
			IKioskClientServiceResult kioskClientServiceResult = await ServiceLocator.Instance.GetService<IKioskClientService>().Inventory.DiscActionAsync(storeNumber, username, jobName, jobTimestamp, actionList);
			if (kioskClientServiceResult == null || !kioskClientServiceResult.Success)
			{
				LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscAction - KCS call to disc action failed!");
				if (kioskClientServiceResult.Error != null)
				{
					LogHelper.Instance.Log($"InventoryServicesProxy.KioskClientServiceDiscAction - error returned: {kioskClientServiceResult.Error.Code}, {kioskClientServiceResult.Error.Description}");
				}
			}
			else
			{
				LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscAction - KCS disc action call successful");
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("InventoryServicesProxy.KioskClientServiceDiscAction - An unhandled exception was raised!", ex);
			errors = new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("Q999", ex.Message, ex) };
		}
		finally
		{
			try
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (callback != null && service != null)
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
						Message = "Invoking callback from InventoryServicesProxy.KioskClientServiceDiscAction."
					});
				}
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		}
	}

	private DateTime UnLUATimmestamp(string source)
	{
		string text = source.Substring(0, 4) + "-" + source.Substring(4, 2) + "-" + source.Substring(6, 2);
		text = text + " " + source.Substring(8, 2) + ":" + source.Substring(10, 2) + ":00";
		if (DateTime.TryParse(text, null, DateTimeStyles.AssumeLocal, out var result))
		{
			return result;
		}
		return DateTime.Now;
	}

	internal InventoryServicesProxy()
	{
	}
}
