using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using Microsoft.Win32;
using Redbox.HAL.Client;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace HALUtilities;

internal sealed class Program
{
	internal class InventoryLocationSorter : IComparer<InventoryHelper.Location>
	{
		public int Compare(InventoryHelper.Location x, InventoryHelper.Location y)
		{
			if (x.Deck > y.Deck)
			{
				return 1;
			}
			if (x.Deck < y.Deck)
			{
				return -1;
			}
			if (x.Slot == y.Slot)
			{
				return 0;
			}
			if (x.Slot <= y.Slot)
			{
				return -1;
			}
			return 1;
		}
	}

	private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";

	private const string ServiceName = "halsvc$default";

	private const int ServiceTimeout = 30000;

	private const string DuplicateCounterFile = "DuplicateCounter.log";

	private const string KioskLogs = "C:\\Program Files\\Redbox\\KioskLogs";

	private static readonly ManualResetEvent TimerWait = new ManualResetEvent(initialState: false);

	private static string m_kioskID = null;

	private static string TimeStamp => $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";

	private static string KioskID
	{
		get
		{
			if (!string.IsNullOrEmpty(m_kioskID))
			{
				return m_kioskID;
			}
			RegistryKey registryKey = null;
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
				if (registryKey == null)
				{
					m_kioskID = "UNKNOWN-ID";
				}
				else
				{
					string text = (string)registryKey.GetValue("ID");
					m_kioskID = ((!string.IsNullOrEmpty(text)) ? text : "UNKNOWN-ID");
				}
			}
			catch (Exception)
			{
				m_kioskID = "UNKNOWN-ID";
			}
			finally
			{
				registryKey?.Close();
			}
			return m_kioskID;
		}
	}

	internal static string ServicePath => "c:\\Program Files\\Redbox\\HALService\\bin";

	private static bool DeleteFile(string file)
	{
		if (!File.Exists(file))
		{
			return false;
		}
		try
		{
			File.Delete(file);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string MakeLogPath(string logname)
	{
		return Path.Combine(RuntimeService.Instance.AssemblyDirectory, logname);
	}

	private static string MakeLogPath(string logname, bool delete)
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, logname);
		if (delete && File.Exists(text))
		{
			try
			{
				File.Delete(text);
			}
			catch
			{
			}
		}
		return text;
	}

	private static void CheckQRStatus()
	{
		new QuickReturnHelper().ProperlyConfigured(ConfigureCommunication());
	}

	private static void UpdateQREEPROM(string sourcePath, int waitTime)
	{
		string path = MakeLogPath("updateEEprom.log", delete: true);
		HardwareService service = ConfigureCommunication();
		QuickReturnHelper quickReturnHelper = new QuickReturnHelper();
		using StreamWriter streamWriter = new StreamWriter(path);
		if (!quickReturnHelper.ProperlyConfigured(service, streamWriter))
		{
			streamWriter.WriteLine("ERROR: Can't update Quick-Return because its not installed or service is off");
			return;
		}
		try
		{
			File.Delete(Path.Combine(ServicePath, "qr.state.bak"));
		}
		catch (Exception ex)
		{
			streamWriter.WriteLine("ERROR: Can't update because the backup defs file couldn't be deleted. Exception: {0}", ex.Message);
		}
		if (!string.IsNullOrEmpty(sourcePath))
		{
			string text = Path.Combine(ServicePath, "qr-chamfer");
			try
			{
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				string[] files = Directory.GetFiles(sourcePath, "*.grf");
				if (files.Length <= 0)
				{
					streamWriter.WriteLine("ERROR: There are no files to copy.");
					return;
				}
				if (files.Length < 8)
				{
					streamWriter.WriteLine("Warning: did we get all the files we need?");
				}
				streamWriter.WriteLine("Stopping Service.");
				if (!ServiceHelper.Instance.StopHALService(check: true))
				{
					streamWriter.WriteLine("ERROR: Failed to stop service, ending update.");
					return;
				}
				string[] array = files;
				foreach (string text2 in array)
				{
					string text3 = Path.Combine(text, Path.GetFileName(text2));
					streamWriter.WriteLine("Copying {0} to {1}", text2, text3);
					File.Copy(text2, text3, overwrite: true);
				}
			}
			catch (Exception ex2)
			{
				streamWriter.WriteLine("ERROR: Failed to copy files" + ex2.Message);
				return;
			}
			streamWriter.WriteLine("Starting Service.");
			if (!ServiceHelper.Instance.StartHALService())
			{
				streamWriter.WriteLine("ERROR: Failed to start service, ending update.");
				return;
			}
			Thread.Sleep(180000);
		}
		quickReturnHelper.UpdateEEPROM(service, waitTime);
	}

	private static void WriteToOutput(string m)
	{
	}

	private static HardwareService ConfigureCommunication()
	{
		IPCProtocol iPCProtocol = null;
		HardwareService hardwareService = null;
		try
		{
			iPCProtocol = IPCProtocol.Parse("rcp://127.0.0.1:7001");
			hardwareService = new HardwareService(iPCProtocol);
			if (hardwareService == null)
			{
				WriteToOutput("Unable to create a hardware service.");
				return hardwareService;
			}
			if (hardwareService.ExecuteImmediate("NOP", out var _).Success)
			{
				WriteToOutput("Hardware service initialized.");
			}
			else
			{
				WriteToOutput("Failed to communicate with HAL.");
			}
			return hardwareService;
		}
		catch (UriFormatException)
		{
			WriteToOutput("Unable to parse communication protocol.");
			return null;
		}
	}

	private static bool RunImmediateInstruction(string inst)
	{
		HardwareJob job;
		return ConfigureCommunication()?.ExecuteImmediate(inst, out job).Success ?? false;
	}

	private static void DumpFilesAndRecurse(string directory, TextWriter manifestFile, int trim)
	{
		string[] files = Directory.GetFiles(directory);
		string[] array = files;
		foreach (string text in array)
		{
			string value = text.Remove(0, trim);
			manifestFile.WriteLine(value);
		}
		string[] directories = Directory.GetDirectories(directory);
		string[] array2 = directories;
		foreach (string directory2 in array2)
		{
			DumpFilesAndRecurse(directory2, manifestFile, trim);
		}
	}

	private static string MakeNewFile(string fileName)
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, fileName);
		if (File.Exists(text))
		{
			DeleteFile(text);
		}
		return text;
	}

	private static bool InventoryItemsEqual(IConsole console, ProgramResult result)
	{
		if (result.Code != "InventoryState")
		{
			console.WriteLine("The result code {0} is not recognized.", result.Code);
			return false;
		}
		if (result.ItemID.ToString() == result.PreviousItem.ToString())
		{
			return true;
		}
		console.WriteLine("Sync at deck {0} slot {1} went from {2} -> {3}", result.Deck, result.Slot, result.PreviousItem.ToString(), result.ItemID.ToString());
		return false;
	}

	private static int GetValue(string s)
	{
		if (!int.TryParse(s, out var result))
		{
			return -1;
		}
		return result;
	}

	private static void ShutDownService(object source, ElapsedEventArgs e)
	{
		DateTime now = DateTime.Now;
		Console.WriteLine("{0} {1} Shut down service timer fired.", now.ToShortDateString(), now.ToShortTimeString());
		ServiceHelper.Instance.StopHALService(check: true);
		Thread.Sleep(10000);
		ServiceHelper.Instance.StartHALService();
		TimerWait.Set();
	}

	private static void PrintUsageAndExit(string[] secureOptions)
	{
		Console.WriteLine("InventoryHelper <-u> <-e> <-unknown> <-fixDuplicates> <-findDuplicates> <-fullSync> <-fullSyncWithReport> <-dumpInventory> <-findCameraErrors> <-importInventory fullpathtofile> <-updateConfiguration CONFIG:PROPERTY:NEWVALUE> <-help>");
		Console.WriteLine("  Where: ");
		Console.WriteLine("  \t\t-e syncs empties ");
		Console.WriteLine("  \t\t-unknown syncs unknowns ");
		Console.WriteLine("  \t\t-fullSync schedules a low-priority sync");
		Console.WriteLine("  \t\t-dumpInventory writes the inventory to the file \"inventory.xml\"");
		Console.WriteLine("  \t\t-inventorysnapshot   if specified, generates an inventory snapshot to a csv file in \"C:\\Program Files\\Redbox\\inventory-snapshot\"");
		Console.WriteLine("  \t\t-importInventory takes inventory from an XML file and copies it into inventory");
		Console.WriteLine("  \t\t-findCameraErrors searches the HAL logs for camera problems, and outputs the results to the file \"camera-errors.txt\"");
		Console.WriteLine("  \t\t-updateConfiguration CONFIG:OPTION:NEWVALUE:VALIDATE Update OPTION in configuration CONFIG to value NEWVALUE.");
		Console.WriteLine("  \t\t-updateConfigFile CONFIG:OPTION:NEWVALUE Update OPTION in configuration CONFIG to value NEWVALUE.");
		Console.WriteLine("  \t\t-removeOldFolders Remove old HAL install folders.");
		Console.WriteLine("  \t\t-fullSyncWithReport Schedules a low priority sync, and reports the job's results.");
		Console.WriteLine("  \t\t-restartQR Tries to restart the QR device.");
		Console.WriteLine("  \t\t-stopQR Tries to stop the QR device (does not remove it from configuration).");
		Console.WriteLine("  \t\t-checkQRStatus If the QR device is not configured and active, will generate a 'qr-status.log' file with any errors.");
		Console.WriteLine("  \t\t-version Report the tool's verison.");
		Console.WriteLine("  \t\t-checkDupCounter Generate a log file if the machine has counters.");
		Console.WriteLine("  \t\t-resetDupCounter Reset the duplicate barcode counter.");
		Console.WriteLine("  \t\t-dumpStats Dumps the captured statistics.");
		Console.WriteLine("  \t\t-dumpEmptyStuckUnknown Dumps the empty/stuck locations which caused UNKNOWNs.");
		Console.WriteLine("  \t\t-dumpQRStats Dumps the statistics for the QR device.");
		Console.WriteLine("  \t\t-grabQRCounters Queries the quick return counters and writes them to QRStats.log.");
		Console.WriteLine("  \t\t-updateReboot={time} Updates the reboot time and cycles the service.");
		Console.WriteLine("  \t\t-cleanupQRTools Cleans up old Quick Return tools.");
		Console.WriteLine("  \t\t-checkInventory={path} Checks HAL inventory against file specified.");
		Console.WriteLine("  \t\t-assemblyVersions Dumps the HAL assembly versions to the console.");
		Console.WriteLine("  \t\t-bmpconverter Specifies to run as bitmap converter; passes remaining arguments to converter.");
		Console.WriteLine("  \t\t-tsdriver  Writes the installed touch screen driver to the log 'tsdrivers.log'");
		foreach (string value in secureOptions)
		{
			Console.WriteLine(value);
		}
		Console.WriteLine("  \t\t-help print this message and exit.");
		Console.WriteLine(" At least one argument is expected; it's not recommended to specify -fullSync and another option.");
		Console.WriteLine(" Current version is {0}", typeof(Program).Assembly.GetName().Version);
		Environment.Exit(1);
	}

	private static void PrintSecureHelp()
	{
		string[] secureOptions = new string[10] { "  \t\t-installDefaultInventoryDB Backs up old inventory DB, and copies default.", "  \t\t-installDefaultCountersDB Backs up old counters DB, and copies default.", "  \t\t-updateQREEPROM:\"<c:\asdf>\") Updates the images on the QR device, path is optional, if not provided it will use the assembly/qr-chamfer.", "  \t\t-configureDoorSensors:on|off Updates HAL to include/exclude door sensors.", "  \t\t-configureIRCamera:on|off Tells HAL the kiosk has/doesn't have an IR camera.", "  \t\t-configureVMZ Configures the HAL instance for a VMZ machine.", "  \t\t-dumpTouchScreens Dumps touchscreen monitor info to TouchScreen.txt.", "  \t\t-backupConfig Backs up the hal.xml & gamp files.", "  \t\t-analyzeEmptyStuck:fullDump Analyzes empty/stuck in vend logs. If file is specified, will analyze just that file.", "  \t\t--computeHash=<vid>:<pid> Computes a secure CCR hash." };
		PrintUsageAndExit(secureOptions);
	}

	private static void MakeBackup()
	{
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "backup-log.txt");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, KioskID);
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
			}
			catch
			{
				streamWriter.WriteLine("Unable to create backup directory.");
				return;
			}
		}
		try
		{
			File.Copy("c:\\gamp\\SystemData.dat", Path.Combine(text, "SystemData.dat"), overwrite: true);
			File.Copy("c:\\gamp\\SlotData.dat", Path.Combine(text, "SlotData.dat"), overwrite: true);
			File.Copy("c:\\Program Files\\Redbox\\HALService\\bin\\HAL.xml", Path.Combine(text, "HAL.xml"), overwrite: true);
		}
		catch
		{
			streamWriter.WriteLine("Error backing up file.");
		}
	}

	private static string FormatTime(DateTime t)
	{
		return $"{t.ToShortDateString()} {t.ToShortTimeString()}";
	}

	private static void DumpDevices(DeviceClass clazz)
	{
		string path = MakeLogPath((clazz == DeviceClass.None) ? "AllDevices.txt" : (clazz.ToString() + ".txt"), delete: true);
		using StreamWriter writer = new StreamWriter(path);
		DeviceHelper deviceHelper = new DeviceHelper();
		deviceHelper.EnumDevices(clazz, writer);
	}

	private static string ComputeNewFile()
	{
		int num = 1;
		DateTime now = DateTime.Now;
		string text;
		string path;
		do
		{
			text = $"{KioskID}-{now.Month}-{now.Day}-{now.Year}-{num++}.csv";
			path = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", text);
		}
		while (File.Exists(path));
		return text;
	}

	private static bool InstallDefault(TextWriter log, string database)
	{
		if (!ServiceHelper.Instance.StopHALService(check: true))
		{
			log.WriteLine("Unable stop the HAL servcie.");
			return false;
		}
		string text = Path.Combine(ServicePath, $"{database}.default");
		if (!File.Exists(text))
		{
			log.WriteLine("The source default file '{0}' doesn't exist.", text);
			ServiceHelper.Instance.StartHALService();
			return false;
		}
		string text2 = Path.Combine(ServicePath, $"data\\{database}");
		if (!File.Exists(text2))
		{
			log.WriteLine("The target database '{0}' doesn't exist.", text2);
			ServiceHelper.Instance.StartHALService();
			return false;
		}
		try
		{
			FileHelper.CreateBackup(text2, BackupAction.Move);
		}
		catch (Exception ex)
		{
			log.WriteLine("Unable to create backup of {0}: {1}", text2, ex.Message);
			return false;
		}
		if (!FileHelper.SafeCopy(log, text, text2))
		{
			return false;
		}
		return ServiceHelper.Instance.StartHALService();
	}

	private static void Main(string[] args)
	{
		bool flag = false;
		bool fixDuplicates = false;
		string text = null;
		bool flag2 = false;
		string text2 = string.Empty;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		string[] secureOptions = new string[0];
		int waitTime = 300000;
		if (args.Length == 0)
		{
			PrintUsageAndExit(secureOptions);
		}
		for (int i = 0; i < args.Length; i++)
		{
			if ("-bmpconverter" == args[i])
			{
				string[] array = new string[args.Length - 1];
				for (int j = 1; j < args.Length; j++)
				{
					array[j] = args[j];
				}
				new ConversionController().Execute(array);
				return;
			}
			if (args[i].StartsWith("--findUPSWithAtLeastPID"))
			{
				string optionVal = CommandLineOption.GetOptionVal(args[i], "=", string.Empty);
				if (string.IsNullOrEmpty(optionVal))
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				string[] array2 = optionVal.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length != 2)
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				List<string> errors = new List<string>();
				List<string> list = new List<string>();
				DeviceHelper deviceHelper = new DeviceHelper();
				deviceHelper.FindDeviceWithPid(DeviceClass.HIDClass, array2[0], array2[1], MatchType.GTEPid, errors, list);
				if (list.Count == 0)
				{
					return;
				}
				string path = MakeLogPath("UPSUpgradeneeded.log", delete: true);
				using StreamWriter streamWriter = new StreamWriter(path);
				streamWriter.WriteLine("The following devices matched the criteria vid = {0} && pid = {1}", array2[0], array2[1]);
				foreach (string item in list)
				{
					streamWriter.WriteLine(item);
				}
				return;
			}
			if (args[i].StartsWith("--computeHash"))
			{
				string optionVal2 = CommandLineOption.GetOptionVal(args[i], "=", string.Empty);
				if (string.IsNullOrEmpty(optionVal2))
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				string[] array3 = optionVal2.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (array3.Length != 2)
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				string s = $"{array3[0]},{array3[1]}";
				SHA256Managed sHA256Managed = new SHA256Managed();
				Console.WriteLine(Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(s)))));
				return;
			}
			if (args[i] == "-interactive")
			{
				int num = args.Length - (i + 1);
				string[] array4 = new string[num];
				Array.Copy(args, i + 1, array4, 0, array4.Length);
				using ScriptMode scriptMode = new ScriptMode();
				scriptMode.Run(array4);
				return;
			}
			if (args[i] == "-resetProteus")
			{
				bool flag6 = false;
				using (DeviceHelper deviceHelper2 = new DeviceHelper())
				{
					DeviceDescriptor descriptor = new DeviceDescriptor("1589", "a001", "Proteus XES");
					flag6 = deviceHelper2.ResetDriver(descriptor);
				}
				Environment.Exit((!flag6) ? 2 : 0);
				continue;
			}
			if (args[i].StartsWith("-analyzeSync"))
			{
				string optionVal3 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
				SyncCheck syncCheck = new SyncCheck(optionVal3);
				syncCheck.Summarize();
				return;
			}
			if (args[i].StartsWith("-analyzeRunningSync"))
			{
				using (DefaultConsole defaultConsole = new DefaultConsole())
				{
					string optionVal4 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
					if (optionVal4 == "EMPTY")
					{
						defaultConsole.WriteLine("Invalid job id specified.");
						return;
					}
					ClientHelper clientHelper = new ClientHelper();
					HardwareService hardwareService = clientHelper.AutoConnect();
					if (!hardwareService.GetJobs(out var jobs).Success)
					{
						defaultConsole.WriteLine("Unable to get job list.");
						return;
					}
					HardwareJob hardwareJob = null;
					HardwareJob[] array5 = jobs;
					foreach (HardwareJob hardwareJob2 in array5)
					{
						if (hardwareJob2.ID == optionVal4)
						{
							hardwareJob = hardwareJob2;
							break;
						}
					}
					if (hardwareJob == null)
					{
						defaultConsole.WriteLine("Cannot find job {0}", optionVal4);
						return;
					}
					clientHelper.WaitForJob(hardwareJob, out var endStatus);
					if (endStatus != HardwareJobStatus.Completed)
					{
						defaultConsole.WriteLine("job {0} ended with status {1}", optionVal4, endStatus.ToString());
						return;
					}
					if (!hardwareJob.GetResults(out var results).Success)
					{
						defaultConsole.WriteLine("Can't get job results for {0}", optionVal4);
						return;
					}
					int num2 = 0;
					int num3 = 0;
					ProgramResult[] array6 = results;
					foreach (ProgramResult result in array6)
					{
						if (InventoryItemsEqual(defaultConsole, result))
						{
							num2++;
						}
						else
						{
							num3++;
						}
					}
					defaultConsole.WriteLine("There were:");
					defaultConsole.WriteLine("  {0} matches.", num2);
					defaultConsole.WriteLine("  {0} mismatches.", num3);
					if (num2 + num3 != 717)
					{
						defaultConsole.WriteLine("Wrong analysis.");
					}
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeEmptyStuck"))
			{
				bool optionVal5 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				EmptyStuckAnalyzer.Instance.AnalyzeByPattern(optionVal5);
				return;
			}
			if ("-installDefaultInventoryDB" == args[i])
			{
				string path2 = MakeLogPath("InventoryDefaultCopy.log", delete: true);
				using StreamWriter log = new StreamWriter(path2);
				InstallDefault(log, "HALData.vdb3");
				return;
			}
			if ("-installDefaultCountersDB" == args[i])
			{
				string path3 = MakeLogPath("CountersDefaultCopy.log", delete: true);
				using StreamWriter log2 = new StreamWriter(path3);
				InstallDefault(log2, "HALCounters.vdb3");
				return;
			}
			if ("-bm" == args[i])
			{
				string text3 = null;
				string text4 = null;
				int num4 = 0;
				i++;
				while (num4 < 2)
				{
					Console.WriteLine("Arg: {0}", args[i]);
					if (args[i].StartsWith("-startDir"))
					{
						text4 = args[++i];
						if (text4.EndsWith("\\"))
						{
							text4 = text4.Remove(text4.Length - 1, 1);
						}
						Console.WriteLine("Startdirectory: {0}", text4);
					}
					else if (args[i].StartsWith("-output"))
					{
						text3 = args[++i];
						Console.WriteLine("Outfile: {0}", text3);
					}
					num4++;
					i++;
				}
				if (string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4))
				{
					Console.WriteLine("Required options for build manifest are not specified.");
					return;
				}
				int trim = text4.Length + 1;
				using StreamWriter manifestFile = new StreamWriter(text3);
				DumpFilesAndRecurse(text4, manifestFile, trim);
				return;
			}
			if (args[i].StartsWith("-assemblyVersions"))
			{
				string path4 = "c:\\Program Files\\Redbox\\HALService\\bin";
				List<string> list2 = new List<string>();
				list2.AddRange(Directory.GetFiles(path4, "*.dll"));
				string[] files = Directory.GetFiles(path4, "*.exe");
				string[] array7 = files;
				foreach (string text5 in array7)
				{
					if (!text5.Contains("Redbox.HAL.AssemblyVersionDumper.exe"))
					{
						list2.Add(text5);
					}
				}
				string optionVal6 = CommandLineOption.GetOptionVal(args[i], "NONE");
				if (optionVal6 != "NONE")
				{
					string path5 = MakeLogPath(optionVal6, delete: true);
					using StreamWriter streamWriter2 = new StreamWriter(path5);
					streamWriter2.WriteLine("Date: {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
					foreach (string item2 in list2)
					{
						try
						{
							AssemblyName assemblyName = AssemblyName.GetAssemblyName(item2);
							streamWriter2.WriteLine("Assembly {0} has version {1}", item2, assemblyName.Version.ToString());
						}
						catch (Exception ex)
						{
							streamWriter2.WriteLine("Unable to get version for {0}; message = {1}", item2, ex.Message);
						}
					}
					return;
				}
				{
					foreach (string item3 in list2)
					{
						try
						{
							AssemblyName assemblyName2 = AssemblyName.GetAssemblyName(item3);
							Console.WriteLine("Assembly {0} has version {1}", item3, assemblyName2.Version.ToString());
						}
						catch (Exception ex2)
						{
							Console.WriteLine("Unable to get version for {0}; message = {1}", item3, ex2.Message);
						}
					}
					return;
				}
			}
			if ("-dumpStats" == args[i])
			{
				RunImmediateInstruction("STATS LOG");
				return;
			}
			if (args[i].StartsWith("-qrUpdateWait"))
			{
				waitTime = CommandLineOption.GetOptionVal(args[i], 300000);
				continue;
			}
			if (args[i].StartsWith("-dumpTouchScreens"))
			{
				DumpDevices(DeviceClass.Monitor);
				return;
			}
			if (args[i].StartsWith("-dumpDevices"))
			{
				DumpDevices(DeviceClass.None);
				return;
			}
			if (args[i].StartsWith("-backupConfig"))
			{
				MakeBackup();
				return;
			}
			if (args[i].StartsWith("-configureVMZ"))
			{
				string path6 = MakeLogPath("configure-VMZ.txt");
				using StreamWriter streamWriter3 = new StreamWriter(File.Open(path6, FileMode.Append, FileAccess.Write, FileShare.Read));
				streamWriter3.WriteLine("The upgrade process is no longer supported.");
				return;
			}
			if (args[i] == "-restoreDefaultCameraPlugin")
			{
				RunImmediateInstruction("SETCFG \"CameraDriver\" \"Vidcap\" TYPE=Camera");
				ServiceHelper.Instance.StopHALService(check: true);
				Thread.Sleep(5000);
				ServiceHelper.Instance.StartHALService();
				return;
			}
			if (args[i].StartsWith("-configureDoorSensors"))
			{
				string optionValue = CommandLineOption.GetOptionValue(args[i]);
				if (string.IsNullOrEmpty(optionValue))
				{
					PrintSecureHelp();
				}
				string inst = $"DOORSENSORS SOFTWAREOVERRIDE OVERRIDE=\"{optionValue}\"";
				RunImmediateInstruction(inst);
				return;
			}
			if (args[i].Contains("-checkInventory"))
			{
				string optionValue2 = CommandLineOption.GetOptionValue(args[i], "=");
				string path7 = MakeLogPath("InventoryCompare.txt", delete: true);
				using StreamWriter log3 = new StreamWriter(path7);
				InventoryChecker.Instance.CheckInventory(ConfigureCommunication(), optionValue2, log3);
				return;
			}
			if (args[i].Contains("-updateReboot"))
			{
				string path8 = MakeLogPath("RebootTimeUpdate.txt", delete: true);
				using StreamWriter streamWriter4 = new StreamWriter(path8);
				string optionValue3 = CommandLineOption.GetOptionValue(args[i], "=");
				if (string.IsNullOrEmpty(optionValue3))
				{
					streamWriter4.WriteLine("Unable to read the update time from {0}", args[i]);
					return;
				}
				streamWriter4.WriteLine("Setting reboot time to '{0}'", optionValue3);
				RegistryOptions.Instance.SetSecretValue("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "RebootTime", optionValue3);
				streamWriter4.WriteLine("Stop service returned {0}", ServiceHelper.Instance.StopHALService(check: true));
				Thread.Sleep(5000);
				streamWriter4.WriteLine("Start service returned {0}", ServiceHelper.Instance.StartHALService());
				return;
			}
			if ("-dumpQRStats" == args[i])
			{
				RunImmediateInstruction("QUICKRETURN DUMPCOUNTERS");
				return;
			}
			if ("-dumpEmptyStuckUnknown" == args[i])
			{
				RunImmediateInstruction("STATS LOGUKNOWN");
				return;
			}
			if ("-resetDupCounter" == args[i])
			{
				HardwareService hardwareService2 = ConfigureCommunication();
				if (hardwareService2 != null)
				{
					new CounterHelper().ResetDuplicateCounterValue(hardwareService2);
				}
				return;
			}
			if ("-checkDupCounter" == args[i])
			{
				string path9 = MakeNewFile("DuplicateCounter.log");
				HardwareService hardwareService3 = ConfigureCommunication();
				if (hardwareService3 == null)
				{
					return;
				}
				CounterHelper counterHelper = new CounterHelper();
				counterHelper.GetDuplicateCounterValue(hardwareService3, out var val);
				if (val > 0)
				{
					using StreamWriter streamWriter5 = new StreamWriter(path9);
					streamWriter5.WriteLine("The kiosk has duplicates.");
					streamWriter5.Flush();
				}
				continue;
			}
			if (args[i].Equals("-restartQR"))
			{
				RunImmediateInstruction("QUICKRETURN RESTART");
				continue;
			}
			if (args[i].Equals("-stopQR"))
			{
				RunImmediateInstruction("QUICKRETURN HALTPOLL");
				continue;
			}
			if (args[i].StartsWith("-configureIRCamera"))
			{
				string optionValue4 = CommandLineOption.GetOptionValue(args[i]);
				if (!string.IsNullOrEmpty(optionValue4))
				{
					RunImmediateInstruction($"CAMERA CONFIGUREIR STATE=\"{optionValue4}\"");
				}
				return;
			}
			if (args[i].Equals("-checkQRStatus"))
			{
				CheckQRStatus();
				continue;
			}
			if (args[i].StartsWith("-updateQREEPROM"))
			{
				string optionValue5 = CommandLineOption.GetOptionValue(args[i]);
				UpdateQREEPROM(optionValue5, waitTime);
				return;
			}
			if (args[i].StartsWith("-grabQRCounters"))
			{
				bool optionVal7 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				new QuickReturnHelper().DumpCounters(ConfigureCommunication(), optionVal7);
				return;
			}
			if (args[i].StartsWith("-vendTest"))
			{
				int optionVal8 = CommandLineOption.GetOptionVal(args[i], 3);
				int? startAt = null;
				if (args.Length > 1 && args[i + 1].StartsWith("-next"))
				{
					startAt = CommandLineOption.GetOptionVal(args[i + 1], -1);
				}
				VendTest vendTest = new VendTest(optionVal8);
				using DefaultConsole console = new DefaultConsole();
				vendTest.Execute(console, startAt);
				return;
			}
			if (args[i].StartsWith("-syncTest"))
			{
				using (DefaultConsole defaultConsole2 = new DefaultConsole())
				{
					int num5 = 1000;
					string optionVal9 = CommandLineOption.GetOptionVal<string>(args[i], null);
					if (optionVal9 == null)
					{
						defaultConsole2.WriteLine("There is no specified barcode.");
						return;
					}
					if (args.Length > 1 && args[i + 1].StartsWith("-iter"))
					{
						num5 = CommandLineOption.GetOptionVal(args[i + 1], num5);
					}
					ClientHelper clientHelper2 = new ClientHelper();
					HardwareService hardwareService4 = clientHelper2.AutoConnect();
					HardwareCommandResult hardwareCommandResult = hardwareService4.ExecuteImmediate($"LOOKUP \"{optionVal9}\"", out var job);
					if (!hardwareCommandResult.Success)
					{
						defaultConsole2.WriteLine("Call failed!");
						return;
					}
					string value = null;
					job.Pop<string>(out value);
					if (value != "FOUND")
					{
						defaultConsole2.WriteLine("Could not find barcode {0}", optionVal9);
						return;
					}
					int value2 = 0;
					int value3 = 0;
					job.Pop<int>(out value2);
					job.Pop<int>(out value3);
					for (int l = 0; l < num5; l++)
					{
						HardwareCommandResult hardwareCommandResult2 = hardwareService4.ScheduleJob("sync-locations", $"Sync iter {l}", enableDebugging: false, new HardwareJobSchedule
						{
							Priority = HardwareJobPriority.Low,
							StartTime = DateTime.Now
						}, out var job2);
						if (hardwareCommandResult2.Success)
						{
							job2.Push(value3);
							job2.Push(value2);
							job2.Push(1);
							job2.Pend();
						}
						clientHelper2.WaitForJob(job2, out var endStatus2);
						if (endStatus2 != HardwareJobStatus.Completed)
						{
							defaultConsole2.WriteLine("Job ended with status {0}", endStatus2.ToString());
							return;
						}
						if (!job2.GetResults(out var results2).Success)
						{
							defaultConsole2.WriteLine("Could not get job results!");
							return;
						}
						ProgramResult[] array6 = results2;
						foreach (ProgramResult programResult in array6)
						{
							if (programResult.Code == "InventoryState")
							{
								if (programResult.ItemID.Barcode == null)
								{
									defaultConsole2.WriteLine("There was no barcode metadata.");
									return;
								}
								if (programResult.ItemID.Barcode != optionVal9)
								{
									defaultConsole2.WriteLine("Barcode mismatch - job failure.");
									return;
								}
							}
						}
						job2.Trash();
						if (l % 50 == 0)
						{
							hardwareService4.ExecuteServiceCommand("JOB collect-garbage force: true");
						}
					}
					defaultConsole2.WriteLine("Successfully completed {0} iterations.", num5);
				}
				continue;
			}
			if (args[i].StartsWith("-resetTS"))
			{
				CommandLineOption.GetOptionVal(args[i], 10);
				new TouchScreenHelper().Reset();
				return;
			}
			if (args[i].StartsWith("-tsMonitor"))
			{
				int optionVal10 = CommandLineOption.GetOptionVal(args[i], 20);
				using TouchScreenHelper touchScreenHelper = new TouchScreenHelper();
				touchScreenHelper.RunMonitor(optionVal10);
				return;
			}
			if (args[i].Equals("-simulateReboot"))
			{
				RedboxTimer redboxTimer = new RedboxTimer("Reboot simulator", ShutDownService);
				redboxTimer.ScheduleAtNext(1, 0);
				TimerWait.WaitOne();
				DateTime now = DateTime.Now;
				Console.WriteLine("{0} {1} Timer done.", now.ToShortDateString(), now.ToShortTimeString());
				continue;
			}
			if ("-e".Equals(args[i]))
			{
				MakeNewFile("empties.txt");
				InventoryHelper.Instance.FixType("empties.txt", "EMPTY", "EMPTY location sync.");
				return;
			}
			if ("-findDuplicates".Equals(args[i]))
			{
				flag = true;
				continue;
			}
			if ("-fixDuplicates".Equals(args[i]))
			{
				fixDuplicates = true;
				flag = true;
				continue;
			}
			if (args[i].StartsWith("-updateCameraPlugin"))
			{
				using (StreamWriter streamWriter6 = new StreamWriter(MakeLogPath("pluginupdate.log", delete: true)))
				{
					string optionVal11 = CommandLineOption.GetOptionVal(args[i], "NONE");
					if (optionVal11 == "NONE")
					{
						streamWriter6.WriteLine("The plugin name is not correctly specified.");
						return;
					}
					XmlDocument xmlDocument = new XmlDocument();
					string text6 = "c:\\Program Files\\Redbox\\halservice\\bin\\hal.xml";
					string path10 = FileHelper.CreateBackup(text6, BackupAction.Copy);
					bool flag7 = true;
					try
					{
						xmlDocument.Load(text6);
						if (xmlDocument.DocumentElement != null)
						{
							XmlNodeExtensions.SelectSingleNodeAndSetValue(xmlDocument.DocumentElement, "Camera/CameraDriver", optionVal11);
							xmlDocument.Save(text6);
						}
						return;
					}
					catch (Exception)
					{
						flag7 = false;
						return;
					}
					finally
					{
						if (flag7)
						{
							FileHelper.SafeDeleteFile(path10);
						}
					}
				}
			}
			if ("-unknown".Equals(args[i]))
			{
				string fileName = MakeNewFile("unknowns.txt");
				InventoryHelper.Instance.FixType(fileName, "UNKNOWN", "UNKNOWN inventory sync.");
				return;
			}
			if ("-fullSync".Equals(args[i]))
			{
				InventoryHelper.Instance.ScheduleFullSync();
				return;
			}
			if ("-dumpInventory".Equals(args[i]))
			{
				string path11 = MakeNewFile("inventory.xml");
				InventoryHelper.Instance.DumpInventoryToFile(path11);
				return;
			}
			if ("-secureHelp".Equals(args[i]))
			{
				PrintSecureHelp();
				return;
			}
			if ("-help".Equals(args[i]))
			{
				PrintUsageAndExit(secureOptions);
				continue;
			}
			if ("-importInventory".Equals(args[i]))
			{
				if (i >= args.Length - 1)
				{
					PrintUsageAndExit(secureOptions);
				}
				text = args[++i];
				if (InventoryHelper.Instance.ImportInventoryFromFile(text))
				{
					Console.WriteLine("Inventory import OK.");
				}
				else
				{
					Console.WriteLine("Failed to import Inventory!");
				}
				return;
			}
			if ("-findCameraErrors".Equals(args[i]))
			{
				string path12 = MakeNewFile("camera-errors.txt");
				using StreamWriter outputFile = new StreamWriter(path12);
				new CameraErrorFinder().FindErrors(InventoryHelper.Instance.Inventory, outputFile);
				return;
			}
			if ("-inventorysnapshot".Equals(args[i]))
			{
				flag2 = true;
				continue;
			}
			if ("-updateConfiguration".Equals(args[i]))
			{
				text2 = args[++i];
				flag3 = true;
				continue;
			}
			if ("-updateConfigFile".Equals(args[i]))
			{
				text2 = args[++i];
				flag4 = true;
				continue;
			}
			if ("-removeOldFolders".Equals(args[i]))
			{
				flag5 = true;
				continue;
			}
			if ("-version".Equals(args[i]))
			{
				string path13 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "version.txt");
				Version version = typeof(Program).Assembly.GetName().Version;
				using (StreamWriter streamWriter7 = new StreamWriter(path13))
				{
					streamWriter7.WriteLine(version);
				}
				Console.WriteLine("HALUtilities version: {0}", version);
				return;
			}
			if ("-dumpExcluded".Equals(args[i]))
			{
				string path14 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "excluded.log");
				List<Location> list3 = ConfigurationHelper.ExcludedSlots(InventoryHelper.Instance.Service);
				using StreamWriter streamWriter8 = new StreamWriter(path14);
				streamWriter8.WriteLine("There are {0} locations total.", list3.Count);
				foreach (Location item4 in list3)
				{
					streamWriter8.WriteLine("Excluded location deck = {0} slot = {1}", item4.Deck, item4.Slot);
				}
				return;
			}
			if (!("-tsdriver" == args[i]))
			{
				continue;
			}
			RegistryKey registryKey = null;
			string path15 = MakeLogPath("tsdrivers.log", delete: true);
			using StreamWriter streamWriter9 = new StreamWriter(path15);
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\3M Touch Systems, Inc.");
				string[] subKeyNames = registryKey.GetSubKeyNames();
				string[] array7 = subKeyNames;
				foreach (string value4 in array7)
				{
					streamWriter9.WriteLine(value4);
				}
			}
			catch
			{
			}
			finally
			{
				registryKey?.Close();
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			char c = ':';
			int num6 = text2.IndexOf('|');
			if (-1 != num6)
			{
				c = '|';
			}
			string[] array8 = text2.Split(c);
			if (array8.Length < 3)
			{
				Console.WriteLine("Unable to parse option {0}", text2);
				Environment.Exit(1);
			}
			string path16 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "ConfigurationUpdateError.log");
			string text7 = array8[0];
			string text8 = array8[1];
			string text9 = array8[2];
			bool flag8 = array8.Length == 4;
			if (flag3)
			{
				if (File.Exists(path16))
				{
					try
					{
						File.Delete(path16);
					}
					catch
					{
					}
				}
				HardwareService hardwareService5 = ConfigureCommunication();
				if (hardwareService5 == null)
				{
					using StreamWriter streamWriter10 = new StreamWriter(path16);
					streamWriter10.WriteLine("Unable to talk to HAL.");
					return;
				}
				HardwareCommandResult hardwareCommandResult3 = hardwareService5.ExecuteImmediate($"SETCFG \"{text8}\" \"{text9}\" TYPE={text7}", out var _);
				if (!hardwareCommandResult3.Success)
				{
					using StreamWriter streamWriter11 = new StreamWriter(path16);
					streamWriter11.WriteLine("The configuration update failed.");
					foreach (Error error in hardwareCommandResult3.Errors)
					{
						streamWriter11.WriteLine(error.ToString());
					}
					return;
				}
				if (flag8)
				{
					string hALConfiguration = InventoryHelper.Instance.GetHALConfiguration(text7);
					string path17 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "ConfigurationUpdateValidateError.log");
					if (File.Exists(path17))
					{
						try
						{
							File.Delete(path17);
						}
						catch
						{
						}
					}
					XmlDocument xmlDocument2 = new XmlDocument();
					xmlDocument2.LoadXml(hALConfiguration);
					XmlNodeList xmlNodeList = xmlDocument2.DocumentElement.SelectNodes("property");
					foreach (XmlNode item5 in xmlNodeList)
					{
						XmlNode namedItem = item5.Attributes.GetNamedItem("name");
						if (!namedItem.Value.Equals(text8, StringComparison.CurrentCultureIgnoreCase))
						{
							continue;
						}
						string value5 = item5.Attributes["value"].Value;
						if (!value5.Equals(text9, StringComparison.CurrentCultureIgnoreCase))
						{
							using StreamWriter streamWriter12 = new StreamWriter(path17);
							streamWriter12.WriteLine($"The new value {value5} doesn't match {text9}");
						}
					}
				}
			}
			else if (flag4)
			{
				string path18 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "UpdateConfigurationFileError.log");
				string text10 = Path.Combine(ServicePath, "HAL.xml");
				string text11 = text10 + ".bak";
				if (!File.Exists(text10))
				{
					using StreamWriter streamWriter13 = new StreamWriter(path18);
					streamWriter13.WriteLine($"Unable to find configuration file \"{text10}\"");
				}
				else
				{
					if (File.Exists(text11))
					{
						try
						{
							File.Delete(text11);
						}
						catch
						{
						}
					}
					try
					{
						File.Copy(text10, text11);
					}
					catch
					{
					}
					XmlDocument xmlDocument3 = new XmlDocument();
					xmlDocument3.Load(text10);
					XmlNode xmlNode2 = xmlDocument3.DocumentElement.SelectSingleNode($"{text7}/{text8}");
					if (xmlNode2 != null)
					{
						xmlNode2.InnerText = text9;
						xmlDocument3.Save(text10);
					}
					else
					{
						using StreamWriter streamWriter14 = new StreamWriter(path18);
						streamWriter14.WriteLine($"Unable to find configuration \"{text7}\"");
					}
				}
			}
		}
		if (flag5)
		{
			string path19 = "c:\\Program Files\\Redbox";
			string[] directories = Directory.GetDirectories(path19, "Hardware Abstraction Layer (HAL)*");
			string[] array7 = directories;
			foreach (string text12 in array7)
			{
				string path20 = Path.Combine(text12, "bin\\halsvc-win32.exe");
				if (!File.Exists(path20))
				{
					try
					{
						Directory.Delete(text12, recursive: true);
					}
					catch
					{
					}
				}
			}
		}
		if (flag)
		{
			string outFile = MakeNewFile("duplicates.txt");
			InventoryHelper.Instance.FindDuplicates(fixDuplicates, outFile);
		}
		if (!flag2)
		{
			return;
		}
		if (!Directory.Exists("C:\\Program Files\\Redbox\\inventory-snapshots"))
		{
			try
			{
				Directory.CreateDirectory("C:\\Program Files\\Redbox\\inventory-snapshots");
			}
			catch (Exception ex4)
			{
				Console.WriteLine("Unable to create directory: {0}", ex4.Message);
			}
		}
		string path21 = ComputeNewFile();
		List<InventoryHelper.Location> inventory = InventoryHelper.Instance.Inventory;
		inventory.Sort(new InventoryLocationSorter());
		string path22 = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", path21);
		using StreamWriter streamWriter15 = new StreamWriter(path22);
		streamWriter15.WriteLine("Deck,Slot,Barcode");
		foreach (InventoryHelper.Location item6 in inventory)
		{
			streamWriter15.WriteLine($"{item6.Deck},{item6.Slot},'{item6.ID}'");
		}
	}
}
