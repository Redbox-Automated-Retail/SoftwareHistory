using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Campaigns;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Assets;

public interface IAssetsService
{
	Task<ApiBaseResponse> Refresh(IEnumerable<StartScreenDetails> startScreens);

	bool IsNotAValidAsset(string file);

	void BuildLocalPaths(IEnumerable<Asset> assets);
}
