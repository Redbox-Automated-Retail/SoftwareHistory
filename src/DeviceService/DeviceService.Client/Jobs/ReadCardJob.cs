using System;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.Client.Jobs;

public class ReadCardJob : BaseReadCardJob
{
	private readonly CardReadRequest _cardReadRequest;

	public ReadCardJob(IDeviceServiceClient dsc, CardReadRequest request, Action<BaseResponseEvent> jobCompleteCallback, int? timeout)
		: base(dsc, jobCompleteCallback, timeout)
	{
		_cardReadRequest = request;
	}

	protected override void PerformCardRead()
	{
		_deviceServiceClient.ReadCard(_readCardCommandRequestId, _cardReadRequest, _jobCompletedCallback, _timeout);
	}
}
