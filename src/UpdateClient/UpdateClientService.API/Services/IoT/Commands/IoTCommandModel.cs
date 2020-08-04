using Newtonsoft.Json;

namespace UpdateClientService.API.Services.IoT.Commands;

public class IoTCommandModel
{
	[JsonProperty("command")]
	public CommandEnum Command { get; set; }

	[JsonProperty("requestId")]
	public string RequestId { get; set; }

	[JsonProperty("payload")]
	public object Payload { get; set; }
}
