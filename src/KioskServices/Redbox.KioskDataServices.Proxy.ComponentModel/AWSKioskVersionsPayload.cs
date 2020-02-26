using System;
using System.Collections.Generic;

namespace Redbox.KioskDataServices.Proxy.ComponentModel;

public class AWSKioskVersionsPayload
{
	public int KioskId { get; set; }

	public DateTime CreatedOn { get; set; }

	public IDictionary<string, string> Versions { get; set; }
}
