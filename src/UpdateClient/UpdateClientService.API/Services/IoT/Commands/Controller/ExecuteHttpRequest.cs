using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class ExecuteHttpRequest : ICommandIoTController
{
	private readonly IHttpService _httpService;

	private readonly ILogger<ExecuteHttpRequest> _logger;

	private readonly IMqttProxy _mqttProxy;

	private readonly IStoreService _store;

	public CommandEnum CommandEnum => CommandEnum.ExecuteHttpRequest;

	public ExecuteHttpRequest(IHttpService httpService, ILogger<ExecuteHttpRequest> logger, IMqttProxy mqttProxy, IStoreService store)
	{
		_httpService = httpService;
		_logger = logger;
		_mqttProxy = mqttProxy;
		_store = store;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		KioskHttpRequest kioskHttpRequest = JsonConvert.DeserializeObject<KioskHttpRequest>(ioTCommand.Payload.ToString());
		APIResponse<object> aPIResponse = await _httpService.SendRequest<object>(null, kioskHttpRequest.Url, kioskHttpRequest.JsonBody, new HttpMethod(kioskHttpRequest.HttpMethod));
		MqttResponse<string> mqttResponse = new MqttResponse<string>();
		mqttResponse.Data = aPIResponse.Response.ToJson();
		if (aPIResponse.Response == null)
		{
			mqttResponse.Data = aPIResponse.HttpResponse.ToJson();
		}
		if (aPIResponse.Response == null && !aPIResponse.IsSuccessStatusCode)
		{
			mqttResponse.Error = $"Error occured while calling: {kioskHttpRequest.Url} from kiosk #{_store.KioskId}. Errors: {string.Join(',', aPIResponse.Errors.Select((Error x) => x.Message))} HttpResponse: {aPIResponse.HttpResponse.ToJson()}";
			_logger.LogErrorWithSource($"Executed request has failed. Errors: {mqttResponse}. Received ioTCommand: {ioTCommand.ToJson()}.", "Execute");
		}
		_logger.LogInfoWithSource($"Executed http request, received response: {aPIResponse.Response}", "Execute");
		await _mqttProxy.PublishMessageAsync($"redbox/kiosk/{_store.KioskId}/request-id/{ioTCommand.RequestId}/command/execute-http-request/response", mqttResponse.ToJson());
	}
}
