using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UpdateClientService.API.Services.IoT.Commands.Controller;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace UpdateClientService.API.Services.IoT.Commands;

public class IoTCommandService : IIoTCommandService
{
	private readonly IMqttProxy _mqttProxy;

	private readonly IStoreService _store;

	private readonly List<ICommandIoTController> _commandList;

	private readonly ILogger<IoTCommandService> _logger;

	public IoTCommandService(IMqttProxy mqttProxy, IStoreService store, List<ICommandIoTController> commandList, ILogger<IoTCommandService> logger)
	{
		_mqttProxy = mqttProxy;
		_store = store;
		_commandList = commandList;
		_logger = logger;
	}

	public async Task SubscribeToCommandTopics()
	{
		await _mqttProxy.SubscribeToTopicsAsync(new List<string>
		{
			$"redbox/kiosk/{_store.KioskId}/command",
			"redbox/kiosk/market/" + _store.Market + "/command",
			"redbox/kiosk/all/command"
		}, async delegate(object sender, MqttMsgPublishEventArgs args)
		{
			await IoTCommandCallbackMethodAsync(args);
		});
	}

	private async Task IoTCommandCallbackMethodAsync(MqttMsgPublishEventArgs args)
	{
		try
		{
			string text = Encoding.UTF8.GetString(args.Message);
			_logger.LogInfoWithSource("message received topic: " + args.Topic + ", message: " + text, "IoTCommandCallbackMethodAsync");
			IoTCommandModel iotCommandModel = JsonConvert.DeserializeObject<IoTCommandModel>(text);
			ICommandIoTController requestedCommand = _commandList?.FirstOrDefault((ICommandIoTController x) => x.CommandEnum == iotCommandModel.Command);
			if (requestedCommand == null)
			{
				_logger.LogErrorWithSource($"IoT Command {iotCommandModel.Command} could not found in registered command list: {_commandList?.ToJson()}.", "IoTCommandCallbackMethodAsync");
				return;
			}
			_logger.LogInfoWithSource($"Processing command: {requestedCommand.CommandEnum}", "IoTCommandCallbackMethodAsync");
			await requestedCommand.Execute(iotCommandModel);
			_logger.LogInfoWithSource($"Finished processing command: {requestedCommand.CommandEnum}. Returning. . .", "IoTCommandCallbackMethodAsync");
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error occured in callback from topic " + args.Topic + ", message was: " + Encoding.UTF8.GetString(args.Message));
		}
	}
}
