using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.Proxy;

public class PersonalizationServicesProxy : IPersonalizationServices
{
	private static readonly byte[] m_keyValue = new Guid("{02CAD7C4-E0F5-4b1b-BD04-808B840A61D2}").ToByteArray();

	private static readonly byte[] m_initialVector = new Guid("{AD186FCD-2ACD-41a8-958D-6CA7DAEB1DE5}").ToByteArray();

	private int m_working;

	public static PersonalizationServicesProxy Instance => Singleton<PersonalizationServicesProxy>.Instance;

	public string CustomerProfileNumber { get; set; }

	private PersonalizationServicesProxy()
	{
	}

	public void Login(int kioskId, string emailAddress, SecureString password, string phoneNumber, SecureString pin, string languageCode, RemoteServiceCallback completeCallback)
	{
		string password2 = SecureStringToString(password);
		string pin2 = SecureStringToString(pin);
		PerformLiveLogin(kioskId, emailAddress, password2, phoneNumber, pin2, languageCode, completeCallback);
	}

	public void UpdateAccount(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback, bool queued = false)
	{
		if (queued)
		{
			QueueAccountUpdate(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, completeCallback);
		}
		else
		{
			PerformLiveAccountUpdate(kioskId, customerProfileNumber, emailAddress, phoneNumber, pin, punchcardOptIn, textClubOptIn, completeCallback);
		}
	}

