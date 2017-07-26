using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework.Server;
using Redbox.IPC.Framework;

namespace HALUtilities;

internal sealed class Program
{
	private static readonly ManualResetEvent TimerWait = new ManualResetEvent(initialState: false);

	private static readonly ManualResetEvent GenericWaiter = new ManualResetEvent(initialState: false);

	private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";

	private const int ServiceTimeout = 30000;

	private const string DuplicateCounterFile = "DuplicateCounter.log";

	private static string ServicePath => "c:\\Program Files\\Redbox\\HALService\\bin";

	private static void WriteToOutput(string m)
	{
	}

	private static bool RunImmediateInstruction(string inst, HardwareService service)
	{
		HardwareJob job;
		return service.ExecuteImmediate(inst, out job).Success;
	}

	private static void DumpFilesAndRecurse(string directory, TextWriter manifestFile, int trim)
	{
		string[] files = Directory.GetFiles(directory);
		for (int i = 0; i < files.Length; i++)
		{
			string value = files[i].Remove(0, trim);
			manifestFile.WriteLine(value);
		}
		files = Directory.GetDirectories(directory);
		for (int i = 0; i < files.Length; i++)
		{
			DumpFilesAndRecurse(files[i], manifestFile, trim);
		}
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
		for (int i = 0; i < secureOptions.Length; i++)
		{
			Console.WriteLine(secureOptions[i]);
		}
		Console.WriteLine("  \t\t-help print this message and exit.");
		Environment.Exit(1);
	}

	private static void PrintSecureHelp()
	{
		PrintUsageAndExit(new string[11]
		{
			"  \t\t-randomSync  Runs a random sync of the kiosk.", "  \t\t-installDefaultInventoryDB Backs up old inventory DB, and copies default.", "  \t\t-installDefaultCountersDB Backs up old counters DB, and copies default.", "  \t\t-updateQREEPROM:\"<c:\asdf>\") Updates the images on the QR device, path is optional, if not provided it will use the assembly/qr-chamfer.", "  \t\t-configureDoorSensors:on|off Updates HAL to include/exclude door sensors.", "  \t\t-configureVMZ Configures the HAL instance for a VMZ machine.", "  \t\t-dumpTouchScreens Dumps touchscreen monitor info to TouchScreen.txt.", "  \t\t-backupConfig Backs up the hal.xml & gamp files.", "  \t\t-analyzeEmptyStuck:fullDump Analyzes empty/stuck in vend logs. If file is specified, will analyze just that file.", "  \t\t--computeHash=<vid>:<pid> Computes a secure CCR hash.",
			"  \t\t-fraudStressTest [--scanPause:pause --iterations:iter --readPause:rp] Runs a stress test on the fraud sensor."
		});
	}

	private static void GenericimerFired(object source, ElapsedEventArgs e)
	{
		GenericWaiter.Set();
	}

	private static void RunServiceHost(string protocol, bool debug)
	{
		new ConsoleLogger(debug);
		IpcServiceHostFactory ipcServiceHostFactory = new IpcServiceHostFactory();
		Assembly assembly = typeof(Program).Assembly;
		IHostInfo info = ((IIpcServiceHostFactory)ipcServiceHostFactory).Create(assembly);
		IPCProtocol protocol2 = IPCProtocol.Parse(protocol);
		IIpcServiceHost ipcServiceHost = ((IIpcServiceHostFactory)ipcServiceHostFactory).Create((IIpcProtocol)protocol2, info);
		Thread thread = new Thread(ipcServiceHost.Start);
		thread.Start();
		Console.WriteLine("Press any key to halt.");
		Console.ReadLine();
		ipcServiceHost.Stop();
		thread.Join(5000);
		Console.WriteLine("Program finished.");
		Environment.Exit(0);
	}

