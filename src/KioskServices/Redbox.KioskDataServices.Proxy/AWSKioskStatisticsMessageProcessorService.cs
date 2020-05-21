using System.Collections.Generic;
using Redbox.KioskDataServices.Proxy.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

internal class AWSKioskStatisticsMessageProcessorService : AWSBaseMessageProcessorService<AWSKioskStatisticsPayload, AWSKioskStatisticsPayload>
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.KioskStatistics;

	public override List<string> SupportedMessageTypes => new List<string> { "KioskStatistics" };

	protected override void CopyFields(AWSKioskStatisticsPayload destination, AWSKioskStatisticsPayload source)
	{
		destination.KioskId = source.KioskId;
		destination.CreatedOn = source.CreatedOn.ToUniversalTime();
		destination.Statistics = new Dictionary<string, string>();
		foreach (string key in source.Statistics.Keys)
		{
			if (key.IsNotInFilteredList())
			{
				destination.Statistics.Add(key, source.Statistics[key]);
			}
		}
		destination.Statistics = source.Statistics;
	}
}
