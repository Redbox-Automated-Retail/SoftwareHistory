using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.KernelExtension;

internal class KSAuditEventMessageProcessorService : BaseQueueMessageProcessorService
{
	public override string Name => GetType().Name;

	public override bool IsDefault => false;

	public override List<string> SupportedMessageTypes => new List<string> { "AuditEvent" };

	public override ProcessMessageDelegate ProcessMessage => OnProcessMessage;

	private static bool OnProcessMessage(IMessage message, out IDictionary<string, object> response)
	{
		response = new Dictionary<string, object>();
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendAuditEventsToKioskServices)
		{
			if (!HealthServicesProxy.Instance.IsOnline || HealthServicesProxy.Instance.IsServerBusy || StoreServicesProxy.Instance.IsStandAlone)
			{
				LogHelper.Instance.Log("KSAuditEventMeessageProcessorService.OnProcessMessage() - not sending message due to kiosk offline");
				return false;
			}
			try
			{
				string jsonRequest = Encoding.ASCII.GetString(message.Data);
				response = KioskServicesProxy.Instance.ExecuteMessage(jsonRequest);
				return response.ContainsKey("Success") && (bool)response["Success"];
			}
			catch (Exception e)
			{
				LogHelper.Instance.LogException("KSAuditEventMeessageProcessorService.OnProcessMessage() - exception trying to send audit event to kiosk services!", e);
				return false;
			}
		}
		LogHelper.Instance.Log("KSAuditEventMeessageProcessorService.OnProcessMessage() - bypassing live call of audit event to kiosk services due to configuration flag being off");
		return true;
	}
}
