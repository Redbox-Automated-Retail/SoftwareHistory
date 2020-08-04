using System.Threading.Tasks;
using Newtonsoft.Json;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class ExecuteDownloadFileJob : ICommandIoTController
{
	private readonly IDownloadFilesService _downloadFilesService;

	private readonly IMqttProxy _mqttRepo;

	private readonly IIoTService _ioTService;

	private readonly IStoreService _store;

	public CommandEnum CommandEnum => CommandEnum.ExecuteDownloadFileJob;

	public ExecuteDownloadFileJob(IDownloadFilesService downloadFilesService, IMqttProxy mqttRepo, IIoTService ioTService, IStoreService store)
	{
		_downloadFilesService = downloadFilesService;
		_mqttRepo = mqttRepo;
		_ioTService = ioTService;
		_store = store;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		DownloadFileJob downloadFileJob = JsonConvert.DeserializeObject<DownloadFileJob>(ioTCommand.Payload.ToString());
		if (downloadFileJob.TargetKiosks.Contains(_store.KioskId))
		{
			await _downloadFilesService.HandleDownloadFileJob(downloadFileJob);
		}
	}
}
