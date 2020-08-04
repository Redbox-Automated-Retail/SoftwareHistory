using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class PeekFIles : ICommandIoTController
{
	private readonly IKioskFilesService _kioskFilesService;

	private readonly IMqttProxy _mqttRepo;

	private readonly IStoreService _store;

	public CommandEnum CommandEnum => CommandEnum.PeekFiles;

	public PeekFIles(IKioskFilesService kioskFilesService, IMqttProxy mqttRepo, IStoreService store)
	{
		_kioskFilesService = kioskFilesService;
		_mqttRepo = mqttRepo;
		_store = store;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		KioskFilePeekRequest logRequestModel = JsonConvert.DeserializeObject<KioskFilePeekRequest>(ioTCommand.Payload.ToString());
		MqttResponse<Dictionary<string, string>> obj = await _kioskFilesService.PeekRequestedFilessAsync(logRequestModel);
		await _mqttRepo.PublishMessageAsync($"redbox/kiosk/{_store.KioskId}/request-id/{ioTCommand.RequestId}/command/peek-files/response", obj.ToJson());
	}
}
