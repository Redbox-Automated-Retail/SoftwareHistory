using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.Proxy;

public class PersonalizationServicesProxySimulator : IPersonalizationServices
{
	private int m_working;

	public static PersonalizationServicesProxySimulator Instance => Singleton<PersonalizationServicesProxySimulator>.Instance;

	public KioskLoginResponseErrors ResponseCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "KioskLoginResponseError", "Error");
			return (KioskLoginResponseErrors)Enum.Parse(typeof(KioskLoginResponseErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "KioskLoginResponseError", value);
		}
	}

	public UpdateAccountResponseErrors UpdateAccountResponseCode
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "UpdateAccountResponseError", "Error");
			return (UpdateAccountResponseErrors)Enum.Parse(typeof(UpdateAccountResponseErrors), value, ignoreCase: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "UpdateAccountResponseError", value);
		}
	}

	public bool LiveCall
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LiveCall", defaultValue: true);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LiveCall", value);
		}
	}

	public bool SimulateSuccess
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "SimulateSuccess", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "SimulateSuccess", value);
		}
	}

	public string CustomerProfileNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "CustomerProfileNumber", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "CustomerProfileNumber", value);
		}
	}

	public string FirstName
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "FirstName", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "FirstName", value);
		}
	}

	public string LoginEmail
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoginEmail", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoginEmail", value);
		}
	}

	public string PhoneNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PhoneNumber", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PhoneNumber", value);
		}
	}

	public string PostalCode
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PostalCode", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PostalCode", value);
		}
	}

	public DateTime? BirthDate
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "BirthDate", DateTime.MinValue);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "BirthDate", value);
		}
	}

	public int LoyaltyPointBalance
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoyaltyPointBalance", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoyaltyPointBalance", value);
		}
	}

	public string LoyaltyTier
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoyaltyTier", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoyaltyTier", value);
		}
	}

	public int LoyaltyTierCounter
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "LoyaltyTierCounter", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "LoyaltyTierCounter", value);
		}
	}

	public int PointsExpiring
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PointsExpiring", 0);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PointsExpiring", value);
		}
	}

	public DateTime? PointsExpirationDate
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PointsExpirationDate", DateTime.MinValue);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PointsExpirationDate", value);
		}
	}

	public string MobilePhoneNumber
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "MobilePhoneNumber", "SimulatedValue");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "MobilePhoneNumber", value);
		}
	}

	public bool PromptForPerks
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PromptForPerks", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PromptForPerks", value);
		}
	}

	public bool PromptForEarlyId
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PromptForEarlyId", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PromptForEarlyId", value);
		}
	}

	public bool TextClubEnabled
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "TextClubEnabled", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "TextClubEnabled", value);
		}
	}

	public string RecommendedTitlesOfferCode
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "RecommendedTitlesOfferCode", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "RecommendedTitlesOfferCode", value);
		}
	}

	public string TivoQueryId
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "TivoQueryId", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "TivoQueryId", value);
		}
	}

	public RecommendedTitlesIdType RecommendedTitlesIdType
	{
		get
		{
			if (!(ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "RecommendedTitlesIdType", "1") == "1"))
			{
				return RecommendedTitlesIdType.ProductGroupID;
			}
			return RecommendedTitlesIdType.TitleID;
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			int num = (int)value;
			service.SetValue("Remote Services\\Personalization Services", "RecommendedTitlesIdType", num.ToString());
		}
	}

	public string RecommendedTitleIds
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "RecommendedTitleIds", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "RecommendedTitleIds", value);
		}
	}

	public string PersonalizationTitleTags
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Personalization Services", "PersonalizationTitleTags", string.Empty);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Personalization Services", "PersonalizationTitleTags", value);
		}
	}

	public List<IStoredPromoCode> StoredPromoCodes => new List<IStoredPromoCode>
	{
		new StoredPromoCode
		{
			PromoCode = "TESTCODE1",
			UsageDescription = "Onceonly",
			ExpirationText = "EXPIRES TODAY",
			CampaignName = "Rent 1 disc, get a Free 1-night DVD rental",
			ExpirationDate = DateTime.Now,
			RedemptionTypeDescription = "FixedValue",
			DaysUntilExpiration = null
		},
		new StoredPromoCode
		{
			PromoCode = "TESTBDAY",
			UsageDescription = "birthday rental",
			ExpirationText = "TWO DAYS LEFT",
			CampaignName = "1 free birthday rental",
			ExpirationDate = DateTime.Now.AddDays(2.0),
			RedemptionTypeDescription = "FixedValue",
			DaysUntilExpiration = 1
		}
	};

	public List<IAcceptedOffer> AcceptedOffers => new List<IAcceptedOffer>
	{
		new AcceptedOffer
		{
			Status = "Active",
			EndValueText = "2x pts",
			Name = "Multi-night Rentals",
			TimeToComplete = "7 D",
			Description = "Earn 2X the points for every multi-night rental.",
			CompleteDate = DateTime.Now.AddHours(1.0),
			MaxValue = 0,
			RemainderValue = 2,
			CurrentValue = 0
		},
		new AcceptedOffer
		{
			Status = "Active",
			EndValueText = "5000 points",
			Name = "Game Dash - Genre",
			TimeToComplete = null,
			Description = "Rent 6 Xbox or Playstation games in 14 days. ",
			CompleteDate = DateTime.Now.AddHours(26.0),
			CurrentValue = 2,
			MaxValue = 6,
			RemainderValue = 4
		},
		new AcceptedOffer
		{
			Status = "Completed",
			EndValueText = "250 pts",
			Name = "Maximize Your Cart",
			TimeToComplete = "junk",
			Description = "Rent 6 Xbox or Playstation games in 14 days. ",
			CompleteDate = DateTime.Now.AddDays(15.0)
		}
	};

	private PersonalizationServicesProxySimulator()
	{
	}

	public void Login(int kioskId, string emailAddress, SecureString password, string phoneNumber, SecureString pin, string languageCode, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Simulate call to remote IPersonalization.Login");
				remoteServiceResult.Success = SimulateSuccess;
				remoteServiceResult.Properties["CustomerProfileNumber"] = CustomerProfileNumber;
				remoteServiceResult.Properties["FirstName"] = FirstName;
				remoteServiceResult.Properties["LoginEmail"] = LoginEmail;
				remoteServiceResult.Properties["PhoneNumber"] = PhoneNumber;
				remoteServiceResult.Properties["PostalCode"] = PostalCode;
				remoteServiceResult.Properties["BirthDate"] = BirthDate;
				remoteServiceResult.Properties["LoyaltyPointBalance"] = LoyaltyPointBalance;
				remoteServiceResult.Properties["LoyaltyTier"] = LoyaltyTier;
				remoteServiceResult.Properties["LoyaltyTierCounter"] = LoyaltyTierCounter;
				remoteServiceResult.Properties["PointsExpiring"] = PointsExpiring;
				remoteServiceResult.Properties["PointsExpirationDate"] = PointsExpirationDate;
				remoteServiceResult.Properties["MobilePhoneNumber"] = MobilePhoneNumber;
				remoteServiceResult.Properties["PromptForPerks"] = PromptForPerks;
				remoteServiceResult.Properties["PromptForEarlyId"] = PromptForEarlyId;
				remoteServiceResult.Properties["TextClubEnabled"] = TextClubEnabled;
				remoteServiceResult.Properties["server_name"] = "SimulatedCall";
				RecommendedTitles recommendedTitles = new RecommendedTitles
				{
					TivoQueryId = TivoQueryId,
					OfferCode = RecommendedTitlesOfferCode
				};
				string[] array = PersonalizationTitleTags.Split(new char[3] { '[', ']', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				int num = 0;
				if (!string.IsNullOrWhiteSpace(RecommendedTitleIds))
				{
					string[] array2 = RecommendedTitleIds.Split(',');
					for (int i = 0; i < array2.Length; i++)
					{
						if (long.TryParse(array2[i], out var result))
						{
							string[] source = new string[0];
							if (num < array.Length)
							{
								source = array[num].Split(',');
							}
							recommendedTitles.Add(new RecommendedTitleId
							{
								Id = result,
								IdType = RecommendedTitlesIdType,
								PersonalizationTitleTags = source.ToList()
							});
						}
						num++;
					}
				}
				remoteServiceResult.Properties["RecommendedTitles"] = new List<IRecommendedTitles> { recommendedTitles };
				remoteServiceResult.Properties["StoredPromoCodes"] = StoredPromoCodes;
				remoteServiceResult.Properties["AcceptedOffers"] = AcceptedOffers;
				if (!SimulateSuccess)
				{
					remoteServiceResult.Properties["response"] = ResponseCode;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.PerformSimulatedLogin.", e);
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
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformSimulatedLogin."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void UpdateAccount(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback, bool queued = false)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			using ExecutionTimer executionTimer = new ExecutionTimer();
			try
			{
				Interlocked.Increment(ref m_working);
				LogHelper.Instance.Log("Simulate call to remote IPersonalization.UpdateAccount");
				remoteServiceResult.Success = SimulateSuccess;
				remoteServiceResult.Properties["CustomerProfileNumber"] = CustomerProfileNumber;
				remoteServiceResult.Properties["MobilePhoneNumber"] = MobilePhoneNumber;
				remoteServiceResult.Properties["server_name"] = "SimulatedCall";
				if (!SimulateSuccess)
				{
					remoteServiceResult.Properties["response"] = UpdateAccountResponseCode;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PersonalizationServicesProxy.PerformSimulatedUpdateAccount.", e);
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
						Message = "Invoking completeCallback from PersonalizationServicesProxy.PerformSimulatedUpdateAccount."
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
