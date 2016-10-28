using System;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class StoreManagerService : IStoreManager
{
	private Country _country;

	private IConfigurationService _configService;

	private long _kioskId;

	public Country Country => _country;

	public long KioskId
	{
		get
		{
			try
			{
				if (_kioskId == 0L)
				{
					string text = "ID";
					string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object value = Registry.GetValue(text2, text, "");
					if (value == null)
					{
						LogHelper.Instance.Log("Kiosk ID is not set in the registry at {0}\\{1}", text2, text);
						return 0L;
					}
					_kioskId = Convert.ToInt32(value);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unhandled exception occurred in TicketsContext.GetKioskId", e);
			}
			return _kioskId;
		}
	}

	public StoreManagerService(Country country, IConfigurationService configService)
	{
		_country = country;
		_configService = configService;
	}

	public string GetDefaultCulture()
	{
		string text = GetDefaultCultureByCountry();
		if (_configService.TryGetValue<string>(ConfigFile.System, ConfigCategory.Culture, ConfigSetting.DefaultCulture, out var value))
		{
			LogHelper.Instance.Log("Found default culture in configuration file: {0}", value);
			if (IsCultureValid(value))
			{
				text = value;
			}
			else
			{
				LogHelper.Instance.Log("Culture found in configuration file is not valid: {0}", value);
			}
		}
		else
		{
			LogHelper.Instance.Log("Default culture was not found in configuration file, using: {0}", text);
		}
		return text;
	}

	public string GetAlternateCulture()
	{
		string text = GetDefaultAlternateCultureByCountry();
		if (_configService.TryGetValue<string>(ConfigFile.System, ConfigCategory.Culture, ConfigSetting.AlternateCulture, out var value))
		{
			LogHelper.Instance.Log("Found alternate culture in configuration file: {0}", value);
			if (IsCultureValid(value))
			{
				text = value;
			}
			else
			{
				LogHelper.Instance.Log("Alternate Culture found in configuration file is not valid: {0}", value);
			}
		}
		else
		{
			LogHelper.Instance.Log("Alternate culture was not found in configuration file, using: {0}", text);
		}
		string defaultCulture = GetDefaultCulture();
		if (text == defaultCulture)
		{
			text = ((defaultCulture == GetDefaultCultureByCountry()) ? GetDefaultAlternateCultureByCountry() : GetDefaultCultureByCountry());
			LogHelper.Instance.Log("Alternate culture cannot equal the default.  Setting alternate culter to {0}", text);
		}
		return text;
	}

	private string GetDefaultCultureByCountry()
	{
		if (Country != Country.Canada)
		{
			return "en-US";
		}
		return "en-CA";
	}

	private string GetDefaultAlternateCultureByCountry()
	{
		if (Country != Country.Canada)
		{
			return "es-MX";
		}
		return "fr-CA";
	}

	private bool IsCultureValid(string culture)
	{
		return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Any((CultureInfo c) => c.Name.Equals(culture, StringComparison.CurrentCultureIgnoreCase));
	}

	public string GetWallpaperPath()
	{
		string result = string.Empty;
		if (_configService.TryGetValue<string>(ConfigFile.System, ConfigCategory.Culture, ConfigSetting.DesktopBackground, out var value))
		{
			LogHelper.Instance.Log("Found desktop background image path in configuration file: {0}", value);
			result = value;
		}
		else
		{
			LogHelper.Instance.Log("Desktop image background path was not found in configuration file");
		}
		return result;
	}

	public int GetSellThruDays()
	{
		int result = 30;
		if (_configService.TryGetValue<int>("system", "General", "SellThruDays", out var value))
		{
			result = value;
		}
		return result;
	}
}
