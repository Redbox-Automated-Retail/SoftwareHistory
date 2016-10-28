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

namespace HALUtilities;

internal sealed class Program
{
	private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";

	private const int ServiceTimeout = 30000;

	private const string DuplicateCounterFile = "DuplicateCounter.log";

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

	private static string ServicePath => "c:\\Program Files\\Redbox\\HALService\\bin";

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

	private static bool RunImmediateInstruction(string inst, ClientHelper helper)
	{
		HardwareJob job;
		return helper.AutoConnect()?.ExecuteImmediate(inst, out job).Success ?? false;
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

	private static void PrintUsageAndExit()
	{
		PrintUsageAndExit(new string[0]);
	}

	private static void PrintUsageAndExit(string[] secureOptions)
	{
		Console.WriteLine("HALUtilities.exe version {0}", typeof(Program).Assembly.GetName().Version);
		Console.WriteLine("## Options ## ");
		Console.WriteLine("  Where: ");
		Console.WriteLine("  \t\t--findTripplite:<true|false>  Probes for any tripplite devices attached to the computer.");
		Console.WriteLine("  \t\t-findABEDevice  Probes for an attached ABE device via HAL service.");
		Console.WriteLine("  \t\t-findABEDeviceThroughScan  Probes for an attached ABE device via USB query.");
		Console.WriteLine("  \t\t-findKFCDevices  Probes for active touchscreen & camera attached to computer. Logs to KFC.log.");
		Console.WriteLine("  \t\t-findActiveTouchScreen  Probes for active touchscreen attached to computer. Logs to TSLocator.log.");
		Console.WriteLine("  \t\t-findActiveCamera  Probes for active camera attached to computer. Logs to CameraLocator.log.");
		Console.WriteLine("  \t\t-resetControlSystem  Resets the kiosk control system.");
		Console.WriteLine("  \t\t-resetMotionControl  Resets the motion control system.");
		Console.WriteLine("  \t\t-resetPDCMCounter  Resets the PDCM failure count.");
		Console.WriteLine("  \t\t-interactive  Starts interactive mode.");
		Console.WriteLine("  \t\t-dumpTouchScreens  Dumps monitor class devices to Monitor.log");
		Console.WriteLine("  \t\t-dumpDevices  Dumps all device classes to AllDevices.log");
		Console.WriteLine("  \t\t-interactive  Starts interactive mode.");
		Console.WriteLine("  \t\t-e syncs empties ");
		Console.WriteLine("  \t\t-unknown syncs unknowns ");
		Console.WriteLine("  \t\t-fullSync schedules a low-priority sync");
		Console.WriteLine("  \t\t-dumpInventory writes the inventory to the file \"inventory.xml\"");
		Console.WriteLine("  \t\t-inventorysnapshot   if specified, generates an inventory snapshot to a csv file in \"C:\\Program Files\\Redbox\\inventory-snapshot\"");
		Console.WriteLine("  \t\t-importInventory takes inventory from an XML file and copies it into inventory");
		Console.WriteLine("  \t\t-updateConfiguration CONFIG:OPTION:NEWVALUE:VALIDATE Update OPTION in configuration CONFIG to value NEWVALUE.");
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
		foreach (string value in secureOptions)
		{
			Console.WriteLine(value);
		}
		Console.WriteLine("  \t\t-help print this message and exit.");
		Environment.Exit(1);
	}

	private static void PrintSecureHelp()
	{
		string[] secureOptions = new string[11]
		{
			"  \t\t-randomSync  Runs a random sync of the kiosk.", "  \t\t-installDefaultInventoryDB Backs up old inventory DB, and copies default.", "  \t\t-installDefaultCountersDB Backs up old counters DB, and copies default.", "  \t\t-updateQREEPROM:\"<c:\asdf>\") Updates the images on the QR device, path is optional, if not provided it will use the assembly/qr-chamfer.", "  \t\t-configureDoorSensors:on|off Updates HAL to include/exclude door sensors.", "  \t\t-configureVMZ Configures the HAL instance for a VMZ machine.", "  \t\t-dumpTouchScreens Dumps touchscreen monitor info to TouchScreen.txt.", "  \t\t-backupConfig Backs up the hal.xml & gamp files.", "  \t\t-analyzeEmptyStuck:fullDump Analyzes empty/stuck in vend logs. If file is specified, will analyze just that file.", "  \t\t--computeHash=<vid>:<pid> Computes a secure CCR hash.",
			"  \t\t-fraudStressTest [--scanPause:pause --iterations:iter --readPause:rp] Runs a stress test on the fraud sensor."
		};
		PrintUsageAndExit(secureOptions);
	}

	private static void DumpDevices(DeviceClass clazz)
	{
		string path = MakeLogPath((clazz == DeviceClass.None) ? "AllDevices.txt" : (clazz.ToString() + ".txt"), delete: true);
		using StreamWriter writer = new StreamWriter(path);
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		service.EnumDevices(clazz, writer);
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

	private static void Main(string[] args)
	{
		IRuntimeService runtimeService = new RuntimeService();
		ServiceLocator.Instance.AddService(typeof(IRuntimeService), runtimeService);
		UsbDeviceService usbDeviceService = new UsbDeviceService(debug: true);
		ServiceLocator.Instance.AddService(typeof(IUsbDeviceService), usbDeviceService);
		ServiceLocator.Instance.AddService(typeof(IDeviceSetupClassFactory), new DeviceSetupClassFactory());
		TouchscreenService touchscreenService = new TouchscreenService();
		ServiceLocator.Instance.AddService(typeof(ITouchscreenService), touchscreenService);
		if (args.Length == 0)
		{
			PrintUsageAndExit();
		}
		ClientHelper clientHelper = new ClientHelper();
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("-randomSync"))
			{
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
				continue;
			}
			if (args[i] == "-ci")
			{
				new ComputerInfo().Display();
				continue;
			}
			if (args[i].StartsWith("--pipeServer"))
			{
				string defVal = "HAL7001";
				defVal = CommandLineOption.GetOptionVal(args[i], defVal);
				PipeMessageServer pipeMessageServer = new PipeMessageServer(defVal);
				pipeMessageServer.Run();
				continue;
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
				continue;
			}
			if (args[i].StartsWith("--findTripplite"))
			{
				bool optionVal = CommandLineOption.GetOptionVal(args[i], defVal: false);
				new ConsoleLogger();
				TrippliteVersion trippliteVersion = TrippliteVersion.None;
				using (DefaultConsole defaultConsole = new DefaultConsole())
				{
					using (TrippliteUtilities trippliteUtilities = new TrippliteUtilities(optionVal))
					{
						trippliteVersion = trippliteUtilities.FindTripplite(defaultConsole);
					}
					defaultConsole.WriteLine("Find Tripplite version returned {0}", trippliteVersion.ToString());
				}
				Environment.Exit((trippliteVersion == TrippliteVersion.None) ? 1 : 0);
				continue;
			}
			if (args[i].StartsWith("--extractTrippliteInstaller"))
			{
				bool optionVal2 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				using (StreamWriter writer = new StreamWriter("Extract.log "))
				{
					using TrippliteUtilities trippliteUtilities2 = new TrippliteUtilities(optionVal2);
					trippliteUtilities2.Extract(writer);
				}
				Environment.Exit(0);
				continue;
			}
			if (args[i] == "-findABEDevice")
			{
				new ConsoleLogger();
				HardwareService hardwareService = clientHelper.AutoConnect();
				Console.WriteLine("ABE device {0}", hardwareService.HasABEDevice ? "found" : "not found");
				continue;
			}
			if (args[i] == "-findABEDeviceThroughScan")
			{
				AbeDeviceDescriptor descriptor = new AbeDeviceDescriptor();
				IUsbDeviceSearchResult usbDeviceSearchResult = usbDeviceService.FindDevice(descriptor);
				Console.WriteLine("ABE device {0}", usbDeviceSearchResult.Found ? "found" : "not found");
				continue;
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
				using (FraudSensorStressTest fraudSensorStressTest = new FraudSensorStressTest(clientHelper.AutoConnect(), num2, num3, num4))
				{
					fraudSensorStressTest.Run();
				}
				continue;
			}
			if ("-findKFCDevices" == args[i])
			{
				string path = MakeLogPath("KFC.log", delete: true);
				using (StreamWriter streamWriter = new StreamWriter(path))
				{
					new ConsoleLogger();
					IUsbDeviceService service2 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
					ITouchscreenDescriptor touchscreenDescriptor = touchscreenService.FindTouchScreen(matchDriver: true);
					if (touchscreenDescriptor == null)
					{
						streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
					}
					else
					{
						streamWriter.WriteLine("Found touchscreen HWID {0} {1}", touchscreenDescriptor.ToString(), touchscreenDescriptor.Friendlyname);
					}
					IDeviceDescriptor deviceDescriptor = service2.FindActiveCamera(matchDriver: true);
					if (deviceDescriptor == null)
					{
						streamWriter.WriteLine("Could not find a camera or match its driver.");
					}
					else
					{
						streamWriter.WriteLine("Found camera {0} {1}", deviceDescriptor.ToString(), deviceDescriptor.Friendlyname);
					}
				}
				continue;
			}
			if ("-findActiveTouchScreen" == args[i])
			{
				string path2 = MakeLogPath("TSLocator.log", delete: true);
				using (StreamWriter streamWriter2 = new StreamWriter(path2))
				{
					new ConsoleLogger();
					ServiceLocator.Instance.GetService<IUsbDeviceService>();
					ITouchscreenDescriptor touchscreenDescriptor2 = touchscreenService.FindTouchScreen(matchDriver: true);
					if (touchscreenDescriptor2 == null)
					{
						streamWriter2.WriteLine("Could not find a touchscreen or match its driver.");
					}
					else
					{
						streamWriter2.WriteLine("Found touchscreen HWID {0} {1}", touchscreenDescriptor2.ToString(), touchscreenDescriptor2.Friendlyname);
					}
				}
				continue;
			}
			if ("-find3mFirmwareRevision" == args[i])
			{
				new ConsoleLogger();
				ITouchscreenDescriptor touchscreenDescriptor3 = touchscreenService.FindTouchScreen(matchDriver: true);
				if (touchscreenDescriptor3 == null)
				{
					LogHelper.Instance.Log("Could not find a touchscreen or match its driver.");
					continue;
				}
				LogHelper.Instance.Log("Found touchscreen HWID {0} {1}, FW = {2}", touchscreenDescriptor3.ToString(), touchscreenDescriptor3.Friendlyname, touchscreenDescriptor3.ReadFirmware());
				continue;
			}
			if (args[i].StartsWith("--findActiveCamera"))
			{
				bool optionVal3 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				string path3 = MakeLogPath("CameraLocator.log", delete: true);
				using (StreamWriter streamWriter3 = new StreamWriter(path3))
				{
					new ConsoleLogger();
					IUsbDeviceService service3 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
					IDeviceDescriptor deviceDescriptor2 = service3.FindActiveCamera(matchDriver: true);
					if (deviceDescriptor2 == null)
					{
						streamWriter3.WriteLine("Could not find a camera or match its driver.");
						continue;
					}
					streamWriter3.WriteLine("Found camera {0} {1}", deviceDescriptor2.ToString(), deviceDescriptor2.Friendlyname);
					if (optionVal3)
					{
						streamWriter3.WriteLine("RESET of device {0} returned {1}", deviceDescriptor2.ToString(), deviceDescriptor2.ResetDriver());
					}
				}
				continue;
			}
			if ("-resetControlSystem" == args[i])
			{
				ConsoleLogger consoleLogger = new ConsoleLogger();
				using (ResetControlSystemJob resetControlSystemJob = new ResetControlSystemJob(clientHelper.AutoConnect()))
				{
					resetControlSystemJob.Run();
					consoleLogger.Log($"ResetControlSystem job {resetControlSystemJob.ID} ended with status {resetControlSystemJob.EndStatus.ToString()}", LogEntryType.Info);
				}
				continue;
			}
			if ("-resetMotionControl" == args[i])
			{
				ConsoleLogger consoleLogger2 = new ConsoleLogger();
				using (ResetMotionControlExecutor resetMotionControlExecutor = new ResetMotionControlExecutor(clientHelper.AutoConnect()))
				{
					resetMotionControlExecutor.Run();
					consoleLogger2.Log($"ResetMotionControl job {resetMotionControlExecutor.ID} ended with status {resetMotionControlExecutor.EndStatus.ToString()}", LogEntryType.Info);
				}
				continue;
			}
			if ("-resetPDCMCounter" == args[i])
			{
				Console.WriteLine("Resetting PDCM state ...");
				RegistryStore registryStore = new RegistryStore("SOFTWARE\\Redbox\\HAL");
				registryStore.SetSecretValue("PDCMUpdateCount", 0);
				int secretValue = registryStore.GetSecretValue("PDCMUpdateCount", -1);
				Console.WriteLine("PDCM state now at {0}", secretValue);
				continue;
			}
			if (args[i] == "-interactive")
			{
				string[] programArgs = GetProgramArgs(args, i);
				using (ScriptMode scriptMode = new ScriptMode())
				{
					scriptMode.Run(programArgs);
				}
				continue;
			}
			if (args[i].StartsWith("-resetDeck"))
			{
				string optionVal4 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				string path4 = runtimeService.RuntimePath("resetdecks.txt");
				using (StreamWriter streamWriter4 = new StreamWriter(File.Open(path4, FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					if (string.IsNullOrEmpty(optionVal4))
					{
						streamWriter4.WriteLine("Invalid deck list specified.");
						break;
					}
					StringBuilder stringBuilder = new StringBuilder();
					char[] separator = new char[1] { ',' };
					string[] array2 = optionVal4.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					string[] array3 = array2;
					foreach (string arg in array3)
					{
						streamWriter4.WriteLine(" Resetting deck {0}", arg);
						stringBuilder.AppendLine($" DECK RESET NUMBER=\"{arg}\"");
					}
					HardwareService hardwareService2 = clientHelper.AutoConnect();
					if (hardwareService2 == null)
					{
						streamWriter4.WriteLine("Cannot connect to service. Please try again.");
						break;
					}
					hardwareService2.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeSync"))
			{
				string optionVal5 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
				SyncCheck syncCheck = new SyncCheck(optionVal5);
				syncCheck.Summarize();
				continue;
			}
			if (args[i].StartsWith("-analyzeRunningSync"))
			{
				using (DefaultConsole defaultConsole2 = new DefaultConsole())
				{
					string optionVal6 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
					if (optionVal6 == "EMPTY")
					{
						defaultConsole2.WriteLine("Invalid job id specified.");
						break;
					}
					HardwareService hardwareService3 = clientHelper.AutoConnect();
					if (!hardwareService3.GetJobs(out var jobs).Success)
					{
						defaultConsole2.WriteLine("Unable to get job list.");
						break;
					}
					HardwareJob hardwareJob = null;
					HardwareJob[] array4 = jobs;
					foreach (HardwareJob hardwareJob2 in array4)
					{
						if (hardwareJob2.ID == optionVal6)
						{
							hardwareJob = hardwareJob2;
							break;
						}
					}
					if (hardwareJob == null)
					{
						defaultConsole2.WriteLine("Cannot find job {0}", optionVal6);
						break;
					}
					clientHelper.WaitForJob(hardwareJob, out var endStatus);
					if (endStatus != HardwareJobStatus.Completed)
					{
						defaultConsole2.WriteLine("job {0} ended with status {1}", optionVal6, endStatus.ToString());
						break;
					}
					if (!hardwareJob.GetResults(out var results).Success)
					{
						defaultConsole2.WriteLine("Can't get job results for {0}", optionVal6);
						break;
					}
					int num5 = 0;
					int num6 = 0;
					ProgramResult[] array5 = results;
					foreach (ProgramResult result in array5)
					{
						if (InventoryItemsEqual(defaultConsole2, result))
						{
							num5++;
						}
						else
						{
							num6++;
						}
					}
					defaultConsole2.WriteLine("There were:");
					defaultConsole2.WriteLine("  {0} matches.", num5);
					defaultConsole2.WriteLine("  {0} mismatches.", num6);
					if (num5 + num6 != 717)
					{
						defaultConsole2.WriteLine("Wrong analysis.");
					}
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeEmptyStuck"))
			{
				bool optionVal7 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				EmptyStuckAnalyzer.Instance.AnalyzeByPattern(optionVal7);
				continue;
			}
			if ("-bm" == args[i])
			{
				string text = null;
				string text2 = null;
				int num7 = 0;
				i++;
				while (num7 < 2)
				{
					Console.WriteLine("Arg: {0}", args[i]);
					if (args[i].StartsWith("-startDir"))
					{
						text2 = args[++i];
						if (text2.EndsWith("\\"))
						{
							text2 = text2.Remove(text2.Length - 1, 1);
						}
						Console.WriteLine("Startdirectory: {0}", text2);
					}
					else if (args[i].StartsWith("-output"))
					{
						text = args[++i];
						Console.WriteLine("Outfile: {0}", text);
					}
					num7++;
					i++;
				}
				if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
				{
					Console.WriteLine("Required options for build manifest are not specified.");
					break;
				}
				int trim = text2.Length + 1;
				using (StreamWriter manifestFile = new StreamWriter(text))
				{
					DumpFilesAndRecurse(text2, manifestFile, trim);
				}
				continue;
			}
			if (args[i].StartsWith("-assemblyVersions"))
			{
				string path5 = "c:\\Program Files\\Redbox\\HALService\\bin";
				List<string> list = new List<string>();
				list.AddRange(Directory.GetFiles(path5, "*.dll"));
				Directory.GetFiles(path5, "*.exe");
				string optionVal8 = CommandLineOption.GetOptionVal(args[i], "NONE");
				string text3 = null;
				StreamWriter streamWriter5 = null;
				if (optionVal8 != "NONE")
				{
					text3 = MakeLogPath(optionVal8, delete: true);
					streamWriter5 = new StreamWriter(text3);
				}
				else
				{
					streamWriter5 = new StreamWriter(Console.OpenStandardOutput());
				}
				try
				{
					streamWriter5.WriteLine("Date: {0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
					foreach (string item in list)
					{
						try
						{
							AssemblyName assemblyName = AssemblyName.GetAssemblyName(item);
							streamWriter5.WriteLine("Assembly {0} has version {1}", item, assemblyName.Version.ToString());
						}
						catch (Exception ex)
						{
							streamWriter5.WriteLine("Unable to get version for {0}; message = {1}", item, ex.Message);
						}
					}
				}
				finally
				{
					streamWriter5.Dispose();
				}
				continue;
			}
			if (args[i].StartsWith("-dumpTouchScreens"))
			{
				DumpDevices(DeviceClass.Monitor);
				continue;
			}
			if (args[i].StartsWith("-dumpDevices"))
			{
				DumpDevices(DeviceClass.None);
				continue;
			}
			if (args[i].StartsWith("-backupConfig"))
			{
				using (ServiceConfigurationHelper serviceConfigurationHelper = new ServiceConfigurationHelper(clientHelper))
				{
					serviceConfigurationHelper.BackupConfiguration(KioskID);
				}
				continue;
			}
			if (args[i].StartsWith("-configureVMZ"))
			{
				string path6 = runtimeService.RuntimePath("configure-VMZ.txt");
				using (StreamWriter streamWriter6 = new StreamWriter(File.Open(path6, FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					streamWriter6.WriteLine("The upgrade process is no longer supported.");
				}
				continue;
			}
			if (args[i].StartsWith("-configureDoorSensors"))
			{
				string optionValue = CommandLineOption.GetOptionValue(args[i]);
				if (string.IsNullOrEmpty(optionValue))
				{
					PrintSecureHelp();
				}
				string inst = $"DOORSENSORS SOFTWAREOVERRIDE OVERRIDE=\"{optionValue}\"";
				RunImmediateInstruction(inst, clientHelper);
				continue;
			}
			if (args[i].Contains("-checkInventory"))
			{
				string optionValue2 = CommandLineOption.GetOptionValue(args[i], "=");
				string path7 = MakeLogPath("InventoryCompare.txt", delete: true);
				using (StreamWriter log = new StreamWriter(path7))
				{
					InventoryChecker.Instance.CheckInventory(clientHelper.AutoConnect(), optionValue2, log);
				}
				continue;
			}
			if ("-dumpQRStats" == args[i])
			{
				RunImmediateInstruction("QUICKRETURN DUMPCOUNTERS", clientHelper);
				continue;
			}
			if ("-resetDupCounter" == args[i])
			{
				HardwareService hardwareService4 = clientHelper.AutoConnect();
				if (hardwareService4 != null)
				{
					new CounterHelper().ResetDuplicateCounterValue(hardwareService4);
				}
				continue;
			}
			if ("-checkDupCounter" == args[i])
			{
				string path8 = MakeNewFile("DuplicateCounter.log");
				HardwareService hardwareService5 = clientHelper.AutoConnect();
				if (hardwareService5 == null)
				{
					break;
				}
				CounterHelper counterHelper = new CounterHelper();
				counterHelper.GetDuplicateCounterValue(hardwareService5, out var val);
				if (val > 0)
				{
					using StreamWriter streamWriter7 = new StreamWriter(path8);
					streamWriter7.WriteLine("The kiosk has duplicates.");
					streamWriter7.Flush();
				}
				continue;
			}
			if (args[i].Equals("-restartQR"))
			{
				RunImmediateInstruction("QUICKRETURN RESTART", clientHelper);
				continue;
			}
			if (args[i].Equals("-stopQR"))
			{
				RunImmediateInstruction("QUICKRETURN HALTPOLL", clientHelper);
				continue;
			}
			if (args[i].Equals("-checkQRStatus"))
			{
				new QuickReturnHelper().ProperlyConfigured(clientHelper.AutoConnect());
				continue;
			}
			if (args[i].StartsWith("-grabQRCounters"))
			{
				bool optionVal9 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				new QuickReturnHelper().DumpCounters(clientHelper.AutoConnect(), optionVal9);
				continue;
			}
			if (args[i].StartsWith("-fraudVend"))
			{
				HardwareService hardwareService6 = clientHelper.AutoConnect();
				string text4 = null;
				string text5 = null;
				using (KioskInventory kioskInventory = new KioskInventory(hardwareService6))
				{
					foreach (IInventoryLocation item2 in kioskInventory.DeckInventory)
					{
						if (text4 == null && item2.Location.Deck == 1 && item2.Matrix != "EMPTY" && item2.Matrix != "UNKNOWN")
						{
							text4 = item2.Matrix;
							Console.WriteLine("Selecting item {0} from Deck {1} Slot {2}", item2.Matrix, item2.Location.Deck, item2.Location.Slot);
						}
						else if (text5 == null && item2.Location.Deck == 6 && item2.Matrix != "EMPTY" && item2.Matrix != "UNKNOWN")
						{
							text5 = item2.Matrix;
							Console.WriteLine("Selecting item {0} from Deck {1} Slot {2}", item2.Matrix, item2.Location.Deck, item2.Location.Slot);
						}
					}
				}
				HardwareCommandResult hardwareCommandResult = hardwareService6.PreposVend(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, new string[2] { text4, text5 }, out var job2);
				if (!hardwareCommandResult.Success)
				{
					Console.WriteLine("Failed to schedule job.");
					continue;
				}
				job2.Pend();
				Console.WriteLine("Vend job {0} scheduled", job2.ID);
				Console.WriteLine("Press any key to signal preposition.");
				string arg2 = Console.ReadLine();
				hardwareService6.ExecuteServiceCommand($"job signal job: '{job2.ID}' value: '{arg2}'");
				clientHelper.WaitForJob(job2, out var endStatus2);
				Console.WriteLine("Vend job {0} ended with status {1}", job2.ID, endStatus2.ToString());
				continue;
			}
			if (args[i].StartsWith("-vendTest"))
			{
				int optionVal10 = CommandLineOption.GetOptionVal(args[i], 3);
				int? startAt = null;
				if (args.Length > 1 && args[i + 1].StartsWith("-next"))
				{
					startAt = CommandLineOption.GetOptionVal(args[i + 1], -1);
				}
				VendTest vendTest = new VendTest(optionVal10);
				using DefaultConsole console = new DefaultConsole();
				vendTest.Execute(console, startAt);
				break;
			}
			if (args[i].StartsWith("-scheduleClean"))
			{
				int? optionVal11 = CommandLineOption.GetOptionVal<int?>(args[i], null);
				using (DefaultConsole defaultConsole3 = new DefaultConsole())
				{
					if (optionVal11.HasValue && optionVal11 != -1)
					{
						RedboxTimer redboxTimer = new RedboxTimer("Clean", GenericimerFired);
						redboxTimer.ScheduleAtNext(optionVal11.Value, 0);
						defaultConsole3.WriteLine("Clean timer should fire {0} {1}", redboxTimer.NextFireTime.Value.ToShortDateString(), redboxTimer.NextFireTime.Value.ToShortTimeString());
						GenericWaiter.WaitOne();
					}
					HardwareService hardwareService7 = clientHelper.AutoConnect();
					if (!hardwareService7.CleanZone(out var job3).Success)
					{
						defaultConsole3.WriteLine("Failed to schedule clean job.");
						break;
					}
					job3.Pend();
					clientHelper.WaitForJob(job3, out var endStatus3);
					defaultConsole3.WriteLine("Clean {0} ended with status {1}", job3.ID, endStatus3.ToString());
				}
				continue;
			}
			if ("-e".Equals(args[i]))
			{
				MakeNewFile("empties.txt");
				using (InventoryHelper inventoryHelper = new InventoryHelper(clientHelper))
				{
					inventoryHelper.FixType("empties.txt", "EMPTY", "EMPTY location sync.");
				}
				continue;
			}
			if ("-unknown".Equals(args[i]))
			{
				string fileName = MakeNewFile("unknowns.txt");
				using (InventoryHelper inventoryHelper2 = new InventoryHelper(clientHelper))
				{
					inventoryHelper2.FixType(fileName, "UNKNOWN", "UNKNOWN inventory sync.");
				}
				continue;
			}
			if ("-fullSync".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper3 = new InventoryHelper(clientHelper))
				{
					inventoryHelper3.ScheduleFullSync();
				}
				continue;
			}
			if ("-dumpInventory".Equals(args[i]))
			{
				string path9 = MakeNewFile("inventory.xml");
				using (InventoryHelper inventoryHelper4 = new InventoryHelper(clientHelper))
				{
					inventoryHelper4.DumpInventoryToFile(path9);
				}
				continue;
			}
			if ("-secureHelp".Equals(args[i]))
			{
				PrintSecureHelp();
				continue;
			}
			if ("-help".Equals(args[i]))
			{
				PrintUsageAndExit();
				continue;
			}
			if ("-importInventory".Equals(args[i]))
			{
				if (i >= args.Length - 1)
				{
					PrintUsageAndExit();
				}
				using (InventoryHelper inventoryHelper5 = new InventoryHelper(clientHelper))
				{
					if (inventoryHelper5.ImportInventoryFromFile(args[++i]))
					{
						Console.WriteLine("Inventory import OK.");
					}
					else
					{
						Console.WriteLine("Failed to import Inventory!");
					}
				}
				continue;
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
				using (KioskInventory kioskInventory2 = new KioskInventory())
				{
					IList<IInventoryLocation> deckInventory = kioskInventory2.DeckInventory;
					string path11 = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", path10);
					using StreamWriter streamWriter8 = new StreamWriter(path11);
					streamWriter8.WriteLine("Deck,Slot,Barcode");
					foreach (IInventoryLocation item3 in deckInventory)
					{
						streamWriter8.WriteLine($"{item3.Location.Deck},{item3.Location.Slot},'{item3.Matrix}'");
					}
					int num8 = 1;
					foreach (IDumpbinItem dumpbinItem in kioskInventory2.DumpbinItems)
					{
						streamWriter8.WriteLine($"9,{num8++},'{dumpbinItem.Matrix}'");
					}
				}
				continue;
			}
			if ("-updateConfiguration".Equals(args[i]))
			{
				using (ServiceConfigurationHelper serviceConfigurationHelper2 = new ServiceConfigurationHelper(clientHelper))
				{
					serviceConfigurationHelper2.UpdateOption(args[++i]);
					break;
				}
			}
			if ("-version".Equals(args[i]))
			{
				string path12 = runtimeService.RuntimePath("version.txt");
				Version version = typeof(Program).Assembly.GetName().Version;
				using (StreamWriter streamWriter9 = new StreamWriter(path12))
				{
					streamWriter9.WriteLine(version);
				}
				Console.WriteLine("HALUtilities version: {0}", version);
			}
			else if ("-dumpExcluded".Equals(args[i]))
			{
				string path13 = runtimeService.RuntimePath("excluded.log");
				ClientHelper clientHelper2 = new ClientHelper();
				HardwareService service4 = clientHelper2.AutoConnect();
				using ExcludedLocsJob excludedLocsJob = new ExcludedLocsJob(service4);
				excludedLocsJob.Run();
				using StreamWriter streamWriter10 = new StreamWriter(path13);
				IList<Location> excludedLocations = excludedLocsJob.ExcludedLocations;
				if (excludedLocations.Count == 0)
				{
					streamWriter10.WriteLine("There are no excluded locations.");
					continue;
				}
				streamWriter10.WriteLine("There are {0} locations total.", excludedLocsJob.Results.Count);
				foreach (Location item4 in excludedLocations)
				{
					streamWriter10.WriteLine("Excluded location deck = {0} slot = {1}", item4.Deck, item4.Slot);
				}
			}
			else
			{
				Console.WriteLine("Unrecognized option '{0}'", args[i]);
				PrintUsageAndExit();
			}
		}
	}
}
