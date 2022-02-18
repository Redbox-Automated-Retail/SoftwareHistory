using System;

namespace Redbox.KioskEngine.PreferenceManager.Preferences;

public class PreferencePage : IPreferencePage
{
	public string Name { get; set; }

	public string Path { get; set; }

	public Type PreferencePageHostType { get; set; }

	public object PreferencePageHost { get; set; }
}
