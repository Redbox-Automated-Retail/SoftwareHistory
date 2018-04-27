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

		public const string Personalization = "Personalization";

		public const string BurnInTimeout = "BurnInTimeout";

		public const string BurnInViewDuration = "BurnInViewDuration";

		public const string EnableCreditsButton = "EnableCreditsButton";

		public const string PromptForCredits = "PromptForCredits";

		public const string EarlyIDEnabled = "EarlyIDEnabled";

		public const string LoyaltyEnabled = "LoyaltyEnabled";

		public const string SignInTimeout = "SignInTimeout";

		public const string SignInADATimeout = "SignInADATimeout";

		public const string EnablePricingEngine = "EnablePricingEngine";

		public const string ComingSoonOptInEnabled = "ComingSoonOptIn";

		public const string ServiceFeeEnabled = "ServiceFeeEnabled";

		public const string BlurayUpsellEnabled = "BluRayUpsellEnabled";

		public const string _4kUpsellEnabled = "4KUpsellEnabled";

		public const string OfflineItemsLimit = "OfflineItemsLimit";

		public const string OfflineGameRentalLimit = "OfflineGameRentalLimit";

		public const string OfflineGamePurchaseLimit = "OfflineGamePurchaseLimit";
	}

	private Dictionary<TitleType, bool> _enabledGameTypes;

	private Dictionary<TitleType, bool> _gameHasContentTypes;

	private int _burnInTimeout;

	private int _burnInViewDuration;

	private bool _comingSoonOptInEnabled;

	private bool _enableCreditsButton;

	private bool _earlyIdEnabled;

	private bool _loyaltyEnabled;

	private int _signInTimeout;

	private int _signInADATimeout;

	private bool _enablePricingEngine;

	private bool _serviceFeeEnabled;

	private bool __4KUpsellEnabled;

	private bool _blurayUpsellEnabled;

	private int _offlineItemsLimit;

	private int _offlineGameRentalLimit;

	private int _offlineGamePurchaseLimit;

	private bool _enableFlashDeals;

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
				_enabledGameTypes.Add(TitleType.NintendoSwitch, GetGameConfig(service, ConfigSetting.GameEnableNintendoSwitch));
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
				_gameHasContentTypes.Add(TitleType.NintendoSwitch, GetMachineSetting(service, MachineSettings.GameHasContentNintendoSwitch));
			}
			return _gameHasContentTypes;
		}
	}

	public int BurnInTimeout => _burnInTimeout;

	public int BurnInViewDuration => _burnInViewDuration;

	public bool EnableCreditsButton => _enableCreditsButton;

	public bool EarlyIdEnabled => _earlyIdEnabled;

	public bool ComingSoonOptInEnabled => _comingSoonOptInEnabled;

	public bool LoyaltyEnabled => _loyaltyEnabled;

	public int SignInTimeout => _signInTimeout;

	public int SignInADATimeout => _signInADATimeout;

	public bool EnablePricingEngine => _enablePricingEngine;

	public bool ServiceFeeEnabled => _serviceFeeEnabled;

	public bool BlurayUpsellEnabled => _blurayUpsellEnabled;

	public bool _4KUpsellEnabled => __4KUpsellEnabled;

	public int OfflineItemsLimit => _offlineItemsLimit;

	public int OfflineGameRentalLimit => _offlineGameRentalLimit;

	public int OfflineGamePurchaseLimit => _offlineGamePurchaseLimit;

	public bool EnableFlashDeals => _enableFlashDeals;

	private Configuration()
	{
		Initialize();
	}

	private void Initialize()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		_burnInTimeout = ConfigGetValue(service, "system", "General", "BurnInTimeout", 1800000);
		_burnInViewDuration = ConfigGetValue(service, "system", "General", "BurnInViewDuration", 30000);
		_signInTimeout = ConfigGetValue(service, "system", "Personalization", "SignInTimeout", 20000);
		_signInADATimeout = ConfigGetValue(service, "system", "Personalization", "SignInADATimeout", 90000);
		_enablePricingEngine = ConfigGetValue(service, "system", "General", "EnablePricingEngine", defaultValue: true);
		SessionReload();
	}

	public void SessionReload()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		_comingSoonOptInEnabled = ConfigGetValue(service, "system", "General", "ComingSoonOptIn", defaultValue: true);
		_enableCreditsButton = ConfigGetValue(service, "system", "General", "EnableCreditsButton", defaultValue: true);
		_earlyIdEnabled = ConfigGetValue(service, "system", "Personalization", "EarlyIDEnabled", defaultValue: false);
		_loyaltyEnabled = ConfigGetValue(service, "system", "Personalization", "LoyaltyEnabled", defaultValue: false);
		_serviceFeeEnabled = ConfigGetValue(service, "system", "General", "ServiceFeeEnabled", defaultValue: false);
		_blurayUpsellEnabled = ConfigGetValue(service, "system", "General", "BluRayUpsellEnabled", defaultValue: false);
		__4KUpsellEnabled = ConfigGetValue(service, "system", "General", "4KUpsellEnabled", defaultValue: false);
		_offlineItemsLimit = ConfigGetValue(service, "system", "General", "OfflineItemsLimit", 3);
		_offlineGameRentalLimit = ConfigGetValue(service, "system", "General", "OfflineGameRentalLimit", 1);
		_offlineGamePurchaseLimit = ConfigGetValue(service, "system", "General", "OfflineGamePurchaseLimit", 2);
		_enableFlashDeals = ConfigGetValue(service, ConfigFile.Marketing, ConfigCategory.MarketingGeneral, ConfigSetting.EnableFlashDeals, defaultValue: false);
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
		bool value = false;
		if (configService != null && !configService.TryGetValue<bool>("system", "Games", settingName, out value))
		{
			value = false;
		}
		return value;
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
