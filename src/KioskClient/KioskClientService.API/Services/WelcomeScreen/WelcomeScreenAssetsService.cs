using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.WelcomeScreen;

public class WelcomeScreenAssetsService : IWelcomeScreenAssetsService
{
	private readonly IProxyApi _proxyApi;

	private readonly IStoreService _storeService;

	private readonly IHttpService _httpService;

	private readonly ILogger<IWelcomeScreenAssetsService> _logger;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly string _welcomeScreenAssetPath;

	private readonly IOptionsSnapshotKioskConfiguration _kioskConfiguration;

	private const string CacheName = "WelcomeScreenAssets";

	private const string WelcomeScreenAssetFolder = "WelcomeScreenAssets";

	private static int _updatePeriod = 3600;

	private static int _updateSeconds = new Random().Next(0, _updatePeriod - 300);

	private static DateTime? _lastSync = null;

	public WelcomeScreenAssetsService(IProxyApi proxyApi, IStoreService storeService, IHttpService httpService, IPersistentDataCacheService cache, ILogger<IWelcomeScreenAssetsService> logger, IOptionsSnapshotKioskConfiguration kioskConfiguration)
	{
		_proxyApi = proxyApi;
		_storeService = storeService;
		_httpService = httpService;
		_persistentDataCacheService = cache;
		_logger = logger;
		_kioskConfiguration = kioskConfiguration;
		_welcomeScreenAssetPath = Path.Combine(_storeService.DataPath, "WelcomeScreenAssets");
	}

