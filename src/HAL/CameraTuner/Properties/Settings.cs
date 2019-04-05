using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.CameraTuner.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
[CompilerGenerated]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("False")]
	public bool DetailedLog => (bool)this["DetailedLog"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("5000")]
	[ApplicationScopedSetting]
	public int WakeupPause => (int)this["WakeupPause"];
}
