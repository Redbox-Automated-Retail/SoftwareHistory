using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.Services.KioskHealth;

public class KioskHealthConfiguration : IKioskHealthConfiguration
{
	private IConfigurationService _configService;

	public bool KioskHealthActive => GetBoolean("kioskhealth", "KioskHealth", "KioskHealthActive");

	public int KioskEventsUnloadInterval => GetInt("kioskhealth", "KioskHealth", "KioskEventsUnloadInterval", 240);

	public bool TouchScreenActive => GetBoolean("kioskhealth", "KioskHealth", "TouchScreenActive");

	public bool TouchScreenReporting => GetBoolean("kioskhealth", "KioskHealth", "TouchScreenReporting");

	public bool TouchScreenHealing => GetBoolean("kioskhealth", "KioskHealth", "TouchScreenHealing");

	public TimeSpan TouchScreenStartCheck => GetTimeSpan("kioskhealth", "KioskHealth", "TouchScreenStartCheck", new TimeSpan(16, 0, 0));

	public TimeSpan TouchScreenEndCheck => GetTimeSpan("kioskhealth", "KioskHealth", "TouchScreenEndCheck", new TimeSpan(22, 0, 0));

	public int TouchScreenInActivity => GetInt("kioskhealth", "KioskHealth", "TouchScreenInActivity", 60);

	public int TouchScreenResetsToAlert => GetInt("kioskhealth", "KioskHealth", "TouchScreenResetsToAlert", 3);

	public bool ArcusActive => GetBoolean("kioskhealth", "KioskHealth", "ArcusActive");

	public bool ArcusReporting => GetBoolean("kioskhealth", "KioskHealth", "ArcusReporting");

	public bool RouterActive => GetBoolean("kioskhealth", "KioskHealth", "RouterActive");

	public bool RouterHealing => GetBoolean("kioskhealth", "KioskHealth", "RouterActive");

	public bool RouterReporting => GetBoolean("kioskhealth", "KioskHealth", "RouterHealing");

	public int RouterResetRetryMinutes => GetInt("kioskhealth", "KioskHealth", "RouterResetRetryMinutes", 30);

	public int RouterResetAfterXMessageFailures => GetInt("kioskhealth", "KioskHealth", "RouterResetAfterXMessageFailures", 2);

	public int RouterResetMaxAttempts => GetInt("kioskhealth", "KioskHealth", "RouterResetMaxAttempts", 2);

	public bool HardwareCorrectionStatsActive => GetBoolean("kioskhealth", "KioskHealth", "HardwareCorrectionStatsActive");

	public bool HardwareCorrectionStatsReporting => GetBoolean("kioskhealth", "KioskHealth", "HardwareCorrectionStatsReporting");

	public bool ViewActive => GetBoolean("kioskhealth", "KioskHealth", "ViewActive");

	public bool ViewReporting => GetBoolean("kioskhealth", "KioskHealth", "ViewReporting");

	public int ViewHealingType => GetInt("kioskhealth", "KioskHealth", "ViewHealingType", 0);

	public int ViewInActivity => GetInt("kioskhealth", "KioskHealth", "ViewInActivity", 600);

	public int ViewInActivityABEMode => GetInt("kioskhealth", "KioskHealth", "ViewInActivityABEMode", 900);

	public bool CCReaderActive => GetBoolean("kioskhealth", "KioskHealth", "CCReaderActive");

	public bool CCReaderSendNoDataEvents => GetBoolean("kioskhealth", "KioskHealth", "CCReaderSendNoDataEvents");

	public int CCReaderNoDataAttemptsToAlert => GetInt("kioskhealth", "KioskHealth", "CCReaderNoDataAttemptsToAlert", 3);

	public bool CCReaderSendBadDataEvents => GetBoolean("kioskhealth", "KioskHealth", "CCReaderSendBadDataEvents");

	public int CCReaderBadDataAttemptsToAlert => GetInt("kioskhealth", "KioskHealth", "CCReaderBadDataAttemptsToAlert", 3);

	public bool EMVReaderActive => GetBoolean("kioskhealth", "KioskHealth", "EMVReaderActive");

	public bool EMVReaderTrackMultipleFallbacks => GetBoolean("kioskhealth", "KioskHealth", "EMVReaderTrackMultipleFallbacks");

	public int EMVReaderMultipleFallbacksThreshold => GetInt("kioskhealth", "KioskHealth", "EMVReaderMultipleFallbacksThreshold", 3);

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
