using System;
using System.Collections.Generic;

namespace Redbox.KioskDataServices.Proxy.ComponentModel;

public class AWSKioskStatisticsPayload
{
	public IDictionary<string, string> Statistics;

	public long KioskId { get; set; }

	public DateTime CreatedOn { get; set; }
}
