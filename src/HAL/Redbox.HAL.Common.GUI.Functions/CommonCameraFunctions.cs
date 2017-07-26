using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions;

public static class CommonCameraFunctions
{
	public static bool IRCameraConfigured(HardwareService service)
	{
		HardwareJob val = CommonFunctions.ExecuteInstruction(service, "CAMERA IRCAMERA");
		if (val == null)
		{
			return false;
		}
		string topOfStack = HardwareJobExtensions.GetTopOfStack(val);
		return bool.Parse(topOfStack);
	}

	public static void LaunchAndWaitForTuner(HardwareService service)
	{
		if (service != null)
		{
			bool flag = IRCameraConfigured(service);
			string text = "c:\\Program Files\\Redbox\\halservice\\bin\\CameraTuner.exe";
			if (File.Exists(text))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("CAMERA STOP FORCE=TRUE" + Environment.NewLine);
				stringBuilder.Append("RINGLIGHT ON" + Environment.NewLine);
				HardwareJob val = default(HardwareJob);
				service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), ref val);
				PerfFunctions.SpinWait(5000);
				Process process = Process.Start(text, flag ? "-secure" : string.Empty);
				process.WaitForExit();
				stringBuilder = new StringBuilder();
				stringBuilder.Append("RINGLIGHT OFF" + Environment.NewLine);
				stringBuilder.Append("CAMERA START" + Environment.NewLine);
				service.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), ref val);
			}
		}
	}
}
