using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Promotion;

public interface IPromotionService
{
	Task<APIResponse<PromotionCodeValidationResponse>> ValidatePromotionCodeAsync(PromotionCodeValidationPayload payload);
}
