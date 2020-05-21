using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskClientService.API.Services.Inventory;

public interface IInventoryService
{
	Task<GetMerchandizingOrdersResponse> GetAllCurrentMerchandizingOrders();

	Task<UpdateMerchandizingStatusResponse> UpdateMerchandizingStatus(List<MerchandStatusUpdate> updates);
}
