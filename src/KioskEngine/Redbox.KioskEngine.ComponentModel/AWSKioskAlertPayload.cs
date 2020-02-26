using System;

namespace Redbox.KioskEngine.ComponentModel;

public class AWSKioskAlertPayload
{
	public int KioskId { get; set; }

	public int AlertType { get; set; }

	public string SubAlertType { get; set; }

	public string AlertMessage { get; set; }

	public DateTime CreatedOn { get; set; }
}
