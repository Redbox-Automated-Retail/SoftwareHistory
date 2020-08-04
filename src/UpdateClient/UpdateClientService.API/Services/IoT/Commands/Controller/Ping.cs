using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class Ping : ICommandIoTController
{
	private readonly IMqttProxy _mqttProxy;

	private readonly IStoreService _store;

	public CommandEnum CommandEnum => CommandEnum.Ping;

	public Ping(IMqttProxy mqttProxy, IStoreService store)
	{
		_mqttProxy = mqttProxy;
		_store = store;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		await _mqttProxy.PublishMessageAsync($"redbox/kiosk/{_store.KioskId}/request-id/{ioTCommand.RequestId}/command/ping/response", string.Empty);
	}
}
