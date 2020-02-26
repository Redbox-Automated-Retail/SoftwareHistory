using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.KernelExtension;

internal class KSKioskFunctionCheckMessageProcessorService : BaseQueueMessageProcessorService
{
	public override string Name => GetType().Name;

	public override bool IsDefault => false;

	public override List<string> SupportedMessageTypes => new List<string> { "KioskFunctionCheck" };

	public override ProcessMessageDelegate ProcessMessage => OnProcessMessage;

	private static bool OnProcessMessage(IMessage message, out IDictionary<string, object> response)
	{
		response = new Dictionary<string, object>();
		if (ServiceLocator.Instance.GetService<IConfiguration>().SendKioskFunctionChecksToKioskServices)
		{
			if (!HealthServicesProxy.Instance.IsOnline || HealthServicesProxy.Instance.IsServerBusy || StoreServicesProxy.Instance.IsStandAlone)
			{
				return false;
			}
			try
			{
				string jsonRequest = Encoding.ASCII.GetString(message.Data);
				response = KioskServicesProxy.Instance.ExecuteMessage(jsonRequest);
				return response.ContainsKey("Success") && (bool)response["Success"];
			}
			catch
			{
				return false;
			}
		}
		LogHelper.Instance.Log("KSKioskFunctionCheckMessageProcessorService.OnProcessMessage() - bypassing live call to KioskServices due to configuration flag being off");
		return true;
	}
}
