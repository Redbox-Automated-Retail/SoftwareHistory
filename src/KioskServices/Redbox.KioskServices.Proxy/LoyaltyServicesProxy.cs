using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.KioskServices.Proxy.Request;
using Redbox.KioskServices.Proxy.Requests;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskServices.Proxy;

public class LoyaltyServicesProxy : ILoyaltyServices
{
	private BindingList<ItemAccrualRedemptions> _items;

	private int m_working;

	public static LoyaltyServicesProxy Instance => Singleton<LoyaltyServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "LiveCall", value);
		}
	}

	public bool SimulateAccrualSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "SimulateAccrualSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "SimulateAccrualSuccess", value);
		}
	}

	public bool SimulateRedemptionSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "SimulateRedemptionSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "SimulateRedemptionSuccess", value);
		}
	}

	public int DefaultAccrualPoints
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "DefaultAccrualPoints", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "DefaultAccrualPoints", value);
		}
	}

	public int DefaultRedemptionPoints
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "DefaultRedemptionPoints", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "DefaultRedemptionPoints", value);
		}
	}

	public EstimateAccrualErrors AccrualError
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "AccrualError", "Error");
			return (EstimateAccrualErrors)Enum.Parse(typeof(EstimateAccrualErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "AccrualError", value);
		}
	}

	public EstimateRedemptionErrors RedemptionError
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Loyalty Services", "RedemptionError", "Error");
			return (EstimateRedemptionErrors)Enum.Parse(typeof(EstimateRedemptionErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Loyalty Services", "RedemptionError", value);
		}
	}

	public BindingList<ItemAccrualRedemptions> Items
	{
		get
		{
			if (_items == null)
			{
				_items = new BindingList<ItemAccrualRedemptions>();
			}
			IRentalShoppingCartService session = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (session?.CurrentRentalShoppingCart?.Items != null && session.CurrentRentalShoppingCart.Items.Any())
			{
				_items.ToList().RemoveAll((ItemAccrualRedemptions x) => !session.CurrentRentalShoppingCart.Items.Any((IRentalShoppingCartItem y) => x.ProductId == y.ProductId));
			}
			else
			{
				_items.Clear();
			}
			session?.CurrentRentalShoppingCart?.Items?.Where((IRentalShoppingCartItem x) => !_items.Any((ItemAccrualRedemptions y) => y.ProductId == x.ProductId)).ForEach(delegate(IRentalShoppingCartItem x)
			{
				_items.Add(new ItemAccrualRedemptions
				{
					ProductId = x.ProductId,
					Name = x.SortName,
					AccrualPoints = DefaultAccrualPoints,
					RedemptionPoints = DefaultRedemptionPoints
				});
			});
			return _items;
		}
	}

	private LoyaltyServicesProxy()
	{
	}

	public void EstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformLiveEstimatePointsAccrued(customerProfileNumber, completeCallback);
		}
		else
		{
			PerformSimulatedEstimatePointsAccrued(customerProfileNumber, completeCallback);
		}
	}

	private void PerformLiveEstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!StoreServicesProxy.Instance.IsStandAlone)
				{
					LogHelper.Instance.Log("Live call to remote ILoyaltyServices.EstimatePointsAccrued, URL: {0}", KioskServicesProxy.Instance.Url);
					CartMessage message = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateAccrual");
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
					HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("Items"))
					{
						remoteServiceResult.Properties["Items"] = dictionary["Items"] as ArrayList;
					}
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
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(EstimateAccrualErrors?), (string)dictionary2["Code"]);
								remoteServiceResult.Properties["response"] = fieldsForResponseCode ?? ((object)EstimateAccrualErrors.Error);
							}
						}
						else
						{
							remoteServiceResult.Properties["response"] = EstimateAccrualErrors.Error;
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to login user.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = EstimateAccrualErrors.LoyaltyError;
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformLiveEstimatePointsAccrued.", e);
				remoteServiceResult.Properties["response"] = EstimateAccrualErrors.Error;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformLiveEstimatePointsAccrued."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void PerformSimulatedEstimatePointsAccrued(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Thread.Sleep(1500);
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Simulate call to remote ILoyaltyServices.EstimatePointsAccrued");
				CartMessage instance = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateAccrual");
				LogHelper.Instance.Log(instance.ToJson(), LogEntryType.Info);
				if (SimulateAccrualSuccess)
				{
					ArrayList list = new ArrayList();
					Items.ForEach(delegate(ItemAccrualRedemptions x)
					{
						Dictionary<string, object> value = new Dictionary<string, object>
						{
							{ "ProductId", x.ProductId },
							{ "Points", x.AccrualPoints }
						};
						list.Add(value);
					});
					remoteServiceResult.Properties["Items"] = list;
				}
				else
				{
					remoteServiceResult.Properties["response"] = AccrualError;
				}
				remoteServiceResult.Success = SimulateAccrualSuccess;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformSimulatedEstimatePointsAccrued.", e);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
					LogHelper.Instance.Log("IRemoteServiceResult: {0}", remoteServiceResult.ToObfuscatedString());
					ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = completeCallback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformSimulatedEstimatePointsAccrued."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void EstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		if (LiveCall)
		{
			PerformLiveEstimateRedemptionPoints(customerProfileNumber, completeCallback);
		}
		else
		{
			PerformSimulatedEstimateRedemptionPoints(customerProfileNumber, completeCallback);
		}
	}

	private void PerformLiveEstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				if (!StoreServicesProxy.Instance.IsStandAlone)
				{
					LogHelper.Instance.Log("Live call to remote ILoyaltyServices.EstimateRedemptionPoints, URL: {0}", KioskServicesProxy.Instance.Url);
					CartMessage message = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateRedemption");
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
					HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					if (dictionary.ContainsKey("Items"))
					{
						remoteServiceResult.Properties["Items"] = dictionary["Items"] as ArrayList;
					}
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
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(EstimateRedemptionErrors?), (string)dictionary2["Code"]);
								remoteServiceResult.Properties["response"] = fieldsForResponseCode ?? ((object)EstimateRedemptionErrors.Error);
							}
						}
						else
						{
							remoteServiceResult.Properties["response"] = EstimateRedemptionErrors.Error;
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to login user.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = EstimateRedemptionErrors.LoyaltyError;
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformLiveEstimateRedemptionPoints.", e);
				remoteServiceResult.Properties["response"] = EstimateRedemptionErrors.Error;
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
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformLiveEstimateRedemptionPoints."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void PerformSimulatedEstimateRedemptionPoints(string customerProfileNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				Thread.Sleep(1500);
				LogHelper.Instance.Log("Simulate call to remote ILoyaltyServices.EstimateRedemptionPoints, URL: {0}", KioskServicesProxy.Instance.Url);
				CartMessage instance = ProxyMessageHelper.CreateCartMessage(customerProfileNumber, "EstimateRedemption");
				LogHelper.Instance.Log(instance.ToJson(), LogEntryType.Info);
				if (SimulateRedemptionSuccess)
				{
					ArrayList list = new ArrayList();
					Items.ForEach(delegate(ItemAccrualRedemptions x)
					{
						Dictionary<string, object> value = new Dictionary<string, object>
						{
							{ "ProductId", x.ProductId },
							{ "Points", x.RedemptionPoints }
						};
						list.Add(value);
					});
					remoteServiceResult.Properties["Items"] = list;
				}
				else
				{
					remoteServiceResult.Properties["response"] = RedemptionError;
				}
				remoteServiceResult.Success = SimulateRedemptionSuccess;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in LoyaltyServicesProxy.PerformSimulatedEstimateRedemptionPoints.", e);
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
						Message = "Invoking completeCallback from LoyaltyServicesProxy.PerformSimulatedEstimateRedemptionPoints."
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
