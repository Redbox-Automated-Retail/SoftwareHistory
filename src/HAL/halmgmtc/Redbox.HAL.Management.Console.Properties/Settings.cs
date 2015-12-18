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

	[ApplicationScopedSetting]
	[DefaultSettingValue("120000")]
	[DebuggerNonUserCode]
	public int ImmediateInstructionTimeout => (int)this["ImmediateInstructionTimeout"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp-p://127.0.0.1:7001")]
	[ApplicationScopedSetting]
	public string NewConnectionURL => (string)this["NewConnectionURL"];

	[DefaultSettingValue("Red")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string ButtonRunningColor => (string)this["ButtonRunningColor"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	[ApplicationScopedSetting]
	public string ButtonCompleteColor => (string)this["ButtonCompleteColor"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("rcp://localhost:7001")]
	[DebuggerNonUserCode]
	public string DefaultConnectionURL => (string)this["DefaultConnectionURL"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("thin,thin-vmz,qlm-unload,unload-thin,sync,sync-locations,quick-return")]
	public string SecureJob => (string)this["SecureJob"];

	[DefaultSettingValue("False")]
	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	public bool DeveloperMode => (bool)this["DeveloperMode"];
}
