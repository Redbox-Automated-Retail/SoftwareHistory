using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.Environment.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.KioskServices.Proxy.Request;
using Redbox.KioskServices.Proxy.Requests;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;

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

	public string CancelledReservations
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "CancelledReservations", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "CancelledReservations", value);
		}
	}

	public string InvalidReservations
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Merchant Services", "InvalidReservations", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Merchant Services", "InvalidReservations", value);
		}
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reconcile(string sessionId, string customerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
		if (session == null)
		{
			LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
			return;
		}
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().GetRentalShoppingCart(new Guid(sessionId));
		ISession session2 = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		IPromotion promo = rentalShoppingCart?.Promotion;
		bool flag = session.ShoppingCart.Type == ShoppingCartType.Offline;
		Guid messageId = Guid.NewGuid();
		bool blocklisted = session.ShoppingCart.Blocklisted;
		string email = session.ShoppingCart.Email;
		string engineVersion = service["EngineVersion"];
		string bundleVersion = service["ProductVersion"];
		int kioskId = int.Parse(session.StoreNumber);
		long? referenceNumber = session.ShoppingCart.ReferenceNumber;
		Dictionary<string, object> creditCardData = ProxyMessageHelper.GetCreditCardData(session, flag);
		DateTime now = DateTime.Now;
		bool playPassPromptAccepted = session.ShoppingCart.PlayPassPromptAccepted;
		int playPassPointsEarned = session.ShoppingCart.PlayPassPointsEarned;
		IDictionary<string, string> propertyBag = session.ShoppingCart.PropertyBag;
		ShoppingCart shoppingCart = ProxyMessageHelper.CreateShoppingCart(rentalShoppingCart, promo);
		long? redeemedStoredOfferid;
		if (session2?.PromoOfferState != null)
		{
			IPromoOfferState promoOfferState = session2.PromoOfferState;
			if (promoOfferState != null && promoOfferState.SelectedOffer?.OfferScopeType == OfferScopeType.StoredRedeemable)
			{
				IPromoOfferState promoOfferState2 = session2.PromoOfferState;
				if (promoOfferState2 != null && promoOfferState2.WasOfferAccepted)
				{
					redeemedStoredOfferid = session2.PromoOfferState.SelectedOffer.StoredOfferId;
					goto IL_01aa;
				}
			}
		}
		redeemedStoredOfferid = null;
		goto IL_01aa;
		IL_01aa:
		var reconcileRequest = new
		{
			MessageType = "Reconcile",
			MessageId = messageId,
			IsOffline = flag,
			Blocklisted = blocklisted,
			Email = email,
			EngineVersion = engineVersion,
			BundleVersion = bundleVersion,
			KioskId = kioskId,
			ReferenceNumber = referenceNumber,
			CreditCard = creditCardData,
			TransactionDate = now,
			PlayPassPromptAccepted = playPassPromptAccepted,
			PlayPassPointsEarned = playPassPointsEarned,
			PropertyBag = propertyBag,
			ShoppingCart = shoppingCart,
			CustomerProfileNumber = customerProfileNumber,
			RedeemedStoredOfferid = redeemedStoredOfferid,
			UtcOffset = ProxyMessageHelper.CalcUtcOffset(DateTime.Now)
		};
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IMerchantServices.Reconcile, URL: {0}", KioskServicesProxy.Instance.Url);
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "Reconcile", reconcileRequest);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.Reconcile.", e);
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

	public void PreAuth(Guid sessionId, string customerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IPreauthorizeResult preauthorizeResult = new PreauthorizeResult
			{
				SessionId = sessionId
			};
			remoteServiceResult.Properties["PreauthorizeResult"] = preauthorizeResult;
			preauthorizeResult.IsOnline = true;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId.ToString());
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
							var message = new
							{
								MessageType = "PreAuth",
								MessageId = Guid.NewGuid(),
								EngineVersion = service["EngineVersion"],
								BundleVersion = service["ProductVersion"],
								KioskId = int.Parse(session.StoreNumber),
								CreditCard = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true),
								TransactionDate = DateTime.Now,
								UseCredits = session.ShoppingCart.UseCredits.ToString(),
								ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
								CustomerProfileNumber = customerProfileNumber
							};
							IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
							remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
							if (dictionary.ContainsKey("server_name"))
							{
								preauthorizeResult.ServerName = dictionary["server_name"]?.ToString();
							}
							preauthorizeResult.IsOnline = !dictionary.ContainsKey("ExceptionRaised");
							preauthorizeResult.PromptRequired = dictionary.ContainsKey("PromptRequired") && (bool)dictionary["PromptRequired"];
							int result = 0;
							if (dictionary.ContainsKey("AuthRuleId"))
							{
								int.TryParse(dictionary["AuthRuleId"]?.ToString(), out result);
							}
							preauthorizeResult.AuthRuleId = result;
							decimal result2 = 0m;
							if (dictionary.ContainsKey("AuthAmount"))
							{
								decimal.TryParse(dictionary["AuthAmount"]?.ToString(), out result2);
							}
							preauthorizeResult.AuthAmount = result2;
							int result3 = 0;
							if (dictionary.ContainsKey("AuthDays"))
							{
								int.TryParse(dictionary["AuthDays"]?.ToString(), out result3);
							}
							preauthorizeResult.AuthDays = result3;
							preauthorizeResult.SkipAuthRule = !dictionary.ContainsKey("SkipAuthRule") || (bool)dictionary["SkipAuthRule"];
						}
						else
						{
							LogHelper.Instance.Log("In stand alone mode, skipping realtime call to PreAuth.", LogEntryType.Info);
							remoteServiceResult.Success = true;
							preauthorizeResult.IsOnline = false;
							preauthorizeResult.PromptRequired = false;
							preauthorizeResult.AuthRuleId = 0;
							preauthorizeResult.AuthAmount = 0m;
							preauthorizeResult.AuthDays = 0;
							preauthorizeResult.SkipAuthRule = true;
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
								Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitAbsolute
							};
							remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
							remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
							preauthorizeResult.IsOnline = false;
						}
						else if (SimulateCommunicationsFailure)
						{
							remoteServiceResult.Success = false;
							RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
							{
								Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitForSuccessfulPing
							};
							remoteServiceResult.InnerInstructions.Add(item);
							preauthorizeResult.IsOnline = false;
						}
						else
						{
							remoteServiceResult.Success = true;
							preauthorizeResult.IsOnline = false;
						}
						preauthorizeResult.PromptRequired = false;
						preauthorizeResult.AuthRuleId = 0;
						preauthorizeResult.AuthAmount = 0m;
						preauthorizeResult.AuthDays = 0;
						preauthorizeResult.SkipAuthRule = true;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.PreAuth.", e);
				remoteServiceResult.Success = false;
				preauthorizeResult.IsOnline = false;
				preauthorizeResult.PromptRequired = false;
				preauthorizeResult.AuthRuleId = 0;
				preauthorizeResult.AuthAmount = 0m;
				preauthorizeResult.AuthDays = 0;
				preauthorizeResult.SkipAuthRule = true;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					preauthorizeResult.StandAlone = StoreServicesProxy.Instance.IsStandAlone;
					completeCallback(remoteServiceResult);
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void Authorize(Guid sessionId, AuthorizeType authorizeType, string customerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			IAuthorizeResult authorizeResult = new AuthorizeResult
			{
				SessionId = sessionId,
				AuthorizeType = authorizeType
			};
			remoteServiceResult.Properties["AuthorizeResult"] = authorizeResult;
			authorizeResult.IsOnline = true;
			ITrackData trackData = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.TrackData;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId.ToString());
			bool flag = authorizeType == AuthorizeType.Authorize;
			bool flag2 = authorizeType == AuthorizeType.GiftCardAuthorize;
			string text = (flag ? "Authorize" : "GCAuthorize");
			string response = (flag ? AuthorizeResponse.Approved.ToString() : GCAuthorizeResponse.Approved.ToString());
			try
			{
				Interlocked.Increment(ref m_working);
				string text2 = (flag ? AuthorizeResponse.Error.ToString() : GCAuthorizeResponse.Error.ToString());
				if (session == null)
				{
					authorizeResult.Response = text2;
				}
				else if ((flag && LiveCall) || (flag2 && GCLiveCall))
				{
					if (!StoreServicesProxy.Instance.IsStandAlone)
					{
						LogHelper.Instance.Log($"Live call to remote IMerchantServices.{text}, URL: {KioskServicesProxy.Instance.Url}");
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						object message = null;
						if (flag)
						{
							message = new
							{
								MessageType = text,
								MessageId = Guid.NewGuid(),
								EngineVersion = service["EngineVersion"],
								BundleVersion = service["ProductVersion"],
								KioskId = int.Parse(session.StoreNumber),
								CreditCard = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true),
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
						}
						else if (flag2)
						{
							message = new
							{
								MessageType = text,
								MessageId = Guid.NewGuid(),
								EngineVersion = service["EngineVersion"],
								BundleVersion = service["ProductVersion"],
								KioskId = int.Parse(session.StoreNumber),
								CreditCard = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true),
								TransactionDate = DateTime.Now,
								UseCredits = session.ShoppingCart.UseCredits.ToString(),
								ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
								CustomerProfileNumber = customerProfileNumber
							};
						}
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("server_name"))
						{
							authorizeResult.ServerName = dictionary["server_name"]?.ToString();
						}
						if (remoteServiceResult.Success && dictionary.ContainsKey("ReferenceNumber"))
						{
							session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(dictionary["ReferenceNumber"]));
						}
						authorizeResult.Response = response;
						authorizeResult.IsOnline = !dictionary.ContainsKey("ExceptionRaised");
						authorizeResult.Email = ((!dictionary.ContainsKey("Email")) ? string.Empty : dictionary["Email"]?.ToString());
						authorizeResult.ReferenceNumber = ((!dictionary.ContainsKey("ReferenceNumber")) ? null : dictionary["ReferenceNumber"]?.ToString());
						PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							authorizeResult.HasProfile = (bool)dictionary2["Valid"];
							authorizeResult.FirstName = dictionary2["FirstName"]?.ToString();
							authorizeResult.IsRBI = dictionary2.ContainsKey("IsRBI") && (bool)dictionary2["IsRBI"];
						}
						authorizeResult.ConfirmationStatus = (dictionary.ContainsKey("ConfirmationStatus") ? ((ConfirmationStatus)dictionary["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString() : ConfirmationStatus.DoNotShow.ToString());
						ArrayList arrayList = (dictionary.ContainsKey("Offers") ? (dictionary["Offers"] as ArrayList) : null);
						Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
						if (arrayList != null)
						{
							int num = 1;
							foreach (Dictionary<string, object> item2 in arrayList)
							{
								dictionary3.Add("offer_" + num, item2["Name"]);
								num++;
							}
						}
						authorizeResult.Offers = dictionary3;
						if (flag2)
						{
							Dictionary<string, object> dictionary5 = (dictionary.ContainsKey("GiftCardSummary") ? (dictionary["GiftCardSummary"] as Dictionary<string, object>) : null);
							if (dictionary5 != null)
							{
								authorizeResult.GiftCardExists = (bool)dictionary5["Exists"];
								authorizeResult.GiftCardStatus = (GiftCardStatus)dictionary5["Status"];
								decimal result = 0m;
								if (dictionary5.ContainsKey("Balance"))
								{
									decimal.TryParse(dictionary5["Balance"]?.ToString(), out result);
								}
								authorizeResult.GiftCardBalance = result;
								int result2 = 0;
								if (dictionary5.ContainsKey("MaxDiscs"))
								{
									int.TryParse(dictionary5["MaxDiscs"]?.ToString(), out result2);
								}
								authorizeResult.GiftCardMaxDiscs = result2;
								int result3 = 0;
								if (dictionary5.ContainsKey("MaxGames"))
								{
									int.TryParse(dictionary5["MaxGames"]?.ToString(), out result3);
								}
								authorizeResult.GiftCardMaxGames = MaxGames;
							}
						}
						authorizeResult.AccountNumber = ((!dictionary.ContainsKey("AccountNumber")) ? string.Empty : dictionary["AccountNumber"]?.ToString());
						authorizeResult.CustomerProfileNumber = ((!dictionary.ContainsKey("CustomerProfileNumber")) ? string.Empty : dictionary["CustomerProfileNumber"]?.ToString());
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList2 = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList2 != null && arrayList2.Count > 0)
							{
								if (arrayList2[0] is Dictionary<string, object> dictionary6)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(flag ? typeof(AuthorizeResponse?) : typeof(GCAuthorizeResponse?), (string)dictionary6["Code"]);
									authorizeResult.Response = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : text2);
								}
							}
							else
							{
								authorizeResult.Response = text2;
							}
						}
					}
					else
					{
						LogHelper.Instance.Log($"In stand alone mode, skipping realtime call to {text}.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						authorizeResult.Response = response;
						authorizeResult.IsOnline = false;
						PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
						authorizeResult.ConfirmationStatus = ConfirmationStatus.DoNotShow.ToString();
						new AutoResetEvent(initialState: false).WaitOne(1000);
					}
				}
				else
				{
					LogHelper.Instance.Log($"Simulating call to IMerchantServices.{text}.");
					int num2 = new Random().Next(1, 3);
					new AutoResetEvent(initialState: false).WaitOne(num2 * 1000);
					authorizeResult.StoreNumber = session.StoreNumber;
					PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData);
					if ((flag && SimulateServerBusyError) || (flag2 && GCSimulateServerBusyError))
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						authorizeResult.Response = text2;
						authorizeResult.IsOnline = false;
					}
					else if ((flag && SimulateCommunicationsFailure) || (flag2 && GCSimulateCommunicationsFailure))
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
						{
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitForSuccessfulPing
						};
						remoteServiceResult.InnerInstructions.Add(item);
						authorizeResult.Response = "Communications Failure";
						authorizeResult.IsOnline = false;
					}
					else
					{
						remoteServiceResult.Success = (flag ? (AuthorizeResponse == AuthorizeResponse.Approved) : (GCAuthorizeResponse == GCAuthorizeResponse.Approved));
						authorizeResult.Response = (flag ? AuthorizeResponse.ToString() : GCAuthorizeResponse.ToString());
						authorizeResult.IsOnline = true;
						authorizeResult.HasProfile = HasCustomerProfile;
						authorizeResult.Email = EmailAddress;
						authorizeResult.FirstName = CPFirstName;
						authorizeResult.LastName = CPLastName;
						authorizeResult.IsRBI = IsRBI;
						authorizeResult.ReferenceNumber = TransactionId;
						authorizeResult.AccountNumber = AccountNumber;
						authorizeResult.CustomerProfileNumber = CPAccountNumber;
						authorizeResult.ConfirmationStatus = ConfirmationStatus.DoNotShow.ToString();
						Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
						int num3 = 1;
						string[] userOffers = UserOffers;
						foreach (string value in userOffers)
						{
							dictionary7.Add("offer_" + num3, value);
							num3++;
						}
						authorizeResult.Offers = dictionary7;
						if (flag2)
						{
							authorizeResult.GiftCardExists = Exists;
							authorizeResult.GiftCardStatus = GCStatusType;
							decimal result4 = 0m;
							decimal.TryParse(Balance, out result4);
							authorizeResult.GiftCardBalance = result4;
							authorizeResult.GiftCardMaxDiscs = MaxDiscs;
							authorizeResult.GiftCardMaxGames = MaxGames;
						}
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"An unhandled exception was raised in MerchantServicesProxy.{text}.", e);
				remoteServiceResult.Success = true;
				authorizeResult.Response = response;
				authorizeResult.IsOnline = false;
				PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData, defaultToNA: true);
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					authorizeResult.StandAlone = StoreServicesProxy.Instance.IsStandAlone;
					authorizeResult.Success = remoteServiceResult.Success;
					completeCallback(remoteServiceResult);
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void ReservationAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			ITrackData trackData = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.TrackData;
			IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
			try
			{
				Interlocked.Increment(ref m_working);
				if (session == null)
				{
					remoteServiceResult.Properties["response"] = AuthorizeResponse.Error.ToString();
				}
				else if (ResvAuthLiveCall)
				{
					if (!StoreServicesProxy.Instance.IsStandAlone)
					{
						LogHelper.Instance.Log("Live call to remote IMerchantServices.ReservationAuthorize, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						var message = new
						{
							MessageType = "ResvAuth",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(session.StoreNumber),
							CreditCard = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true),
							TransactionDate = DateTime.Now,
							UseCredits = session.ShoppingCart.UseCredits.ToString(),
							ReferenceNumber = referenceNumber,
							ShoppingCart = ProxyMessageHelper.CreateShoppingCart(sessionId),
							CustomerProfileNumber = customerProfileNumber
						};
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("server_name"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["server_name"];
						}
						if (remoteServiceResult.Success && dictionary.ContainsKey("ReferenceNumber"))
						{
							session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(dictionary["ReferenceNumber"]));
						}
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
						remoteServiceResult.Properties["email"] = (dictionary.ContainsKey("Email") ? dictionary["Email"] : string.Empty);
						remoteServiceResult.Properties["reference_number"] = ((!dictionary.ContainsKey("ReferenceNumber")) ? null : dictionary["ReferenceNumber"]?.ToString());
						PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData);
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							remoteServiceResult.Properties["has_profile"] = (bool)dictionary2["Valid"];
							remoteServiceResult.Properties["first_name"] = dictionary2["FirstName"];
						}
						remoteServiceResult.Properties["confirmation_status"] = (dictionary.ContainsKey("ConfirmationStatus") ? ((ConfirmationStatus)dictionary["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString() : ConfirmationStatus.DoNotShow.ToString());
						ArrayList arrayList = (dictionary.ContainsKey("Offers") ? (dictionary["Offers"] as ArrayList) : null);
						Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
						if (arrayList != null)
						{
							int num = 1;
							foreach (Dictionary<string, object> item2 in arrayList)
							{
								dictionary3.Add("offer_" + num, item2["Name"]);
								num++;
							}
						}
						remoteServiceResult.Properties["offers"] = dictionary3;
						remoteServiceResult.Properties["account_number"] = (dictionary.ContainsKey("AccountNumber") ? dictionary["AccountNumber"] : string.Empty);
						remoteServiceResult.Properties["customer_profile_number"] = (dictionary.ContainsKey("CustomerProfileNumber") ? dictionary["CustomerProfileNumber"] : string.Empty);
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList2 = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList2 != null && arrayList2.Count > 0)
							{
								if (arrayList2[0] is Dictionary<string, object> dictionary5)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), (string)dictionary5["Code"]);
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
						PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData);
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
					PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData);
					if (ResvAuthSimulateServerBusyError)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitAbsolute
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
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitForSuccessfulPing
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
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
						int num3 = 1;
						string[] userOffers = UserOffers;
						foreach (string value in userOffers)
						{
							dictionary6.Add("offer_" + num3, value);
							num3++;
						}
						remoteServiceResult.Properties["offers"] = dictionary6;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.ReservationAuthorize.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
				remoteServiceResult.Properties["is_online"] = false;
				PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData, defaultToNA: true);
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

	public void PickupAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, bool cartWasModified, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			ITrackData trackData = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession()?.TrackData;
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
						LogHelper.Instance.Log("Live call to remote IMerchantServices.PickupAuthorize, URL: {0}", KioskServicesProxy.Instance.Url);
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						var message = new
						{
							MessageType = "PickupAuthorize",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(session.StoreNumber),
							ReferenceNumber = referenceNumber,
							CartWasModified = cartWasModified,
							CreditCard = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true),
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
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("server_name"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["server_name"];
						}
						if (remoteServiceResult.Success && dictionary.ContainsKey("ReferenceNumber"))
						{
							session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(dictionary["ReferenceNumber"]));
						}
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
						remoteServiceResult.Properties["email"] = (dictionary.ContainsKey("Email") ? dictionary["Email"] : string.Empty);
						remoteServiceResult.Properties["reference_number"] = ((!dictionary.ContainsKey("ReferenceNumber")) ? null : dictionary["ReferenceNumber"]?.ToString());
						PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData);
						Dictionary<string, object> dictionary2 = (dictionary.ContainsKey("CustomerProfile") ? (dictionary["CustomerProfile"] as Dictionary<string, object>) : null);
						if (dictionary2 != null)
						{
							remoteServiceResult.Properties["has_profile"] = (bool)dictionary2["Valid"];
							remoteServiceResult.Properties["first_name"] = dictionary2["FirstName"];
							remoteServiceResult.Properties["is_rbi"] = dictionary2.ContainsKey("IsRBI") && (bool)dictionary2["IsRBI"];
						}
						remoteServiceResult.Properties["confirmation_status"] = (dictionary.ContainsKey("ConfirmationStatus") ? ((ConfirmationStatus)dictionary["ConfirmationStatus"]/*cast due to constrained. prefix*/).ToString() : ConfirmationStatus.DoNotShow.ToString());
						ArrayList arrayList = (dictionary.ContainsKey("Offers") ? (dictionary["Offers"] as ArrayList) : null);
						Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
						if (arrayList != null)
						{
							int num = 1;
							foreach (Dictionary<string, object> item2 in arrayList)
							{
								dictionary3.Add("offer_" + num, item2["Name"]);
								num++;
							}
						}
						remoteServiceResult.Properties["offers"] = dictionary3;
						remoteServiceResult.Properties["account_number"] = (dictionary.ContainsKey("AccountNumber") ? dictionary["AccountNumber"] : string.Empty);
						remoteServiceResult.Properties["customer_profile_number"] = (dictionary.ContainsKey("CustomerProfileNumber") ? dictionary["CustomerProfileNumber"] : string.Empty);
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList2 = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList2 != null && arrayList2.Count > 0)
							{
								if (arrayList2[0] is Dictionary<string, object> dictionary5)
								{
									object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(AuthorizeResponse?), (string)dictionary5["Code"]);
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
						LogHelper.Instance.Log("In stand alone mode, skipping realtime call to PickupAuthorize.", LogEntryType.Info);
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = false;
						PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData);
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						new AutoResetEvent(initialState: false).WaitOne(1000);
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to IMerchantServices.PickupAuthorize.");
					int num2 = new Random().Next(1, 3);
					new AutoResetEvent(initialState: false).WaitOne(num2 * 1000);
					remoteServiceResult.Properties["store_number"] = session.StoreNumber;
					PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData);
					if (SimulateServerBusyError)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitAbsolute
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
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitForSuccessfulPing
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
						remoteServiceResult.Properties["reference_number"] = TransactionId;
						remoteServiceResult.Properties["account_number"] = AccountNumber;
						remoteServiceResult.Properties["customer_profile_number"] = CPAccountNumber;
						remoteServiceResult.Properties["confirmation_status"] = ConfirmationStatus.DoNotShow.ToString();
						Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
						int num3 = 1;
						string[] userOffers = UserOffers;
						foreach (string value in userOffers)
						{
							dictionary6.Add("offer_" + num3, value);
							num3++;
						}
						remoteServiceResult.Properties["offers"] = dictionary6;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.PickupAuthorize.", e);
				remoteServiceResult.Success = true;
				remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
				remoteServiceResult.Properties["is_online"] = false;
				PopulateFirstNameAndLastNameFromTrackData(remoteServiceResult, trackData, defaultToNA: true);
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
						Message = "Invoking completeCallback from MerchantServicesProxy.PickupAuthorize."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void ValidatePickup(string sessionId, List<long> referenceNumbers, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback)
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
						var message = new
						{
							MessageType = "PickupValidation",
							MessageId = Guid.NewGuid(),
							EngineVersion = service["EngineVersion"],
							BundleVersion = service["ProductVersion"],
							KioskId = int.Parse(StoreServicesProxy.Instance.StoreNumber),
							CreditCard = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: true),
							TransactionDate = DateTime.Now,
							ReferenceNumbers = referenceNumbers
						};
						IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(message);
						HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
						remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
						if (dictionary.ContainsKey("server_name"))
						{
							remoteServiceResult.Properties["server_name"] = dictionary["server_name"];
						}
						remoteServiceResult.Properties["response"] = AuthorizeResponse.Approved.ToString();
						remoteServiceResult.Properties["is_online"] = !dictionary.ContainsKey("ExceptionRaised");
						if (remoteServiceResult.Success && dictionary.ContainsKey("ReferenceNumbers"))
						{
							ArrayList arrayList = (ArrayList)dictionary["ReferenceNumbers"];
							if (arrayList.Count > 0)
							{
								remoteServiceResult.Properties["reference_number"] = arrayList[0]?.ToString();
								session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(arrayList[0]));
							}
						}
						if (!remoteServiceResult.Properties.ContainsKey("reference_number"))
						{
							remoteServiceResult.Properties["reference_number"] = null;
						}
						if (dictionary.ContainsKey("ValidReservations"))
						{
							ArrayList value = (ArrayList)dictionary["ValidReservations"];
							remoteServiceResult.Properties["validated_reservations"] = value.ToLuaTable();
						}
						remoteServiceResult.Properties["account_number"] = (dictionary.ContainsKey("AccountNumber") ? dictionary["AccountNumber"] : string.Empty);
						remoteServiceResult.Properties["customer_profile_number"] = (dictionary.ContainsKey("CustomerProfileNumber") ? dictionary["CustomerProfileNumber"] : string.Empty);
						ArrayList arrayList2 = (dictionary.ContainsKey("CancelledReservations") ? (dictionary["CancelledReservations"] as ArrayList) : null);
						List<long> list = new List<long>();
						if (arrayList2 != null)
						{
							int num = 1;
							foreach (int item2 in arrayList2)
							{
								list.Add(item2);
								num++;
							}
						}
						remoteServiceResult.Properties["cancelled_reservations"] = list.ToLuaTable();
						if (!remoteServiceResult.Success && !dictionary.ContainsKey("ExceptionRaised"))
						{
							ArrayList arrayList3 = (dictionary.ContainsKey("Errors") ? (dictionary["Errors"] as ArrayList) : null);
							if (arrayList3 != null && arrayList3.Count > 0)
							{
								if (arrayList3[0] is Dictionary<string, object> dictionary2)
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
					int num3 = new Random().Next(1, 3);
					new AutoResetEvent(initialState: false).WaitOne(num3 * 1000);
					remoteServiceResult.Properties["store_number"] = session.StoreNumber;
					if (SimulateServerBusyError)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitAbsolute
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
							Type = Redbox.KioskEngine.ComponentModel.KioskServices.InstructionType.WaitForSuccessfulPing
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
						remoteServiceResult.Properties["reference_number"] = TransactionId;
						if (!string.IsNullOrEmpty(TransactionId))
						{
							session.ShoppingCart.SetReferenceNumber(Convert.ToInt64(TransactionId));
						}
						remoteServiceResult.Properties["account_number"] = AccountNumber;
						remoteServiceResult.Properties["customer_profile_number"] = CPAccountNumber;
						string[] array = CancelledReservations.Split(',');
						string[] source = InvalidReservations.Split(',');
						List<long> list2 = new List<long>();
						string[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							if (long.TryParse(array2[i], out var result))
							{
								list2.Add(result);
							}
						}
						remoteServiceResult.Properties["cancelled_reservations"] = list2.ToLuaTable();
						List<long> list3 = new List<long>();
						foreach (long referenceNumber in referenceNumbers)
						{
							if (!list2.Contains(referenceNumber) && !source.Contains(referenceNumber.ToString()))
							{
								list3.Add(referenceNumber);
							}
						}
						remoteServiceResult.Properties["validated_reservations"] = list3.ToLuaTable();
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
					remoteServiceResult.Properties["stand_alone"] = StoreServicesProxy.Instance.IsStandAlone;
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

	public void ModResvPickup(string storeNumber, string emailAddress, long referenceNumber, string sessionId, bool modifiedCart, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback)
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

	public void PickupReconcile(string sessionId, string customerProfileNumber, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		IShoppingSession session = ServiceLocator.Instance.GetService<IShoppingSessionService>().GetSession(sessionId);
		if (session == null)
		{
			LogHelper.Instance.Log("The specified shopping session does not exist: {0}", sessionId);
			return;
		}
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>().GetRentalShoppingCart(new Guid(sessionId));
		foreach (string key in pickupResults.Keys)
		{
			foreach (IProductItem result in pickupResults[key])
			{
				IRentalShoppingCartItem rentalShoppingCartItem = rentalShoppingCart.Items.SingleOrDefault((IRentalShoppingCartItem i) => i.Barcode == result.Barcode);
				if (rentalShoppingCartItem != null)
				{
					rentalShoppingCartItem.Status = result.Status;
				}
				else
				{
					LogHelper.Instance.LogError("Vend Reservation Mismatch", $"The item with barcode {result.Barcode} was not found in the shopping cart.");
				}
			}
		}
		IPromotion promo = rentalShoppingCart?.Promotion;
		bool isOffline = session.ShoppingCart.Type == ShoppingCartType.Offline;
		var reconcileRequest = new
		{
			MessageType = "PickupReconcile",
			MessageId = Guid.NewGuid(),
			IsOffline = isOffline,
			Blocklisted = session.ShoppingCart.Blocklisted,
			Email = session.ShoppingCart.Email,
			EngineVersion = service["EngineVersion"],
			BundleVersion = service["ProductVersion"],
			KioskId = int.Parse(session.StoreNumber),
			ReferenceNumber = referenceNumber,
			CartWasModified = rentalShoppingCart.PickupRecommendationsSelected,
			PickupDateTime = DateTime.Now,
			CreditCard = ProxyMessageHelper.GetCreditCardData(session, includeTrack2: false),
			TransactionDate = DateTime.Now,
			PlayPassPromptAccepted = session.ShoppingCart.PlayPassPromptAccepted,
			PlayPassPointsEarned = session.ShoppingCart.PlayPassPointsEarned,
			PropertyBag = session.ShoppingCart.PropertyBag,
			ShoppingCart = ProxyMessageHelper.CreateShoppingCart(rentalShoppingCart, promo),
			CustomerProfileNumber = customerProfileNumber
		};
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IMerchantServices.PickupReconcile, URL: {0}", KioskServicesProxy.Instance.Url);
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(200, "PickupReconcile", reconcileRequest);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in MerchantServicesProxy.PickupReconcile.", e);
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
							Message = "Invoking callback from MerchantServicesProxy.PickupReconcile."
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

	private void PopulateFirstNameAndLastNameFromTrackData(IAuthorizeResult authorizeResult, ITrackData trackData)
	{
		PopulateFirstNameAndLastNameFromTrackData(authorizeResult, trackData, defaultToNA: false);
	}

	private void PopulateFirstNameAndLastNameFromTrackData(IAuthorizeResult authorizeResult, ITrackData trackData, bool defaultToNA)
	{
		authorizeResult.FirstName = (((trackData == null) & defaultToNA) ? "NA" : trackData?.FirstName);
		authorizeResult.LastName = (((trackData == null) & defaultToNA) ? "NA" : trackData?.LastName);
	}

	private void PopulateFirstNameAndLastNameFromTrackData(RemoteServiceResult result, ITrackData trackData)
	{
		PopulateFirstNameAndLastNameFromTrackData(result, trackData, defaultToNA: false);
	}

	private void PopulateFirstNameAndLastNameFromTrackData(RemoteServiceResult result, ITrackData trackData, bool defaultToNA)
	{
		result.Properties["first_name"] = (((trackData == null) & defaultToNA) ? "NA" : trackData?.FirstName);
		result.Properties["last_name"] = (((trackData == null) & defaultToNA) ? "NA" : trackData?.LastName);
	}
}
