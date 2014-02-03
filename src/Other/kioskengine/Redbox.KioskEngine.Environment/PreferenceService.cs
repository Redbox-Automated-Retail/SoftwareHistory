using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class PreferenceService : IPreferenceService
{
	private IDictionary<string, IPreferencePage> m_pages;

	public static IPreferenceService Instance => Singleton<PreferenceService>.Instance;

	public ReadOnlyCollection<IPreferencePage> PreferencePages => new List<IPreferencePage>(Pages.Values).AsReadOnly();

	internal IDictionary<string, IPreferencePage> Pages
	{
		get
		{
			if (m_pages == null)
			{
				m_pages = new Dictionary<string, IPreferencePage>();
			}
			return m_pages;
		}
	}

	public T GetValue<T>(string pageName, string settingName)
	{
		IPreferencePage preferencePage = GetPreferencePage(pageName);
		if (preferencePage == null)
		{
			return default(T);
		}
		if (preferencePage.Target != PreferencePageTarget.LocalSystem)
		{
			return UserSettingsStore.Instance.GetValue<T>(preferencePage.SettingPath, settingName);
		}
		return MachineSettingsStore.Instance.GetValue<T>(preferencePage.SettingPath, settingName);
	}

	public void SetValue(string pageName, string settingName, object value)
	{
		IPreferencePage preferencePage = GetPreferencePage(pageName);
		if (preferencePage != null)
		{
			if (preferencePage.Target == PreferencePageTarget.LocalSystem)
			{
				MachineSettingsStore.Instance.SetValue(preferencePage.SettingPath, settingName, value);
			}
			else
			{
				UserSettingsStore.Instance.SetValue(preferencePage.SettingPath, settingName, value);
			}
		}
	}

	public void RemoveAll()
	{
		Pages.Clear();
	}

	public IPreferencePage AddPreferencePage(string pageName, string settingPath, string displayPath, PreferencePageTarget target, IPreferencePageHost host)
	{
		IPreferencePage preferencePage = GetPreferencePage(pageName);
		if (preferencePage != null)
		{
			return preferencePage;
		}
		PreferencePage preferencePage2 = new PreferencePage();
		preferencePage2.Host = host;
		preferencePage2.Name = pageName;
		preferencePage2.Target = target;
		preferencePage2.DisplayPath = displayPath;
		preferencePage2.SettingPath = settingPath;
		preferencePage = preferencePage2;
		Pages[pageName] = preferencePage;
		return preferencePage;
	}

	public bool RemovePreferencePage(string pageName)
	{
		return Pages.Remove(pageName);
	}

	public IPreferencePage GetPreferencePage(string pageName)
	{
		if (!Pages.ContainsKey(pageName))
		{
			return null;
		}
		return Pages[pageName];
	}

	private PreferenceService()
	{
	}
}
