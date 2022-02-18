using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads.AdServer;

public interface IAdServerClient
{
	Task<APIResponse<AdResponse>> GetAd(AdLocation location, AdProviderName providerName);

	Task<APIResponse<AdsResponse>> GetAds(List<AdLocation> locations, AdProviderName providerName);

	Task<APIResponse<AdsResponse>> CacheAds(List<AdLocation> locations, AdProviderName providerName);

	Task<APIResponse<dynamic>> PostProofOfPlay(ProofOfPlayRequest proofOfPlay);

	Task<APIResponse<dynamic>> PostExpire(string id);
}
