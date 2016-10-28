using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework;

namespace HALUtilities;

internal class TrippliteUtilities : IDisposable
{
	private const string TrippliteVID = "09ae";

	private const int TrippliteMinWatchdogPID = 2009;

	private const string BaseInstallerPath = "c:\\Drivers\\Redbox\\UPS";

	private const string NonWatchdogInstaller = "Automated Tripplite Install _Non Watchdog (HW ID _ PID Less than 2009)\\Install.bat";

	private bool m_disposed;

	private readonly string[] InstallerFiles;

	private readonly bool m_debug;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
		}
	}

	internal void Install(TrippliteVersion version)
	{
		string path = Program.MakeLogPath("InstallFailure.log", delete: false);
		using StreamWriter streamWriter = new StreamWriter(path);
		if (version == TrippliteVersion.None)
		{
			streamWriter.WriteLine("Could not find a Tripplite device; bailing install.");
			return;
		}
		string text = ComputeInstaller(version);
		if (!File.Exists(text))
		{
			Extract(streamWriter);
			if (!File.Exists(text))
			{
				streamWriter.WriteLine("There is no install file. Sorry, I really tried, but I just can't do anything right.");
				return;
			}
		}
		Process process = null;
		try
		{
			process = Process.Start(text);
			process.WaitForExit();
			string path2 = Program.MakeLogPath("TrippliteInstall.log", delete: true);
			using StreamWriter streamWriter2 = new StreamWriter(path2);
			streamWriter2.WriteLine("The install script exited with code {0}", process.ExitCode);
		}
		catch (Exception ex)
		{
			streamWriter.WriteLine("Install failed; exception = {0}.", ex.Message);
		}
	}

	internal TrippliteVersion FindTripplite(IConsole console)
	{
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		using (IUsbDeviceSearchResult usbDeviceSearchResult = service.FindVendorDevices("09ae"))
		{
			if (usbDeviceSearchResult.Found)
			{
				using IEnumerator<IDeviceDescriptor> enumerator = usbDeviceSearchResult.Matches.GetEnumerator();
				if (enumerator.MoveNext())
				{
					IDeviceDescriptor current = enumerator.Current;
					if (m_debug)
					{
						console.WriteLine("Examine vendor {0} product {1}", current.Vendor, current.Product);
					}
					int num = -1;
					try
					{
						num = int.Parse(current.Product);
						return (num < 2009) ? TrippliteVersion.Prewatchdog : TrippliteVersion.Watchdog;
					}
					catch
					{
						console.WriteLine("The product '{0}' is malformed; cannot obtain PID.", current.Product);
						return TrippliteVersion.None;
					}
				}
			}
		}
		return TrippliteVersion.None;
	}

	internal void Extract(TextWriter writer)
	{
		string text = Path.Combine("c:\\Drivers\\Redbox\\UPS", "Tripplite.zip");
		if (!File.Exists(text))
		{
			writer.WriteLine("There are no installer and/or zip files to explode. Sorry, I can't do anything right.");
			return;
		}
		ZipFileManager zipFileManager = new ZipFileManager(text);
		zipFileManager.Unzip("c:\\Drivers\\Redbox\\UPS");
	}

	internal TrippliteUtilities(bool debug)
	{
		m_debug = debug;
		InstallerFiles = new string[2] { "Tripplite_DAT+SFW+COM_Ver12.04.0058\\Tripplite_Software_Install_v6.bat", "Tripplite_DAT+SFW+COM_Ver12.04.0053\\Tripplite_Software_Install_v6.bat" };
	}

	private string ComputeInstaller(TrippliteVersion version)
	{
		if (TrippliteVersion.Prewatchdog == version)
		{
			return Path.Combine("c:\\Drivers\\Redbox\\UPS", "Automated Tripplite Install _Non Watchdog (HW ID _ PID Less than 2009)\\Install.bat");
		}
		string[] installerFiles = InstallerFiles;
		foreach (string path in installerFiles)
		{
			string text = Path.Combine("c:\\Drivers\\Redbox\\UPS", path);
			if (File.Exists(text))
			{
				return text;
			}
		}
		return string.Empty;
	}
}
