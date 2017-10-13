using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public class KioskEventData
{
	public Guid Id { get; set; }

	public int KioskEventType { get; set; }

	public int KioskEventSubType { get; set; }

	public bool Success { get; set; }

	public DateTime EventTime { get; set; }

	public int? Elapsed { get; set; }

	public Guid? RelatedId { get; set; }

	public string Data { get; set; }
}
