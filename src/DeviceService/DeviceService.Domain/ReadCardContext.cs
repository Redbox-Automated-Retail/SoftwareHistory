using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Microsoft.Extensions.Logging;

namespace DeviceService.Domain;

public class ReadCardContext
{
	private readonly ILogger<IUC285Proxy> _logger;

	private bool CompleteCallbackSent;

	public ICardReadRequest Request { get; set; }

	public int TimeRemainingUntilTimeout => Math.Max(0, (int)((Request?.Timeout ?? 30000) - (Watch?.ElapsedMilliseconds ?? 0)));

	public CancellationToken CancelToken { get; set; }

	public Action<Base87CardReadModel> JobCompletedCallback { get; set; }

	public Action<string, string> EventsCallback { get; set; }

	public CardSourceType CardSource { get; set; }

	public Stopwatch Watch { get; set; }

	public IList<Error> Errors { get; set; } = new List<Error>();

	public ReadCardContextState State { get; set; }

	public Base87CardReadModel ResponseDataModel { get; set; }

	public bool CardRemoved { get; set; }

	public CardBrandEnum CardBrand { get; set; }

	public CardStats CardStats { get; set; }

	public UnitDataModel UnitData { get; set; }

	public CardReadExitType ExitType { get; set; }

	public ReadCardContext(ILogger<IUC285Proxy> logger)
	{
		_logger = logger;
		Watch = new Stopwatch();
		Watch.Start();
	}

	public void SendCompletedCallback()
	{
		if (CompleteCallbackSent)
		{
			return;
		}
		CompleteCallbackSent = true;
		_logger?.LogInformation($"Send Card Read Model - Callback Exists: {JobCompletedCallback != null}, Model Exists: {ResponseDataModel != null}");
		Watch.Stop();
		if (ResponseDataModel == null || CardBrandAndSourceIsDeactivated())
		{
			switch (CardSource)
			{
			case CardSourceType.EMVContact:
			case CardSourceType.QuickChip:
				ResponseDataModel = new EMVCardReadModel();
				break;
			case CardSourceType.EMVContactless:
			case CardSourceType.Mobile:
				ResponseDataModel = new EMVCardReadModel();
				break;
			default:
				ResponseDataModel = new EncryptedCardReadModel();
				break;
			}
			switch (State)
			{
			case ReadCardContextState.Tampered:
				Errors.Add(new Error
				{
					Code = "TAMPERED",
					Message = "Device is reporting Tampered Alert."
				});
				ResponseDataModel.Status = ResponseStatus.Tampered;
				break;
			case ReadCardContextState.Cancelled:
				Errors.Add(new Error
				{
					Code = "CANCELLED",
					Message = "Command Was Cancelled."
				});
				ResponseDataModel.Status = ResponseStatus.Cancelled;
				break;
			case ReadCardContextState.Timeout:
				Errors.Add(new Error
				{
					Code = "TIMEOUT",
					Message = $"Command Timed Out.  Exceeded {Request?.Timeout ?? 30000} Timeout"
				});
				ResponseDataModel.Status = ResponseStatus.Timeout;
				break;
			case ReadCardContextState.Blocked:
				Errors.Add(new Error
				{
					Code = "BLOCKED",
					Message = $"Card brand {CardBrand.ToString()} and it's source {CardSource} is disabled in system.config"
				});
				ResponseDataModel.Status = ResponseStatus.Blocked;
				break;
			default:
				ResponseDataModel.Status = ResponseStatus.Errored;
				Errors.Add(new Error
				{
					Code = "SCC001",
					Message = "Sending Default Error, ResponseDataModel Missing."
				});
				break;
			}
		}
		if (ResponseDataModel is EMVCardReadModel)
		{
			((EMVCardReadModel)ResponseDataModel).CardRemoved = CardRemoved;
		}
		ResponseDataModel.TimeTaken = Watch.Elapsed;
		ResponseDataModel.AddErrors(Errors);
		ResponseDataModel.CardSource = CardSource;
		CardStats = new CardStats
		{
			SessionId = Request?.SessionId,
			Id = Guid.NewGuid(),
			CardBrand = CardBrand,
			ManufacturerSerialNumber = UnitData.ManufacturingSerialNumber,
			RBAVersion = UnitData?.GetApplicationVersionString(),
			ReadResult = ExitType,
			SourceType = CardSource,
			ErrorCode = ResponseDataModel.ErrorCode
		};
		JobCompletedCallback?.Invoke(ResponseDataModel);
	}

	public void SendCardRemoved()
	{
		CardRemoved = true;
		EventsCallback?.Invoke("CardRemovedResponseEvent", null);
	}

	public void SendCardProcessing()
	{
		EventsCallback?.Invoke("CardProcessingStartedResponseEvent", CardSource.ToString());
	}

	public bool CardBrandAndSourceIsDeactivated()
	{
		if (Request.ExcludeCardBrandBySource == null)
		{
			return false;
		}
		bool flag = false;
		CardBrandAndSource? cardBrandAndSource = null;
		foreach (CardBrandAndSource item in Request.ExcludeCardBrandBySource)
		{
			GeneralSourceType generalSourceType = item.GetGeneralSourceType();
			GeneralSourceType generalSourceType2 = CardSource.GetGeneralSourceType();
			if (generalSourceType == generalSourceType2 && generalSourceType != GeneralSourceType.None && item.GetBrand() == CardBrand)
			{
				flag = true;
				cardBrandAndSource = item;
				break;
			}
		}
		if (flag)
		{
			State = ReadCardContextState.Blocked;
			_logger?.LogInformation($"{cardBrandAndSource} is deactivated in system.config");
		}
		return flag;
	}
}
