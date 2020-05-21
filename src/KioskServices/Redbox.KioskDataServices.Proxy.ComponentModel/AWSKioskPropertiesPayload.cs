using System;
using System.Collections.Generic;

namespace Redbox.KioskDataServices.Proxy.ComponentModel;

public class AWSKioskPropertiesPayload
{
	public IDictionary<string, string> Properties;

	public long KioskId { get; set; }

	public DateTime CreatedOn { get; set; }
}
