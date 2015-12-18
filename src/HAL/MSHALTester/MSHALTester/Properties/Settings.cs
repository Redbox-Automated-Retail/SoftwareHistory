using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
public sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("Green")]
	public string SensorColor => (string)this["SensorColor"];

	[DefaultSettingValue("False")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public bool ExitOnAdvancedModeLaunch => (bool)this["ExitOnAdvancedModeLaunch"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("Red")]
	[ApplicationScopedSetting]
	public string ButtonRunningColor => (string)this["ButtonRunningColor"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("2")]
	public int PollSensorOneCount => (int)this["PollSensorOneCount"];

	[DefaultSettingValue("rcp://127.0.0.1:7001")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string CommunicationURL => (string)this["CommunicationURL"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("70000")]
	[DebuggerNonUserCode]
	public int ImmediateMoveTimeout => (int)this["ImmediateMoveTimeout"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("")]
	[DebuggerNonUserCode]
	public string FormBackgroundColor => (string)this["FormBackgroundColor"];

	[DefaultSettingValue("True")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public bool DisableQuickReturn => (bool)this["DisableQuickReturn"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("True")]
	[DebuggerNonUserCode]
	public bool CheckForRunningHAL => (bool)this["CheckForRunningHAL"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	[ApplicationScopedSetting]
	public bool DeveloperMode => (bool)this["DeveloperMode"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
