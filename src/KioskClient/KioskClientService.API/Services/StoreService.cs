using System;
using System.Globalization;
using System.IO;
using System.Linq;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace KioskClientService.API.Services;

public class StoreService : IStoreService
{
	private IConfigurationService _configService;

	private ILogger<StoreService> _logger;

	private long _kioskId;

	private string _market;

	private string _banner;

	public string _dataPath;

	public Country Country { get; } = Country.US;

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
					if (value == null || string.IsNullOrEmpty(value.ToString()))
					{
						_logger.LogInformation("Kiosk ID is not set in the registry at {0}\\{1}", text2, text);
						return 0L;
					}
					_kioskId = Convert.ToInt32(value);
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Unhandled exception occurred in StoreManagerService.GetKioskId");
			}
			return _kioskId;
		}
	}

	public string Market
	{
		get
		{
			try
			{
				if (string.IsNullOrWhiteSpace(_market))
				{
					string text = "Market";
					string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object value = Registry.GetValue(text2, text, null);
					if (value == null)
					{
						_logger.LogInformation("Market is not set in the registry at {0}\\{1}", text2, text);
						return null;
					}
					_market = ((string)value).Trim();
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Unhandled exception occurred in StoreManagerService.GetMarket");
			}
			return _market;
		}
	}

	public string Banner
	{
		get
		{
			try
			{
				if (string.IsNullOrWhiteSpace(_banner))
				{
					string text = "Banner";
					string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
					object value = Registry.GetValue(text2, text, null);
					if (value == null)
					{
						_logger.LogInformation("Banner is not set in the registry at {0}\\{1}", text2, text);
						return null;
					}
					_banner = ((string)value).Trim();
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Unhandled exception occurred in StoreManagerService.GetMarket");
			}
			return _banner;
		}
	}

	public string DataPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_dataPath))
			{
				return _dataPath;
			}
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create);
			string path = Path.Combine(folderPath, "Redbox\\KioskClient");
			_dataPath = Path.GetFullPath(path);
			return _dataPath;
		}
	}

	public string RunningPath => Path.GetDirectoryName(typeof(StoreService).Assembly.Location);

	public StoreService(ILogger<StoreService> logger, IConfigurationService configService)
	{
		_logger = logger;
		_configService = configService;
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
}
