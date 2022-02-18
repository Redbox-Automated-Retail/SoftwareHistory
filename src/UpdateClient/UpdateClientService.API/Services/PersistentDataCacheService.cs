using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services;

public class PersistentDataCacheService : IPersistentDataCacheService
{
	private readonly ILogger<PersistentDataCacheService> _logger;

	private readonly IStoreService _storeService;

	private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

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

	public async Task<PersistentDataWrapper<T>> Read<T>(string fileName, bool useCache = false, string baseDirectory = null, bool log = true) where T : IPersistentData
	{
		PersistentDataWrapper<T> result = new PersistentDataWrapper<T>();
		try
		{
			if (log)
			{
				_logger?.LogInfoWithSource($"Read({fileName},useCache={useCache})", "Read");
			}
			if (useCache && _cache.TryGetValue(fileName, out var value) && value != null)
			{
				if (log)
				{
					_logger?.LogInformation("Found cached value");
				}
				return value as PersistentDataWrapper<T>;
			}
			string path = ((baseDirectory == null) ? GetDataFilePath(fileName) : Path.Combine(baseDirectory, fileName));
			if (File.Exists(path))
			{
				using FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using StreamReader reader = new StreamReader(file);
				PersistentDataWrapper<T> persistentDataWrapper = JsonConvert.DeserializeObject<PersistentDataWrapper<T>>(await reader.ReadToEndAsync());
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

	public async Task<List<PersistentDataWrapper<T>>> ReadLike<T>(string pattern, string directory = null) where T : IPersistentData
	{
		List<PersistentDataWrapper<T>> result = new List<PersistentDataWrapper<T>>();
		try
		{
			directory = directory ?? DataCachePath;
			if (Directory.Exists(directory))
			{
				string[] files = Directory.GetFiles(directory, "*" + pattern + "*");
				string[] array = files;
				foreach (string path in array)
				{
					List<PersistentDataWrapper<T>> list = result;
					list.Add(await Read<T>(Path.GetFileName(path), useCache: true, Path.GetDirectoryName(path), log: false));
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred calling ReadLike(" + pattern + ")");
			return result;
		}
	}

	public async Task<List<PersistentDataWrapper<T>>> ReadLike<T>(Regex pattern, string directory = null) where T : IPersistentData
	{
		List<PersistentDataWrapper<T>> result = new List<PersistentDataWrapper<T>>();
		try
		{
			directory = directory ?? DataCachePath;
			if (Directory.Exists(directory))
			{
				string[] files = Directory.GetFiles(directory, "*.*");
				files = files.Where((string f) => pattern.IsMatch(Path.GetFileName(f))).ToArray();
				string[] array = files;
				foreach (string path in array)
				{
					List<PersistentDataWrapper<T>> list = result;
					list.Add(await Read<T>(Path.GetFileName(path), useCache: true, Path.GetDirectoryName(path), log: false));
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, $"unhandled exception occurred calling ReadLike({pattern})");
			return result;
		}
	}

	public async Task<bool> Write<T>(T data, string fileName, string baseDirectory = null) where T : IPersistentData
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
			string path = ((baseDirectory == null) ? GetDataFilePath(fileName) : Path.Combine(baseDirectory, fileName));
			if (baseDirectory != null)
			{
				Directory.CreateDirectory(baseDirectory);
			}
			await File.WriteAllTextAsync(path, contents);
			ClearCache(fileName);
			return true;
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "unhandled exception occurred");
			return false;
		}
	}

	public async Task<List<Error>> TryWrite<T>(T data, string fileName, string baseDirectory = null) where T : IPersistentData
	{
		List<Error> errors = new List<Error>();
		bool flag = false;
		string filePath = ((baseDirectory == null) ? GetDataFilePath(fileName) : Path.Combine(baseDirectory, fileName));
		if (File.Exists(filePath))
		{
			errors.Add(new Error
			{
				Message = "PersistentDataCacheService.TryWrite -> File at " + filePath + " already exists"
			});
		}
		else
		{
			flag = await Write(data, fileName, baseDirectory);
		}
		if (!flag)
		{
			errors.Add(new Error
			{
				Message = "PersistentDataCacheService.TryWrite -> Unhandled exception while writing to " + filePath + ". Check logs for more details."
			});
		}
		return errors;
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

	public async Task<bool> DeleteLike(string pattern, string directory = null)
	{
		try
		{
			_logger?.LogInfoWithSource("DeleteLike(" + pattern + ")", "DeleteLike");
			directory = directory ?? DataCachePath;
			if (Directory.Exists(directory))
			{
				string[] files = Directory.GetFiles(directory, "*" + pattern + "*");
				string[] array = files;
				foreach (string path in array)
				{
					File.Delete(path);
				}
			}
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
