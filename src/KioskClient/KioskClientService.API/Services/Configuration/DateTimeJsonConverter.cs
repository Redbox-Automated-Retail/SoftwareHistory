using System;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Configuration;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
	public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
	{
		writer.WriteValue(value.ToString("s"));
	}

	public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartObject)
		{
			UpdateServiceDate updateServiceDate = new UpdateServiceDate();
			serializer.Populate(reader, updateServiceDate);
			return new DateTime(updateServiceDate.theDate);
		}
		if (reader.TokenType == JsonToken.Date)
		{
			return (DateTime)reader.Value;
		}
		return existingValue;
	}
}
