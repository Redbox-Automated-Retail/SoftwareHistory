using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
	private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";

	private const string ServiceName = "halsvc$default";

	private const int ServiceTimeout = 30000;

	private const string DuplicateCounterFile = "DuplicateCounter.log";

	private const string KioskLogs = "C:\\Program Files\\Redbox\\KioskLogs";

	private static readonly ManualResetEvent TimerWait = new ManualResetEvent(initialState: false);

	private static readonly ManualResetEvent GenericWaiter = new ManualResetEvent(initialState: false);

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

	internal static string MakeLogPath(string logname)
	{
		return Path.Combine(RuntimeService.Instance.AssemblyDirectory, logname);
	}

	internal static string MakeLogPath(string logname, bool delete)
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
		ClientHelper clientHelper = new ClientHelper();
		HardwareService service = clientHelper.AutoConnect();
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
				if (!ServiceHelper.StopHALService(check: true))
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
			if (!ServiceHelper.StartHALService())
			{
				streamWriter.WriteLine("ERROR: Failed to start service, ending update.");
				return;
			}
			Thread.Sleep(180000);
		}
		quickReturnHelper.UpdateEEPROM(clientHelper, waitTime);
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
		ServiceHelper.StopHALService(check: true);
		Thread.Sleep(10000);
		ServiceHelper.StartHALService();
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

	private static void GenericimerFired(object source, ElapsedEventArgs e)
	{
		GenericWaiter.Set();
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
		if (!ServiceHelper.StopHALService(check: true))
		{
			log.WriteLine("Unable stop the HAL servcie.");
			return false;
		}
		string text = Path.Combine(ServicePath, $"{database}.default");
		if (!File.Exists(text))
		{
			log.WriteLine("The source default file '{0}' doesn't exist.", text);
			ServiceHelper.StartHALService();
			return false;
		}
		string text2 = Path.Combine(ServicePath, $"data\\{database}");
		if (!File.Exists(text2))
		{
			log.WriteLine("The target database '{0}' doesn't exist.", text2);
			ServiceHelper.StartHALService();
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
		return ServiceHelper.StartHALService();
	}

	private static void Main(string[] args)
	{
		string text = null;
		string text2 = string.Empty;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		string[] secureOptions = new string[0];
		int waitTime = 300000;
		if (args.Length == 0)
		{
			PrintUsageAndExit(secureOptions);
		}
		string[] array7;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("-genStats"))
			{
				string optionVal = CommandLineOption.GetOptionVal(args[i], string.Empty);
				if (string.IsNullOrEmpty(optionVal))
				{
					return;
				}
				using DefaultConsole console = new DefaultConsole();
				DecodeParser decodeParser = new DecodeParser();
				decodeParser.ParseStats(optionVal, console);
				decodeParser.DumpScanTimes(console);
				return;
			}
			if ("-resetPDCMCounter" == args[i])
			{
				Console.WriteLine("Resetting PDCM state ...");
				RegistryStore registryStore = new RegistryStore("SOFTWARE\\Redbox\\HAL");
				registryStore.SetSecretValue("PDCMUpdateCount", 0);
				int secretValue = registryStore.GetSecretValue("PDCMUpdateCount", -1);
				Console.WriteLine("PDCM state now at {0}", secretValue);
				return;
			}
			if ("-kioskTest" == args[i])
			{
				string[] array = new string[args.Length - 1];
				Array.Copy(args, 1, array, 0, array.Length);
				int exitCode = -1;
				using (KioskTest kioskTest = new KioskTest(array))
				{
					exitCode = kioskTest.Run();
				}
				Environment.Exit(exitCode);
			}
			if ("-fullSyncAt" == args[i])
			{
				RedboxTimer redboxTimer = new RedboxTimer("Sync", GenericimerFired);
				using DefaultConsole defaultConsole = new DefaultConsole();
				redboxTimer.ScheduleAtNext(2, 45);
				GenericWaiter.WaitOne();
				defaultConsole.WriteLine("Scheduling sync.");
				ClientHelper clientHelper = new ClientHelper();
				HardwareService hardwareService = clientHelper.AutoConnect();
				hardwareService.FullSync();
				return;
			}
			if ("-bmpconverter" == args[i])
			{
				string[] array2 = new string[args.Length - 1];
				for (int j = 1; j < args.Length; j++)
				{
					array2[j] = args[j];
				}
				new ConversionController().Execute(array2);
				return;
			}
			if ("-return" == args[i])
			{
				bool flag4 = true;
				using DefaultConsole defaultConsole2 = new DefaultConsole();
				ClientHelper clientHelper2 = new ClientHelper();
				HardwareService hardwareService2 = clientHelper2.AutoConnect();
				while (flag4)
				{
					flag4 = hardwareService2.Return(new HardwareJobSchedule
					{
						Priority = HardwareJobPriority.Highest
					}, out var job).Success;
					if (!flag4)
					{
						continue;
					}
					clientHelper2.WaitForJob(job, out var _);
					job.GetResults(out var results);
					ProgramResult[] array3 = results;
					foreach (ProgramResult programResult in array3)
					{
						if (programResult.Code == "EmptySlotCount")
						{
							int num = int.Parse(programResult.Message);
							if (num == 1)
							{
								defaultConsole2.WriteLine("There are no more empty slots.");
								flag4 = false;
							}
						}
					}
				}
				return;
			}
			if (args[i].StartsWith("--findUPSWithAtLeastPID"))
			{
				string optionVal2 = CommandLineOption.GetOptionVal(args[i], "=", string.Empty);
				if (string.IsNullOrEmpty(optionVal2))
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				string[] array4 = optionVal2.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (array4.Length != 2)
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				List<string> errors = new List<string>();
				List<string> list = new List<string>();
				DeviceHelper deviceHelper = new DeviceHelper();
				deviceHelper.FindDeviceWithPid(DeviceClass.HIDClass, array4[0], array4[1], MatchType.GTEPid, errors, list);
				if (list.Count == 0)
				{
					return;
				}
				string path = MakeLogPath("UPSUpgradeneeded.log", delete: true);
				using StreamWriter streamWriter = new StreamWriter(path);
				streamWriter.WriteLine("The following devices matched the criteria vid = {0} && pid = {1}", array4[0], array4[1]);
				foreach (string item in list)
				{
					streamWriter.WriteLine(item);
				}
				return;
			}
			if (args[i] == "-installTripplite")
			{
				TrippliteInstaller trippliteInstaller = new TrippliteInstaller();
				TrippliteVersion trippliteVersion = trippliteInstaller.FindTripplite();
				if (trippliteVersion != TrippliteVersion.None)
				{
					trippliteInstaller.Install(trippliteVersion);
				}
				return;
			}
			if (args[i] == "-extractTrippliteInstaller")
			{
				TrippliteInstaller trippliteInstaller2 = new TrippliteInstaller();
				trippliteInstaller2.Extract();
				return;
			}
			if (args[i] == "-interactive")
			{
				int num2 = args.Length - (i + 1);
				string[] array5 = new string[num2];
				Array.Copy(args, i + 1, array5, 0, array5.Length);
				using ScriptMode scriptMode = new ScriptMode();
				scriptMode.Run(array5);
				return;
			}
			if (args[i] == "-resetProteus")
			{
				bool flag5 = false;
				using (DeviceHelper deviceHelper2 = new DeviceHelper())
				{
					DeviceDescriptor descriptor = new DeviceDescriptor("1589", "a001", "Proteus XES");
					flag5 = deviceHelper2.ResetDriver(descriptor);
				}
				Environment.Exit((!flag5) ? 2 : 0);
				continue;
			}
			if (args[i].StartsWith("-resetDeck"))
			{
				string optionVal3 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				string path2 = MakeLogPath("resetdecks.txt");
				using (StreamWriter streamWriter2 = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					if (string.IsNullOrEmpty(optionVal3))
					{
						streamWriter2.WriteLine("Invalid deck list specified.");
						return;
					}
					StringBuilder stringBuilder = new StringBuilder();
					char[] separator = new char[1] { ',' };
					string[] array6 = optionVal3.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					array7 = array6;
					foreach (string arg in array7)
					{
						streamWriter2.WriteLine(" Resetting deck {0}", arg);
						stringBuilder.AppendLine($" DECK RESET NUMBER=\"{arg}\"");
					}
					ClientHelper clientHelper3 = new ClientHelper();
					HardwareService hardwareService3 = clientHelper3.AutoConnect();
					if (hardwareService3 == null)
					{
						streamWriter2.WriteLine("Cannot connect to service. Please try again.");
						return;
					}
					hardwareService3.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeSync"))
			{
				string optionVal4 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
				SyncCheck syncCheck = new SyncCheck(optionVal4);
				syncCheck.Summarize();
				return;
			}
			if (args[i].StartsWith("-analyzeRunningSync"))
			{
				using (DefaultConsole defaultConsole3 = new DefaultConsole())
				{
					string optionVal5 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
					if (optionVal5 == "EMPTY")
					{
						defaultConsole3.WriteLine("Invalid job id specified.");
						return;
					}
					ClientHelper clientHelper4 = new ClientHelper();
					HardwareService hardwareService4 = clientHelper4.AutoConnect();
					if (!hardwareService4.GetJobs(out var jobs).Success)
					{
						defaultConsole3.WriteLine("Unable to get job list.");
						return;
					}
					HardwareJob hardwareJob = null;
					HardwareJob[] array8 = jobs;
					foreach (HardwareJob hardwareJob2 in array8)
					{
						if (hardwareJob2.ID == optionVal5)
						{
							hardwareJob = hardwareJob2;
							break;
						}
					}
					if (hardwareJob == null)
					{
						defaultConsole3.WriteLine("Cannot find job {0}", optionVal5);
						return;
					}
					clientHelper4.WaitForJob(hardwareJob, out var endStatus2);
					if (endStatus2 != HardwareJobStatus.Completed)
					{
						defaultConsole3.WriteLine("job {0} ended with status {1}", optionVal5, endStatus2.ToString());
						return;
					}
					if (!hardwareJob.GetResults(out var results2).Success)
					{
						defaultConsole3.WriteLine("Can't get job results for {0}", optionVal5);
						return;
					}
					int num3 = 0;
					int num4 = 0;
					ProgramResult[] array3 = results2;
					foreach (ProgramResult result in array3)
					{
						if (InventoryItemsEqual(defaultConsole3, result))
						{
							num3++;
						}
						else
						{
							num4++;
						}
					}
					defaultConsole3.WriteLine("There were:");
					defaultConsole3.WriteLine("  {0} matches.", num3);
					defaultConsole3.WriteLine("  {0} mismatches.", num4);
					if (num3 + num4 != 717)
					{
						defaultConsole3.WriteLine("Wrong analysis.");
					}
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeEmptyStuck"))
			{
				bool optionVal6 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				EmptyStuckAnalyzer.Instance.AnalyzeByPattern(optionVal6);
				return;
			}
			if ("-installDefaultInventoryDB" == args[i])
			{
				string path3 = MakeLogPath("InventoryDefaultCopy.log", delete: true);
				using StreamWriter log = new StreamWriter(path3);
				InstallDefault(log, "HALData.vdb3");
				return;
			}
			if ("-installDefaultCountersDB" == args[i])
			{
				string path4 = MakeLogPath("CountersDefaultCopy.log", delete: true);
				using StreamWriter log2 = new StreamWriter(path4);
				InstallDefault(log2, "HALCounters.vdb3");
				return;
			}
			if ("-bm" == args[i])
			{
				string text3 = null;
				string text4 = null;
				int num5 = 0;
				i++;
				while (num5 < 2)
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
					num5++;
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
				string path5 = "c:\\Program Files\\Redbox\\HALService\\bin";
				List<string> list2 = new List<string>();
				list2.AddRange(Directory.GetFiles(path5, "*.dll"));
				Directory.GetFiles(path5, "*.exe");
				string optionVal7 = CommandLineOption.GetOptionVal(args[i], "NONE");
				string text5 = null;
				StreamWriter streamWriter3 = null;
				if (optionVal7 != "NONE")
				{
					text5 = MakeLogPath(optionVal7, delete: true);
					streamWriter3 = new StreamWriter(text5);
				}
				else
				{
					streamWriter3 = new StreamWriter(Console.OpenStandardOutput());
				}
				try
				{
					streamWriter3.WriteLine("Date: {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
					foreach (string item2 in list2)
					{
						try
						{
							AssemblyName assemblyName = AssemblyName.GetAssemblyName(item2);
							streamWriter3.WriteLine("Assembly {0} has version {1}", item2, assemblyName.Version.ToString());
						}
						catch (Exception ex)
						{
							streamWriter3.WriteLine("Unable to get version for {0}; message = {1}", item2, ex.Message);
						}
					}
					return;
				}
				finally
				{
					streamWriter3.Dispose();
				}
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
				using StreamWriter streamWriter4 = new StreamWriter(File.Open(path6, FileMode.Append, FileAccess.Write, FileShare.Read));
				streamWriter4.WriteLine("The upgrade process is no longer supported.");
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
			if ("-dumpQRStats" == args[i])
			{
				RunImmediateInstruction("QUICKRETURN DUMPCOUNTERS");
				return;
			}
			if ("-resetDupCounter" == args[i])
			{
				HardwareService hardwareService5 = ConfigureCommunication();
				if (hardwareService5 != null)
				{
					new CounterHelper().ResetDuplicateCounterValue(hardwareService5);
				}
				return;
			}
			if ("-checkDupCounter" == args[i])
			{
				string path8 = MakeNewFile("DuplicateCounter.log");
				HardwareService hardwareService6 = ConfigureCommunication();
				if (hardwareService6 == null)
				{
					return;
				}
				CounterHelper counterHelper = new CounterHelper();
				counterHelper.GetDuplicateCounterValue(hardwareService6, out var val);
				if (val > 0)
				{
					using StreamWriter streamWriter5 = new StreamWriter(path8);
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
				string optionValue3 = CommandLineOption.GetOptionValue(args[i]);
				if (!string.IsNullOrEmpty(optionValue3))
				{
					string arg2 = optionValue3.Equals("on", StringComparison.CurrentCultureIgnoreCase).ToString();
					RunImmediateInstruction($"SETCFG \"EnableInfared\" \"{arg2}\" TYPE=CAMERA");
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
				string optionValue4 = CommandLineOption.GetOptionValue(args[i]);
				UpdateQREEPROM(optionValue4, waitTime);
				return;
			}
			if (args[i].StartsWith("-grabQRCounters"))
			{
				bool optionVal8 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				new QuickReturnHelper().DumpCounters(ConfigureCommunication(), optionVal8);
				return;
			}
			if (args[i].StartsWith("-vendTest"))
			{
				int optionVal9 = CommandLineOption.GetOptionVal(args[i], 3);
				int? startAt = null;
				if (args.Length > 1 && args[i + 1].StartsWith("-next"))
				{
					startAt = CommandLineOption.GetOptionVal(args[i + 1], -1);
				}
				VendTest vendTest = new VendTest(optionVal9);
				using DefaultConsole console2 = new DefaultConsole();
				vendTest.Execute(console2, startAt);
				return;
			}
			if (args[i].StartsWith("-scheduleClean"))
			{
				int? optionVal10 = CommandLineOption.GetOptionVal<int?>(args[i], null);
				using DefaultConsole defaultConsole4 = new DefaultConsole();
				if (optionVal10.HasValue && optionVal10 != -1)
				{
					RedboxTimer redboxTimer2 = new RedboxTimer("Clean", GenericimerFired);
					redboxTimer2.ScheduleAtNext(optionVal10.Value, 0);
					defaultConsole4.WriteLine("Clean timer should fire {0} {1}", redboxTimer2.NextFireTime.Value.ToShortDateString(), redboxTimer2.NextFireTime.Value.ToShortTimeString());
					GenericWaiter.WaitOne();
				}
				ClientHelper clientHelper5 = new ClientHelper();
				HardwareService hardwareService7 = clientHelper5.AutoConnect();
				if (!hardwareService7.CleanZone(out var job3).Success)
				{
					defaultConsole4.WriteLine("Failed to schedule clean job.");
					return;
				}
				job3.Pend();
				clientHelper5.WaitForJob(job3, out var endStatus3);
				defaultConsole4.WriteLine("Clean {0} ended with status {1}", job3.ID, endStatus3.ToString());
				return;
			}
			if (args[i].StartsWith("-syncTest"))
			{
				using (DefaultConsole defaultConsole5 = new DefaultConsole())
				{
					int num6 = 1000;
					string optionVal11 = CommandLineOption.GetOptionVal<string>(args[i], null);
					if (optionVal11 == null)
					{
						defaultConsole5.WriteLine("There is no specified barcode.");
						return;
					}
					if (args.Length > 1 && args[i + 1].StartsWith("-iter"))
					{
						num6 = CommandLineOption.GetOptionVal(args[i + 1], num6);
					}
					ClientHelper clientHelper6 = new ClientHelper();
					HardwareService hardwareService8 = clientHelper6.AutoConnect();
					HardwareCommandResult hardwareCommandResult = hardwareService8.ExecuteImmediate($"LOOKUP \"{optionVal11}\"", out var job4);
					if (!hardwareCommandResult.Success)
					{
						defaultConsole5.WriteLine("Call failed!");
						return;
					}
					string value = null;
					job4.Pop<string>(out value);
					if (value != "FOUND")
					{
						defaultConsole5.WriteLine("Could not find barcode {0}", optionVal11);
						return;
					}
					int value2 = 0;
					int value3 = 0;
					job4.Pop<int>(out value2);
					job4.Pop<int>(out value3);
					for (int l = 0; l < num6; l++)
					{
						HardwareCommandResult hardwareCommandResult2 = hardwareService8.ScheduleJob("sync-locations", $"Sync iter {l}", enableDebugging: false, new HardwareJobSchedule
						{
							Priority = HardwareJobPriority.Low
						}, out var job5);
						if (hardwareCommandResult2.Success)
						{
							job5.Push(value3);
							job5.Push(value2);
							job5.Push(1);
							job5.Pend();
						}
						clientHelper6.WaitForJob(job5, out var endStatus4);
						if (endStatus4 != HardwareJobStatus.Completed)
						{
							defaultConsole5.WriteLine("Job ended with status {0}", endStatus4.ToString());
							return;
						}
						if (!job5.GetResults(out var results3).Success)
						{
							defaultConsole5.WriteLine("Could not get job results!");
							return;
						}
						ProgramResult[] array3 = results3;
						foreach (ProgramResult programResult2 in array3)
						{
							if (programResult2.Code == "InventoryState")
							{
								if (programResult2.ItemID.Barcode == null)
								{
									defaultConsole5.WriteLine("There was no barcode metadata.");
									return;
								}
								if (programResult2.ItemID.Barcode != optionVal11)
								{
									defaultConsole5.WriteLine("Barcode mismatch - job failure.");
									return;
								}
							}
						}
						job5.Trash();
						if (l % 50 == 0)
						{
							hardwareService8.ExecuteServiceCommand("JOB collect-garbage force: true");
						}
					}
					defaultConsole5.WriteLine("Successfully completed {0} iterations.", num6);
				}
				continue;
			}
			if (args[i].Equals("-simulateReboot"))
			{
				RedboxTimer redboxTimer3 = new RedboxTimer("Reboot simulator", ShutDownService);
				redboxTimer3.ScheduleAtNext(1, 0);
				TimerWait.WaitOne();
				DateTime now = DateTime.Now;
				Console.WriteLine("{0} {1} Timer done.", now.ToShortDateString(), now.ToShortTimeString());
				continue;
			}
			if ("-e".Equals(args[i]))
			{
				MakeNewFile("empties.txt");
				using InventoryHelper inventoryHelper = new InventoryHelper();
				inventoryHelper.FixType("empties.txt", "EMPTY", "EMPTY location sync.");
				return;
			}
			if ("-unknown".Equals(args[i]))
			{
				string fileName = MakeNewFile("unknowns.txt");
				using InventoryHelper inventoryHelper2 = new InventoryHelper();
				inventoryHelper2.FixType(fileName, "UNKNOWN", "UNKNOWN inventory sync.");
				return;
			}
			if ("-fullSync".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper3 = new InventoryHelper())
				{
					inventoryHelper3.ScheduleFullSync();
					return;
				}
			}
			if ("-dumpInventory".Equals(args[i]))
			{
				using (KioskInventory kioskInventory = new KioskInventory())
				{
					string path9 = MakeNewFile("inventory.xml");
					kioskInventory.DumpInventoryToFile(path9);
					return;
				}
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
				using InventoryHelper inventoryHelper4 = new InventoryHelper();
				text = args[++i];
				if (inventoryHelper4.ImportInventoryFromFile(text))
				{
					Console.WriteLine("Inventory import OK.");
				}
				else
				{
					Console.WriteLine("Failed to import Inventory!");
				}
				return;
			}
			if ("-inventorysnapshot".Equals(args[i]))
			{
				if (!Directory.Exists("C:\\Program Files\\Redbox\\inventory-snapshots"))
				{
					try
					{
						Directory.CreateDirectory("C:\\Program Files\\Redbox\\inventory-snapshots");
					}
					catch (Exception ex2)
					{
						Console.WriteLine("Unable to create directory: {0}", ex2.Message);
					}
				}
				string path10 = ComputeNewFile();
				using KioskInventory kioskInventory2 = new KioskInventory();
				IList<IInventoryLocation> deckInventory = kioskInventory2.DeckInventory;
				string path11 = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", path10);
				using StreamWriter streamWriter6 = new StreamWriter(path11);
				streamWriter6.WriteLine("Deck,Slot,Barcode");
				foreach (IInventoryLocation item3 in deckInventory)
				{
					streamWriter6.WriteLine($"{item3.Location.Deck},{item3.Location.Slot},'{item3.Matrix}'");
				}
				int num7 = 1;
				foreach (IDumpbinItem dumpbinItem in kioskInventory2.DumpbinItems)
				{
					streamWriter6.WriteLine($"9,{num7++},'{dumpbinItem.Matrix}'");
				}
				return;
			}
			if ("-updateConfiguration".Equals(args[i]))
			{
				text2 = args[++i];
				flag = true;
				continue;
			}
			if ("-updateConfigFile".Equals(args[i]))
			{
				text2 = args[++i];
				flag2 = true;
				continue;
			}
			if ("-removeOldFolders".Equals(args[i]))
			{
				flag3 = true;
				continue;
			}
			if ("-version".Equals(args[i]))
			{
				string path12 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "version.txt");
				Version version = typeof(Program).Assembly.GetName().Version;
				using (StreamWriter streamWriter7 = new StreamWriter(path12))
				{
					streamWriter7.WriteLine(version);
				}
				Console.WriteLine("HALUtilities version: {0}", version);
				return;
			}
			if ("-dumpExcluded".Equals(args[i]))
			{
				string path13 = RuntimeService.Instance.RuntimePath("excluded.log");
				ClientHelper clientHelper7 = new ClientHelper();
				HardwareService service = clientHelper7.AutoConnect();
				List<Location> list3 = ConfigurationHelper.ExcludedSlots(service);
				using StreamWriter streamWriter8 = new StreamWriter(path13);
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
			string path14 = MakeLogPath("tsdrivers.log", delete: true);
			using StreamWriter streamWriter9 = new StreamWriter(path14);
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\3M Touch Systems, Inc.");
				string[] subKeyNames = registryKey.GetSubKeyNames();
				array7 = subKeyNames;
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
			int num8 = text2.IndexOf('|');
			if (-1 != num8)
			{
				c = '|';
			}
			string[] array9 = text2.Split(c);
			if (array9.Length < 3)
			{
				Console.WriteLine("Unable to parse option {0}", text2);
				Environment.Exit(1);
			}
			string path15 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "ConfigurationUpdateError.log");
			string text6 = array9[0];
			string text7 = array9[1];
			string text8 = array9[2];
			bool flag6 = array9.Length == 4;
			if (flag)
			{
				if (File.Exists(path15))
				{
					try
					{
						File.Delete(path15);
					}
					catch
					{
					}
				}
				HardwareService hardwareService9 = ConfigureCommunication();
				if (hardwareService9 == null)
				{
					using StreamWriter streamWriter10 = new StreamWriter(path15);
					streamWriter10.WriteLine("Unable to talk to HAL.");
					return;
				}
				HardwareCommandResult hardwareCommandResult3 = hardwareService9.ExecuteImmediate($"SETCFG \"{text7}\" \"{text8}\" TYPE={text6}", out var _);
				if (!hardwareCommandResult3.Success)
				{
					using StreamWriter streamWriter11 = new StreamWriter(path15);
					streamWriter11.WriteLine("The configuration update failed.");
					foreach (Error error in hardwareCommandResult3.Errors)
					{
						streamWriter11.WriteLine(error.ToString());
					}
					return;
				}
				if (flag6)
				{
					ClientHelper clientHelper8 = new ClientHelper();
					HardwareService hardwareService10 = clientHelper8.AutoConnect();
					HardwareCommandResult configuration = hardwareService10.GetConfiguration(text6);
					if (!configuration.Success)
					{
						Console.WriteLine("Unable to get configuration from service.");
						return;
					}
					string xml = configuration.CommandMessages[0];
					string path16 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "ConfigurationUpdateValidateError.log");
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
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(xml);
					XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("property");
					foreach (XmlNode item5 in xmlNodeList)
					{
						XmlNode namedItem = item5.Attributes.GetNamedItem("name");
						if (!namedItem.Value.Equals(text7, StringComparison.CurrentCultureIgnoreCase))
						{
							continue;
						}
						string value5 = item5.Attributes["value"].Value;
						if (!value5.Equals(text8, StringComparison.CurrentCultureIgnoreCase))
						{
							using StreamWriter streamWriter12 = new StreamWriter(path16);
							streamWriter12.WriteLine($"The new value {value5} doesn't match {text8}");
						}
					}
				}
			}
			else if (flag2)
			{
				string path17 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "UpdateConfigurationFileError.log");
				string text9 = Path.Combine(ServicePath, "HAL.xml");
				string text10 = text9 + ".bak";
				if (!File.Exists(text9))
				{
					using StreamWriter streamWriter13 = new StreamWriter(path17);
					streamWriter13.WriteLine($"Unable to find configuration file \"{text9}\"");
				}
				else
				{
					if (File.Exists(text10))
					{
						try
						{
							File.Delete(text10);
						}
						catch
						{
						}
					}
					try
					{
						File.Copy(text9, text10);
					}
					catch
					{
					}
					XmlDocument xmlDocument2 = new XmlDocument();
					xmlDocument2.Load(text9);
					XmlNode xmlNode2 = xmlDocument2.DocumentElement.SelectSingleNode($"{text6}/{text7}");
					if (xmlNode2 != null)
					{
						xmlNode2.InnerText = text8;
						xmlDocument2.Save(text9);
					}
					else
					{
						using StreamWriter streamWriter14 = new StreamWriter(path17);
						streamWriter14.WriteLine($"Unable to find configuration \"{text6}\"");
					}
				}
			}
		}
		if (!flag3)
		{
			return;
		}
		string path18 = "c:\\Program Files\\Redbox";
		string[] directories = Directory.GetDirectories(path18, "Hardware Abstraction Layer (HAL)*");
		array7 = directories;
		foreach (string text11 in array7)
		{
			string path19 = Path.Combine(text11, "bin\\halsvc-win32.exe");
			if (!File.Exists(path19))
			{
				try
				{
					Directory.Delete(text11, recursive: true);
				}
				catch
				{
				}
			}
		}
	}
}
