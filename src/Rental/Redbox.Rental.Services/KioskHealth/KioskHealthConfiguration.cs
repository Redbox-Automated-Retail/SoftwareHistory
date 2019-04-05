using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.Services.KioskHealth;

public class KioskHealthConfiguration : IKioskHealthConfiguration
{
	private IConfigurationService _configService;

	public bool KioskHealthActive => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.KioskHealthActive);

	public int KioskEventsUnloadInterval => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.KioskEventsUnloadInterval, 240);

	public bool TouchScreenActive => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.TouchScreenActive);

	public bool TouchScreenReporting => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.TouchScreenReporting);

	public bool TouchScreenHealing => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.TouchScreenHealing);

	public TimeSpan TouchScreenStartCheck => GetTimeSpan(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.TouchScreenStartCheck, new TimeSpan(16, 0, 0));

	public TimeSpan TouchScreenEndCheck => GetTimeSpan(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.TouchScreenEndCheck, new TimeSpan(22, 0, 0));

	public int TouchScreenInActivity => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.TouchScreenInActivity, 60);

	public int TouchScreenResetsToAlert => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.TouchScreenResetsToAlert, 3);

	public bool ArcusActive => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.ArcusActive);

	public bool ArcusReporting => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.ArcusReporting);

	public bool RouterActive => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.RouterActive);

	public bool RouterHealing => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.RouterActive);

	public bool RouterReporting => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.RouterHealing);

	public int RouterResetRetryMinutes => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.RouterResetRetryMinutes, 30);

	public int RouterResetAfterXMessageFailures => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.RouterResetAfterXMessageFailures, 2);

	public int RouterResetMaxAttempts => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.RouterResetMaxAttempts, 2);

	public bool HardwareCorrectionStatsActive => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.HardwareCorrectionStatsActive);

	public bool HardwareCorrectionStatsReporting => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.HardwareCorrectionStatsReporting);

	public bool ViewActive => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.ViewActive);

	public bool ViewReporting => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.ViewReporting);

	public int ViewHealingType => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.ViewHealingType, 0);

	public int ViewInActivity => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.ViewInActivity, 600);

	public int ViewInActivityABEMode => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.ViewInActivityABEMode, 900);

	public bool CCReaderActive => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.CCReaderActive);

	public bool CCReaderSendNoDataEvents => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.CCReaderSendNoDataEvents);

	public int CCReaderNoDataAttemptsToAlert => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.CCReaderNoDataAttemptsToAlert, 3);

	public bool CCReaderSendBadDataEvents => GetBoolean(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.CCReaderSendBadDataEvents);

	public int CCReaderBadDataAttemptsToAlert => GetInt(ConfigFile.KioskHealth, ConfigCategory.KioskHealth, KioskHealthSettings.CCReaderBadDataAttemptsToAlert, 3);

	public KioskHealthConfiguration(IConfigurationService configService)
	{
		_configService = configService;
	}

	private bool GetBoolean(string key, string category, string setting, bool defaultValue = true)
	{
		if (_configService.TryGetValue<bool>(key, category, setting, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	private int GetInt(string key, string category, string setting, int defaultValue)
	{
		if (_configService.TryGetValue<int>(key, category, setting, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	private TimeSpan GetTimeSpan(string key, string category, string setting, TimeSpan defaultValue)
	{
		if (_configService.TryGetValue<string>(key, category, setting, out var value) && value != null && TimeSpan.TryParse(value, out var result))
		{
			return result;
		}
		return defaultValue;
	}
}
