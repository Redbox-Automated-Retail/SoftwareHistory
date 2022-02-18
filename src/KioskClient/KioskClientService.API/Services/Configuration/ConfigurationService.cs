using System;
using System.Collections.Generic;
using System.Globalization;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Configuration;

public class ConfigurationService : IConfigurationService
{
	private ILogger<ConfigurationService> _logger;

	private IApplicationSettings _applicationSettings;

	public ConfigurationService(ILogger<ConfigurationService> logger, IApplicationSettings applicationSettings)
	{
		_logger = logger;
		_applicationSettings = applicationSettings;
	}

	public ServiceResult Initialize()
	{
		try
		{
			ConfigurationClient.Instance.Initialize(_logger, _applicationSettings.AppSettings.UpdateManagerAddress, _applicationSettings.AppSettings.UpdateManagerPort);
			return new ServiceResult(success: true);
		}
		catch (Exception exception)
		{
			return new ServiceResult(exception, "ConfigurationService.Initialize");
		}
	}

	public ServiceResult Add(string key, string filepath, string defaultCategory)
	{
		try
		{
			ConfigurationClient.Instance.Add(key, filepath, defaultCategory);
			return new ServiceResult(success: true);
		}
		catch (Exception ex)
		{
			if (ex.Message.StartsWith("Index (zero based)"))
			{
				return new ServiceResult(success: true);
			}
			return new ServiceResult(ex, "ConfigurationService.Add2");
		}
	}

	public ServiceResult Remove(string key)
	{
		try
		{
			ConfigurationClient.Instance.Remove(key);
			return new ServiceResult(success: true);
		}
		catch (Exception exception)
		{
			return new ServiceResult(exception, "ConfigurationService.Remove");
		}
	}

	public ServiceResult RemoveAll()
	{
		try
		{
			ConfigurationClient.Instance.RemoveAll();
			return new ServiceResult(success: true);
		}
		catch (Exception exception)
		{
			return new ServiceResult(exception, "ConfigurationService.RemoveAll");
		}
	}

	public List<ConfigurationClientItem> GetItems()
	{
		List<ConfigurationClientItem> items = ConfigurationClient.Instance.GetItems();
		List<ConfigurationClientItem> list = new List<ConfigurationClientItem>();
		foreach (ConfigurationClientItem item in items)
		{
			list.Add(new ConfigurationClientItem
			{
				Key = item.Key,
				DefaultCategory = item.DefaultCategory,
				CheckTime = item.CheckTime,
				FilePath = item.FilePath,
				HasCanActivate = item.HasCanActivate,
				HasActivateAction = item.HasActivateAction
			});
		}
		return list;
	}

	public ServiceResult Start()
	{
		try
		{
			return new ServiceResult(success: true);
		}
		catch (Exception exception)
		{
			return new ServiceResult(exception, "ConfigurationService.Start");
		}
	}

	public ServiceResult Stop()
	{
		try
		{
			return new ServiceResult(success: true);
		}
		catch (Exception exception)
		{
			return new ServiceResult(exception, "ConfigurationService.Stop");
		}
	}

	public ServiceResult CheckForUpdates(out Dictionary<string, string> dict)
	{
		dict = null;
		try
		{
			dict = ConfigurationClient.Instance.CheckAllForUpdates();
			return new ServiceResult(success: true);
		}
		catch (Exception exception)
		{
			return new ServiceResult(exception, "ConfigurationService.CheckForUpdates");
		}
	}

	public bool GetCategoryNames(string key, out List<string> list)
	{
		return ConfigurationClient.Instance.GetCategoryNames(key, out list);
	}

	public bool GetSettingNames(string key, out List<string> list)
	{
		return ConfigurationClient.Instance.GetSettingNames(key, out list);
	}

	public bool GetSettingNames(string key, string category, out List<string> list)
	{
		return ConfigurationClient.Instance.GetSettingNames(key, category, out list);
	}

