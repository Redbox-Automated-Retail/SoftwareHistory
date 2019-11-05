using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Session;

namespace Redbox.KioskEngine.Environment.CardDataReader;

public class DeviceServiceTrackDataService : IDeviceServiceTrackDataService, ITrackDataService
{
	private IReadCardJob _readCardJob;

	private bool _cardReadAborted;

	public bool IsInTechnicalFallback
	{
		get
		{
			bool result = false;
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			if (service != null && service.CardReadTechnicalFallbackEnabled)
			{
				int num = (ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession())?.CardReadTechnicalFallbackCount ?? 0;
				int num2 = service?.CardReadTechnicalFallbackCountLimit ?? 3;
				result = num >= num2;
			}
			return result;
		}
	}

	public FallbackType? NextFallbackReason { get; set; }

	private bool IsCardReadInProgress => _readCardJob != null;

	public event CardReadCompleteHandler OnCardReadComplete;

	public event CardReadStartedEventHandler OnCardReadStarted;

	public void ClearCardReadHandlers()
	{
		OnCardReadComplete = null;
		OnCardReadStarted = null;
	}

	public void StartCardRead(int timeout, DeviceInputType deviceInputType, Action<IReadCardJob> cardReadStartedAction)
	{
		IDeviceServiceClientHelper deviceServiceClientHelper = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (deviceServiceClientHelper == null)
		{
			return;
		}
		_cardReadAborted = false;
		Task.Run(delegate
		{
			try
			{
				_readCardJob = deviceServiceClientHelper.StartCardRead(deviceInputType, CardReadResponseHandler, CardReadEventHandler, timeout);
				cardReadStartedAction?.Invoke(_readCardJob);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceTrackDataService.StartCardRead.", e);
			}
		});
	}

