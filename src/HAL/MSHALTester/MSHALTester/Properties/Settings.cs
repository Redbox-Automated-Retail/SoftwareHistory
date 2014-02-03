using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
[CompilerGenerated]
public sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DefaultSettingValue("Green")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public string SensorColor => (string)this["SensorColor"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool ExitOnAdvancedModeLaunch => (bool)this["ExitOnAdvancedModeLaunch"];

	[DefaultSettingValue("Red")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string ButtonRunningColor => (string)this["ButtonRunningColor"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("2")]
	[DebuggerNonUserCode]
	public int PollSensorOneCount => (int)this["PollSensorOneCount"];

	[DefaultSettingValue("rcp://127.0.0.1:7001")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public string CommunicationURL => (string)this["CommunicationURL"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("70000")]
	[DebuggerNonUserCode]
	public int ImmediateMoveTimeout => (int)this["ImmediateMoveTimeout"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string FormBackgroundColor => (string)this["FormBackgroundColor"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("True")]
	public bool DisableQuickReturn => (bool)this["DisableQuickReturn"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	[ApplicationScopedSetting]
	public bool CheckForRunningHAL => (bool)this["CheckForRunningHAL"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
