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

	[DefaultSettingValue("rcp://127.0.0.1:7001")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public string CommunicationURL => (string)this["CommunicationURL"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("70000")]
	[ApplicationScopedSetting]
	public int ImmediateMoveTimeout => (int)this["ImmediateMoveTimeout"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool CheckForRunningHAL => (bool)this["CheckForRunningHAL"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	[ApplicationScopedSetting]
	public bool AllowSessionDisplay => (bool)this["AllowSessionDisplay"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("True")]
	public bool UsbServiceDebug => (bool)this["UsbServiceDebug"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("True")]
	public bool TestCommOnStartup => (bool)this["TestCommOnStartup"];

	[UserScopedSetting]
	[DefaultSettingValue("c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs\\Timeouts.log")]
	[DebuggerNonUserCode]
	public string CountersFile
	{
		get
		{
			return (string)this["CountersFile"];
		}
		set
		{
			this["CountersFile"] = value;
		}
	}

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("5000")]
	public int RouterRelayPause => (int)this["RouterRelayPause"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("False")]
	[DebuggerNonUserCode]
	public bool DisplayEngineeringTab => (bool)this["DisplayEngineeringTab"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("c:\\Program Files\\Redbox\\HALService\\video")]
	[ApplicationScopedSetting]
	public string ImageDirectory => (string)this["ImageDirectory"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe")]
	public string TunerApplication => (string)this["TunerApplication"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("2500")]
	[ApplicationScopedSetting]
	public int TunerStartPause => (int)this["TunerStartPause"];

	private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
	{
	}

	private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
	{
	}
}
