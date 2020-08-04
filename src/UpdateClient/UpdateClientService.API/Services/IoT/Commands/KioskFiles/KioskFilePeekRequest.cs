using System;
using Newtonsoft.Json;

namespace UpdateClientService.API.Services.IoT.Commands.KioskFiles;

public class KioskFilePeekRequest
{
	[JsonProperty("date")]
	public DateTime? Date { get; set; }

	[JsonProperty("fileType")]
	public FileTypeEnum FileType { get; set; }
}
