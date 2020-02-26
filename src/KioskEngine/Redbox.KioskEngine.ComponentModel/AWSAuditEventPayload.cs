using System;

namespace Redbox.KioskEngine.ComponentModel;

public class AWSAuditEventPayload
{
	public int KioskId { get; set; }

	public DateTime LogDate { get; set; }

	public string UserName { get; set; }

	public string Message { get; set; }

	public string Source { get; set; }

	public string EventType { get; set; }
}
