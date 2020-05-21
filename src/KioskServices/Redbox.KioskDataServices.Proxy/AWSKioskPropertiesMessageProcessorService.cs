using System.Collections.Generic;
using Redbox.KioskDataServices.Proxy.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

internal class AWSKioskPropertiesMessageProcessorService : AWSBaseMessageProcessorService<AWSKioskPropertiesPayload, AWSKioskPropertiesPayload>
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.KioskProperties;

	public override List<string> SupportedMessageTypes => new List<string> { "KioskProperties" };

	protected override void CopyFields(AWSKioskPropertiesPayload destination, AWSKioskPropertiesPayload source)
	{
		destination.KioskId = source.KioskId;
		destination.CreatedOn = source.CreatedOn.ToUniversalTime();
		destination.Properties = source.Properties;
	}
}
