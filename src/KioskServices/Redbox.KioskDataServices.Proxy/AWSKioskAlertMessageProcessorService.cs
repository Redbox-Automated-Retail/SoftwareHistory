using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

internal class AWSKioskAlertMessageProcessorService : AWSBaseMessageProcessorService<KioskAlertPayload, AWSKioskAlertPayload>
{
	public override ProxyDelegate ProxyMethod => KioskDataServicesProxy.Instance.KioskAlert;

	public override List<string> SupportedMessageTypes => new List<string> { "KioskAlert" };

	protected override void CopyFields(AWSKioskAlertPayload destination, KioskAlertPayload source)
	{
		destination.KioskId = source.KioskId;
		destination.AlertType = source.Type.ToAlertTypeNumeric();
		destination.SubAlertType = source.SubType;
		destination.AlertMessage = source.Detail;
		destination.CreatedOn = source.Time.ToUniversalTime();
	}
}
