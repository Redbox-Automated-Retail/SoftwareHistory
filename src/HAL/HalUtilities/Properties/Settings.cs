using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HALUtilities.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	public bool KillEngine => (bool)this["KillEngine"];

	[DefaultSettingValue("10")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public int TouchDriverWakeup => (int)this["TouchDriverWakeup"];

	[DefaultSettingValue("15000")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public int TouchDriverResetTimeout => (int)this["TouchDriverResetTimeout"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	[ApplicationScopedSetting]
	public bool DebugHardware => (bool)this["DebugHardware"];
}
