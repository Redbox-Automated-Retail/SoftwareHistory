using System.Threading.Tasks;

namespace KioskClientService.API.Services.WelcomeScreen;

public interface IWelcomeScreenAssetsService
{
	Task<GetWelcomeScreenAssetsResponse> GetActiveAssets();

	Task<SyncWelcomeScreenAssetsResponse> SyncAssets();

	Task<SyncWelcomeScreenAssetsResponse> RefreshIfNeeded();

	bool DeleteCache();
}
