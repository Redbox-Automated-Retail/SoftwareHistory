using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class TrippliteInstaller
{
	private const string TrippliteVID = "09ae";

	private const int TrippliteMinWatchdogPID = 2009;

	private const string BaseInstallerPath = "c:\\Drivers\\Redbox\\UPS";

	private const string NonWatchdogInstaller = "Automated Tripplite Install _Non Watchdog (HW ID _ PID Less than 2009)\\Install.bat";

	private const string WatchdogInstaller = "Tripplite_DAT+SFW+COM_Ver12.04.0053\\Tripplite_Software_Install_v6.bat";

	private readonly char[] Separator = new char[1] { '&' };

	internal void Install(TrippliteVersion version)
	{
		string text = ((version == TrippliteVersion.Prewatchdog) ? Path.Combine("c:\\Drivers\\Redbox\\UPS", "Automated Tripplite Install _Non Watchdog (HW ID _ PID Less than 2009)\\Install.bat") : Path.Combine("c:\\Drivers\\Redbox\\UPS", "Tripplite_DAT+SFW+COM_Ver12.04.0053\\Tripplite_Software_Install_v6.bat"));
		if (!File.Exists(text))
		{
			Extract();
			if (!File.Exists(text))
			{
				WriteFailureMessage("There is no install file. Sorry, I really tried, but I just can't do anything right.");
				return;
			}
		}
		Process process = null;
		try
		{
			process = Process.Start(text);
			process.WaitForExit();
			string path = Program.MakeLogPath("TrippliteInstall.log", delete: true);
			using StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.WriteLine("The install script exited with code {0}", process.ExitCode);
		}
		catch (Exception ex)
		{
			WriteFailureMessage("Install failed; exception = {0}.", ex.Message);
		}
	}

	internal void Extract()
	{
		string text = Path.Combine("c:\\Drivers\\Redbox\\UPS", "Tripplite.zip");
		if (!File.Exists(text))
		{
			WriteFailureMessage("There are no installer and/or zip files to explode. Sorry, I can't do anything right.");
			return;
		}
		ZipFileManager zipFileManager = new ZipFileManager(text);
		zipFileManager.Unzip("c:\\Drivers\\Redbox\\UPS");
	}

	internal TrippliteVersion FindTripplite()
	{
		List<string> errors = new List<string>();
		List<string> list = new List<string>();
		DeviceHelper deviceHelper = new DeviceHelper();
		deviceHelper.FindDeviceWithVid(DeviceClass.HIDClass, "09ae", errors, list);
		if (list.Count == 0)
		{
			WriteFailureMessage("There are no Tripplite devices attached; bailing install.");
			return TrippliteVersion.None;
		}
		if (list.Count > 1)
		{
			WriteFailureMessage("There is more than one Tripplite devices attached; bailing install.");
			return TrippliteVersion.None;
		}
		string text = list[0];
		if (!text.ToLower().StartsWith("vid"))
		{
			int num = text.IndexOf("vid");
			if (num == -1)
			{
				WriteFailureMessage("Invalid HWID string '{0}'.", text);
				return TrippliteVersion.None;
			}
			text = text.Substring(num).ToLower();
		}
		string[] array = text.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length < 2)
		{
			WriteFailureMessage("The HWID '{0}' is malformed.", text);
			return TrippliteVersion.None;
		}
		string s = array[1].Substring(4);
		int num2 = -1;
		try
		{
			num2 = int.Parse(s);
		}
		catch
		{
			WriteFailureMessage("The HWID '{0}' is malformed; cannot obtain PID.", text);
			return TrippliteVersion.None;
		}
		if (num2 < 2009)
		{
			return TrippliteVersion.Prewatchdog;
		}
		return TrippliteVersion.Watchdog;
	}

	private void WriteFailureMessage(string fmt, params object[] stuff)
	{
		WriteFailureMessage(string.Format(fmt, stuff));
	}

	private void WriteFailureMessage(string msg)
	{
		string path = Program.MakeLogPath("InstallFailure.log", delete: false);
		using StreamWriter streamWriter = new StreamWriter(path);
		streamWriter.WriteLine(msg);
	}

	private void WriteFailMessages(List<string> msgs)
	{
		string path = Program.MakeLogPath("InstallFailure.log", delete: false);
		using StreamWriter streamWriter = new StreamWriter(path);
		foreach (string msg in msgs)
		{
			streamWriter.WriteLine(msg);
		}
	}
}
