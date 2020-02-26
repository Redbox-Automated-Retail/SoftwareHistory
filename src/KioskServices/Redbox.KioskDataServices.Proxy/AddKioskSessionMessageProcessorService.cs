using System.Collections.Generic;

namespace Redbox.KioskDataServices.Proxy;

internal class AddKioskSessionMessageProcessorService : BaseMessageProcessorService
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.AddKioskSession;

	public override List<string> SupportedMessageTypes => new List<string> { "AddKioskSession" };
}
