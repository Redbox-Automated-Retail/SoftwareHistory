using System;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace Redbox.KioskEngine.Environment;

internal class ReadCardJob : IReadCardJob
{
	private Guid _requestId;

	public CardReadRequest CardReadRequest { get; private set; }

	public Action<BaseResponseEvent> ReadCardResponseHandler { get; private set; }

	public Action<BaseResponseEvent> ReadCardEventHandler { get; private set; }

	public int CommandTimeout { get; private set; }

	public bool IsCardInserted { get; set; }

	public bool IsExecuting { get; private set; }

	public Guid RequestId => _requestId;

	public ReadCardJob(CardReadRequest cardReadRequest, Action<BaseResponseEvent> readCardResponseHandler, Action<BaseResponseEvent> readCardEventHandler, int commandTimeout)
	{
		CardReadRequest = cardReadRequest;
		ReadCardResponseHandler = readCardResponseHandler;
		ReadCardEventHandler = readCardEventHandler;
		CommandTimeout = commandTimeout;
		_requestId = Guid.NewGuid();
		IsExecuting = false;
	}

	public bool Cancel(Action<BaseResponseEvent> cancelCompleteCallback)
	{
		IsExecuting = false;
		cancelCompleteCallback(new CancelCommandResponseEvent(null)
		{
			RequestId = _requestId,
			Success = true,
			EventName = "Cancel"
		});
		return true;
	}

	public bool Execute()
	{
		IsExecuting = true;
		return true;
	}

	public void Stop()
	{
		IsExecuting = false;
	}
}
