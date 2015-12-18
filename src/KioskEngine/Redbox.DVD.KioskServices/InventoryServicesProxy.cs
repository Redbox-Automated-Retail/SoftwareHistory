using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

internal class InventoryServicesProxy : IInventoryServices
{
	private int m_working;

	public static InventoryServicesProxy Instance => Singleton<InventoryServicesProxy>.Instance;

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

	public bool EnableSellThryOfUsed
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Inventory Services", "EnableSellThruOfUsed", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Inventory Services", "EnableSellThruOfUsed", value);
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

	public void Return(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.Return, URL: {0}", KioskServicesProxy.Instance.Url);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				IQueueService service2 = ServiceLocator.Instance.GetService<IQueueService>();
				if (!DateTime.TryParse(returnTime, null, DateTimeStyles.AssumeLocal, out var result))
				{
					LogHelper.Instance.Log("Invalid date time passed to Return : {0}.", returnTime);
					result = DateTime.Now;
				}
				errorList = service2.Enqueue(200, "Return", new
				{
					MessageType = "Return",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					ReturnDate = result,
					ReturnType = returnType,
					Barcode = barcode,
					Deck = deck,
					Slot = slot
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.Return.", e);
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

	public void BackwardsDiscReturn(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.BackwardsDiscReturn, URL: {0}", KioskServicesProxy.Instance.Url);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "Return", new
				{
					MessageType = "Return",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					ReturnDate = DateTime.Parse(returnTime),
					ReturnType = returnType,
					Barcode = barcode,
					Deck = deck,
					Slot = slot
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.BackwardsDiscReturn.", e);
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
							Message = "Invoking callback from InventoryServicesProxy.BackwardsDiscReturn."
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
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			Guid messageId = Guid.NewGuid();
			int num = 0;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.SoftSync, URL: {0}", KioskServicesProxy.Instance.Url);
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
				errorList = ServiceLocator.Instance.GetService<IQueueService>().EnqueueOnlyOne(200, "SoftSync", message);
				Redbox.KioskEngine.ComponentModel.Error error = errorList.Find((Redbox.KioskEngine.ComponentModel.Error err) => err.Code == "QID");
				if (error != null)
				{
					num = Convert.ToInt32(error.Description);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.SoftSync.", e);
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
							remoteServiceResult.Properties["softsync_queue_id"] = num.ToString();
							remoteServiceResult.Properties["message_guid"] = messageId.ToString();
						}
						service2.EnqueueCallback(new RemoteServiceCallbackEntry
						{
							Callback = callback,
							Result = remoteServiceResult,
							Message = "Invoking callback from InventoryServicesProxy.Snapshot."
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

	public void SnapshotImmediate(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback)
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
				HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
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

	public void GetRentalCount(string storeNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["rental_count"] = 0;
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (HealthServicesProxy.Instance.IsOnline && !HealthServicesProxy.Instance.IsServerBusy && !StoreServicesProxy.Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IInventoryServices.GetRentalCount, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "RentalCount",
							MessageId = Guid.NewGuid(),
							KioskId = int.Parse(storeNumber),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						if (dictionary.ContainsKey("Count"))
						{
							remoteServiceResult.Properties["rental_count"] = (int)dictionary["Count"];
						}
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IInventoryServices.GetRentalCount.", LogEntryType.Info);
						remoteServiceResult.Properties["is_online"] = false;
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IInventoryServices.GetRentalCount", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.GetRentalCount.", e);
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
						Message = "Invoking completeCallback from InventoryServicesProxy.GetRentalCount."
					});
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

	public void MarkAsUnplayable(string storeNumber, string barcode, string email, string returnTime, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.UnplayableReturn, URL: {0}", KioskServicesProxy.Instance.Url);
				IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
				{
					MessageType = "UnplayableReturn",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					Barcode = barcode,
					Email = email,
					ReturnTime = returnTime
				});
				remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
				remoteServiceResult.Properties["is_unplayable"] = dictionary.ContainsKey("IsUnplayable") && (bool)dictionary["IsUnplayable"];
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.UnplayableReturn.", e);
			}
			finally
			{
				try
				{
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from InventoryServicesProxy.UnplayableReturn."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.UpdateFraudStatus, URL: {0}", KioskServicesProxy.Instance.Url);
				if (!DateTime.TryParseExact(readTime, "yyyyMMddHHmmss", null, DateTimeStyles.None, out var result))
				{
					result = DateTime.Now;
				}
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
				{
					MessageType = "DiscFraud",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					Barcode = barcode,
					IsFraud = bool.Parse(isFraud),
					TransactionDate = result
				});
				remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.UpdateFraudStatus.", e);
			}
			finally
			{
				try
				{
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
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

	public void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> actionList, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IInventoryServices.FmaDiscAction, URL: {0}", KioskServicesProxy.Instance.Url);
				string text = jobTimeStamp.Substring(0, 4) + "-" + jobTimeStamp.Substring(4, 2) + "-" + jobTimeStamp.Substring(6, 2);
				text = text + " " + jobTimeStamp.Substring(8, 2) + ":" + jobTimeStamp.Substring(10, 2) + ":00";
				DateTime result = default(DateTime);
				if (!DateTime.TryParse(text, null, DateTimeStyles.AssumeLocal, out result))
				{
					result = DateTime.Now;
				}
				string value = $"{result:yyyy-MM-ddTHH:mm:ss}";
				Dictionary<string, object> actionDate = new Dictionary<string, object>
				{
					{ "theDate", result.Ticks },
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
				LogHelper.Instance.Log("An unhandled exception was raised in InventoryServicesProxy.UpdateFraudStatus.", e);
			}
			finally
			{
				try
				{
					ServiceLocator.Instance.GetService<ICallbackService>().EnqueueCallback(new RemoteServiceCallbackEntry
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

	private InventoryServicesProxy()
	{
	}
}
