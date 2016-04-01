using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HALUtilities.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
[CompilerGenerated]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DefaultSettingValue("False")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public bool KillEngine => (bool)this["KillEngine"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("10")]
	public int TouchDriverWakeup => (int)this["TouchDriverWakeup"];

	[DefaultSettingValue("15000")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public int TouchDriverResetTimeout => (int)this["TouchDriverResetTimeout"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	[ApplicationScopedSetting]
	public bool DebugHardware => (bool)this["DebugHardware"];
}