	public void StopCardRead()
	{
		_cardReadAborted = true;
		if (!IsCardReadInProgress)
		{
			return;
		}
		IReadCardJob tempReadCardJob = _readCardJob;
		ISession session = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		if (tempReadCardJob != null)
		{
			ICardReadAttempt cardReadAttempt = session?.CardReadAttemptCollection?.GetByCardReadJobCommandRequestId(tempReadCardJob.RequestId);
			if (cardReadAttempt != null)
			{
				cardReadAttempt.AttemptedCancel = true;
				LogHelper.Instance.Log($"Setting CardReadAttempt.AttemptedCancel = true for card read command {tempReadCardJob.RequestId}");
			}
		}
		Task.Run(delegate
		{
			try
			{
				tempReadCardJob.Cancel(delegate(BaseResponseEvent baseResponseEvent)
				{
					LogHelper.Instance.Log($"DeviceServiceTrackDataService.StopCardRead - cancel card read success = {(baseResponseEvent as CancelCommandResponseEvent)?.Success ?? false}");
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in DeviceServiceTrackDataService.StopCardRead.", e);
			}
		});
	}

	private void CardReadEventHandler(BaseResponseEvent baseResponseEvent)
	{
		if (baseResponseEvent?.EventName == "CardProcessingStartedResponseEvent")
		{
			OnCardReadStarted?.Invoke();
		}
	}

	private void CardReadResponseHandler(BaseResponseEvent baseResponseEvent)
	{
		_readCardJob = null;
		bool chipCardRemovedFromReader = true;
		ITrackData trackData = GetTrackDataFromEMVCardReadResponseEvent(baseResponseEvent, ref chipCardRemovedFromReader);
		if (trackData == null)
		{
			trackData = GetTrackDataFromEncryptedCardReadResponseEvent(baseResponseEvent);
			if (trackData == null)
			{
				trackData = GetTrackDataFromUnencryptedCardReadResponseEvent(baseResponseEvent);
				if (trackData == null)
				{
					trackData = GetTrackDataFromCardReadUnencryptedResponseEvent(baseResponseEvent);
				}
			}
		}
		UpdateTechnicalFallbackState(trackData);
		DeclineChipEnabledCardThatWasSwiped(trackData);
		DeclineAmexFallback(trackData);
		UpdateCardReadTechnicalFallbackInfo(trackData);
		UpdateNextCardReadTechnicalFallbackState(trackData);
		CardHelper.CheckForTrackDataErrors(trackData);
		UpdateCreditCardHealthService(trackData, baseResponseEvent);
		if (!_cardReadAborted)
		{
			OnCardReadComplete?.Invoke(trackData, chipCardRemovedFromReader);
		}
	}

	private static void UpdateCreditCardHealthService(ITrackData trackData, BaseResponseEvent responseEvent)
	{
		if (ResponseEventIsTimedOut(responseEvent) || GetCardReadAttemptCancelled(responseEvent) || GetCardReadResponseCancelledOrBlocked(responseEvent))
		{
			LogHelper.Instance.Log("Card Read Cancelled or Blocked or Timed Out, Don't Notify Health Service.");
			return;
		}
		ICCReaderHealth service = ServiceLocator.Instance.GetService<ICCReaderHealth>();
		bool flag = trackData?.HasValidData() ?? false;
		bool hasError = !flag;
		service?.EventOccurred(flag, hasError);
	}

	private static bool ResponseEventIsTimedOut(BaseResponseEvent responseEvent)
	{
		return responseEvent is TimeoutResponseEvent;
	}

	private static bool GetCardReadAttemptCancelled(BaseResponseEvent responseEvent)
	{
		bool result = false;
		if (responseEvent != null)
		{
			result = ((ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession())?.CardReadAttemptCollection?.GetByCardReadJobCommandRequestId(responseEvent.RequestId))?.AttemptedCancel ?? false;
		}
		return result;
	}

	private static bool GetCardReadResponseCancelledOrBlocked(BaseResponseEvent responseEvent)
	{
		Base87CardReadModel base87CardReadModel = (responseEvent as ICardReadResponseEvent)?.GetBase87CardReadModel();
		if (base87CardReadModel != null)
		{
			if (base87CardReadModel.Status != ResponseStatus.Cancelled)
			{
				return base87CardReadModel.Status == ResponseStatus.Blocked;
			}
			return true;
		}
		return false;
	}

	private ITrackData GetTrackDataFromEMVCardReadResponseEvent(BaseResponseEvent baseResponseEvent, ref bool chipCardRemovedFromReader)
	{
		ITrackData result = null;
		EMVCardReadModel eMVCardReadModel = (baseResponseEvent as EMVCardReadResponseEvent)?.Data;
		if (eMVCardReadModel != null)
		{
			chipCardRemovedFromReader = eMVCardReadModel.CardRemoved;
			EMVTrackData eMVTrackData = new EMVTrackData();
			PopulateEncryptedTrackDataFields(eMVTrackData, eMVCardReadModel);
			eMVTrackData.Tags = new Dictionary<string, string>(eMVCardReadModel.Tags);
			eMVTrackData.AID = eMVCardReadModel.AID;
			result = eMVTrackData;
		}
		return result;
	}

	private static void UpdateCardReadTechnicalFallbackInfo(ITrackData trackData)
	{
		if (trackData == null)
		{
			return;
		}
		IRentalSessionService service = ServiceLocator.Instance.GetService<IRentalSessionService>();
		IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
		ISession session = service?.GetCurrentSession();
		switch (trackData.FallbackStatusAction)
		{
		case FallbackStatusAction.IncrementFallbackCounter:
			if (session != null)
			{
				session.CardReadTechnicalFallbackCount++;
				session.LastFallbackType = trackData?.FallbackReason;
			}
			break;
		case FallbackStatusAction.ResetFallbackCounter:
			if (session != null)
			{
				session.CardReadTechnicalFallbackCount = 0;
				if (!trackData.IsInTechnicalFallback)
				{
					session.LastFallbackType = null;
				}
			}
			break;
		case FallbackStatusAction.ImmediateFallback:
			if (session != null && service2 != null)
			{
				session.CardReadTechnicalFallbackCount = service2.CardReadTechnicalFallbackCountLimit;
				session.LastFallbackType = trackData?.FallbackReason;
			}
			break;
		case FallbackStatusAction.NoAction:
			break;
		}
	}

	private void UpdateTechnicalFallbackState(ITrackData trackData)
	{
		if (trackData != null)
		{
			trackData.IsInTechnicalFallback = IsInTechnicalFallback;
		}
	}

	private void UpdateNextCardReadTechnicalFallbackState(ITrackData trackData)
	{
		if (trackData != null)
		{
			trackData.NextCardReadIsInTechnicalFallback = IsInTechnicalFallback;
		}
	}

	private void DeclineAmexFallback(ITrackData trackData)
	{
		if (trackData != null && trackData.IsInTechnicalFallback && trackData.CardSourceType == CardSourceType.Swipe && trackData.CardHasChip && trackData.CardType == Redbox.Core.CardType.AmericanExpress)
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			if (service != null && service.ChipEnabled && (service == null || service.CardReadTechnicalFallbackEnabled) && service2 != null && service2.SupportsEMV)
			{
				trackData.Errors.Add(CardHelper.ErrorCodes.NewAmexCannotFallbackError());
				trackData.FallbackStatusAction = FallbackStatusAction.ResetFallbackCounter;
			}
		}
	}

	private void DeclineChipEnabledCardThatWasSwiped(ITrackData trackData)
	{
		if (trackData != null && !trackData.IsInTechnicalFallback && trackData.CardSourceType == CardSourceType.Swipe && trackData.CardHasChip)
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
			if (service != null && service.ChipEnabled && service2 != null && service2.SupportsEMV)
			{
				trackData.Errors.Add(CardHelper.ErrorCodes.NewSwipedCardIsChipEnabledError());
			}
		}
	}

