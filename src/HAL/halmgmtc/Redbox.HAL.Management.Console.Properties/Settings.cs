using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.Management.Console.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
[CompilerGenerated]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("120000")]
	public int ImmediateInstructionTimeout => (int)this["ImmediateInstructionTimeout"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp-p://127.0.0.1:7001")]
	public string NewConnectionURL => (string)this["NewConnectionURL"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("Red")]
	public string ButtonRunningColor => (string)this["ButtonRunningColor"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("")]
	public string ButtonCompleteColor => (string)this["ButtonCompleteColor"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://localhost:7001")]
	public string DefaultConnectionURL => (string)this["DefaultConnectionURL"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("thin,qlm-unload,unload-thin,sync,sync-locations,quick-return")]
	[ApplicationScopedSetting]
	public string SecureJob => (string)this["SecureJob"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("VendTestForm.exe")]
	public string VendTestLocation => (string)this["VendTestLocation"];
}
