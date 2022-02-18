using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.Configuration;

public abstract class BaseConfiguration
{
	private object _lock = new object();

	protected readonly ILogger<BaseConfiguration> _logger;

	protected KioskConfigurationSettings _kioskConfigurationSettings;

	private Dictionary<(string categoryName, string settingName), object> _defaultValues;

	protected IOptionsMonitor<KioskConfigurationSettings> _optionsMonitorKioskConfigurationSettings;

	public BaseConfiguration(ILogger<BaseConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings)
	{
		_logger = logger;
		_kioskConfigurationSettings = optionsKioskConfigurationSettings.Value;
		PopulateConfigurationSettingValues();
	}

	public BaseConfiguration(ILogger<BaseConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings)
	{
		_logger = logger;
		_optionsMonitorKioskConfigurationSettings = optionsMonitorKioskConfigurationSettings;
		_optionsMonitorKioskConfigurationSettings.OnChange(OnChangeKioskConfigurationSettings);
		_kioskConfigurationSettings = optionsMonitorKioskConfigurationSettings.CurrentValue;
		PopulateConfigurationSettingValues();
	}

	private void OnChangeKioskConfigurationSettings(KioskConfigurationSettings kioskConfigurationSettings, string data)
	{
		_kioskConfigurationSettings = kioskConfigurationSettings;
		PopulateConfigurationSettingValues();
	}

	private void PopulateConfigurationSettingValues()
	{
		lock (_lock)
		{
			if (!Task.Run(delegate
			{
				try
				{
					if (_optionsMonitorKioskConfigurationSettings != null)
					{
						_kioskConfigurationSettings = _optionsMonitorKioskConfigurationSettings.CurrentValue;
					}
					bool flag = _defaultValues == null;
					if (flag)
					{
						_defaultValues = new Dictionary<(string, string), object>();
					}
					PropertyInfo[] properties = GetType().GetProperties();
					foreach (PropertyInfo propertyInfo in properties)
					{
						if (propertyInfo.PropertyType.IsSubclassOf(typeof(BaseCategorySetting)))
						{
							object categoryClassInstance = GetCategoryClassInstance(propertyInfo);
							PopulateCategorySettingValues(categoryClassInstance, flag);
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogErrorWithSource(ex, "Exception while populating configuration setting values.", "PopulateConfigurationSettingValues");
				}
			}).Wait(TimeSpan.FromSeconds(10.0)))
			{
				_logger.LogErrorWithSource("Population of configuration setting values timed out.", "PopulateConfigurationSettingValues");
			}
		}
	}

	private void PopulateCategorySettingValues(object categoryClassInstance, bool populateDefaultValues)
	{
		if (categoryClassInstance == null)
		{
			return;
		}
		string categoryName = GetCategoryName(categoryClassInstance);
		PropertyInfo[] properties = categoryClassInstance.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!propertyInfo.CanWrite)
			{
				continue;
			}
			string settingName = GetSettingName(propertyInfo);
			if (populateDefaultValues)
			{
				_defaultValues[(categoryName, settingName)] = propertyInfo.GetValue(categoryClassInstance, null);
			}
			bool flag = false;
			KioskSettingValue kioskSettingValue = _kioskConfigurationSettings.GetKioskSettingValue(categoryName, settingName);
			if (kioskSettingValue != null)
			{
				try
				{
					object value = Convert.ChangeType(kioskSettingValue.Value, propertyInfo.PropertyType);
					propertyInfo.SetValue(categoryClassInstance, value, null);
				}
				catch (Exception ex)
				{
					flag = true;
					_logger.LogErrorWithSource(ex, "Unable to set " + categoryName + " property " + settingName, "PopulateCategorySettingValues");
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			try
			{
				if (_defaultValues.TryGetValue((categoryName, settingName), out var value2))
				{
					propertyInfo.SetValue(categoryClassInstance, value2, null);
				}
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Unable to set " + categoryName + " property " + settingName + " with default value.", "PopulateCategorySettingValues");
			}
		}
	}

	private string GetSettingName(PropertyInfo settingPropertyInfo)
	{
		string name = settingPropertyInfo.Name;
		SettingAttribute settingAttribute = (SettingAttribute)Attribute.GetCustomAttribute(settingPropertyInfo, typeof(SettingAttribute));
		if (settingAttribute != null)
		{
			name = settingAttribute.Name;
		}
		return name;
	}

	private string GetCategoryName(object categoryClassInstance)
	{
		string result = categoryClassInstance.GetType().Name;
		CategoryAttribute categoryAttribute = (CategoryAttribute)Attribute.GetCustomAttribute(categoryClassInstance.GetType(), typeof(CategoryAttribute));
		if (!string.IsNullOrEmpty(categoryAttribute?.Name))
		{
			result = categoryAttribute?.Name;
		}
		return result;
	}

	private object GetCategoryClassInstance(PropertyInfo categoryClassPropertyInfo)
	{
		object obj = categoryClassPropertyInfo?.GetValue(this, null);
		if (obj == null)
		{
			try
			{
				obj = Activator.CreateInstance(categoryClassPropertyInfo.PropertyType);
				if (obj != null)
				{
					categoryClassPropertyInfo.SetValue(this, obj);
				}
				else
				{
					_logger.LogErrorWithSource("Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name, "GetCategoryClassInstance");
				}
				SetParentKioskConfiguration(obj);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unable to instantiate configuration category class " + categoryClassPropertyInfo?.PropertyType?.Name, "GetCategoryClassInstance");
			}
		}
		return obj;
	}

	private void SetParentKioskConfiguration(object categoryClassInstance)
	{
		if (categoryClassInstance == null)
		{
			return;
		}
		PropertyInfo property = categoryClassInstance.GetType().GetProperty("ParentKioskConfiguration");
		if (property != null)
		{
			object value = property.GetValue(categoryClassInstance);
			if (value == null)
			{
				property.SetValue(categoryClassInstance, this);
			}
		}
	}
}
