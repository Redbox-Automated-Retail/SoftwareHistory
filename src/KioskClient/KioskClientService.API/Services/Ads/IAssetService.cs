using System.Threading.Tasks;

namespace KioskClientService.API.Services.Ads;

public interface IAssetService
{
	Task<CacheResponse> Cache();

	Asset GetAsset(string assetId);

	string GetAssetCreativeFilePath(Asset asset);

	Task<bool> DownloadAsset(Asset asset);
}