	private static string SecureStringToString(SecureString value)
	{
		if (value == null)
		{
			return null;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.SecureStringToGlobalAllocUnicode(value);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}

	private DateTime GetDateTime(IDictionary<string, object> dateInfo)
	{
		object obj = dateInfo["theDate"];
		DateTime result;
		if (obj.GetType() == typeof(int) && (int)obj != 0)
		{
			result = new DateTime((int)dateInfo["theDate"]);
		}
		else if (obj.GetType() == typeof(long) && (long)obj != 0L)
		{
			result = new DateTime((long)dateInfo["theDate"]);
		}
		else
		{
			if (!((string)dateInfo["dateTimeKind"] != "Unspecified"))
			{
				return Convert.ToDateTime((string)dateInfo["dateString"]);
			}
			result = new DateTime((long)dateInfo["theDate"]);
		}
		return result;
	}

	private void PerformLiveLogin(int kioskId, string emailAddress, string password, string phoneNumber, string pin, string languageCode, RemoteServiceCallback completeCallback)
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
					LogHelper.Instance.Log("Live call to remote IPersonalization.Login, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
					{
						KioskId = kioskId,
						MessageId = Guid.NewGuid(),
						MessageType = "KioskLogin",
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						EmailAddress = emailAddress,
						Password = Encrypt(password),
						PhoneNumber = phoneNumber,
						Pin = Encrypt(pin),
						LanguageCode = languageCode
					});
					HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					remoteServiceResult.Properties["CustomerProfileNumber"] = GetResponseValue(dictionary, "CustomerProfileNumber", null)?.ToString();
					remoteServiceResult.Properties["FirstName"] = GetResponseValue(dictionary, "FirstName", null)?.ToString();
					remoteServiceResult.Properties["LoginEmail"] = GetResponseValue(dictionary, "LoginEmail", null)?.ToString();
					remoteServiceResult.Properties["PhoneNumber"] = GetResponseValue(dictionary, "PhoneNumber", null)?.ToString();
					remoteServiceResult.Properties["PostalCode"] = GetResponseValue(dictionary, "PostalCode", null)?.ToString();
					remoteServiceResult.Properties["BirthDate"] = GetResponseValue(dictionary, "BirthDate", null);
					remoteServiceResult.Properties["LoyaltyPointBalance"] = (int)GetResponseValue(dictionary, "LoyaltyPointBalance", 0);
					remoteServiceResult.Properties["LoyaltyTier"] = GetResponseValue(dictionary, "LoyaltyTier", null)?.ToString();
					remoteServiceResult.Properties["LoyaltyTierCounter"] = GetResponseValue(dictionary, "LoyaltyTierCounter", null)?.ToString();
					remoteServiceResult.Properties["PointsExpiring"] = GetResponseValue(dictionary, "PointsExpiring", null)?.ToString();
					remoteServiceResult.Properties["PointsExpirationDate"] = GetResponseValue(dictionary, "PointsExpirationDate", null);
					if (remoteServiceResult.Properties.ContainsKey("PointsExpirationDate") && remoteServiceResult.Properties["PointsExpirationDate"] != null)
					{
						IDictionary<string, object> dateInfo = remoteServiceResult.Properties["PointsExpirationDate"] as IDictionary<string, object>;
						remoteServiceResult.Properties["PointsExpirationDate"] = GetDateTime(dateInfo);
					}
					if (remoteServiceResult.Properties.ContainsKey("BirthDate") && remoteServiceResult.Properties["BirthDate"] != null)
					{
						IDictionary<string, object> dateInfo2 = remoteServiceResult.Properties["BirthDate"] as IDictionary<string, object>;
						remoteServiceResult.Properties["BirthDate"] = GetDateTime(dateInfo2);
					}
					remoteServiceResult.Properties["MobilePhoneNumber"] = GetResponseValue(dictionary, "MobilePhoneNumber", null)?.ToString();
					remoteServiceResult.Properties["PromptForPerks"] = dictionary.ContainsKey("PromptForPerks") && (bool)dictionary["PromptForPerks"];
					remoteServiceResult.Properties["PromptForEarlyId"] = dictionary.ContainsKey("PromptForEarlyId") && (bool)dictionary["PromptForEarlyId"];
					remoteServiceResult.Properties["TextClubEnabled"] = dictionary.ContainsKey("TextClubEnabled") && (bool)dictionary["TextClubEnabled"];
					if (dictionary.ContainsKey("RecommendedTitles") && dictionary["RecommendedTitles"] != null)
					{
						ArrayList obj = (ArrayList)dictionary["RecommendedTitles"];
						remoteServiceResult.Properties["RecommendedTitles"] = new List<IRecommendedTitles>();
						foreach (IDictionary<string, object> item in obj)
						{
							RecommendedTitles recommendedTitles = new RecommendedTitles
							{
								OfferCode = GetResponseValue(item, "OfferCode", string.Empty)?.ToString(),
								TivoQueryId = GetResponseValue(item, "QueryId", string.Empty)?.ToString()
							};
							foreach (IDictionary<string, object> item2 in (ArrayList)GetResponseValue(item, "TitleIds", new ArrayList()))
							{
								RecommendedTitlesIdType idType = (RecommendedTitlesIdType)GetResponseValue(item2, "IdType", RecommendedTitlesIdType.ProductGroupID);
								int num = (int)GetResponseValue(item2, "Id", -1);
								ArrayList source = (ArrayList)GetResponseValue(item2, "Tags", new ArrayList());
								recommendedTitles.Add(new RecommendedTitleId
								{
									Id = num,
									IdType = idType,
									PersonalizationTitleTags = source.Cast<string>().ToList()
								});
							}
							((List<IRecommendedTitles>)remoteServiceResult.Properties["RecommendedTitles"]).Add(recommendedTitles);
						}
					}
					if (dictionary.ContainsKey("StoredPromoCodes") && dictionary["StoredPromoCodes"] != null)
					{
						ArrayList obj2 = (ArrayList)dictionary["StoredPromoCodes"];
						List<IStoredPromoCode> list = new List<IStoredPromoCode>();
						foreach (IDictionary<string, object> item3 in obj2)
						{
							StoredPromoCode storedPromoCode = new StoredPromoCode
							{
								PromoCode = GetResponseValue(item3, "PromoCode", string.Empty).ToString(),
								CampaignName = GetResponseValue(item3, "CampaignName", string.Empty).ToString(),
								UsageDescription = GetResponseValue(item3, "UsageDescription", string.Empty).ToString(),
								RedemptionTypeDescription = GetResponseValue(item3, "RedemptionTypeDescription", string.Empty).ToString()
							};
							if (item3.ContainsKey("ExpirationDate") && item3["ExpirationDate"] != null)
							{
								IDictionary<string, object> dateInfo3 = item3["ExpirationDate"] as IDictionary<string, object>;
								storedPromoCode.ExpirationDate = GetDateTime(dateInfo3);
							}
							storedPromoCode.ExpirationText = GetResponseValue(item3, "ExpirationText", string.Empty).ToString();
							storedPromoCode.DaysUntilExpiration = (int?)GetResponseValue(item3, "DaysUntilExpiration", null);
							list.Add(storedPromoCode);
						}
						remoteServiceResult.Properties["StoredPromoCodes"] = list;
					}
					if (dictionary.ContainsKey("AcceptedOffers"))
					{
						ArrayList obj3 = (ArrayList)dictionary["AcceptedOffers"];
						List<IAcceptedOffer> list2 = new List<IAcceptedOffer>();
						foreach (IDictionary<string, object> item4 in obj3)
						{
							list2.Add(new AcceptedOffer
							{
								Code = GetResponseValue(item4, "Code", string.Empty).ToString(),
								Status = GetResponseValue(item4, "Status", string.Empty).ToString(),
								StartDate = GetResponseDateTime(item4, "StartDate"),
								EndDate = GetResponseDateTime(item4, "EndDate"),
								Name = GetResponseValue(item4, "Name", string.Empty).ToString(),
								Description = GetResponseValue(item4, "Description", string.Empty).ToString(),
								OptedInDate = GetResponseDateTime(item4, "OptedInDate"),
								TimeToComplete = GetResponseValue(item4, "TimeToComplete", string.Empty).ToString(),
								CompleteDate = GetResponseDateTime(item4, "CompleteDate"),
								LegalInformation = GetResponseValue(item4, "LegalInformation", string.Empty).ToString(),
								CurrentValue = (int?)GetResponseValue(item4, "CurrentValue", null),
								MaxValue = (int?)GetResponseValue(item4, "MaxValue", null),
								EndValueText = GetResponseValue(item4, "EndValueText", string.Empty).ToString(),
								RemainderValue = (int?)GetResponseValue(item4, "RemainderValue", null),
								ShowInBannerPriority = (int?)GetResponseValue(item4, "ShowInBannerPriority", null),
								ProgressTrackerCode = GetResponseValue(item4, "ProgressTrackerCode", string.Empty).ToString()
							});
						}
						remoteServiceResult.Properties["AcceptedOffers"] = list2;
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
							if (arrayList[0] is Dictionary<string, object> dictionary3)
							{
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(KioskLoginResponseErrors?), (string)dictionary3["Code"]);
								remoteServiceResult.Properties["response"] = fieldsForResponseCode ?? ((object)KioskLoginResponseErrors.Error);
							}
						}
						else
						{
							remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.Error;
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to login user.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.LoyaltyIsDown;
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.PerformLiveLogin.", e);
				remoteServiceResult.Properties["response"] = KioskLoginResponseErrors.Error;
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
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformLiveLogin."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private DateTime? GetResponseDateTime(IDictionary<string, object> response, string key)
	{
		if (response.ContainsKey(key) && response[key] != null)
		{
			IDictionary<string, object> dateInfo = response[key] as IDictionary<string, object>;
			return GetDateTime(dateInfo);
		}
		return null;
	}

	private void PerformLiveAccountUpdate(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback)
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
					LogHelper.Instance.Log("Live call to remote IPersonalization.UpdateAccount, URL: {0}", KioskServicesProxy.Instance.Url);
					IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
					IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(new
					{
						KioskId = kioskId,
						MessageId = Guid.NewGuid(),
						MessageType = "UpdateAccount",
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						CustomerProfileNumber = customerProfileNumber,
						Email = emailAddress,
						MobilePhoneNumber = phoneNumber,
						Pin = Encrypt(SecureStringToString(pin)),
						PunchCardOptin = punchcardOptIn,
						TextClubOptin = textClubOptIn
					});
					HealthServicesProxy.Instance.UpdateOnlineStatus(dictionary);
					remoteServiceResult.Success = dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
					remoteServiceResult.Properties["MobilePhoneNumber"] = GetResponseValue(dictionary, "MobilePhoneNumber", null)?.ToString();
					remoteServiceResult.Properties["CustomerProfileNumber"] = GetResponseValue(dictionary, "CustomerProfileNumber", null)?.ToString();
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
								object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(UpdateAccountResponseErrors?), (string)dictionary2["Code"]);
								remoteServiceResult.Properties["response"] = fieldsForResponseCode ?? ((object)UpdateAccountResponseErrors.Error);
							}
						}
						else
						{
							remoteServiceResult.Properties["response"] = UpdateAccountResponseErrors.Error;
						}
					}
				}
				else
				{
					LogHelper.Instance.Log("In stand alone mode, skipping call to UpdateAccount.", LogEntryType.Info);
					remoteServiceResult.Properties["response"] = UpdateAccountResponseErrors.CommunicationError;
					remoteServiceResult.Properties["is_online"] = false;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.UpdateAccount.", e);
				remoteServiceResult.Properties["response"] = UpdateAccountResponseErrors.Error;
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
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformLiveUpdateAccount."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	private void QueueAccountUpdate(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Enqueue call to remote IPersonalizationServices.QueueAccountUpdate, URL: {0}", KioskServicesProxy.Instance.Url);
				IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
				errorList = ServiceLocator.Instance.GetService<IQueueService>().Enqueue(254, "UpdateAccount", new
				{
					KioskId = kioskId,
					MessageId = Guid.NewGuid(),
					MessageType = "UpdateAccount",
					EngineVersion = service["EngineVersion"],
					BundleVersion = service["ProductVersion"],
					CustomerProfileNumber = customerProfileNumber,
					Email = emailAddress,
					MobilePhoneNumber = phoneNumber,
					Pin = Encrypt(SecureStringToString(pin)),
					PunchCardOptin = punchcardOptIn,
					TextClubOptin = textClubOptIn
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.QueueAccountUpdate.", e);
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
							Message = "Invoking callback from PersonalizationServicesProxy.QueueAccountUpdate."
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

	private static object GetResponseValue(IDictionary<string, object> response, string key, object DefaultValue)
	{
		if (!response.ContainsKey(key) || response[key] == null)
		{
			return DefaultValue;
		}
		return response[key];
	}

	private static string Encrypt(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return null;
		}
		byte[] bytes = Encoding.Default.GetBytes(data);
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		return Convert.ToBase64String(memoryStream.ToArray());
	}
}
