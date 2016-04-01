using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
public sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("Green")]
	public string SensorColor => (string)this["SensorColor"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool ExitOnAdvancedModeLaunch => (bool)this["ExitOnAdvancedModeLaunch"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("Red")]
	public string ButtonRunningColor => (string)this["ButtonRunningColor"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("2")]
	public int PollSensorOneCount => (int)this["PollSensorOneCount"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("rcp://127.0.0.1:7001")]
	[DebuggerNonUserCode]
	public string CommunicationURL => (string)this["CommunicationURL"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("70000")]
	public int ImmediateMoveTimeout => (int)this["ImmediateMoveTimeout"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	[ApplicationScopedSetting]
	public string FormBackgroundColor => (string)this["FormBackgroundColor"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("True")]
	[DebuggerNonUserCode]
	public bool DisableQuickReturn => (bool)this["DisableQuickReturn"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("True")]
	public bool CheckForRunningHAL => (bool)this["CheckForRunningHAL"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	[ApplicationScopedSetting]
	public bool DeveloperMode => (bool)this["DeveloperMode"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("False")]
	[DebuggerNonUserCode]
	public bool AllowSessionDisplay => (bool)this["AllowSessionDisplay"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
