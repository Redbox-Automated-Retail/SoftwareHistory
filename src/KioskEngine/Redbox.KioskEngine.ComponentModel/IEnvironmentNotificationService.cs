using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Redbox.KioskEngine.ComponentModel;

public interface IEnvironmentNotificationService
{
	bool HasValidCardReader { get; }

	bool CheckCardReader { get; set; }

	int NullKioskId { get; set; }

	void Reset();

	void Register(IntPtr handle);

	void Unregister(IntPtr handle);

	void ToggleTouchScreen();

	string TouchScreenDriver();

	TouchScreenState TouchScreenStatus();

	void RaiseValidReader();

	void RaiseInvalidReader();

	void LaunchSecureBrowser(IDictionary<string, string> approvedUrls, NavigateUrlCallback navigateUrlCallback);

	void NotifyConfigurationCallbacks(string store, string action, string path, string key, string newValue);

	void RegisterConfigurationCallback(string name, ConfigurationCallback callback);

	void UnregisterConfigurationCallback(string name);

	void UnregisterHardwareChangeCallback();

	void RegisterHardwareChangeCallback(HardwareChangeCallback callback);

	void UnRegisterCorruptDbCallback();

	void RegisterCorruptDbCallback(CorruptDbCallback callback);

	bool RaiseCorruptDb(string dbName);

	EnvironmentNotificationType ProcessNotification(ref Message msg);
}
