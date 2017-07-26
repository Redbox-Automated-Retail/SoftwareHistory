using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.Configuration;

public class Configuration : IConfiguration
{
	public class Constants
	{
		public const string System = "system";

		public const string General = "General";

		public const string EnablePlatformScreen = "EnablePlatformScreen";

		public const string BurnInTimeout = "BurnInTimeout";

		public const string BurnInViewDuration = "BurnInViewDuration";
	}

	private Dictionary<TitleType, bool> _enabledGameTypes;

	private Dictionary<TitleType, bool> _gameHasContentTypes;

	private bool _enablePlatformScreen;

	private int _burnInTimeout;

	private int _burnInViewDuration;

	public static IConfiguration Instance => Singleton<Configuration>.Instance;

	public Dictionary<TitleType, bool> EnabledGameTypes
	{
		get
		{
			if (_enabledGameTypes == null)
			{
				IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
				_enabledGameTypes = new Dictionary<TitleType, bool>();
				_enabledGameTypes.Add(TitleType.Xbox360, GetGameConfig(service, ConfigSetting.GameEnableXbox360));
				_enabledGameTypes.Add(TitleType.XboxOne, GetGameConfig(service, ConfigSetting.GameEnableXboxOne));
				_enabledGameTypes.Add(TitleType.PS3, GetGameConfig(service, ConfigSetting.GameEnablePS3));
				_enabledGameTypes.Add(TitleType.PS4, GetGameConfig(service, ConfigSetting.GameEnablePS4));
				_enabledGameTypes.Add(TitleType.Wii, GetGameConfig(service, ConfigSetting.GameEnableWii));
				_enabledGameTypes.Add(TitleType.WiiU, GetGameConfig(service, ConfigSetting.GameEnableWiiU));
			}
			return _enabledGameTypes;
		}
	}

	public Dictionary<TitleType, bool> GameHasContentTypes
	{
		get
		{
			if (_gameHasContentTypes == null)
			{
				IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
				_gameHasContentTypes = new Dictionary<TitleType, bool>();
				_gameHasContentTypes.Add(TitleType.Xbox360, GetMachineSetting(service, MachineSettings.GameHasContentXbox360));
				_gameHasContentTypes.Add(TitleType.XboxOne, GetMachineSetting(service, MachineSettings.GameHasContentXboxOne));
				_gameHasContentTypes.Add(TitleType.PS3, GetMachineSetting(service, MachineSettings.GameHasContentPS3));
				_gameHasContentTypes.Add(TitleType.PS4, GetMachineSetting(service, MachineSettings.GameHasContentPS4));
				_gameHasContentTypes.Add(TitleType.Wii, GetMachineSetting(service, MachineSettings.GameHasContentWii));
				_gameHasContentTypes.Add(TitleType.WiiU, GetMachineSetting(service, MachineSettings.GameHasContentWiiU));
			}
			return _gameHasContentTypes;
		}
	}

	public bool EnablePlatformScreen => _enablePlatformScreen;

	public int BurnInTimeout => _burnInTimeout;

	public int BurnInViewDuration => _burnInViewDuration;

	private Configuration()
	{
		Initialize();
	}

	private void Initialize()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		_enablePlatformScreen = GetGameConfig(service, "EnablePlatformScreen");
		_burnInTimeout = ConfigGetValue(service, "system", "General", "BurnInTimeout", 1800000);
		_burnInViewDuration = ConfigGetValue(service, "system", "General", "BurnInViewDuration", 30000);
	}

	private bool GetMachineSetting(IMachineSettingsStore machineSettingStore, string key)
	{
		bool result = false;
		if (machineSettingStore != null)
		{
			bool.TryParse(machineSettingStore.GetValue("Store", key, "false"), out result);
		}
		return result;
	}

	private bool GetGameConfig(IConfigurationService configService, string settingName)
	{
		if (configService == null)
		{
			return false;
		}
		if (configService.TryGetValue<bool>("system", "Games", settingName, out var value))
		{
			return value;
		}
		return false;
	}

	private static T ConfigGetValue<T>(IConfigurationService configService, string configFile, string categoryName, string settingName, T defaultValue)
	{
		T value = defaultValue;
		if (configService != null && !configService.TryGetValue<T>(configFile, categoryName, settingName, out value))
		{
			value = defaultValue;
		}
		return value;
	}
}
