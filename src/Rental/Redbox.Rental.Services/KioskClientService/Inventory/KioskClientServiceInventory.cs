using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Inventory;

namespace Redbox.Rental.Services.KioskClientService.Inventory;

public class KioskClientServiceInventory : BaseKioskClientService, IKioskClientServiceInventory
{
	private const string _apiPath = "api/inventory";

	public KioskClientServiceInventory()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new KcsUpdateMerchStatusMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskClientServiceInventory.  Unable to find QueueService.");
		}
	}

	public IGetMerchandizingOrdersResponse GetCurrentMerchandizingOrders()
	{
		WebResult<GetMerchandizingOrdersResponse> webResult = ExecuteWebRequest<GetMerchandizingOrdersResponse>(new Uri(string.Format("{0}/{1}/MerchandizingOrders", base.KioskClientServiceBaseUrl, "api/inventory")), null, "GET", ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceGetMerchandizingOrdersTimeout ?? 60000);
		LogKioskClientServiceResponse("KioskClientServiceInventory.GetCurrentMerchandizingOrders", webResult);
		return webResult?.Response;
	}

	public IUpdateMerchandizingStatusResponse SendMerchandizingStatusUpdates(List<MerchStatusUpdate> updates)
	{
		if (updates != null && updates.Count > 0)
		{
			WebResult<UpdateMerchandizingStatusResponse> webResult = ExecuteWebRequest<UpdateMerchandizingStatusResponse>(new Uri(string.Format("{0}/{1}/updatemerchandizingstatus", base.KioskClientServiceBaseUrl, "api/inventory")), updates, "POST", ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceGetMerchandizingOrdersTimeout ?? 60000);
			LogKioskClientServiceResponse("KioskClientServiceInventory.SendMerchandizingStatusUpdates", webResult);
			return webResult?.Response;
		}
		LogHelper.Instance.Log("Error in KioskClientServiceInventory.SendMerchandizingStatusUpdates. No updates specified.");
		return new UpdateMerchandizingStatusResponse
		{
			Success = false
		};
	}

	public IDictionary<string, object> ProcessQueuedMerchandStatusUpdates(byte[] payload)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { "success", false } };
		try
		{
			List<MerchStatusUpdate> list = Encoding.ASCII.GetString(payload).ToObject<List<MerchStatusUpdate>>();
			LogHelper.Instance.Log($"KioskClientServiceInventory.ProcessQueuedMerchandStatusUpdates sending {list?.Count ?? 0} status updates.");
			IUpdateMerchandizingStatusResponse updateMerchandizingStatusResponse = SendMerchandizingStatusUpdates(list);
			if (updateMerchandizingStatusResponse != null && updateMerchandizingStatusResponse.Success)
			{
				dictionary["success"] = true;
				dictionary["FailedUpdates"] = updateMerchandizingStatusResponse.FailedBarcodes;
				dictionary["message"] = updateMerchandizingStatusResponse.Error;
			}
			else
			{
				dictionary["FailedUpdates"] = list;
				dictionary["message"] = updateMerchandizingStatusResponse?.Error;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskClientServiceInventory.ProcessQueuedMerchandStatusUpdates.", e);
			dictionary["ExceptionRaised"] = true;
		}
		return dictionary;
	}
}
