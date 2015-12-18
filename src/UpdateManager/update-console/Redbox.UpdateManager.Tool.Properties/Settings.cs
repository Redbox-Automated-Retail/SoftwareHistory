using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.Tool.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DebuggerNonUserCode]
	[DefaultSettingValue("..\\.store\\.data")]
	[ApplicationScopedSetting]
	public string DataStoreRoot => (string)this["DataStoreRoot"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("${RunningPath}\\..\\Kiosk Engine\\bin\\kioskengine.exe")]
	public string KioskEnginePath => (string)this["KioskEnginePath"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("..\\.store\\.repository")]
	[DebuggerNonUserCode]
	public string ManifestRoot => (string)this["ManifestRoot"];

	[DefaultSettingValue("rcp://localhost:7002")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string UpdateServiceUrl => (string)this["UpdateServiceUrl"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://localhost:7004")]
	[ApplicationScopedSetting]
	public string UpdateManagerUrl => (string)this["UpdateManagerUrl"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("60")]
	[ApplicationScopedSetting]
	public int MinimumRetryDelay => (int)this["MinimumRetryDelay"];

	[DefaultSettingValue("rcp://localhost:8004")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string KioskEngineUrl => (string)this["KioskEngineUrl"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
