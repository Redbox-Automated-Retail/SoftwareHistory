using System;

namespace Redbox.KioskDataServices.Proxy.ComponentModel;

public class AWSKioskEvent
{
	public Guid Id { get; set; }

	public string KioskEventType { get; set; }

	public string KioskEventSubType { get; set; }

	public bool Success { get; set; }

	public DateTime EventTime { get; set; }

	public int? Elapsed { get; set; }

	public Guid? RelatedId { get; set; }

	public string Data { get; set; }
}
