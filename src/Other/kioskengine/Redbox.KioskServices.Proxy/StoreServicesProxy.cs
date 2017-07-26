using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

public class StoreServicesProxy : IStoreServices
{
	internal sealed class State : IState
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

	internal sealed class Banner : IBanner
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

	internal sealed class Store : IStore
	{
		public int Id { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string ZipCode { get; set; }

		public string DueTime { get; set; }

		public string Address { get; set; }

		public string MarketName { get; set; }

		public string KaseyaMarketName { get; set; }
	}

	internal sealed class NewsEntry : INewsEntry
	{
		public int Id { get; set; }

		public string Body { get; set; }

		public string Header { get; set; }

		public DateTime EndOn { get; set; }

		public DateTime StartOn { get; set; }
	}

	private int m_working;

	public static StoreServicesProxy Instance => Singleton<StoreServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "LiveCall", value);
		}
	}

	public bool RunInitialSync
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "RunInitialSync", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "RunInitialSync", value);
		}
	}

	public bool IsStandAlone
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "StandAlone", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "StandAlone", value);
		}
	}

	public string StoreNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "ID", "N/A");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "ID", value);
		}
	}

	public string Environment
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Store", "Environment", null);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "Environment", value);
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service != null)
			{
				service.Filter["environment"] = value;
			}
		}
	}

	public string Application
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue<string>("Store", "Application", null);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "Application", value);
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (service != null)
			{
				service.Filter["application"] = value;
			}
		}
	}

	public string RebootTime
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			string text = service.GetValue<string>("Store", "RebootTime", null);
			if (string.IsNullOrEmpty(text))
			{
				string s = IdHelper.GeneratePseudoUniqueId(1, "12345");
				string s2 = IdHelper.GeneratePseudoUniqueId(2, "012345");
				text = $"{int.Parse(s):00}:{int.Parse(s2):00}";
				service.SetValue("Store", "RebootTime", text);
			}
			return text;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "RebootTime", value);
		}
	}

	public string[] PendingStates
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "PendingStates", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "PendingStates", value);
		}
	}

	public string[] PendingBanners
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "PendingBanners", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "PendingBanners", value);
		}
	}

	public string[] PendingStores
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "PendingStores", new string[0]);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "PendingStores", value);
		}
	}

	public string InitialSyncType
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "InitialSyncType", "Foreground");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "InitialSyncType", value);
		}
	}

	public bool SynchronizeTimeToServer
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "SynchronizeTimeToServer", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "SynchronizeTimeToServer", value);
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public string GetEnvironment()
	{
		return Environment;
	}

	public string GetApplication()
	{
		return Application;
	}

	public DateTime? GetRebootTime()
	{
		if (DateTime.TryParse(RebootTime, out var result))
		{
			return result;
		}
		return null;
	}

	public bool ShouldRunInitialSync()
	{
		return RunInitialSync;
	}

	public string GetInitialSyncType()
	{
		return InitialSyncType;
	}

	public void SetRunInitialSync(bool flag)
	{
		RunInitialSync = flag;
	}

	public void GetPendingStates(RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["states"] = new List<IState>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (HealthServicesProxy.Instance.IsOnline && !HealthServicesProxy.Instance.IsServerBusy && !Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IStoreServices.GetPendingStates, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetPendingStates",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						List<IState> list = new List<IState>();
						if (dictionary.ContainsKey("States"))
						{
							foreach (IDictionary<string, object> item in (ArrayList)dictionary["States"])
							{
								list.Add(new State
								{
									Id = (int)item["Id"],
									Name = (item.ContainsKey("Description") ? ((string)item["Description"]) : string.Empty)
								});
							}
						}
						remoteServiceResult.Properties["states"] = list;
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetPendingStates.", LogEntryType.Info);
						remoteServiceResult.Properties["is_online"] = false;
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingStates.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingStates.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = Instance.IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingStates."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void GetPendingBanners(int stateId, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["banners"] = new List<IBanner>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (HealthServicesProxy.Instance.IsOnline && !HealthServicesProxy.Instance.IsServerBusy && !Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IStoreServices.GetPendingBanners, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetPendingBanners",
							MessageId = Guid.NewGuid(),
							StateId = stateId,
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						List<IBanner> list = new List<IBanner>();
						if (dictionary.ContainsKey("Banners"))
						{
							foreach (IDictionary<string, object> item in (ArrayList)dictionary["Banners"])
							{
								list.Add(new Banner
								{
									Id = (int)item["Id"],
									Name = (string)item["Name"]
								});
							}
						}
						remoteServiceResult.Properties["banners"] = list;
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetPendingBanners.", LogEntryType.Info);
						remoteServiceResult.Properties["is_online"] = false;
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingBanners.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingBanners.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = Instance.IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingBanners."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void GetPendingStores(int bannerId, int stateId, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			remoteServiceResult.Properties["stores"] = new List<IStore>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (HealthServicesProxy.Instance.IsOnline && !HealthServicesProxy.Instance.IsServerBusy && !Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IStoreServices.GetPendingStores, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetPendingKiosks",
							MessageId = Guid.NewGuid(),
							BannerId = bannerId,
							StateId = stateId,
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"]
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("ServerName"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
						}
						List<IStore> list = new List<IStore>();
						if (dictionary.ContainsKey("PendingKiosks"))
						{
							foreach (IDictionary<string, object> item in (ArrayList)dictionary["PendingKiosks"])
							{
								list.Add(new Store
								{
									Id = (int)item["Id"],
									City = (string)item["City"],
									State = (string)item["State"],
									ZipCode = (string)item["ZipCode"],
									DueTime = (string)item["DueTime"],
									Address = (string)item["Address"],
									MarketName = (string)item["MarketName"],
									KaseyaMarketName = (string)item["KaseyaMarketName"]
								});
							}
						}
						remoteServiceResult.Properties["stores"] = list;
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetPendingStores.", LogEntryType.Info);
						remoteServiceResult.Properties["is_online"] = false;
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetPendingStores.", LogEntryType.Info);
					remoteServiceResult.Success = true;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetPendingStores.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = Instance.IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetPendingStores."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void GetOpsMarket(string storeNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCall)
				{
					if (HealthServicesProxy.Instance.IsOnline && !HealthServicesProxy.Instance.IsServerBusy && !Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IStoreServices.GetOpsMarket, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
						{
							MessageType = "GetOpsMarket",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(storeNumber)
						});
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["ops_market"] = (dictionary.ContainsKey("OpsMarket") ? dictionary["OpsMarket"] : "UnNamed");
						remoteServiceResult.Properties["kaseya_server"] = (dictionary.ContainsKey("KaseyaServer") ? dictionary["KaseyaServer"] : "Kaseya6");
					}
					else
					{
						LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IStoreServices.GetOpsMarket.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["ops_market"] = "UnNamed";
						remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IStoreServices.GetOpsMarket.", LogEntryType.Info);
					remoteServiceResult.Success = true;
					remoteServiceResult.Properties["is_online"] = false;
					remoteServiceResult.Properties["ops_market"] = "UnNamed";
					remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in StoreServicesProxy.GetOpsMarket.", e);
				remoteServiceResult.Success = false;
				remoteServiceResult.Properties["is_online"] = false;
				remoteServiceResult.Properties["ops_market"] = "UnNamed";
				remoteServiceResult.Properties["kaseya_server"] = "Kaseya6";
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = Instance.IsStandAlone;
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from StoreServicesProxy.GetOpsMarket."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private StoreServicesProxy()
	{
	}
}
