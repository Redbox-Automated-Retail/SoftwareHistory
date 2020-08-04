using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class GetFile : ICommandIoTController
{
	private readonly IKioskFilesService _kioskFilesService;

	private readonly IMqttProxy _mqttRepo;

	private readonly IStoreService _store;

	public CommandEnum CommandEnum => CommandEnum.GetFile;

	public GetFile(IKioskFilesService kioskFilesService, IMqttProxy mqttRepo, IStoreService store)
	{
		_kioskFilesService = kioskFilesService;
		_mqttRepo = mqttRepo;
		_store = store;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		KioskFileRequest kioskFileRequest = JsonConvert.DeserializeObject<KioskFileRequest>(ioTCommand.Payload.ToString());
		MqttResponse<string> obj = await _kioskFilesService.GetKioskFileAsync(kioskFileRequest);
		await _mqttRepo.PublishMessageAsync($"redbox/kiosk/{_store.KioskId}/request-id/{ioTCommand.RequestId}/command/get-kiosk-file/response", obj.ToJson());
	}
}