	private static void OnRunPipeServer(string cmdLineArg, PipeServers serverType, bool debug)
	{
		new ConsoleLogger(debug);
		string defVal = "HAL7001";
		defVal = CommandLineOption.GetOptionVal(cmdLineArg, defVal);
		AbstractPipeSever abstractPipeSever = serverType switch
		{
			PipeServers.Basic => new BasicPipeServer(defVal), 
			PipeServers.Message => new PipeMessageServer(defVal), 
			_ => null, 
		};
		if (abstractPipeSever != null)
		{
			Thread thread = new Thread(abstractPipeSever.Run);
			thread.Start();
			Console.WriteLine("Press any key to halt.");
			Console.ReadLine();
			abstractPipeSever.Stop();
			thread.Join(5000);
			Console.WriteLine("Program finished.");
		}
		Environment.Exit(0);
	}

	private static string[] GetProgramArgs(string[] allArgs, int start)
	{
		string[] array = new string[allArgs.Length - (start + 1)];
		Array.Copy(allArgs, start + 1, array, 0, array.Length);
		return array;
	}

	private static void Main(string[] args)
	{
		if (args.Length == 0)
		{
			PrintUsageAndExit();
		}
		IRuntimeService runtimeService = new RuntimeService();
		ServiceLocator.Instance.AddService(typeof(IRuntimeService), runtimeService);
		ServiceLocator.Instance.AddService(typeof(IDeviceSetupClassFactory), new DeviceSetupClassFactory());
		UsbDeviceService usbDeviceService = new UsbDeviceService(debug: true);
		ServiceLocator.Instance.AddService(typeof(IUsbDeviceService), usbDeviceService);
		HardwareService hardwareService = new HardwareService(IPCProtocol.Parse(Constants.HALIPCStrings.TcpServer));
		ClientHelper clientHelper = new ClientHelper(hardwareService);
		bool flag = true;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("--protocol"))
			{
				string optionValue = CommandLineOption.GetOptionValue(args[i], "=");
				try
				{
					hardwareService = new HardwareService(IPCProtocol.Parse(optionValue));
					clientHelper.Dispose();
					clientHelper = new ClientHelper(hardwareService);
				}
				catch
				{
				}
				continue;
			}
			if (args[i].StartsWith("--consoleDebug"))
			{
				flag = CommandLineOption.GetOptionVal(args[i], flag);
				continue;
			}
			if (args[i] == "-customerReturn")
			{
				Thread thread = new Thread(CustomerReturn.RunExecutor);
				thread.Start(hardwareService);
				thread.Join();
				break;
			}
			if (args[i].StartsWith("-randomSync"))
			{
				int num = 1;
				int vendTime = 1;
				for (int j = 1; j <= num; j++)
				{
					Console.WriteLine("Iteration {0} of {1} total.", j, num);
					using RandomSyncExecutor randomSyncExecutor = new RandomSyncExecutor(hardwareService, vendTime, 1);
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
			if (args[i].StartsWith("--loopy"))
			{
				int optionVal = CommandLineOption.GetOptionVal(args[i], 10);
				Thread thread2 = new Thread(LoopyExecutor.RunExecutor);
				thread2.Start(optionVal);
				thread2.Join();
				break;
			}
			if (args[i].StartsWith("--msgServer"))
			{
				OnRunPipeServer(args[i], PipeServers.Message, flag);
				continue;
			}
			if (args[i].StartsWith("--pipeServer"))
			{
				OnRunPipeServer(args[i], PipeServers.Basic, flag);
				continue;
			}
			if (args[i] == "-rdbxPipeServer")
			{
				RunServiceHost(Constants.HALIPCStrings.PipeServer, flag);
				continue;
			}
			if (args[i] == "-rdbxTcpServer")
			{
				RunServiceHost(Constants.HALIPCStrings.TcpServer, flag);
				continue;
			}
			if ("-kioskTest" == args[i])
			{
				string[] array = new string[args.Length - 1];
				Array.Copy(args, 1, array, 0, array.Length);
				int exitCode = -1;
				using (KioskTest kioskTest = new KioskTest(array, hardwareService))
				{
					exitCode = kioskTest.Run();
				}
				Environment.Exit(exitCode);
				continue;
			}
			if (args[i] == "-findABEDevice")
			{
				new ConsoleLogger(flag);
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
				using (FraudSensorStressTest fraudSensorStressTest = new FraudSensorStressTest(hardwareService, num2, num3, num4))
				{
					fraudSensorStressTest.Run();
				}
				continue;
			}
			if ("-findKFCDevices" == args[i])
			{
				string path = "KFC.log";
				using (StreamWriter streamWriter = new StreamWriter(runtimeService.CreateBackup(path)))
				{
					new ConsoleLogger(flag);
					IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
					ITouchscreenDescriptor touchscreenDescriptor = usbDeviceService.FindTouchScreen(matchDriver: true);
					if (touchscreenDescriptor == null)
					{
						streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
					}
					else
					{
						streamWriter.WriteLine("Found touchscreen HWID {0} {1}", touchscreenDescriptor.ToString(), touchscreenDescriptor.Friendlyname);
					}
					IDeviceDescriptor deviceDescriptor = service.FindActiveCamera(matchDriver: true);
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
				string path2 = "TSLocator.log";
				runtimeService.CreateBackup(path2);
				using (StreamWriter streamWriter2 = new StreamWriter(path2))
				{
					new ConsoleLogger(flag);
					ITouchscreenDescriptor touchscreenDescriptor2 = usbDeviceService.FindTouchScreen(matchDriver: true);
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
			if (args[i].StartsWith("--findActiveCamera"))
			{
				bool optionVal2 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				string path3 = "CameraLocator.log";
				runtimeService.CreateBackup(path3);
				using (StreamWriter streamWriter3 = new StreamWriter(path3))
				{
					new ConsoleLogger(flag);
					IDeviceDescriptor deviceDescriptor2 = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(matchDriver: true);
					if (deviceDescriptor2 == null)
					{
						streamWriter3.WriteLine("Could not find a camera or match its driver.");
						continue;
					}
					streamWriter3.WriteLine("## Camera Info ## ");
					streamWriter3.WriteLine("  HWID         : {0}", deviceDescriptor2.ToString());
					streamWriter3.WriteLine("  Friendly     : {0}", deviceDescriptor2.Friendlyname);
					streamWriter3.WriteLine("  Status       : {0}", deviceDescriptor2.GetStatus().ToString());
					if (optionVal2)
					{
						streamWriter3.WriteLine("  RESET status : {0}", deviceDescriptor2.ResetDriver() ? "Ok" : "Failed");
					}
				}
				continue;
			}
			if ("-resetControlSystem" == args[i])
			{
				ConsoleLogger consoleLogger = new ConsoleLogger(flag);
				using (ResetControlSystemJob resetControlSystemJob = new ResetControlSystemJob(hardwareService))
				{
					resetControlSystemJob.Run();
					consoleLogger.Log($"ResetControlSystem job {resetControlSystemJob.ID} ended with status {resetControlSystemJob.EndStatus.ToString()}", LogEntryType.Info);
				}
				continue;
			}
			if ("-resetMotionControl" == args[i])
			{
				ConsoleLogger consoleLogger2 = new ConsoleLogger(flag);
				using (ResetMotionControlExecutor resetMotionControlExecutor = new ResetMotionControlExecutor(hardwareService))
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
				using (ScriptMode scriptMode = new ScriptMode(hardwareService))
				{
					scriptMode.Run(programArgs);
				}
				continue;
			}
			if (args[i].StartsWith("-resetDeck"))
			{
				string optionVal3 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				using (StreamWriter streamWriter4 = new StreamWriter(File.Open(runtimeService.RuntimePath("resetdecks.txt"), FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					if (string.IsNullOrEmpty(optionVal3))
					{
						streamWriter4.WriteLine("Invalid deck list specified.");
						break;
					}
					StringBuilder stringBuilder = new StringBuilder();
					char[] separator = new char[1] { ',' };
					string[] array2 = optionVal3.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					foreach (string arg in array2)
					{
						streamWriter4.WriteLine(" Resetting deck {0}", arg);
						stringBuilder.AppendLine($" DECK RESET NUMBER=\"{arg}\"");
					}
					if (hardwareService == null)
					{
						streamWriter4.WriteLine("Cannot connect to service. Please try again.");
						break;
					}
					hardwareService.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
				}
				continue;
			}
			if (args[i].StartsWith("--analyzeSync"))
			{
				new SyncCheck(CommandLineOption.GetOptionVal(args[i], "EMPTY")).Summarize();
				continue;
			}
			if (args[i].StartsWith("--analyzeRunningSync"))
			{
				using (DefaultConsole defaultConsole = new DefaultConsole())
				{
					string optionVal4 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
					if (optionVal4 == "EMPTY")
					{
						defaultConsole.WriteLine("Invalid job id specified.");
						break;
					}
					if (!hardwareService.GetJobs(out var jobs).Success)
					{
						defaultConsole.WriteLine("Unable to get job list.");
						break;
					}
					HardwareJob hardwareJob = null;
					HardwareJob[] array3 = jobs;
					foreach (HardwareJob hardwareJob2 in array3)
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
						break;
					}
					clientHelper.WaitForJob(hardwareJob, out var endStatus);
					if (endStatus != HardwareJobStatus.Completed)
					{
						defaultConsole.WriteLine("job {0} ended with status {1}", optionVal4, endStatus.ToString());
						break;
					}
					using InventoryHelper inventoryHelper = new InventoryHelper(hardwareService);
					inventoryHelper.CompareSyncResults(hardwareJob, outputSuccessfulCompare: false);
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeEmptyStuck"))
			{
				bool optionVal5 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				EmptyStuckAnalyzer.Instance.AnalyzeByPattern(optionVal5);
				continue;
			}
			if ("-bm" == args[i])
			{
				string text = null;
				string text2 = null;
				int num5 = 0;
				i++;
				while (num5 < 2)
				{
					Console.WriteLine("Arg: {0}", args[i]);
					if (args[i].StartsWith("-startDir"))
					{
						text2 = args[++i];
						if (text2.EndsWith("\\"))
						{
							string text3 = text2;
							text2 = text3.Remove(text3.Length - 1, 1);
						}
						Console.WriteLine("Startdirectory: {0}", text2);
					}
					else if (args[i].StartsWith("-output"))
					{
						text = args[++i];
						Console.WriteLine("Outfile: {0}", text);
					}
					num5++;
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
				string path4 = "c:\\Program Files\\Redbox\\HALService\\bin";
				List<string> list = new List<string>();
				string[] array2 = new string[2] { "*.dll", "*.exe" };
				foreach (string searchPattern in array2)
				{
					list.AddRange(Directory.GetFiles(path4, searchPattern));
				}
				Console.WriteLine("{0} ( {1} files total )", DateTime.Now, list.Count);
				foreach (string item in list)
				{
					try
					{
						AssemblyName assemblyName = AssemblyName.GetAssemblyName(item);
						Console.WriteLine("Assembly {0} has version {1}", item, assemblyName.Version.ToString());
					}
					catch (Exception ex)
					{
						Console.WriteLine("Unable to get version for {0}; message = {1}", item, ex.Message);
					}
				}
				continue;
			}
			if (args[i].StartsWith("-dumpDevices"))
			{
				string path5 = "AllDevices.txt";
				runtimeService.CreateBackup(path5);
				using (StreamWriter streamWriter5 = new StreamWriter(path5))
				{
					Dictionary<string, List<string>> deviceMap = new Dictionary<string, List<string>>();
					Action<string, string> onDeviceFound = delegate(string clazz, string hwid)
					{
						List<string> list2 = null;
						list2 = (deviceMap.ContainsKey(clazz) ? deviceMap[clazz] : (deviceMap[clazz] = new List<string>()));
						list2.Add(hwid);
					};
					usbDeviceService.EnumDevices(onDeviceFound);
					foreach (string key in deviceMap.Keys)
					{
						streamWriter5.WriteLine("Devices for class {0}", key);
						foreach (string item2 in deviceMap[key])
						{
							streamWriter5.WriteLine("  HWID: {0}", item2);
						}
					}
					foreach (string key2 in deviceMap.Keys)
					{
						deviceMap[key2].Clear();
					}
				}
				continue;
			}
			if (args[i].StartsWith("-backupConfig"))
			{
				using (ServiceConfigurationHelper serviceConfigurationHelper = new ServiceConfigurationHelper(hardwareService))
				{
					serviceConfigurationHelper.BackupConfiguration(runtimeService.KioskId);
				}
				continue;
			}
			if (args[i].StartsWith("-configureVMZ"))
			{
				using (StreamWriter streamWriter6 = new StreamWriter(File.Open(runtimeService.RuntimePath("configure-VMZ.txt"), FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					streamWriter6.WriteLine("The upgrade process is no longer supported.");
				}
				continue;
			}
			if (args[i].StartsWith("-configureDoorSensors"))
			{
				string optionValue2 = CommandLineOption.GetOptionValue(args[i]);
				if (string.IsNullOrEmpty(optionValue2))
				{
					PrintSecureHelp();
				}
				RunImmediateInstruction($"DOORSENSORS SOFTWAREOVERRIDE OVERRIDE=\"{optionValue2}\"", hardwareService);
				continue;
			}
			if (args[i].Contains("-checkInventory"))
			{
				string optionValue3 = CommandLineOption.GetOptionValue(args[i], "=");
				string path6 = "InventoryCompare.txt";
				runtimeService.CreateBackup(path6);
				using (StreamWriter log = new StreamWriter(path6))
				{
					InventoryChecker.Instance.CheckInventory(hardwareService, optionValue3, log);
				}
				continue;
			}
			if ("-resetDupCounter" == args[i])
			{
				if (hardwareService != null)
				{
					new CounterHelper().ResetDuplicateCounterValue(hardwareService);
				}
				continue;
			}
			if ("-checkDupCounter" == args[i])
			{
				runtimeService.CreateBackup("DuplicateCounter.log");
				HardwareService hardwareService2 = hardwareService;
				if (hardwareService2 == null)
				{
					break;
				}
				new CounterHelper().GetDuplicateCounterValue(hardwareService2, out var val);
				if (val > 0)
				{
					using StreamWriter streamWriter7 = new StreamWriter("DuplicateCounter.log");
					streamWriter7.WriteLine("The kiosk has duplicates.");
					streamWriter7.Flush();
				}
				continue;
			}
			if (args[i].Equals("-restartQR"))
			{
				RunImmediateInstruction("QUICKRETURN RESTART", hardwareService);
				continue;
			}
			if (args[i].Equals("-stopQR"))
			{
				RunImmediateInstruction("QUICKRETURN HALTPOLL", hardwareService);
				continue;
			}
			if (args[i].Equals("-checkQRStatus"))
			{
				new QuickReturnHelper(hardwareService).ProperlyConfigured();
				continue;
			}
			if ("-grabQRCounters" == args[i] || "-dumpQRStats" == args[i])
			{
				bool optionVal6 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				new QuickReturnHelper(hardwareService).DumpCounters(optionVal6);
				continue;
			}
			if (args[i].StartsWith("--preposVend"))
			{
				int optionVal7 = CommandLineOption.GetOptionVal(args[i], 3);
				using (DefaultConsole defaultConsole2 = new DefaultConsole())
				{
					using PrepositionTest prepositionTest = new PrepositionTest(optionVal7, hardwareService, defaultConsole2);
					defaultConsole2.Write("Type C for continue, otherwise bail:   ");
					string text4 = Console.ReadLine();
					prepositionTest.PreposJob.Signal((text4 == "C") ? "continue" : "bail");
					clientHelper.WaitForJob(prepositionTest.PreposJob, out var endStatus2);
					defaultConsole2.WriteLine("Preposition job {0} ended with status {1}", prepositionTest.PreposJob.ID, endStatus2.ToString());
				}
				continue;
			}
			if (args[i].StartsWith("-vendTest"))
			{
				int optionVal8 = CommandLineOption.GetOptionVal(args[i], 3);
				int? startAt = null;
				if (args.Length > 1 && args[i + 1].StartsWith("-next"))
				{
					startAt = CommandLineOption.GetOptionVal(args[i + 1], -1);
				}
				using (VendTest vendTest = new VendTest(optionVal8, hardwareService))
				{
					using DefaultConsole console = new DefaultConsole();
					vendTest.Execute(console, startAt);
				}
				continue;
			}
			if (args[i].StartsWith("--scheduleClean"))
			{
				int? optionVal9 = CommandLineOption.GetOptionVal<int?>(args[i], null);
				using (DefaultConsole defaultConsole3 = new DefaultConsole())
				{
					if (optionVal9.HasValue && optionVal9 != -1)
					{
						RedboxTimer redboxTimer = new RedboxTimer("Clean", GenericimerFired);
						redboxTimer.ScheduleAtNext(optionVal9.Value, 0);
						defaultConsole3.WriteLine("Clean timer should fire {0} {1}", redboxTimer.NextFireTime.Value.ToShortDateString(), redboxTimer.NextFireTime.Value.ToShortTimeString());
						GenericWaiter.WaitOne();
					}
					if (!hardwareService.CleanZone(out var job2).Success)
					{
						defaultConsole3.WriteLine("Failed to schedule clean job.");
						break;
					}
					job2.Pend();
					clientHelper.WaitForJob(job2, out var endStatus3);
					defaultConsole3.WriteLine("Clean {0} ended with status {1}", job2.ID, endStatus3.ToString());
				}
				continue;
			}
			if ("-e".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper2 = new InventoryHelper(hardwareService))
				{
					inventoryHelper2.FixType(SyncType.Empty);
				}
				continue;
			}
			if ("-unknown".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper3 = new InventoryHelper(hardwareService))
				{
					inventoryHelper3.FixType(SyncType.Unknown);
				}
				continue;
			}
			if (args[i].StartsWith("--fullSync"))
			{
				string optionVal10 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				SyncOptions syncOptions = SyncOptions.None;
				if (string.Empty != optionVal10)
				{
					string[] array2 = optionVal10.Split(';');
					for (int l = 0; l < array2.Length; l++)
					{
						SyncOptions syncOptions2 = Enum<SyncOptions>.ParseIgnoringCase(array2[l], SyncOptions.None);
						syncOptions |= syncOptions2;
					}
				}
				new ConsoleLogger((syncOptions & SyncOptions.Debug) != 0);
				using (InventoryHelper inventoryHelper4 = new InventoryHelper(hardwareService))
				{
					inventoryHelper4.ScheduleFullSync(syncOptions);
				}
				continue;
			}
			if ("-fullSync".Equals(args[i]))
			{
				using (InventoryHelper inventoryHelper5 = new InventoryHelper(hardwareService))
				{
					inventoryHelper5.ScheduleFullSync();
				}
				continue;
			}
			if ("-dumpInventory".Equals(args[i]))
			{
				string path7 = runtimeService.RuntimePath("inventory.xml");
				runtimeService.CreateBackup(path7);
				using (InventoryHelper inventoryHelper6 = new InventoryHelper(hardwareService))
				{
					inventoryHelper6.DumpInventoryToFile(path7);
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
				using (InventoryHelper inventoryHelper7 = new InventoryHelper(hardwareService))
				{
					if (inventoryHelper7.ImportInventoryFromFile(args[++i]))
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
				int num6 = 1;
				DateTime now = DateTime.Now;
				string empty = string.Empty;
				do
				{
					string path8 = $"{runtimeService.KioskId}-{now.Month}-{now.Day}-{now.Year}-{num6++}.csv";
					empty = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", path8);
				}
				while (File.Exists(empty));
				using (KioskInventory kioskInventory = new KioskInventory(hardwareService))
				{
					IList<IInventoryLocation> deckInventory = kioskInventory.DeckInventory;
					using StreamWriter streamWriter8 = new StreamWriter(empty);
					streamWriter8.WriteLine("Deck,Slot,Barcode");
					foreach (IInventoryLocation item3 in deckInventory)
					{
						streamWriter8.WriteLine($"{item3.Location.Deck},{item3.Location.Slot},'{item3.Matrix}'");
					}
					int num7 = 1;
					foreach (IDumpbinItem dumpbinItem in kioskInventory.DumpbinItems)
					{
						streamWriter8.WriteLine($"9,{num7++},'{dumpbinItem.Matrix}'");
					}
				}
				continue;
			}
			if ("-updateConfiguration".Equals(args[i]))
			{
				using (ServiceConfigurationHelper serviceConfigurationHelper2 = new ServiceConfigurationHelper(hardwareService))
				{
					serviceConfigurationHelper2.UpdateOption(args[++i]);
					break;
				}
			}
			if ("-version".Equals(args[i]))
			{
				string path9 = runtimeService.RuntimePath("version.txt");
				Version version = typeof(Program).Assembly.GetName().Version;
				using (StreamWriter streamWriter9 = new StreamWriter(path9))
				{
					streamWriter9.WriteLine(version);
				}
				Console.WriteLine("HALUtilities version: {0}", version);
			}
			else if ("-dumpExcluded".Equals(args[i]))
			{
				string path10 = runtimeService.RuntimePath("excluded.log");
				using ExcludedLocsJob excludedLocsJob = new ExcludedLocsJob(hardwareService);
				excludedLocsJob.Run();
				using StreamWriter streamWriter10 = new StreamWriter(path10);
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
			else if (args[i].StartsWith("--hwSurvey"))
			{
				new HardwareSurvey(CommandLineOption.GetOptionVal(args[i], SurveyModes.Console)).Run(hardwareService);
			}
			else if (args[i].StartsWith("--3m"))
			{
				new ConsoleLogger(enableDebug: true);
				new _3MTouchscreen(CommandLineOption.GetOptionVal(args[i], Operation_3m.None)).Execute(Console.Out);
			}
			else if (args[i].StartsWith("--dumpStats"))
			{
				HardwareCorrectionStatistic optionVal11 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal11 != HardwareCorrectionStatistic.None)
				{
					using DumpHardwareStatisticsExecutor dumpHardwareStatisticsExecutor = new DumpHardwareStatisticsExecutor(hardwareService, optionVal11);
					dumpHardwareStatisticsExecutor.Run();
				}
			}
			else if (args[i].StartsWith("--resetStats"))
			{
				HardwareCorrectionStatistic optionVal12 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal12 != HardwareCorrectionStatistic.None)
				{
					using ResetHardwareStatisticsExecutor resetHardwareStatisticsExecutor = new ResetHardwareStatisticsExecutor(hardwareService, optionVal12);
					resetHardwareStatisticsExecutor.Run();
				}
			}
			else if (args[i] == "-resetTouchscreen")
			{
				new ConsoleLogger(enableDebug: true);
				if (hardwareService.ResetTouchscreenController(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job3).Success)
				{
					clientHelper.WaitForJob(job3, out var endStatus4);
					Console.WriteLine("Reset touchscreen controller ended with status {0}", endStatus4.ToString());
				}
				else
				{
					Console.WriteLine("Unable to schedule reset touchscreen job.");
				}
			}
			else if (args[i].StartsWith("--dumpKFCSessions"))
			{
				OutputModes optionVal13 = CommandLineOption.GetOptionVal(args[i], OutputModes.Console);
				using KioskFunctionCheckHelper kioskFunctionCheckHelper = new KioskFunctionCheckHelper(hardwareService);
				kioskFunctionCheckHelper.DumpSessions(optionVal13);
			}
			else
			{
				Console.WriteLine("Unrecognized option '{0}'", args[i]);
				PrintUsageAndExit();
			}
		}
	}
}
