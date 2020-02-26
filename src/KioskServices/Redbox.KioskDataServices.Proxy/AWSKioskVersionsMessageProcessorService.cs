using System.Collections.Generic;
using Redbox.KioskDataServices.Proxy.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

internal class AWSKioskVersionsMessageProcessorService : AWSBaseMessageProcessorService<AWSKioskVersionsPayload, AWSKioskVersionsPayload>
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.KioskVersions;

	public override List<string> SupportedMessageTypes => new List<string> { "KioskVersions" };

	protected override void CopyFields(AWSKioskVersionsPayload destination, AWSKioskVersionsPayload source)
	{
		destination.KioskId = source.KioskId;
		destination.CreatedOn = source.CreatedOn.ToUniversalTime();
		destination.Versions = source.Versions;
	}
}
