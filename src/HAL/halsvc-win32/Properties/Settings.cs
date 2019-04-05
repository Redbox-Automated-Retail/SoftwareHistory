using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Redbox.HAL.Service.Win32.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
[CompilerGenerated]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DefaultSettingValue("Modern")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public string MotionControlServiceVersion
	{
		get
		{
			return (string)this["MotionControlServiceVersion"];
		}
		set
		{
			this["MotionControlServiceVersion"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool CountersDBExclusiveMode
	{
		get
		{
			return (bool)this["CountersDBExclusiveMode"];
		}
		set
		{
			this["CountersDBExclusiveMode"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool UsbServiceDebug
	{
		get
		{
			return (bool)this["UsbServiceDebug"];
		}
		set
		{
			this["UsbServiceDebug"] = value;
		}
	}

	[UserScopedSetting]
	[DefaultSettingValue("rcp://127.0.0.1:7001")]
	[DebuggerNonUserCode]
	public string ServiceProtocol
	{
		get
		{
			return (string)this["ServiceProtocol"];
		}
		set
		{
			this["ServiceProtocol"] = value;
		}
	}

	[UserScopedSetting]
	[DefaultSettingValue("Legacy")]
	[DebuggerNonUserCode]
	public string ServiceHostGeneration
	{
		get
		{
			return (string)this["ServiceHostGeneration"];
		}
		set
		{
			this["ServiceHostGeneration"] = value;
		}
	}
}
