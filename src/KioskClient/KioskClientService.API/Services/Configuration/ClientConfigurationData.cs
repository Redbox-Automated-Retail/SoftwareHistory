using System;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Configuration;

public class ClientConfigurationData
{
	public long CategoryID { get; set; }

	public string CategoryName { get; set; }

	public string SettingName { get; set; }

	public string SettingDataType { get; set; }

	public string DefaultTypeName { get; set; }

	public int DefaultTypeRank { get; set; }

	public int DefaultTypeDataType { get; set; }

	public long DefaultTypeKey { get; set; }

	public string DefaultTypeValue { get; set; }

	public string Value { get; set; }

	[JsonConverter(typeof(DateTimeJsonConverter))]
	public DateTime EffectiveTime { get; set; }

	[JsonConverter(typeof(DateTimeNullableJsonConverter))]
	public DateTime? ExpireTime { get; set; }
}
