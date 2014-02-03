using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.Tool.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("..\\.store\\.data")]
	public string DataStoreRoot => (string)this["DataStoreRoot"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("${RunningPath}\\..\\Kiosk Engine\\bin\\kioskengine.exe")]
	[ApplicationScopedSetting]
	public string KioskEnginePath => (string)this["KioskEnginePath"];

	[DefaultSettingValue("..\\.store\\.repository")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public string ManifestRoot => (string)this["ManifestRoot"];

	[DefaultSettingValue("rcp://localhost:7002")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string UpdateServiceUrl => (string)this["UpdateServiceUrl"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://localhost:7004")]
	public string UpdateManagerUrl => (string)this["UpdateManagerUrl"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("60")]
	public int MinimumRetryDelay => (int)this["MinimumRetryDelay"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
