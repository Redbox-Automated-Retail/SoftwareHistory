using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UpdateClientService.API.Services.IoT.UpdateServiceRequests;

public class UpdateServiceRequest
{
	[JsonProperty("requestType")]
	[JsonConverter(typeof(StringEnumConverter))]
	public UpdateServiceRequestEnum RequestType { get; set; }

	[JsonProperty("requestId")]
	public string RequestId { get; set; } = Guid.NewGuid().ToString();

	[JsonProperty("kioskId")]
	public string KioskId { get; set; }

	[JsonProperty("payload")]
	public object Payload { get; set; }
}
