using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;

namespace Redbox.KioskDataServices.Proxy;

public class KioskSessionAnalyticsService : IKioskSessionAnalyticsService
{
	private IQueueMessageProcessorService _queueMessageProcessorService;

	public static KioskSessionAnalyticsService Instance => Singleton<KioskSessionAnalyticsService>.Instance;

	private KioskSessionAnalyticsService()
	{
		RegisterWithQueueService();
	}

	private unsafe void RegisterWithQueueService()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			_queueMessageProcessorService = (IQueueMessageProcessorService)new QueueMessageProcessorService
			{
				Name = "KioskSessionAnalyticsServices",
				IsDefault = false,
				BeforeMessageProcessing = null,
				ProcessMessage = new ProcessMessageDelegate(null, (nint)(delegate*<IMessage, out IDictionary<string, object>, bool>)(&OnProcessMessage)),
				MessageProcessed = null
			};
			_queueMessageProcessorService.SupportedMessageTypes.Add("AddKioskSession");
			service.RegisterMessageProcessorService(_queueMessageProcessorService);
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskSessionAnalyticsServices.RegisterWithQueueService.  Unable to find QueueService.");
		}
	}

	private static bool OnProcessMessage(IMessage message, out IDictionary<string, object> response)
	{
		bool result = false;
		response = new Dictionary<string, object>();
		IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
		if (service == null || service.IsStandAlone)
		{
			LogHelper.Instance.Log("Unable to process Kiosk Session Analytics message because application is in stand alone mode");
			result = false;
		}
		try
		{
			if (message.Type == "AddKioskSession")
			{
				string jsonRequest = Encoding.ASCII.GetString(message.Data);
				response = KioskDataServicesProxy.Instance.ExecuteMessage(message.Type, "api/KioskData/AddKioskSession", jsonRequest);
			}
			else
			{
				LogHelper.Instance.Log($"Error in KioskSessionAnalyticsService.OnProcessMessage.  No handler for message type {message.Type}");
			}
			return response != null && response.ContainsKey("success") && (bool)response["success"];
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in KioskSessionAnalyticsService.OnProcessMessage.", e);
			return result;
		}
	}
}
