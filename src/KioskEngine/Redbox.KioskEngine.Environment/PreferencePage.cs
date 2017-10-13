using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal class PreferencePage : IPreferencePage
{
	public string Name { get; internal set; }

	public string SettingPath { get; internal set; }

	public string DisplayPath { get; internal set; }

	public IPreferencePageHost Host { get; internal set; }

	public PreferencePageTarget Target { get; internal set; }

	public event EventHandler Activate;

	public event EventHandler Deactivate;

	public T GetValue<T>(string settingName)
	{
		return GetValue(settingName, default(T));
	}

	public T GetValue<T>(string settingName, T defaultValue)
	{
		return (Target == PreferencePageTarget.LocalSystem) ? MachineSettingsStore.Instance.GetValue(SettingPath, settingName, defaultValue) : UserSettingsStore.Instance.GetValue(SettingPath, settingName, defaultValue);
	}

	public void SetValue(string settingName, object value)
	{
		if (Target == PreferencePageTarget.LocalSystem)
		{
			MachineSettingsStore.Instance.SetValue(SettingPath, settingName, value);
		}
		else
		{
			UserSettingsStore.Instance.SetValue(SettingPath, settingName, value);
		}
	}

	public void RaiseActivate()
	{
		if (Activate != null)
		{
			Activate(this, EventArgs.Empty);
		}
	}

	public void RaiseDeactivate()
	{
		if (Deactivate != null)
		{
			Deactivate(this, EventArgs.Empty);
		}
	}
}
