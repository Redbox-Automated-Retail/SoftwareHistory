using System.Collections.Generic;

namespace Redbox.FraudServices.Proxy;

internal class OldUploadDiscImageMessageProcessorService : BaseMessageProcessorService
{
	public override List<string> SupportedMessageTypes => new List<string> { "UploadDiscImage" };

	protected override ProxyDelegate ProxyMethod => FraudServicesProxy.Instance.OldUploadDiscImage;
}