	public bool TryGetValue<T>(string key, string category, string setting, out T value)
	{
		return ConfigurationClient.Instance.TryGetValue<T>(key, category, setting, out value);
	}

	public T GetSettingValue<T>(string configFile, string categoryName, string settingName, T defaultValue)
	{
		T value = defaultValue;
		if (!TryGetValue<T>(configFile, categoryName, settingName, out value))
		{
			value = defaultValue;
		}
		return value;
	}

	public bool TryGetTimeSpanRangeValue(string key, string category, string setting, out TimeSpan? startTime, out TimeSpan? endTime)
	{
		startTime = null;
		endTime = null;
		string value = null;
		bool flag = false;
		if (ConfigurationClient.Instance.TryGetValue<string>(key, category, setting, out value))
		{
			string[] array = value.Split('-');
			if (array.Length == 2 && TimeSpan.TryParseExact(array[0], "h\\:mm", DateTimeFormatInfo.InvariantInfo, TimeSpanStyles.None, out var result) && TimeSpan.TryParseExact(array[1], "h\\:mm", DateTimeFormatInfo.InvariantInfo, TimeSpanStyles.None, out var result2))
			{
				if (result <= result2)
				{
					startTime = result;
					endTime = result2;
					flag = true;
				}
				else
				{
					_logger.LogError("Error in ConfigurationService.TryGetTimeSpanRangeValue: end time is less than start time : " + value + " for key: " + key + ", category: " + category + ", setting: " + setting);
				}
			}
			if (!flag)
			{
				_logger.LogError("Error in ConfigurationService.TryGetTimeSpanRangeValue: invalid time span range value : " + value + " for key: " + key + ", category: " + category + ", setting: " + setting);
			}
		}
		return flag;
	}

	public bool TryGetValue<T>(string key, string setting, out T value)
	{
		return ConfigurationClient.Instance.TryGetValue<T>(key, setting, out value);
	}

	public bool TryGetObject(string key, string category, string setting, out object value)
	{
		return ConfigurationClient.Instance.TryGetObject(key, category, setting, out value);
	}

	public bool TryGetObject(string key, string setting, out object value)
	{
		return ConfigurationClient.Instance.TryGetObject(key, setting, out value);
	}

	public bool TryGetConfigurationData(string key, string category, string setting, out ClientConfigurationData value)
	{
		value = null;
		bool flag = ConfigurationClient.Instance.TryGetConfigurationData(key, category, setting, out var value2);
		if (flag)
		{
			value = value2;
		}
		return flag;
	}

	public bool TryGetConfigurationData(string key, string setting, out ClientConfigurationData value)
	{
		value = null;
		bool flag = ConfigurationClient.Instance.TryGetConfigurationData(key, setting, out var value2);
		if (flag)
		{
			value = value2;
		}
		return flag;
	}

	public bool GetAllConfigurationData(string key, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		bool allConfigurationData = ConfigurationClient.Instance.GetAllConfigurationData(key, out var list2);
		if (allConfigurationData)
		{
			foreach (ClientConfigurationData item in list2)
			{
				list.Add(item);
			}
		}
		return allConfigurationData;
	}

	public bool GetAllConfigurationData(string key, string category, string setting, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		bool allConfigurationData = ConfigurationClient.Instance.GetAllConfigurationData(key, category, setting, out var list2);
		if (allConfigurationData)
		{
			foreach (ClientConfigurationData item in list2)
			{
				list.Add(item);
			}
		}
		return allConfigurationData;
	}

	public bool GetAllConfigurationData(string key, string setting, out List<ClientConfigurationData> list)
	{
		list = new List<ClientConfigurationData>();
		bool allConfigurationData = ConfigurationClient.Instance.GetAllConfigurationData(key, setting, out var list2);
		if (allConfigurationData)
		{
			foreach (ClientConfigurationData item in list2)
			{
				list.Add(item);
			}
		}
		return allConfigurationData;
	}
}
