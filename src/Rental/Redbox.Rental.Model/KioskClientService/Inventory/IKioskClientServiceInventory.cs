using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Inventory;

public interface IKioskClientServiceInventory
{
	IGetMerchandizingOrdersResponse GetCurrentMerchandizingOrders();

	IUpdateMerchandizingStatusResponse SendMerchandizingStatusUpdates(List<MerchStatusUpdate> updates);

	IDictionary<string, object> ProcessQueuedMerchandStatusUpdates(byte[] payload);
}
