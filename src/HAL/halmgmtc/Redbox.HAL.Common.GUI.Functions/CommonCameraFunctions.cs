using System.Diagnostics;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions;

public static class CommonCameraFunctions
{
	public static bool IRCameraConfigured(HardwareService service)
	{
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(service, "CAMERA IRCAMERA");
		if (hardwareJob == null)
		{
			return false;
		}
		string topOfStack = CommonFunctions.GetTopOfStack(hardwareJob);
		return bool.Parse(topOfStack);
	}

	public static void LaunchAndWaitForTuner(HardwareService service)
	{
		if (service == null)
		{
			return;
		}
		bool flag = IRCameraConfigured(service);
		string text = "c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe";
		if (File.Exists(text))
		{
			CommonFunctions.ExecuteInstruction(service, "CAMERA STOP FORCE=TRUE");
			CommonFunctions.ExecuteInstruction(service, "RINGLIGHT ON");
			if (Platform.Windows7 == RuntimeService.Instance.GetPlatform())
			{
				PerfFunctions.SpinWait(5000);
			}
			Process process = Process.Start(text, flag ? "-secure" : string.Empty);
			process.WaitForExit();
			CommonFunctions.ExecuteInstruction(service, "RINGLIGHT OFF");
			CommonFunctions.ExecuteInstruction(service, "CAMERA START");
		}
	}
}
