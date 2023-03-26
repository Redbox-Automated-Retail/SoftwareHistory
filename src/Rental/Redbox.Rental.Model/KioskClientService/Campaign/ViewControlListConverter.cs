using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Redbox.Rental.Model.KioskClientService.Campaign;

public class ViewControlListConverter : JsonConverter<ViewControlList>
{
	public override ViewControlList ReadJson(JsonReader reader, Type objectType, ViewControlList existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		ViewControlList viewControlList = existingValue;
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			if (jArray != null)
			{
				viewControlList = new ViewControlList();
				foreach (JToken item in jArray)
				{
					switch (item.GetCaseInsensitveKeyValue("ControlType").ToObject<ViewControlType>())
					{
					case ViewControlType.StartAsset:
						viewControlList.Add(new StartAssetControl
						{
							ControlId = item.GetCaseInsensitveKeyValue("ControlId")?.ToObject<long>(),
							DisplayDuration = item.GetCaseInsensitveKeyValue("DisplayDuration")?.ToObject<int?>(),
							Asset = item.GetCaseInsensitveKeyValue("Asset")?.ToObject<Asset>(),
							Target = item.GetCaseInsensitveKeyValue("Target")?.ToObject<AssetTarget?>(),
							TargetValue = item.GetCaseInsensitveKeyValue("TargetValue")?.ToObject<string>(),
							Order = item.GetCaseInsensitveKeyValue("Order")?.ToObject<short>(),
							ShowPressToStart = item.GetCaseInsensitveKeyValue("ShowPressToStart")?.ToObject<bool?>(),
							IncludeIfNoInventory = item.GetCaseInsensitveKeyValue("IncludeIfNoInventory")?.ToObject<bool?>()
						});
						break;
					case ViewControlType.Carousel:
						viewControlList.Add(new CarouselControl
						{
							ControlId = item.GetCaseInsensitveKeyValue("ControlId")?.ToObject<long>(),
							DisplayDuration = item.GetCaseInsensitveKeyValue("DisplayDuration")?.ToObject<int?>(),
							MaxTitles = item.GetCaseInsensitveKeyValue("MaxTitles")?.ToObject<int?>()
						});
						break;
					}
				}
			}
		}
		return viewControlList;
	}

	public override void WriteJson(JsonWriter writer, ViewControlList value, JsonSerializer serializer)
	{
		writer.WriteStartArray();
		value.ForEach(delegate(IViewControl item)
		{
			serializer.Serialize(writer, item);
		});
		writer.WriteEndArray();
	}
}
