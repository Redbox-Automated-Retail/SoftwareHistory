using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.Management.Console.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("120000")]
	public int ImmediateInstructionTimeout => (int)this["ImmediateInstructionTimeout"];

	[DefaultSettingValue("rcp-p://127.0.0.1:7001")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string NewConnectionURL => (string)this["NewConnectionURL"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("Red")]
	[DebuggerNonUserCode]
	public string ButtonRunningColor => (string)this["ButtonRunningColor"];

	[DefaultSettingValue("")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string ButtonCompleteColor => (string)this["ButtonCompleteColor"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://localhost:7001")]
	[ApplicationScopedSetting]
	public string DefaultConnectionURL => (string)this["DefaultConnectionURL"];

	[DefaultSettingValue("thin,thin-vmz,qlm-unload,unload-thin,sync,sync-locations,quick-return")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public string SecureJob => (string)this["SecureJob"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("False")]
	[ApplicationScopedSetting]
	public bool DeveloperMode => (bool)this["DeveloperMode"];
}
