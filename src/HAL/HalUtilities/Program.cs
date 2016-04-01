using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
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

	internal static string MakeLogPath(string logname, bool delete)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string text = service.RuntimePath(logname);
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string text = service.RuntimePath(fileName);
		if (File.Exists(text))
		{
			FileHelper.SafeDeleteFile(text);
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

	private static void PrintUsageAndExit(string[] secureOptions)
	{
		Console.WriteLine("InventoryHelper <-u> <-e> <-unknown> <-fullSync> <-fullSyncWithReport> <-dumpInventory> <-findCameraErrors> <-importInventory fullpathtofile> <-updateConfiguration CONFIG:PROPERTY:NEWVALUE> <-help>");
		Console.WriteLine("  Where: ");
		Console.WriteLine("  \t\t-e syncs empties ");
		Console.WriteLine("  \t\t-unknown syncs unknowns ");
		Console.WriteLine("  \t\t-fullSync schedules a low-priority sync");
		Console.WriteLine("  \t\t-dumpInventory writes the inventory to the file \"inventory.xml\"");
		Console.WriteLine("  \t\t-inventorysnapshot   if specified, generates an inventory snapshot to a csv file in \"C:\\Program Files\\Redbox\\inventory-snapshot\"");
		Console.WriteLine("  \t\t-importInventory takes inventory from an XML file and copies it into inventory");
		Console.WriteLine("  \t\t-updateConfiguration CONFIG:OPTION:NEWVALUE:VALIDATE Update OPTION in configuration CONFIG to value NEWVALUE.");
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = service.RuntimePath("backup-log.txt");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
		string text = service.RuntimePath(KioskID);
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

	private static string[] GetProgramArgs(string[] allArgs, int start)
	{
		int num = allArgs.Length - (start + 1);
		string[] array = new string[num];
		Array.Copy(allArgs, start + 1, array, 0, array.Length);
		return array;
	}

	private static bool InstallDefault(TextWriter log, string database)
	{
		if (!ProcessHelper.StopHALService())
		{
			log.WriteLine("Unable stop the HAL servcie.");
			return false;
		}
		string text = Path.Combine(ServicePath, $"{database}.default");
		if (!File.Exists(text))
		{
			log.WriteLine("The source default file '{0}' doesn't exist.", text);
			ProcessHelper.StartHALService();
			return false;
		}
		string text2 = Path.Combine(ServicePath, $"data\\{database}");
		if (!File.Exists(text2))
		{
			log.WriteLine("The target database '{0}' doesn't exist.", text2);
			ProcessHelper.StartHALService();
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
		return ProcessHelper.StartHALService();
	}

	private static void Main(string[] args)
	{
		string text = null;
		string text2 = string.Empty;
		bool flag = false;
		string[] secureOptions = new string[0];
		IRuntimeService runtimeService = new RuntimeService();
		ServiceLocator.Instance.AddService(typeof(IRuntimeService), runtimeService);
		if (args.Length == 0)
		{
			PrintUsageAndExit(secureOptions);
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("-randomSync"))
			{
				ClientHelper clientHelper = new ClientHelper();
				HardwareService service = clientHelper.AutoConnect();
				int num = 1;
				int vendTime = 1;
				for (int j = 1; j <= num; j++)
				{
					Console.WriteLine("Iteration {0} of {1} total.", j, num);
					using RandomSyncExecutor randomSyncExecutor = new RandomSyncExecutor(service, vendTime, 1);
					randomSyncExecutor.Run();
					Console.WriteLine("Job {0} ends with status {1}", randomSyncExecutor.ID, randomSyncExecutor.EndStatus.ToString());
					if (HardwareJobStatus.Completed != randomSyncExecutor.EndStatus)
					{
						Console.WriteLine("** Tool is exiting after job failure **");
						Environment.Exit(1);
					}
				}
				return;
			}
			if (args[i].StartsWith("-fraudStressTest"))
			{
				int num2 = 10;
				int num3 = 1500;
				int num4 = 300;
				for (int k = i + 1; k < args.Length; k++)
				{
					if (args[k].StartsWith("--scanPause"))
					{
						num4 = CommandLineOption.GetOptionVal(args[k], num4);
					}
					else if (args[k].StartsWith("--iterations"))
					{
						num2 = CommandLineOption.GetOptionVal(args[k], num2);
					}
					else if (args[k].StartsWith("--readPause"))
					{
						num3 = CommandLineOption.GetOptionVal(args[k], num3);
					}
				}
				FraudSensorStressTest fraudSensorStressTest = new FraudSensorStressTest(num2, num3, num4);
				fraudSensorStressTest.RunJob();
				return;
			}
			if ("-findKFCDevices" == args[i])
			{
				string path = MakeLogPath("KFC.log", delete: true);
				using StreamWriter streamWriter = new StreamWriter(path);
				new ConsoleLogger();
				UsbDeviceService usbDeviceService = new UsbDeviceService();
				IDeviceDescriptor deviceDescriptor = usbDeviceService.FindTouchScreen(matchDriver: true);
				if (deviceDescriptor == null)
				{
					streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
				}
				else
				{
					streamWriter.WriteLine("Found touchscreen HWID {0} {1}", deviceDescriptor.ToString(), deviceDescriptor.Friendlyname);
				}
				deviceDescriptor = usbDeviceService.FindActiveCamera(matchDriver: true);
				if (deviceDescriptor == null)
				{
					streamWriter.WriteLine("Could not find a camera or match its driver.");
				}
				else
				{
					streamWriter.WriteLine("Found camera {0} {1}", deviceDescriptor.ToString(), deviceDescriptor.Friendlyname);
				}
				return;
			}
			if ("-findActiveTouchScreen" == args[i])
			{
				string path2 = MakeLogPath("TSLocator.log", delete: true);
				using StreamWriter streamWriter2 = new StreamWriter(path2);
				new ConsoleLogger();
				UsbDeviceService usbDeviceService2 = new UsbDeviceService();
				IDeviceDescriptor deviceDescriptor2 = usbDeviceService2.FindTouchScreen(matchDriver: true);
				if (deviceDescriptor2 == null)
				{
					streamWriter2.WriteLine("Could not find a touchscreen or match its driver.");
				}
				else
				{
					streamWriter2.WriteLine("Found touchscreen HWID {0} {1}", deviceDescriptor2.ToString(), deviceDescriptor2.Friendlyname);
				}
				return;
			}
			if ("-findActiveCamera" == args[i])
			{
				string path3 = MakeLogPath("TSLocator.log", delete: true);
				using StreamWriter streamWriter3 = new StreamWriter(path3);
				new ConsoleLogger();
				UsbDeviceService usbDeviceService3 = new UsbDeviceService();
				IDeviceDescriptor deviceDescriptor3 = usbDeviceService3.FindActiveCamera(matchDriver: true);
				if (deviceDescriptor3 == null)
				{
					streamWriter3.WriteLine("Could not find a camera or match its driver.");
				}
				else
				{
					streamWriter3.WriteLine("Found camera {0} {1}", deviceDescriptor3.ToString(), deviceDescriptor3.Friendlyname);
				}
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
			if (args[i].StartsWith("--findUPSWithAtLeastPID"))
			{
				string optionVal = CommandLineOption.GetOptionVal(args[i], "=", string.Empty);
				if (string.IsNullOrEmpty(optionVal))
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				string[] array = optionVal.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 2)
				{
					Console.WriteLine("The option value '{0}' is malformed.", args[i]);
					return;
				}
				List<string> errors = new List<string>();
				List<string> list = new List<string>();
				DeviceHelper deviceHelper = new DeviceHelper();
				deviceHelper.FindDeviceWithPid(DeviceClass.HIDClass, array[0], array[1], DriverMatchType.GTEPid, errors, list);
				if (list.Count == 0)
				{
					return;
				}
				string path4 = MakeLogPath("UPSUpgradeneeded.log", delete: true);
				using StreamWriter streamWriter4 = new StreamWriter(path4);
				streamWriter4.WriteLine("The following devices matched the criteria vid = {0} && pid = {1}", array[0], array[1]);
				foreach (string item in list)
				{
					streamWriter4.WriteLine(item);
				}
				return;
			}
			if (args[i] == "-interactive")
			{
				string[] programArgs = GetProgramArgs(args, i);
				using ScriptMode scriptMode = new ScriptMode();
				scriptMode.Run(programArgs);
				return;
			}
			if (args[i].StartsWith("-resetDeck"))
			{
				string optionVal2 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				string path5 = runtimeService.RuntimePath("resetdecks.txt");
				using (StreamWriter streamWriter5 = new StreamWriter(File.Open(path5, FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					if (string.IsNullOrEmpty(optionVal2))
					{
						streamWriter5.WriteLine("Invalid deck list specified.");
						return;
					}
					StringBuilder stringBuilder = new StringBuilder();
					char[] separator = new char[1] { ',' };
					string[] array2 = optionVal2.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					string[] array3 = array2;
					foreach (string arg in array3)
					{
						streamWriter5.WriteLine(" Resetting deck {0}", arg);
						stringBuilder.AppendLine($" DECK RESET NUMBER=\"{arg}\"");
					}
					ClientHelper clientHelper2 = new ClientHelper();
					HardwareService hardwareService = clientHelper2.AutoConnect();
					if (hardwareService == null)
					{
						streamWriter5.WriteLine("Cannot connect to service. Please try again.");
						return;
					}
					hardwareService.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
				}
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
					ClientHelper clientHelper3 = new ClientHelper();
					HardwareService hardwareService2 = clientHelper3.AutoConnect();
					if (!hardwareService2.GetJobs(out var jobs).Success)
					{
						defaultConsole.WriteLine("Unable to get job list.");
						return;
					}
					HardwareJob hardwareJob = null;
					HardwareJob[] array4 = jobs;
					foreach (HardwareJob hardwareJob2 in array4)
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
					clientHelper3.WaitForJob(hardwareJob, out var endStatus);
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
					int num5 = 0;
					int num6 = 0;
					ProgramResult[] array5 = results;
					foreach (ProgramResult result in array5)
					{
						if (InventoryItemsEqual(defaultConsole, result))
						{
							num5++;
						}
						else
						{
							num6++;
						}
					}
					defaultConsole.WriteLine("There were:");
					defaultConsole.WriteLine("  {0} matches.", num5);
					defaultConsole.WriteLine("  {0} mismatches.", num6);
					if (num5 + num6 != 717)
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
				string path6 = MakeLogPath("InventoryDefaultCopy.log", delete: true);
				using StreamWriter log = new StreamWriter(path6);
				InstallDefault(log, "HALData.vdb3");
				return;
			}
			if ("-installDefaultCountersDB" == args[i])
			{
				string path7 = MakeLogPath("CountersDefaultCopy.log", delete: true);
				using StreamWriter log2 = new StreamWriter(path7);
				InstallDefault(log2, "HALCounters.vdb3");
				return;
			}
			if ("-bm" == args[i])
			{
				string text3 = null;
				string text4 = null;
				int num7 = 0;
				i++;
				while (num7 < 2)
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
					num7++;
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
				string path8 = "c:\\Program Files\\Redbox\\HALService\\bin";
				List<string> list2 = new List<string>();
				list2.AddRange(Directory.GetFiles(path8, "*.dll"));
				Directory.GetFiles(path8, "*.exe");
				string optionVal6 = CommandLineOption.GetOptionVal(args[i], "NONE");
				string text5 = null;
				StreamWriter streamWriter6 = null;
				if (optionVal6 != "NONE")
				{
					text5 = MakeLogPath(optionVal6, delete: true);
					streamWriter6 = new StreamWriter(text5);
				}
				else
				{
					streamWriter6 = new StreamWriter(Console.OpenStandardOutput());
				}
				try
				{
					streamWriter6.WriteLine("Date: {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
					foreach (string item2 in list2)
					{
						try
						{
							AssemblyName assemblyName = AssemblyName.GetAssemblyName(item2);
							streamWriter6.WriteLine("Assembly {0} has version {1}", item2, assemblyName.Version.ToString());
						}
						catch (Exception ex)
						{
							streamWriter6.WriteLine("Unable to get version for {0}; message = {1}", item2, ex.Message);
						}
					}
					return;
				}
				finally
				{
					streamWriter6.Dispose();
				}
			}
			if (args[i].StartsWith("-qrUpdateWait"))
			{
				CommandLineOption.GetOptionVal(args[i], 300000);
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
				string path9 = runtimeService.RuntimePath("configure-VMZ.txt");
				using StreamWriter streamWriter7 = new StreamWriter(File.Open(path9, FileMode.Append, FileAccess.Write, FileShare.Read));
				streamWriter7.WriteLine("The upgrade process is no longer supported.");
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
				string path10 = MakeLogPath("InventoryCompare.txt", delete: true);
				using StreamWriter log3 = new StreamWriter(path10);
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
				HardwareService hardwareService3 = ConfigureCommunication();
				if (hardwareService3 != null)
				{
					new CounterHelper().ResetDuplicateCounterValue(hardwareService3);
				}
				return;
			}
			if ("-checkDupCounter" == args[i])
			{
				string path11 = MakeNewFile("DuplicateCounter.log");
				HardwareService hardwareService4 = ConfigureCommunication();
				if (hardwareService4 == null)
				{
					return;
				}
				CounterHelper counterHelper = new CounterHelper();
				counterHelper.GetDuplicateCounterValue(hardwareService4, out var val);
				if (val > 0)
				{
					using StreamWriter streamWriter8 = new StreamWriter(path11);
					streamWriter8.WriteLine("The kiosk has duplicates.");
					streamWriter8.Flush();
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
				Console.WriteLine("The option '-configureIRCamera' is no longer supported.");
				return;
			}
			if (args[i].Equals("-checkQRStatus"))
			{
				new QuickReturnHelper().ProperlyConfigured(ConfigureCommunication());
				continue;
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
			if (args[i].StartsWith("-scheduleClean"))
			{
				int? optionVal9 = CommandLineOption.GetOptionVal<int?>(args[i], null);
				using DefaultConsole defaultConsole2 = new DefaultConsole();
				if (optionVal9.HasValue && optionVal9 != -1)
				{
					RedboxTimer redboxTimer = new RedboxTimer("Clean", GenericimerFired);
					redboxTimer.ScheduleAtNext(optionVal9.Value, 0);
					defaultConsole2.WriteLine("Clean timer should fire {0} {1}", redboxTimer.NextFireTime.Value.ToShortDateString(), redboxTimer.NextFireTime.Value.ToShortTimeString());
					GenericWaiter.WaitOne();
				}
				ClientHelper clientHelper4 = new ClientHelper();
				HardwareService hardwareService5 = clientHelper4.AutoConnect();
				if (!hardwareService5.CleanZone(out var job2).Success)
				{
					defaultConsole2.WriteLine("Failed to schedule clean job.");
					return;
				}
				job2.Pend();
				clientHelper4.WaitForJob(job2, out var endStatus2);
				defaultConsole2.WriteLine("Clean {0} ended with status {1}", job2.ID, endStatus2.ToString());
				return;
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
				string path12 = MakeNewFile("inventory.xml");
				using InventoryHelper inventoryHelper4 = new InventoryHelper();
				inventoryHelper4.DumpInventoryToFile(path12);
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
				using InventoryHelper inventoryHelper5 = new InventoryHelper();
				text = args[++i];
				if (inventoryHelper5.ImportInventoryFromFile(text))
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
				string path13 = ComputeNewFile();
				using KioskInventory kioskInventory = new KioskInventory();
				IList<IInventoryLocation> deckInventory = kioskInventory.DeckInventory;
				string path14 = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", path13);
				using StreamWriter streamWriter9 = new StreamWriter(path14);
				streamWriter9.WriteLine("Deck,Slot,Barcode");
				foreach (IInventoryLocation item3 in deckInventory)
				{
					streamWriter9.WriteLine($"{item3.Location.Deck},{item3.Location.Slot},'{item3.Matrix}'");
				}
				int num8 = 1;
				foreach (IDumpbinItem dumpbinItem in kioskInventory.DumpbinItems)
				{
					streamWriter9.WriteLine($"9,{num8++},'{dumpbinItem.Matrix}'");
				}
				return;
			}
			if ("-updateConfiguration".Equals(args[i]))
			{
				text2 = args[++i];
				flag = true;
				continue;
			}
			if ("-version".Equals(args[i]))
			{
				string path15 = runtimeService.RuntimePath("version.txt");
				Version version = typeof(Program).Assembly.GetName().Version;
				using (StreamWriter streamWriter10 = new StreamWriter(path15))
				{
					streamWriter10.WriteLine(version);
				}
				Console.WriteLine("HALUtilities version: {0}", version);
				return;
			}
			if ("-dumpExcluded".Equals(args[i]))
			{
				string path16 = runtimeService.RuntimePath("excluded.log");
				ClientHelper clientHelper5 = new ClientHelper();
				HardwareService service2 = clientHelper5.AutoConnect();
				using ExcludedLocsJob excludedLocsJob = new ExcludedLocsJob(service2);
				excludedLocsJob.Run();
				using StreamWriter streamWriter11 = new StreamWriter(path16);
				IList<Location> excludedLocations = excludedLocsJob.ExcludedLocations;
				if (excludedLocations.Count == 0)
				{
					streamWriter11.WriteLine("There are no excluded locations.");
					return;
				}
				streamWriter11.WriteLine("There are {0} locations total.", excludedLocsJob.Results.Count);
				foreach (Location item4 in excludedLocations)
				{
					streamWriter11.WriteLine("Excluded location deck = {0} slot = {1}", item4.Deck, item4.Slot);
				}
				return;
			}
			if (!("-tsdriver" == args[i]))
			{
				continue;
			}
			RegistryKey registryKey = null;
			string path17 = MakeLogPath("tsdrivers.log", delete: true);
			using StreamWriter streamWriter12 = new StreamWriter(path17);
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("Software\\3M Touch Systems, Inc.");
				string[] subKeyNames = registryKey.GetSubKeyNames();
				string[] array3 = subKeyNames;
				foreach (string value in array3)
				{
					streamWriter12.WriteLine(value);
				}
			}
			catch (Exception ex3)
			{
				streamWriter12.WriteLine("Caught an exception reading the registry: {0}", ex3.Message);
			}
			finally
			{
				registryKey?.Close();
			}
		}
		if (string.IsNullOrEmpty(text2))
		{
			return;
		}
		char c = ':';
		int num9 = text2.IndexOf('|');
		if (-1 != num9)
		{
			c = '|';
		}
		string[] array6 = text2.Split(c);
		if (array6.Length < 3)
		{
			Console.WriteLine("Unable to parse option {0}", text2);
			Environment.Exit(1);
		}
		string path18 = runtimeService.RuntimePath("ConfigurationUpdateError.log");
		string text6 = array6[0];
		string arg2 = array6[1];
		string text7 = array6[2];
		bool flag2 = array6.Length == 4;
		if (!flag)
		{
			return;
		}
		if (File.Exists(path18))
		{
			try
			{
				File.Delete(path18);
			}
			catch
			{
			}
		}
		HardwareService hardwareService6 = ConfigureCommunication();
		if (hardwareService6 == null)
		{
			using (StreamWriter streamWriter13 = new StreamWriter(path18))
			{
				streamWriter13.WriteLine("Unable to talk to HAL.");
				return;
			}
		}
		HardwareCommandResult hardwareCommandResult = hardwareService6.ExecuteImmediate($"SETCFG \"{arg2}\" \"{text7}\" TYPE={text6}", out var job3);
		if (!hardwareCommandResult.Success)
		{
			using (StreamWriter streamWriter14 = new StreamWriter(path18))
			{
				streamWriter14.WriteLine("The configuration update failed.");
				foreach (Error error in hardwareCommandResult.Errors)
				{
					streamWriter14.WriteLine(error.ToString());
				}
				return;
			}
		}
		if (!flag2)
		{
			return;
		}
		hardwareCommandResult = hardwareService6.ExecuteImmediate($"GETCFG \"{arg2}\" TYPE={text6}", out job3);
		string path19 = runtimeService.RuntimePath("ConfigurationUpdateValidateError.log");
		FileHelper.SafeDeleteFile(path19);
		using StreamWriter streamWriter15 = new StreamWriter(path19);
		if (!hardwareCommandResult.Success)
		{
			streamWriter15.WriteLine("The configuration update failed.");
			{
				foreach (Error error2 in hardwareCommandResult.Errors)
				{
					streamWriter15.WriteLine(error2.ToString());
				}
				return;
			}
		}
		HardwareCommandResult stack = job3.GetStack(out var stack2);
		if (!stack.Success)
		{
			streamWriter15.WriteLine("Couldn't validate: unable to get stack.");
			return;
		}
		string text8 = stack2.Pop();
		if (!text8.Equals(text7, StringComparison.CurrentCultureIgnoreCase))
		{
			streamWriter15.WriteLine("The new value {0} doesn't match {1}", text8, text7);
		}
	}
}
