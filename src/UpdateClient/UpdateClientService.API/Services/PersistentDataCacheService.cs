using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace UpdateClientService.API.Services;

public class PersistentDataCacheService : IPersistentDataCacheService
{
	private ILogger<PersistentDataCacheService> _logger;

	private IStoreService _storeService;

	private ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

	private string _dataCachePath;

	private string DataCachePath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_dataCachePath))
			{
				string path = Path.Combine(_storeService.DataPath, "DataCache");
				_dataCachePath = Path.GetFullPath(path);
				Directory.CreateDirectory(_dataCachePath);
			}
			return _dataCachePath;
		}
	}

	public PersistentDataCacheService(ILogger<PersistentDataCacheService> logger, IStoreService storeService)
	{
		_logger = logger;
		_storeService = storeService;
	}

	public async Task<PersistentDataWrapper<T>> Read<T>(string fileName, bool useCache = false, string filePath = null) where T : IPersistentData
	{
		PersistentDataWrapper<T> result = new PersistentDataWrapper<T>();
		try
		{
			_logger?.LogInfoWithSource($"Read({fileName},useCache={useCache})", "Read");
			if (useCache && _cache.TryGetValue(fileName, out var value) && value != null)
			{
				_logger?.LogInformation("Found cached value");
				return value as PersistentDataWrapper<T>;
			}
			string path = filePath ?? GetDataFilePath(fileName);
			if (File.Exists(path))
			{
				PersistentDataWrapper<T> persistentDataWrapper = JsonConvert.DeserializeObject<PersistentDataWrapper<T>>(await File.ReadAllTextAsync(path));
				result = persistentDataWrapper;
			}
			if (useCache)
			{
				_cache[fileName] = result;
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred");
			return result;
		}
	}

	public async Task<bool> Write<T>(T data, string fileName, string filePath = null) where T : IPersistentData
	{
		try
		{
			_logger?.LogInfoWithSource("Write(" + fileName + ")", "Write");
			PersistentDataWrapper<T> persistentDataWrapper = new PersistentDataWrapper<T>();
			persistentDataWrapper.Data = data;
			persistentDataWrapper.Modified = DateTime.Now;
			persistentDataWrapper.DataType = typeof(T).FullName;
			PersistentDataWrapper<T> value = persistentDataWrapper;
			string contents = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			});
			await File.WriteAllTextAsync(filePath ?? GetDataFilePath(fileName), contents);
			ClearCache(fileName);
			return true;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred");
			return false;
		}
	}

	public async Task<bool> Delete(string fileName, string filePath = null)
	{
		try
		{
			_logger?.LogInfoWithSource("Delete(" + fileName + ")", "Delete");
			string path = filePath ?? GetDataFilePath(fileName);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			ClearCache(fileName);
			return true;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred");
			return false;
		}
	}

	private void ClearCache(string fileName)
	{
		if (_cache.ContainsKey(fileName))
		{
			_logger?.LogInformation("Clearing cached value " + fileName);
			_cache.TryRemove(fileName, out var _);
		}
	}

	private string GetDataFilePath(string fileName)
	{
		return Path.Combine(DataCachePath, Path.ChangeExtension(fileName, "json"));
	}
}