	private ITrackData GetTrackDataFromEncryptedCardReadResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		ITrackData trackData = null;
		EncryptedCardReadModel encryptedCardReadModel = (baseResponseEvent as EncryptedCardReadResponseEvent)?.Data;
		if (encryptedCardReadModel != null)
		{
			trackData = new EncryptedTrackData();
			PopulateEncryptedTrackDataFields(trackData as IEncryptedTrackData, encryptedCardReadModel);
		}
		return trackData;
	}

	private void PopulateEncryptedTrackDataFields(IEncryptedTrackData encryptedTrackData, IEncryptedCardReadModel encryptedCardReadModel)
	{
		if (encryptedTrackData != null)
		{
			if (encryptedCardReadModel != null)
			{
				encryptedTrackData.PANLength = encryptedCardReadModel.PANLength;
				encryptedTrackData.ExtLangCode = encryptedCardReadModel.ExtLangCode;
				encryptedTrackData.LSEncData = encryptedCardReadModel.LSEncData;
				encryptedTrackData.LSEncDataLength = encryptedCardReadModel.LSEncDataLength;
				encryptedTrackData.AESPAN = encryptedCardReadModel.AESPAN;
				encryptedTrackData.AESPANLength = encryptedCardReadModel.AESPANLength;
				encryptedTrackData.ICEncData = encryptedCardReadModel.ICEncData;
				encryptedTrackData.ICEncDataLength = encryptedCardReadModel.ICEncDataLength;
				encryptedTrackData.KSN = encryptedCardReadModel.KSN;
				encryptedTrackData.MfgSerialNumber = encryptedCardReadModel.MfgSerialNumber;
				encryptedTrackData.EncFormat = encryptedCardReadModel.EncFormat;
				encryptedTrackData.Name = encryptedCardReadModel.Name;
				encryptedTrackData.NameLength = encryptedCardReadModel.NameLength;
				encryptedTrackData.LanguageCode = encryptedCardReadModel.LanguageCode;
				encryptedTrackData.ServiceCode = encryptedCardReadModel.ServiceCode;
				encryptedTrackData.Expiry = encryptedCardReadModel.Expiry;
				encryptedTrackData.Mod10CheckFlag = encryptedCardReadModel.Mod10CheckFlag;
				encryptedTrackData.EncryptedFlag = encryptedCardReadModel.EncryptedFlag;
				encryptedTrackData.InjectedSerialNumber = encryptedCardReadModel.InjectedSerialNumber;
				PopulateBaseCardReadProperties(encryptedTrackData, encryptedCardReadModel as Base87CardReadModel);
			}
			else
			{
				LogHelper.Instance.Log("DeviceServiceTrackDataService.PopulateEncryptedTrackDataFields error: encrypted card read model is null.");
			}
		}
		else
		{
			LogHelper.Instance.Log("DeviceServiceTrackDataService.PopulateEncryptedTrackDataFields error: encrypted track data is null.");
		}
	}

	public bool IsInNumericRange(string data, int start, int? end)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			return false;
		}
		if (!int.TryParse(data, out var result))
		{
			return false;
		}
		if (start > result)
		{
			return false;
		}
		if (end.HasValue && end < result)
		{
			return false;
		}
		return true;
	}

	private ITrackData GetTrackDataFromUnencryptedCardReadResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		ITrackData trackData = null;
		if (baseResponseEvent is UnencryptedCardReadResponseEvent { Data: var data })
		{
			if (data != null)
			{
				if (data.FirstSix == "601056" && data.Track1 == null)
				{
					data.Track1 = "REDBOX";
				}
				trackData = GenerateTrackDataFromTrack1AndTrack2(data.Track1, data.Track2);
				IUnencryptedTrackData unencryptedTrackData = trackData as IUnencryptedTrackData;
				if (unencryptedTrackData != null && unencryptedTrackData.HasValidData())
				{
					unencryptedTrackData.CardSourceType = data.CardSource;
					ITrackDataEncryptionService service = ServiceLocator.Instance.GetService<ITrackDataEncryptionService>();
					unencryptedTrackData.EncryptionCerificateKeyId = service.KeyId;
					unencryptedTrackData.EncryptedAccountNumber = service.GetEncryptedAccountNumber(unencryptedTrackData);
					unencryptedTrackData.EncryptedTrack2 = service.GetEncryptedTrack2(unencryptedTrackData);
				}
				unencryptedTrackData?.LockData();
			}
			PopulateBaseCardReadProperties(trackData, data);
		}
		return trackData;
	}

	private void PopulateBaseCardReadProperties(ITrackData trackData, Base87CardReadModel baseCardReadModel)
	{
		if (trackData == null || baseCardReadModel == null)
		{
			return;
		}
		trackData.FallbackStatusAction = baseCardReadModel.FallbackStatusAction;
		trackData.FallbackReason = baseCardReadModel.FallbackReason;
		trackData.CardHasChip = baseCardReadModel.HasChip;
		trackData.CardSourceType = baseCardReadModel.CardSource;
		trackData.FirstSix = baseCardReadModel?.FirstSix;
		trackData.LastFour = baseCardReadModel?.LastFour;
		baseCardReadModel?.Errors?.ForEach(delegate(DeviceService.ComponentModel.Error x)
		{
			trackData.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("DeviceServiceError", x.Code, x.Message));
		});
		trackData.ReadStatus = baseCardReadModel.Status;
		if (string.IsNullOrEmpty(trackData.LastName))
		{
			CardHelper.ParseNameData(baseCardReadModel?.Name, trackData);
		}
		if (!trackData.CardType.HasValue && baseCardReadModel != null && baseCardReadModel.FirstSix != null && baseCardReadModel != null && baseCardReadModel.LastFour != null)
		{
			string arg = string.Empty.PadRight(Math.Max(baseCardReadModel.PANLength - 10, 0), 'X');
			string cardNumber = $"{baseCardReadModel.FirstSix}{arg}{baseCardReadModel.LastFour}";
			CardHelper.AssignCardType(trackData, cardNumber);
			if (trackData.CardType == Redbox.Core.CardType.RedboxGiftCard)
			{
				CardHelper.AssignRedboxGiftCardAccountNumber(trackData);
			}
		}
		if (string.IsNullOrEmpty(trackData.ExpiryYear) || string.IsNullOrEmpty(trackData.ExpiryMonth))
		{
			trackData.ExpiryYear = ((baseCardReadModel != null && baseCardReadModel.Expiry != null && baseCardReadModel.Expiry.Length == 4) ? baseCardReadModel.Expiry.Substring(0, 2) : null);
			trackData.ExpiryMonth = ((baseCardReadModel != null && baseCardReadModel.Expiry != null && baseCardReadModel.Expiry.Length == 4) ? baseCardReadModel.Expiry.Substring(2, 2) : null);
		}
		AddTamperedCardReadErrorsToTrackData(trackData, baseCardReadModel);
	}

	private void AddTamperedCardReadErrorsToTrackData(ITrackData trackData, Base87CardReadModel baseCardReadModel)
	{
		if (trackData != null && baseCardReadModel != null)
		{
			trackData.Errors.AddRange(baseCardReadModel.GetTamperedErrors());
		}
	}

	private ITrackData GetTrackDataFromCardReadUnencryptedResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		ITrackData result = null;
		if (baseResponseEvent is UnencryptedCardReadResponseEvent { Data: not null } unencryptedCardReadResponseEvent)
		{
			result = GenerateTrackDataFromTrack1AndTrack2(unencryptedCardReadResponseEvent.Data.Track1, unencryptedCardReadResponseEvent.Data.Track2);
		}
		return result;
	}

	private ITrackData GenerateTrackDataFromTrack1AndTrack2(string track1, string track2)
	{
		ITrackData result = null;
		if (track1 != null || track2 != null)
		{
			SecureString secureString = new SecureString();
			string text = $"{track1};{track2}?";
			foreach (char c in text)
			{
				secureString.AppendChar(c);
			}
			result = CardHelper.Parse(secureString);
			secureString.Dispose();
		}
		return result;
	}
}
