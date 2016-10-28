using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.MSHALTester.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
[CompilerGenerated]
public sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DefaultSettingValue("Green")]
	[DebuggerNonUserCode]
	public string SensorColor => (string)this["SensorColor"];

	[DefaultSettingValue("False")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public bool ExitOnAdvancedModeLaunch => (bool)this["ExitOnAdvancedModeLaunch"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("Red")]
	public string ButtonRunningColor => (string)this["ButtonRunningColor"];

	[DefaultSettingValue("2")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public int PollSensorOneCount => (int)this["PollSensorOneCount"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("rcp://127.0.0.1:7001")]
	public string CommunicationURL => (string)this["CommunicationURL"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("70000")]
	public int ImmediateMoveTimeout => (int)this["ImmediateMoveTimeout"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("")]
	public string FormBackgroundColor => (string)this["FormBackgroundColor"];

	[DefaultSettingValue("True")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public bool DisableQuickReturn => (bool)this["DisableQuickReturn"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	[ApplicationScopedSetting]
	public bool CheckForRunningHAL => (bool)this["CheckForRunningHAL"];

	[DefaultSettingValue("False")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public bool DeveloperMode => (bool)this["DeveloperMode"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("False")]
	public bool AllowSessionDisplay => (bool)this["AllowSessionDisplay"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	[ApplicationScopedSetting]
	public bool UsbServiceDebug => (bool)this["UsbServiceDebug"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
