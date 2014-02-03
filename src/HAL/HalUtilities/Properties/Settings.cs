using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HALUtilities.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("False")]
	public bool KillEngine => (bool)this["KillEngine"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("10")]
	[ApplicationScopedSetting]
	public int TouchDriverWakeup => (int)this["TouchDriverWakeup"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("15000")]
	[ApplicationScopedSetting]
	public int TouchDriverResetTimeout => (int)this["TouchDriverResetTimeout"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("False")]
	public bool DebugHardware => (bool)this["DebugHardware"];
}
