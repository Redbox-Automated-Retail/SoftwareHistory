using System.ComponentModel;
using Redbox.Core;
using Redbox.GetOpts;

namespace Redbox.UpdateManager.Daemon;

[Usage("updatemanagerd [--register] [--forcestart] [--unregister]")]
public class Options
{
	[Description("Register the Kiosk Engine Daemon service process in Windows.")]
	[Option(LongName = "register")]
	public bool Register;

	[Option(LongName = "unregister")]
	[Description("Unregister the Kiosk Engine Daemon service process in Windows.")]
	public bool Unregister;

	[Description("Start the service in console mode.")]
	[Option(LongName = "console")]
	public bool Console;

	[Description("Force the service to start if register is passed.")]
	[Option(LongName = "forcestart")]
	public bool ForceStart;

	public static Options Instance => Singleton<Options>.Instance;

	private Options()
	{
	}
}
