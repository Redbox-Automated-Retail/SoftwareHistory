using System;
using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Configuration;

public interface IConfigurationService
{
	ServiceResult Initialize();

	ServiceResult Start();

	ServiceResult Stop();

	ServiceResult CheckForUpdates(out Dictionary<string, string> dict);

	ServiceResult Add(string key, string filepath, string defaultCategory);

	ServiceResult Remove(string key);

	ServiceResult RemoveAll();

	List<ConfigurationClientItem> GetItems();

	T GetSettingValue<T>(string configFile, string categoryName, string settingName, T defaultValue);

	bool TryGetValue<T>(string key, string setting, out T value);

	bool TryGetValue<T>(string key, string category, string setting, out T value);

	bool TryGetTimeSpanRangeValue(string key, string category, string setting, out TimeSpan? startTime, out TimeSpan? endTime);

	bool TryGetObject(string key, string setting, out object value);

	bool TryGetObject(string key, string category, string setting, out object value);

	bool TryGetConfigurationData(string key, string setting, out ClientConfigurationData value);

	bool TryGetConfigurationData(string key, string category, string setting, out ClientConfigurationData value);

	bool GetAllConfigurationData(string key, out List<ClientConfigurationData> list);

	bool GetAllConfigurationData(string key, string setting, out List<ClientConfigurationData> list);

	bool GetAllConfigurationData(string key, string category, string setting, out List<ClientConfigurationData> list);

	bool GetCategoryNames(string key, out List<string> list);

	bool GetSettingNames(string key, string category, out List<string> list);

	bool GetSettingNames(string key, out List<string> list);
}
