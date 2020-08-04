using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UpdateClientService.API.Services.IoT.UpdateServiceRequests;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace UpdateClientService.API.Services.IoT;

public class IoTService : IIoTService
{
	private readonly IMqttProxy _mqttRepo;

	private readonly IStoreService _store;

	private readonly ILogger<IoTService> _logger;

	private readonly AutoResetEvent _mre = new AutoResetEvent(initialState: false);

	private string _receivedMessage = string.Empty;

	public IoTService(IMqttProxy mqttRepo, IStoreService store, ILogger<IoTService> logger)
	{
		_mqttRepo = mqttRepo;
		_store = store;
		_logger = logger;
	}

	public async Task<string> UpdateServiceRequest(UpdateServiceRequest request)
	{
		request.KioskId = _store.KioskId.ToString();
		int instanceNumber = 1;
		bool firstIteration = true;
		await _mqttRepo.SubscribeToTemporaryTopicsAsync(new List<string> { $"redbox/kiosk/{_store.KioskId}/request-id/{request.RequestId}/updateservice-request/response" }, TimeSpan.FromSeconds(90.0), delegate(object sender, MqttMsgPublishEventArgs e)
		{
			_receivedMessage = Encoding.UTF8.GetString(e.Message);
			_mqttRepo.LastAvailableUSInstance = instanceNumber;
			_mre.Set();
		});
		for (; (instanceNumber != _mqttRepo.LastAvailableUSInstance) | firstIteration; instanceNumber++)
		{
			if (firstIteration)
			{
				instanceNumber = _mqttRepo.LastAvailableUSInstance;
				firstIteration = false;
			}
			await _mqttRepo.PublishMessageAsync($"redbox/updateservice-instance/{instanceNumber}/request", request.ToJson());
			if (_mre.WaitOne(TimeSpan.FromSeconds(20.0)))
			{
				break;
			}
			if (instanceNumber == 4)
			{
				instanceNumber = 0;
			}
		}
		return _receivedMessage;
	}
}
