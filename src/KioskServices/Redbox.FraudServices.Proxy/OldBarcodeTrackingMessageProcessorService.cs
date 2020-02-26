using System.Collections.Generic;

namespace Redbox.FraudServices.Proxy;

internal class OldBarcodeTrackingMessageProcessorService : BaseMessageProcessorService
{
	public override List<string> SupportedMessageTypes => new List<string> { "BarcodeTracking" };

	protected override ProxyDelegate ProxyMethod => FraudServicesProxy.Instance.OldBarcodeTracking;
}
