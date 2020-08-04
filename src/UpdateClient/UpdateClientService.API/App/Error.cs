using Newtonsoft.Json;

namespace UpdateClientService.API.App;

public class Error
{
	[JsonProperty(Required = Required.DisallowNull)]
	public string Code { get; set; }

	[JsonProperty(Required = Required.DisallowNull)]
	public string Message { get; set; }
}