	public async Task<GetWelcomeScreenAssetsResponse> GetActiveAssets()
	{
		try
		{
			GetWelcomeScreenAssetsResponse result = new GetWelcomeScreenAssetsResponse
			{
				EnableAssetsSync = _kioskConfiguration.MarketingWelcomeScreen.EnableAssetsSync
			};
			if (!result.EnableAssetsSync)
			{
				return result;
			}
			List<IGrouping<string, WelcomeScreenAsset>> list = (from p in await GetValidWelcomeScreenAssets()
				where p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now.Date
				group p by p.Name.ToLower().Trim()).ToList();
			List<WelcomeScreenAsset> list2 = new List<WelcomeScreenAsset>();
			list.ForEach(delegate(IGrouping<string, WelcomeScreenAsset> group)
			{
				list2.Add(group.OrderByDescending((WelcomeScreenAsset p) => p.StartDate).First());
			});
			if (list2.Count == 0)
			{
				result.StatusCode = HttpStatusCode.NotFound;
				result.Error = "No valid welcome screen assets found";
				return result;
			}
			(from a in list2
				orderby a.Order, a.StartDate descending
				select a).ToList().ForEach(delegate(WelcomeScreenAsset a)
			{
				result.WelcomeScreenAssets.Add(new GetWelcomeScreenAsset
				{
					Name = a.Name,
					ProductGroupId = a.ProductGroupId,
					FilePath = a.FilePath
				});
			});
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured", "GetActiveAssets");
			return new GetWelcomeScreenAssetsResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = ex.Message
			};
		}
	}

	private async Task<List<WelcomeScreenAsset>> GetValidWelcomeScreenAssets()
	{
		List<WelcomeScreenAsset> result = new List<WelcomeScreenAsset>();
		WelcomeScreenAssetCache welcomeScreenAssetCache = await ReadCache();
		if (welcomeScreenAssetCache.WelcomeScreenAssets.Count == 0)
		{
			_logger.LogDebugWithSource("No welcome screen assets cache items found", "GetValidWelcomeScreenAssets");
			return result;
		}
		CheckWelcomeScreenAssetPath(_welcomeScreenAssetPath);
		List<string> list = Directory.GetFiles(_welcomeScreenAssetPath).ToList();
		foreach (string item in list)
		{
			WelcomeScreenAsset welcomeScreenAsset = WelcomeScreenAsset.LoadFromFile(item, _logger);
			if (welcomeScreenAsset != null && welcomeScreenAssetCache.WelcomeScreenAssets.Where((WelcomeScreenAssetInfo a) => a.Filename.Equals(welcomeScreenAsset.FileName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault() != null)
			{
				result.Add(welcomeScreenAsset);
			}
		}
		return (from a in result
			orderby a.Name, a.StartDate descending
			select a).ToList();
	}

	public async Task<SyncWelcomeScreenAssetsResponse> SyncAssets()
	{
		SyncWelcomeScreenAssetsResponse result = new SyncWelcomeScreenAssetsResponse();
		if (!_kioskConfiguration.MarketingWelcomeScreen.EnableAssetsSync)
		{
			return result;
		}
		try
		{
			WelcomeScreenAssetCache cache = await ReadCache();
			APIResponse<KioskClientService.API.Services.ProxyApi.GetWelcomeScreenAssetsResponse> aPIResponse = await _proxyApi.GetWelcomeScreenAssets(_storeService.KioskId, cache?.LatestChange);
			if (!aPIResponse.IsSuccessStatusCode)
			{
				result.StatusCode = HttpStatusCode.InternalServerError;
				result.Error = "Error retrieving welcome screen assets from proxy";
				_logger.LogErrorWithSource(result.Error, "SyncAssets");
				return result;
			}
			_lastSync = DateTime.Now;
			if (aPIResponse.StatusCode == HttpStatusCode.AlreadyReported)
			{
				_logger.LogInfoWithSource($"No new WelcomeScreenAsset data was returned for last change date {cache?.LatestChange}", "SyncAssets");
				return result;
			}
			List<WelcomeScreenAssetInfo> welcomeScreenAssets = aPIResponse.Response.WelcomeScreenAssets;
			DateTime latestChange = welcomeScreenAssets.Max((WelcomeScreenAssetInfo p) => p.LastUpdateUtc);
			cache = new WelcomeScreenAssetCache
			{
				LatestChange = latestChange,
				WelcomeScreenAssets = welcomeScreenAssets
			};
			await WriteCache(cache);
			return result;
		}
		catch (Exception ex)
		{
			result.Error = "Error syncing welcome screen assets";
			result.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, result.Error, "SyncAssets");
			return result;
		}
	}

	private async Task SyncFilesIfNeeded()
	{
		_ = 2;
		try
		{
			WelcomeScreenAssetCache syncInfo = await ReadCache();
			if (syncInfo.SyncComplete)
			{
				return;
			}
			CheckWelcomeScreenAssetPath(_welcomeScreenAssetPath);
			DirectoryInfo directoryInfo = new DirectoryInfo(_welcomeScreenAssetPath);
			List<FileInfo> localFiles = directoryInfo.GetFiles().ToList();
			List<WelcomeScreenAssetInfo> serverWelcomeScreenAssets = syncInfo.WelcomeScreenAssets;
			List<WelcomeScreenAssetInfo> downloadList = serverWelcomeScreenAssets.Where((WelcomeScreenAssetInfo p) => !localFiles.Any((FileInfo f) => f.Name.ToUpper() == p.Filename.ToUpper())).ToList();
			IEnumerable<FileInfo> deleteList = localFiles.Where((FileInfo f) => !serverWelcomeScreenAssets.Any((WelcomeScreenAssetInfo p) => p.Filename.ToUpper() == f.Name.ToUpper()));
			if (!(await DownloadWelcomeScreenAssets(downloadList, _welcomeScreenAssetPath)))
			{
				return;
			}
			foreach (FileInfo item in deleteList)
			{
				_logger.LogInfoWithSource("Deleting WelcomeScreenAsset " + item.Name, "SyncFilesIfNeeded");
				DeleteFile(_welcomeScreenAssetPath, item.Name);
			}
			syncInfo.SyncComplete = true;
			await WriteCache(syncInfo);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception syncing WelcomeScreenAssets", "SyncFilesIfNeeded");
		}
	}

	private async Task SyncAssetsIfNeeded()
	{
		PersistentDataWrapper<WelcomeScreenAssetCache> persistentDataWrapper = await ReadCacheWrapper();
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set - refreshing", "SyncAssetsIfNeeded");
			await SyncAssets();
		}
		else if (NeedsUpdate(DateTime.Now - persistentDataWrapper.Modified))
		{
			_logger?.LogInfoWithSource($"Cache is from {persistentDataWrapper.Modified} - refreshing", "SyncAssetsIfNeeded");
			await SyncAssets();
		}
	}

	public async Task<SyncWelcomeScreenAssetsResponse> RefreshIfNeeded()
	{
		SyncWelcomeScreenAssetsResponse result = new SyncWelcomeScreenAssetsResponse();
		if (!_kioskConfiguration.MarketingWelcomeScreen.EnableAssetsSync)
		{
			return result;
		}
		try
		{
			await SyncAssetsIfNeeded();
			await SyncFilesIfNeeded();
			return result;
		}
		catch (Exception ex)
		{
			result.Error = "Error running RefreshIfNeeded";
			result.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, result.Error, "RefreshIfNeeded");
			return result;
		}
	}

	private async Task<PersistentDataWrapper<WelcomeScreenAssetCache>> ReadCacheWrapper()
	{
		return await _persistentDataCacheService.Read<WelcomeScreenAssetCache>("WelcomeScreenAssets", useCache: true);
	}

	private async Task<WelcomeScreenAssetCache> ReadCache()
	{
		PersistentDataWrapper<WelcomeScreenAssetCache> persistentDataWrapper = await ReadCacheWrapper();
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set, return empty cache", "ReadCache");
			return new WelcomeScreenAssetCache();
		}
		return persistentDataWrapper.Data;
	}

	private async Task<bool> WriteCache(WelcomeScreenAssetCache cache)
	{
		bool flag = await _persistentDataCacheService.Write(cache, "WelcomeScreenAssets");
		if (!flag)
		{
			_logger.LogInfoWithSource("Welcome screen asset cache could not be written", "WriteCache");
		}
		return flag;
	}

	public bool DeleteCache()
	{
		return _persistentDataCacheService.Delete("WelcomeScreenAssets");
	}

	private async Task<bool> DownloadWelcomeScreenAssets(List<WelcomeScreenAssetInfo> downloadList, string path)
	{
		foreach (WelcomeScreenAssetInfo WelcomeScreenAsset in downloadList)
		{
			try
			{
				_logger.LogInfoWithSource("Downloading WelcomeScreenAsset " + WelcomeScreenAsset.Filename, "DownloadWelcomeScreenAssets");
				try
				{
					APIResponse<byte[]> aPIResponse = await _httpService.DownloadFile(WelcomeScreenAsset.PresignedUrl);
					if (!aPIResponse.IsSuccessStatusCode)
					{
						_logger.LogErrorWithSource($"Received status {aPIResponse.StatusCode} downloading WelcomeScreenAsset {WelcomeScreenAsset.Filename} from {WelcomeScreenAsset.PresignedUrl}", "DownloadWelcomeScreenAssets");
						return false;
					}
					await File.WriteAllBytesAsync(Path.Combine(path, WelcomeScreenAsset.Filename), aPIResponse.Response);
				}
				catch (Exception ex)
				{
					_logger.LogErrorWithSource(ex, "Error writing to WelcomeScreenAsset " + WelcomeScreenAsset.Filename, "DownloadWelcomeScreenAssets");
					return false;
				}
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Error opening WelcomeScreenAsset file " + WelcomeScreenAsset.Filename, "DownloadWelcomeScreenAssets");
				return false;
			}
		}
		return true;
	}

	private void DeleteFile(string path, string file)
	{
		try
		{
			File.Delete(Path.Combine(path, file));
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unable to delete file " + file);
		}
	}

	private void CheckWelcomeScreenAssetPath(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	private bool NeedsUpdate(TimeSpan age)
	{
		DateTime? lastSync = _lastSync;
		if (!lastSync.HasValue)
		{
			return true;
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = DateTime.Today.AddHours(17.0);
		if (now > dateTime)
		{
			return false;
		}
		DateTime dateTime2 = lastSync.Value.Date.AddHours(lastSync.Value.Hour).AddSeconds(_updatePeriod + _updateSeconds);
		if (DateTime.Now >= dateTime2 && age.TotalSeconds > (double)_updatePeriod)
		{
			return true;
		}
		return false;
	}
}
