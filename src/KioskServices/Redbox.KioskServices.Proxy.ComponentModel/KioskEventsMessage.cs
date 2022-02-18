using System;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public class KioskEventsMessage
{
	public Guid MessageId { get; set; }

	public string MessageType { get; set; }

	public long KioskId { get; set; }

	public string EngineVersion { get; set; }

	public string BundleVersion { get; set; }

	public string KioskEventsData { get; set; }
}
