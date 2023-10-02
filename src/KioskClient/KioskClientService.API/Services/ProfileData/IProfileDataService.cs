using System.Threading.Tasks;

namespace KioskClientService.API.Services.ProfileData;

public interface IProfileDataService
{
	Task<TriggerProductDataUpdateResponse> TriggerProductDataUpdate();
}
