using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.Rental.Model.KioskEvents;

namespace Redbox.KioskDataServices.Proxy;

internal class AWSKioskEventsMessageProcessorService : AWSBaseMessageProcessorService<PromotionServicesProxy.KioskEventsMessage, AWSKioskEventsPayload>
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.KioskEvents;

	public override List<string> SupportedMessageTypes => new List<string> { "KioskEvents" };

	protected override void CopyFields(AWSKioskEventsPayload destination, PromotionServicesProxy.KioskEventsMessage source)
	{
		destination.KioskId = source.KioskId;
		string input = DecompressString(source.KioskEventsData);
		List<KioskEvent> list = JavaScriptConverterRegistry.Instance.GetSerializer().Deserialize<List<KioskEvent>>(input);
		List<AWSKioskEvent> list2 = new List<AWSKioskEvent>();
		foreach (KioskEvent item in list)
		{
			list2.Add(new AWSKioskEvent
			{
				Id = item.Id,
				KioskEventType = Enum.GetName(typeof(KioskEventType), item.KioskEventType),
				KioskEventSubType = KioskEventSubType.GetName(item.KioskEventSubType),
				Success = item.Success,
				EventTime = item.EventTime,
				Elapsed = item.Elapsed,
				RelatedId = item.RelatedId,
				Data = item.Data
			});
		}
		input = JsonConvert.SerializeObject(list2);
		destination.KioskEventsData = CompressString(input);
	}

	private static string CompressString(string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		memoryStream.Position = 0L;
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Read(array, 0, array.Length);
		byte[] array2 = new byte[array.Length + 4];
		Buffer.BlockCopy(array, 0, array2, 4, array.Length);
		Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, array2, 0, 4);
		return Convert.ToBase64String(array2);
	}

	private static string DecompressString(string compressedText)
	{
		byte[] array = Convert.FromBase64String(compressedText);
		using MemoryStream memoryStream = new MemoryStream();
		int num = BitConverter.ToInt32(array, 0);
		memoryStream.Write(array, 4, array.Length - 4);
		byte[] array2 = new byte[num];
		memoryStream.Position = 0L;
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
		{
			gZipStream.Read(array2, 0, array2.Length);
		}
		return Encoding.UTF8.GetString(array2);
	}
}
