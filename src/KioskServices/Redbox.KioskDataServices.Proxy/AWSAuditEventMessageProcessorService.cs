using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

internal class AWSAuditEventMessageProcessorService : AWSBaseMessageProcessorService<AuditEventPayload, AWSAuditEventPayload>
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.AuditEvent;

	public override List<string> SupportedMessageTypes => new List<string> { "AuditEvent" };

	protected override void CopyFields(AWSAuditEventPayload destination, AuditEventPayload source)
	{
		destination.KioskId = source.KioskId;
		destination.LogDate = source.LogDate.ToUniversalTime();
		destination.UserName = source.UserName;
		destination.Message = source.Message;
		destination.Source = source.Source;
		destination.EventType = source.EventType;
	}
}
