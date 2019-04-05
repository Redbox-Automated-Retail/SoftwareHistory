using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Threading;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Configuration;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.BrokerServices.Proxy;

public class ReservationServicesProxy : IReservationServices
{
	internal sealed class Reservation : IReservation
	{
		public string CardId { get; set; }

		public DateTime TransactionDate { get; set; }

		public long ReferenceNumber { get; set; }

		public IDictionary<string, ICollection<string>> Barcodes { get; set; }
	}

	private int m_working;

	public static ReservationServicesProxy Instance => Singleton<ReservationServicesProxy>.Instance;

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Services", "LiveCall", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Services", "LiveCall", value);
		}
	}

	public bool SimulateServerBusyError
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Services", "SimulateServerBusyFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Services", "SimulateServerBusyFailure", value);
		}
	}

	public bool SimulateCommunicationsFailure
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Reservation Services", "SimulateCommunicationFailure", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Reservation Services", "SimulateCommunicationFailure", value);
		}
	}

	internal ReservationRequestCallback3 RequestCallback3 { get; private set; }

	internal CancelReservationCallback CancelRequestCallback { get; private set; }

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
		BrokerServicesProxy.Instance.Reset();
	}

	public void Pickup(string storeNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, bool isBlocklisted, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
				if (service != null)
				{
					IServiceProxy serviceProxy = service.GetObject<IServiceProxy>("Kiosk Services Proxy");
					LogHelper.Instance.Log("Enqueue call to remote IReservationServices.Pickup, URL: {0}", serviceProxy.GetProperty<string>("Url"));
				}
				IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
				var anon = new
				{
					MessageType = "Pickup",
					MessageId = Guid.NewGuid(),
					EngineVersion = service2["EngineVersion"],
					BundleVersion = service2["ProductVersion"],
					Email = emailAddress,
					KioskId = int.Parse(storeNumber),
					ReferenceNumber = referenceNumber,
					Blocklisted = isBlocklisted,
					PickupDateTime = DateTime.Now,
					Items = new Dictionary<string, object>()
				};
				foreach (KeyValuePair<string, ReadOnlyCollection<IProductItem>> pickupResult in pickupResults)
				{
					var anon2 = new
					{
						Items = new List<object>()
					};
					foreach (IProductItem item2 in pickupResult.Value)
					{
						var item = new
						{
							Barcode = item2.Barcode,
							MultiDisc = item2.MultiDisc,
							VendStatus = item2.Status.ToVendStatus()
						};
						anon2.Items.Add(item);
					}
					anon.Items[pickupResult.Key] = anon2;
				}
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "Pickup", anon);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ReservationServicesProxy.Pickup.", e);
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
							Message = "Invoking callback from ReservationServicesProxy.Pickup."
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

	public void ModResvPickup(string storeNumber, string emailAddress, long referenceNumber, string sessionId, bool modifiedCart, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
				if (service != null)
				{
					IServiceProxy serviceProxy = service.GetObject<IServiceProxy>("Kiosk Services Proxy");
					LogHelper.Instance.Log("Enqueue call to remote IReservationServices.ModResvPickup, URL: {0}", serviceProxy.GetProperty<string>("Url"));
				}
				IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
				IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
				if (session == null)
				{
					LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
				}
				else
				{
					var anon = new
					{
						MessageType = "ModResvPickup",
						MessageId = Guid.NewGuid(),
						EngineVersion = service2["EngineVersion"],
						BundleVersion = service2["ProductVersion"],
						Email = emailAddress,
						KioskId = int.Parse(storeNumber),
						ReferenceNumber = referenceNumber,
						Items = new Dictionary<string, object>(),
						PickupDate = DateTime.Now,
						ModifiedCart = modifiedCart
					};
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
								Prices = new Dictionary<string, decimal>(),
								VendStatus = item.Status.ToVendStatus(),
								SourceType = item.SourceType
							};
							foreach (KeyValuePair<string, decimal> price in item.Prices)
							{
								string key = price.Key.Replace("_", string.Empty);
								anon3.Prices[key] = price.Value;
							}
							anon2.Items.Add(anon3);
						}
						anon.Items[group.Key] = anon2;
					}
					errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "ModResvPickup", anon);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ReservationServicesProxy.ModResvPickup.", e);
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
							Message = "Invoking callback from ReservationServicesProxy.ModResvPickup."
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

	public void Expire(string storeNumber, long referenceNumber, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
				if (service != null)
				{
					IServiceProxy serviceProxy = service.GetObject<IServiceProxy>("Kiosk Services Proxy");
					LogHelper.Instance.Log("Enqueue call to remote IReservationServices.Expire, URL: {0}", serviceProxy.GetProperty<string>("Url"));
				}
				IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "Expire", new
				{
					MessageType = "Expire",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service2["EngineVersion"],
					BundleVersion = service2["ProductVersion"],
					ReferenceNumber = referenceNumber
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ReservationServicesProxy.Expire.", e);
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
							Message = "Invoking callback from ReservationServicesProxy.Expire."
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

	public void Cancel(string storeNumber, long referenceNumber, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
				if (service != null)
				{
					IServiceProxy serviceProxy = service.GetObject<IServiceProxy>("Kiosk Services Proxy");
					LogHelper.Instance.Log("Enqueue call to remote IReservationServices.Cancel, URL: {0}", serviceProxy.GetProperty<string>("Url"));
				}
				IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "CancelReservation", new
				{
					MessageType = "CancelReservation",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service2["EngineVersion"],
					BundleVersion = service2["ProductVersion"],
					ReferenceNumber = referenceNumber
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ReservationServicesProxy.Cancel.", e);
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
							Message = "Invoking callback from ReservationServicesProxy.Cancel."
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

	public void ValidateReservation(string storeNumber, long referenceNumber, RemoteServiceCallback callback)
	{
		if (!LiveCall)
		{
			return;
		}
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			Thread.Sleep(30000);
			try
			{
				Interlocked.Increment(ref m_working);
				IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
				if (service != null)
				{
					IServiceProxy serviceProxy = service.GetObject<IServiceProxy>("Kiosk Services Proxy");
					LogHelper.Instance.Log("Enqueue call to remote IReservationServices.ValidateReservation, URL: {0}", serviceProxy.GetProperty<string>("Url"));
				}
				IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "ValidateReservation", new
				{
					MessageType = "ValidateReservation",
					MessageId = Guid.NewGuid(),
					KioskId = int.Parse(storeNumber),
					EngineVersion = service2["EngineVersion"],
					BundleVersion = service2["ProductVersion"],
					ReferenceNumber = referenceNumber
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ReservationServicesProxy.ValidateReservation.", e);
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
							Message = "Invoking callback from ReservationServicesProxy.ValidateReservation."
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

	public void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback3 reservationRequestCallback, CancelReservationCallback cancelRequestCallback)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["Runtime"] = "Kiosk Engine";
		dictionary["Version"] = AssemblyInfoHelper.GetVersion(Assembly.GetExecutingAssembly());
		dictionary["PricingPolicy"] = prices;
		dictionary["GiftCardsEnabled"] = false;
		dictionary["RecoOnPickup"] = false;
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null)
		{
			if (service.TryGetValue<bool>("system", "General", "GiftCardsEnabled", out var value))
			{
				dictionary["GiftCardsEnabled"] = value;
			}
			if (service.TryGetValue<bool>("system", "General", "RecommendationOnPickup", out var value2))
			{
				dictionary["RecoOnPickup"] = value2;
			}
		}
		dictionary["AuthorizeAtPickup"] = Configuration.Instance.AuthorizeAtPickup;
		dictionary["CancelReservation"] = true;
		dictionary["Features"] = Features.MultiDiscVend;
		if (int.TryParse(storeNumber, out var result))
		{
			RequestCallback3 = reservationRequestCallback;
			CancelRequestCallback = cancelRequestCallback;
			BrokerServicesProxy.Instance.Register(result, dictionary.ToJson());
		}
	}

	public void GetReservations(string storeNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			bool flag = false;
			bool flag2 = false;
			IServiceProxy serviceProxy = null;
			IObjectCacheService service = ServiceLocator.Instance.GetService<IObjectCacheService>();
			if (service != null)
			{
				serviceProxy = service.GetObject<IServiceProxy>("Kiosk Services Proxy");
				if (serviceProxy != null)
				{
					flag = serviceProxy.GetProperty<bool>("IsOnline");
					flag2 = serviceProxy.GetProperty<bool>("IsStandAlone");
					RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
					remoteServiceResult.Properties["is_online"] = true;
					remoteServiceResult.Properties["reservations"] = new List<IReservation>();
					try
					{
						Interlocked.Increment(ref m_working);
						if (LiveCall)
						{
							if (flag && !flag2)
							{
								LogHelper.Instance.Log("Live call to remote IReservationServices.GetReservations, URL: {0}", serviceProxy.GetProperty<string>("Url"));
								IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
								IDictionary<string, object> dictionary = (IDictionary<string, object>)serviceProxy.ExecuteMethod("ExecuteMessage", new
								{
									MessageType = "GetReservations",
									MessageId = Guid.NewGuid(),
									KioskId = int.Parse(storeNumber),
									EngineVersion = service2["EngineVersion"],
									BundleVersion = service2["ProductVersion"]
								});
								remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
								if (dictionary.ContainsKey("ServerName"))
								{
									remoteServiceResult.Properties["server_name"] = dictionary["ServerName"];
								}
								List<IReservation> list = new List<IReservation>();
								if (dictionary.ContainsKey("Reservations"))
								{
									foreach (IDictionary<string, object> item in (ArrayList)dictionary["Reservations"])
									{
										IDictionary<string, object> dictionary3 = (IDictionary<string, object>)item["TransactionDate"];
										DateTime transactionDate = new DateTime((long)dictionary3["theDate"]);
										Reservation reservation = new Reservation
										{
											Barcodes = new Dictionary<string, ICollection<string>>(),
											CardId = (string)item["CardId"],
											TransactionDate = transactionDate,
											ReferenceNumber = Convert.ToInt64(item["ReferenceNumber"])
										};
										foreach (KeyValuePair<string, object> item2 in (IDictionary<string, object>)item["Contents"])
										{
											List<string> barcodes = new List<string>();
											Array.ForEach(((ArrayList)item2.Value).ToArray(), delegate(object item)
											{
												barcodes.Add((string)item);
											});
											reservation.Barcodes[item2.Key] = barcodes;
										}
										list.Add(reservation);
									}
								}
								remoteServiceResult.Properties["reservations"] = list;
							}
							else
							{
								LogHelper.Instance.Log("IsOnline is false or stand alone, not issuing realtime call to IReservationServices.GetReservations.", LogEntryType.Info);
								remoteServiceResult.Properties["is_online"] = false;
							}
						}
						else
						{
							LogHelper.Instance.Log("Simulating call to IReservationServices.GetReservations.", LogEntryType.Info);
							remoteServiceResult.Success = true;
						}
						return;
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log("An unhandled exception was raised in ReservationServicesProxy.GetReservations.", e);
						return;
					}
					finally
					{
						try
						{
							remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
							remoteServiceResult.Properties["stand_alone"] = flag2;
							ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new RemoteServiceCallbackEntry
							{
								Callback = completeCallback,
								Result = remoteServiceResult,
								Message = "Invoking completeCallback from ReservationServicesProxy.GetReservations."
							});
						}
						finally
						{
							Interlocked.Decrement(ref m_working);
						}
					}
				}
			}
		});
	}

	public IProductItem NewProductItem()
	{
		return new ProductItem();
	}

	public IReservedItem NewReservedItem()
	{
		return new ReservedItem();
	}

	public IReservationResult NewReservationResult()
	{
		return new LocalReservationResult();
	}

	public ICancelReservationResult NewCancelReservationResult()
	{
		return new LocalCancelReservationResult();
	}

	public IReservationResult SimulateCreateReservation(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount, bool isMultiNightPricing, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles, bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption, decimal defaultServiceFee, decimal actualServiceFee, bool authorizeAtPickup, ReservationCardHash cardHashData, List<ReservationCardHash> alternateCardHashes)
	{
		if (LiveCall)
		{
			return null;
		}
		return RequestCallback3(cardId, email, referenceNumber, reserveDate, productsToReserve, appliedCredit, appliedHiveOnlinePromotion, appliedTitleMarketing, customerNumber, discountedSubTotal, grandTotal, historyTitleIDs, hiveOnlinePromotionDiscount, isMultiNightPricing, numberOfCreditsRemaining, promoCode, subTotal, tax, reservationTitles, isGc, gcIds, ccIds, taxRate, zip, isMdv, isLoyaltyRedemption, defaultServiceFee, actualServiceFee, authorizeAtPickup, cardHashData, alternateCardHashes);
	}

	public ICancelReservationResult CancelReservation(long referenceNumber)
	{
		return CancelRequestCallback(referenceNumber);
	}

	private ReservationServicesProxy()
	{
	}

	public void ProcessValidationResponse(IMessage message, IDictionary<string, object> result, object clientData)
	{
		if (!(message?.Type == "ValidateReservation"))
		{
			return;
		}
		LogHelper.Instance.Log($"Executing queue service post-message processing for message type {message?.Type}");
		bool flag = result.ContainsKey("Success") && (bool)result["Success"];
		Dictionary<string, object> dictionary = Encoding.ASCII.GetString(message.Data).ToObject<Dictionary<string, object>>();
		long result2 = 0L;
		if (dictionary.ContainsKey("ReferenceNumber") && !long.TryParse(dictionary["ReferenceNumber"].ToString(), out result2))
		{
			LogHelper.Instance.Log($"Processing for message type {message?.Type} unable to retrieve ReferenceNumber");
			flag = false;
		}
		if (flag && result.ContainsKey("IsValid"))
		{
			if ((bool)result["IsValid"])
			{
				LogHelper.Instance.Log($"Reservation with ReferenceNumber {result2} is valid.");
			}
			else if (CancelRequestCallback != null)
			{
				LogHelper.Instance.Log($"Reservation with ReferenceNumber {result2} is not valid. Removing the local reservation.");
				CancelRequestCallback(result2, cancelOnServer: false);
			}
			else
			{
				LogHelper.Instance.Log($"ERROR: Reservation with ReferenceNumber {result2} is not valid but no cancel callback is supplied.");
			}
		}
		else
		{
			LogHelper.Instance.Log($"Processing for message type {message?.Type} ReferenceNumber {result2} was not successful.");
		}
	}
}
