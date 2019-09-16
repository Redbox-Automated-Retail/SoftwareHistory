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

	private void RegisterWithQueueService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			_queueMessageProcessorService = new QueueMessageProcessorService
			{
				Name = "KioskSessionAnalyticsServices",
				IsDefault = false,
				BeforeMessageProcessing = null,
				ProcessMessage = OnProcessMessage,
				MessageProcessed = null
			};
			_queueMessageProcessorService.SupportedMessageTypes.Add("AddKioskSession");
			_queueMessageProcessorService.SupportedMessageTypes.Add("AddKioskSessionJson");
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
			else if (message.Type == "AddKioskSessionJson")
			{
				string jsonRequest2 = Encoding.ASCII.GetString(message.Data);
				response = KioskDataServicesProxy.Instance.ExecuteMessage(message.Type, "api/KioskData/AddKioskSessionJson", jsonRequest2);
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
