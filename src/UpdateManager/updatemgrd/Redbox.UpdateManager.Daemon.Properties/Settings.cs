using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.UpdateManager.Daemon.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DefaultSettingValue("rcp://localhost:7004")]
	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	public string Url => (string)this["Url"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("3")]
	[DebuggerNonUserCode]
	public int MaximumWorkerThreads => (int)this["MaximumWorkerThreads"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("..\\scripts")]
	public string ScriptLocation => (string)this["ScriptLocation"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("${REDS}\\Kiosk Engine\\bin\\kioskengine.exe")]
	public string KioskEnginePath => (string)this["KioskEnginePath"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("False")]
	public bool DeveloperMode => (bool)this["DeveloperMode"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool InitialSubscriptionState => (bool)this["InitialSubscriptionState"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	[ApplicationScopedSetting]
	public string StoreNumber => (string)this["StoreNumber"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("3503")]
	[DebuggerNonUserCode]
	public int TrimWorkingSetInterval => (int)this["TrimWorkingSetInterval"];

	[ApplicationScopedSetting]
	[DefaultSettingValue("00:00:30")]
	[DebuggerNonUserCode]
	public TimeSpan UpdateServiceTimeout => (TimeSpan)this["UpdateServiceTimeout"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("${Scripts}\\shutdown.lua")]
	[ApplicationScopedSetting]
	public string ShutdownScriptPath => (string)this["ShutdownScriptPath"];

	[ApplicationScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("60")]
	public int MinimumRetryDelay => (int)this["MinimumRetryDelay"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("http://bundle-dev-02:8002")]
	[ApplicationScopedSetting]
	public string WcfUpdateServiceUrl => (string)this["WcfUpdateServiceUrl"];

	[DebuggerNonUserCode]
	[DefaultSettingValue("rcp://qa-updatesvc01:7002")]
	[ApplicationScopedSetting]
	public string UpdateServiceUrl => (string)this["UpdateServiceUrl"];

	[DebuggerNonUserCode]
	[ApplicationScopedSetting]
	[DefaultSettingValue("rcp://localhost:8004")]
	public string KioskEngineUrl => (string)this["KioskEngineUrl"];
}
