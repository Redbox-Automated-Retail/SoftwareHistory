using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace HALUtilities;

internal sealed class Program
{
	private static readonly ManualResetEvent TimerWait = new ManualResetEvent(initialState: false);

	private static readonly ManualResetEvent GenericWaiter = new ManualResetEvent(initialState: false);

	private const string CSVDirectory = "C:\\Program Files\\Redbox\\inventory-snapshots";

	private const int ServiceTimeout = 30000;

	private const string DuplicateCounterFile = "DuplicateCounter.log";

	private static void DisplayEventLogProperties()
	{
		EventLog eventLog = Array.Find(EventLog.GetEventLogs(), (EventLog each) => each.Log == "System");
		if (eventLog == null)
		{
			return;
		}
		foreach (EventLogEntry entry in eventLog.Entries)
		{
			if (entry.InstanceId == 41)
			{
				Console.WriteLine("Entry -> instance id {0} date {1}", entry.InstanceId, entry.TimeGenerated);
			}
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
		Array.ForEach(secureOptions, delegate(string secMsg)
		{
			Console.WriteLine(secMsg);
		});
		Console.WriteLine("  \t\t-help print this message and exit.");
		Environment.Exit(1);
	}

	private static void PrintSecureHelp()
	{
		PrintUsageAndExit(new string[10] { "  \t\t-randomSync  Runs a random sync of the kiosk.", "  \t\t-installDefaultInventoryDB Backs up old inventory DB, and copies default.", "  \t\t-installDefaultCountersDB Backs up old counters DB, and copies default.", "  \t\t-updateQREEPROM:\"<c:\asdf>\") Updates the images on the QR device, path is optional, if not provided it will use the assembly/qr-chamfer.", "  \t\t-configureDoorSensors:on|off Updates HAL to include/exclude door sensors.", "  \t\t-configureVMZ Configures the HAL instance for a VMZ machine.", "  \t\t-dumpTouchScreens Dumps touchscreen monitor info to TouchScreen.txt.", "  \t\t-backupConfig Backs up the hal.xml & gamp files.", "  \t\t-analyzeEmptyStuck:fullDump Analyzes empty/stuck in vend logs. If file is specified, will analyze just that file.", "  \t\t-fraudStressTest [--scanPause:pause --iterations:iter --readPause:rp] Runs a stress test on the fraud sensor." });
	}

	private static void GenericimerFired(object source, ElapsedEventArgs e)
	{
		GenericWaiter.Set();
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
			if (args[i] == "-gameSync")
			{
				HardwareCommandResult hardwareCommandResult = hardwareService.ScheduleJob("games-sync", string.Empty, enableDebugging: false, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.Highest
				}, out var job);
				if (!hardwareCommandResult.Success)
				{
					Console.WriteLine("Failed to schedule job.");
					hardwareCommandResult.Errors.Dump(Console.Out);
				}
				else
				{
					job.Pend();
					clientHelper.WaitForJob(job, out var endStatus);
					Console.WriteLine("Game sync job ended with status {0}", endStatus);
				}
				continue;
			}
			if (args[i].StartsWith("-findFwUpdates"))
			{
				AbeStatsAggregator.Aggregate("c:\\bin\\Storm\\Upgrades", Environment.ProcessorCount);
				continue;
			}
			if (args[i].StartsWith("--aggregateArcusStats"))
			{
				int optionVal = CommandLineOption.GetOptionVal(args[i], Environment.ProcessorCount);
				List<StatsAggregatorFilter> list = new List<StatsAggregatorFilter>();
				list.Add(new DefaultStatsFilter());
				list.Add(new TopOffendersFilter(5));
				list.Add(new FailedCorrectionsFilter());
				HardwareStatsAggregator.Aggregate("C:\\bin\\Arcus_Correction2", optionVal, list);
				continue;
			}
			if (args[i].StartsWith("--getCorrectiveStats"))
			{
				HardwareCorrectionStatistic optionVal2 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal2 == HardwareCorrectionStatistic.None)
				{
					continue;
				}
				using (DumpHardwareStatisticsExecutor dumpHardwareStatisticsExecutor = new DumpHardwareStatisticsExecutor(hardwareService, optionVal2))
				{
					dumpHardwareStatisticsExecutor.Run();
					if (HardwareJobStatus.Completed != dumpHardwareStatisticsExecutor.EndStatus)
					{
						continue;
					}
					string text = $"c:\\Program Files\\Redbox\\KioskLogs\\Service\\{optionVal2}CorrectionStats.txt";
					if (!File.Exists(text))
					{
						continue;
					}
					using KioskCorrectionInfo kioskCorrectionInfo = new KioskCorrectionInfo(text);
					if (kioskCorrectionInfo.Stats.Count == 0)
					{
						try
						{
							File.Delete(text);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Unable to delete stats file '{0}'", text);
							Console.WriteLine(ex.Message);
						}
					}
				}
				continue;
			}
			if (args[i].StartsWith("--removeStats"))
			{
				HardwareCorrectionStatistic optionVal3 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal3 != HardwareCorrectionStatistic.None)
				{
					if (!hardwareService.RemoveHardwareCorrectionStats(optionVal3, new HardwareJobSchedule
					{
						Priority = HardwareJobPriority.High
					}, out var job2).Success)
					{
						Console.WriteLine("Failed to schedule remove stats job.");
						continue;
					}
					clientHelper.WaitForJob(job2, out var endStatus2);
					Console.WriteLine("Remove all stats job {0} ended with status {1}", job2.ID, endStatus2);
				}
				continue;
			}
			if (args[i].StartsWith("-removeAllStats"))
			{
				if (!hardwareService.RemoveHardwareCorrectionStats(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job3).Success)
				{
					Console.WriteLine("Failed to schedule remove all stats job.");
					continue;
				}
				clientHelper.WaitForJob(job3, out var endStatus3);
				Console.WriteLine("Remove all stats job {0} ended with status {1}", job3.ID, endStatus3);
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
				int num2 = 1;
				int num3 = 1;
				for (int j = i + 1; j < args.Length; j++)
				{
					if (args[j].StartsWith("--iterations"))
					{
						num = CommandLineOption.GetOptionVal(args[j], num);
					}
					else if (args[j].StartsWith("--vendTime"))
					{
						num2 = CommandLineOption.GetOptionVal(args[j], num2);
					}
					else if (args[j].StartsWith("--vendFrequency"))
					{
						num3 = CommandLineOption.GetOptionVal(args[j], num3);
					}
				}
				int exitCode = 0;
				for (int k = 1; k <= num; k++)
				{
					Console.WriteLine("Iteration {0} of {1} total.", k, num);
					using RandomSyncExecutor randomSyncExecutor = new RandomSyncExecutor(hardwareService, num2, num3);
					randomSyncExecutor.Run();
					Console.WriteLine("Job {0} ends with status {1}", randomSyncExecutor.ID, randomSyncExecutor.EndStatus.ToString());
					if (HardwareJobStatus.Completed != randomSyncExecutor.EndStatus)
					{
						Console.WriteLine("** Tool is exiting after job failure **");
						exitCode = 1;
						break;
					}
				}
				Environment.Exit(exitCode);
				continue;
			}
			if (args[i].StartsWith("--loopy"))
			{
				LoopyExecutor.RunExecutor(CommandLineOption.GetOptionVal(args[i], 10), hardwareService);
				break;
			}
			if (args[i].StartsWith("--msgServer"))
			{
				AbstractPipeSever.RunServer(args[i], PipeServers.Message, flag);
				continue;
			}
			if (args[i].StartsWith("--pipeServer"))
			{
				AbstractPipeSever.RunServer(args[i], PipeServers.Basic, flag);
				continue;
			}
			if (args[i] == "-rdbxPipeServer")
			{
				ServiceHostRunner.Run(Constants.HALIPCStrings.PipeServer, flag);
				continue;
			}
			if (args[i] == "-rdbxTcpServer")
			{
				ServiceHostRunner.Run(Constants.HALIPCStrings.TcpServer, flag);
				continue;
			}
			if ("-kioskTest" == args[i])
			{
				Environment.Exit(KioskTestRunner.RunKioskTest(GetProgramArgs(args, i), hardwareService));
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
			if ("-findKFCDevices" == args[i])
			{
				string path = "KFC.log";
				using (StreamWriter streamWriter = new StreamWriter(runtimeService.CreateBackup(path)))
				{
					new ConsoleLogger(flag);
					ITouchscreenDescriptor touchscreenDescriptor = usbDeviceService.FindTouchScreen(matchDriver: true);
					if (touchscreenDescriptor == null)
					{
						streamWriter.WriteLine("Could not find a touchscreen or match its driver.");
					}
					else
					{
						streamWriter.WriteLine("Found touchscreen HWID {0} {1}", touchscreenDescriptor.ToString(), touchscreenDescriptor.Friendlyname);
					}
					IDeviceDescriptor deviceDescriptor = ServiceLocator.Instance.GetService<IUsbDeviceService>().FindActiveCamera(matchDriver: true);
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
				bool optionVal4 = CommandLineOption.GetOptionVal(args[i], defVal: false);
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
					if (optionVal4)
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
				string optionVal5 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				string path4 = runtimeService.RuntimePath("resetdecks.txt");
				StreamWriter vl = new StreamWriter(File.Open(path4, FileMode.Append, FileAccess.Write, FileShare.Read));
				try
				{
					if (string.IsNullOrEmpty(optionVal5))
					{
						vl.WriteLine("Invalid deck list specified.");
						break;
					}
					StringBuilder buffer = new StringBuilder();
					char[] separator = new char[1] { ',' };
					Array.ForEach(optionVal5.Split(separator, StringSplitOptions.RemoveEmptyEntries), delegate(string d)
					{
						vl.WriteLine(" Resetting deck {0}", d);
						buffer.AppendLine($" DECK RESET NUMBER=\"{d}\"");
					});
					if (hardwareService == null)
					{
						vl.WriteLine("Cannot connect to service. Please try again.");
						break;
					}
					hardwareService.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(buffer.ToString()), out var _);
				}
				finally
				{
					if (vl != null)
					{
						((IDisposable)vl).Dispose();
					}
				}
				continue;
			}
			if (args[i].StartsWith("--analyzeSync"))
			{
				new SyncCheck(CommandLineOption.GetOptionVal(args[i], "EMPTY"), runtimeService).Summarize();
				continue;
			}
			if (args[i].StartsWith("--analyzeRunningSync"))
			{
				using (DefaultConsole defaultConsole = new DefaultConsole())
				{
					string optionVal6 = CommandLineOption.GetOptionVal(args[i], "EMPTY");
					if (optionVal6 == "EMPTY")
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
					HardwareJob[] array = jobs;
					foreach (HardwareJob hardwareJob2 in array)
					{
						if (hardwareJob2.ID == optionVal6)
						{
							hardwareJob = hardwareJob2;
							break;
						}
					}
					if (hardwareJob == null)
					{
						defaultConsole.WriteLine("Cannot find job {0}", optionVal6);
						break;
					}
					clientHelper.WaitForJob(hardwareJob, out var endStatus4);
					if (endStatus4 != HardwareJobStatus.Completed)
					{
						defaultConsole.WriteLine("job {0} ended with status {1}", optionVal6, endStatus4.ToString());
						break;
					}
					using InventoryHelper inventoryHelper = new InventoryHelper(hardwareService);
					inventoryHelper.CompareSyncResults(hardwareJob, outputSuccessfulCompare: false);
				}
				continue;
			}
			if (args[i].StartsWith("-analyzeEmptyStuck"))
			{
				bool optionVal7 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				EmptyStuckAnalyzer.Instance.AnalyzeByPattern(optionVal7);
				continue;
			}
			if (args[i].StartsWith("-assemblyVersions"))
			{
				string path5 = "c:\\Program Files\\Redbox\\HALService\\bin";
				List<string> list2 = new List<string>();
				string[] array2 = new string[2] { "*.dll", "*.exe" };
				foreach (string searchPattern in array2)
				{
					list2.AddRange(Directory.GetFiles(path5, searchPattern));
				}
				Console.WriteLine("{0} ( {1} files total )", DateTime.Now, list2.Count);
				list2.ForEach(delegate(string assembly)
				{
					try
					{
						AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly);
						Console.WriteLine("Assembly {0} has version {1}", assembly, assemblyName.Version.ToString());
					}
					catch (Exception ex4)
					{
						Console.WriteLine("Unable to get version for {0}; message = {1}", assembly, ex4.Message);
					}
				});
				continue;
			}
			if (args[i].StartsWith("-dumpDevices"))
			{
				string path6 = "AllDevices.txt";
				runtimeService.CreateBackup(path6);
				using (StreamWriter streamWriter4 = new StreamWriter(path6))
				{
					Dictionary<string, List<string>> deviceMap = new Dictionary<string, List<string>>();
					Action<string, string> onDeviceFound = delegate(string clazz, string hwid)
					{
						List<string> list4 = null;
						list4 = (deviceMap.ContainsKey(clazz) ? deviceMap[clazz] : (deviceMap[clazz] = new List<string>()));
						list4.Add(hwid);
					};
					usbDeviceService.EnumDevices(onDeviceFound);
					foreach (string key in deviceMap.Keys)
					{
						streamWriter4.WriteLine("Devices for class {0}", key);
						foreach (string item in deviceMap[key])
						{
							streamWriter4.WriteLine("  HWID: {0}", item);
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
				using (StreamWriter streamWriter5 = new StreamWriter(File.Open(runtimeService.RuntimePath("configure-VMZ.txt"), FileMode.Append, FileAccess.Write, FileShare.Read)))
				{
					streamWriter5.WriteLine("The upgrade process is no longer supported.");
				}
				continue;
			}
			if (args[i].StartsWith("-configureDoorSensors"))
			{
				string optionValue2 = CommandLineOption.GetOptionValue(args[i]);
				if (string.IsNullOrEmpty(optionValue2))
				{
					Console.WriteLine(" The -configureDoorSensors switch requires an argument");
					PrintSecureHelp();
				}
				HardwareCommandResult hardwareCommandResult2 = hardwareService.ScheduleJob("door-sensor-software-override", string.Empty, enableDebugging: false, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.Highest
				}, out var job5);
				if (!hardwareCommandResult2.Success)
				{
					Console.WriteLine("Failed to schedule configuration job.");
					hardwareCommandResult2.Errors.Dump(Console.Out);
					continue;
				}
				job5.Push(optionValue2);
				job5.Pend();
				clientHelper.WaitForJob(job5, out var endStatus5);
				Console.WriteLine("Change door sensor configuration ended with status {0}", endStatus5);
				continue;
			}
			if (args[i].Contains("-checkInventory"))
			{
				string optionValue3 = CommandLineOption.GetOptionValue(args[i], "=");
				string path7 = "InventoryCompare.txt";
				runtimeService.CreateBackup(path7);
				using (StreamWriter log = new StreamWriter(path7))
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
					using StreamWriter streamWriter6 = new StreamWriter("DuplicateCounter.log");
					streamWriter6.WriteLine("The kiosk has duplicates.");
					streamWriter6.Flush();
				}
				continue;
			}
			if (args[i].Equals("-checkQRStatus"))
			{
				new QuickReturnHelper(hardwareService).ProperlyConfigured();
				continue;
			}
			if ("-grabQRCounters" == args[i] || "-dumpQRStats" == args[i])
			{
				bool optionVal8 = CommandLineOption.GetOptionVal(args[i], defVal: false);
				new QuickReturnHelper(hardwareService).DumpCounters(optionVal8);
				continue;
			}
			if (args[i].StartsWith("--preposVend"))
			{
				int optionVal9 = CommandLineOption.GetOptionVal(args[i], 3);
				using (DefaultConsole defaultConsole2 = new DefaultConsole())
				{
					using PrepositionTest prepositionTest = new PrepositionTest(optionVal9, hardwareService, defaultConsole2);
					defaultConsole2.Write("Type C for continue, otherwise bail:   ");
					string text2 = Console.ReadLine();
					prepositionTest.PreposJob.Signal((text2 == "C") ? "continue" : "bail");
					clientHelper.WaitForJob(prepositionTest.PreposJob, out var endStatus6);
					defaultConsole2.WriteLine("Preposition job {0} ended with status {1}", prepositionTest.PreposJob.ID, endStatus6.ToString());
				}
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
				using (VendTest vendTest = new VendTest(optionVal10, hardwareService))
				{
					using DefaultConsole console = new DefaultConsole();
					vendTest.Execute(console, startAt);
				}
				continue;
			}
			if (args[i].StartsWith("--scheduleClean"))
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
					if (!hardwareService.CleanZone(out var job6).Success)
					{
						defaultConsole3.WriteLine("Failed to schedule clean job.");
						break;
					}
					job6.Pend();
					clientHelper.WaitForJob(job6, out var endStatus7);
					defaultConsole3.WriteLine("Clean {0} ended with status {1}", job6.ID, endStatus7.ToString());
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
				string optionVal12 = CommandLineOption.GetOptionVal(args[i], string.Empty);
				SyncOptions options = SyncOptions.None;
				if (string.Empty != optionVal12)
				{
					string[] array3 = optionVal12.Split(';');
					if (array3.Length == 1)
					{
						options = Enum<SyncOptions>.ParseIgnoringCase(array3[0], SyncOptions.None);
					}
					else
					{
						Array.ForEach(array3, delegate(string option)
						{
							SyncOptions syncOptions = Enum<SyncOptions>.ParseIgnoringCase(option, SyncOptions.None);
							options |= syncOptions;
						});
					}
				}
				new ConsoleLogger((options & SyncOptions.Debug) != 0);
				using (InventoryHelper inventoryHelper4 = new InventoryHelper(hardwareService))
				{
					inventoryHelper4.ScheduleFullSync(options);
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
				string path8 = runtimeService.RuntimePath("inventory.xml");
				runtimeService.CreateBackup(path8);
				using (InventoryHelper inventoryHelper6 = new InventoryHelper(hardwareService))
				{
					inventoryHelper6.DumpInventoryToFile(path8);
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
				int num5 = 1;
				DateTime now = DateTime.Now;
				string empty = string.Empty;
				do
				{
					string path9 = $"{runtimeService.KioskId}-{now.Month}-{now.Day}-{now.Year}-{num5++}.csv";
					empty = Path.Combine("C:\\Program Files\\Redbox\\inventory-snapshots", path9);
				}
				while (File.Exists(empty));
				using (KioskInventory kioskInventory = new KioskInventory(hardwareService))
				{
					IList<IInventoryLocation> deckInventory = kioskInventory.DeckInventory;
					using StreamWriter streamWriter7 = new StreamWriter(empty);
					streamWriter7.WriteLine("Deck,Slot,Barcode");
					foreach (IInventoryLocation item2 in deckInventory)
					{
						streamWriter7.WriteLine($"{item2.Location.Deck},{item2.Location.Slot},'{item2.Matrix}'");
					}
					int num6 = 1;
					foreach (IDumpbinItem dumpbinItem in kioskInventory.DumpbinItems)
					{
						streamWriter7.WriteLine($"9,{num6++},'{dumpbinItem.Matrix}'");
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
				string path10 = runtimeService.RuntimePath("version.txt");
				Version version = typeof(Program).Assembly.GetName().Version;
				using (StreamWriter streamWriter8 = new StreamWriter(path10))
				{
					streamWriter8.WriteLine(version);
				}
				Console.WriteLine("HALUtilities version: {0}", version);
			}
			else if ("-dumpExcluded".Equals(args[i]))
			{
				string path11 = runtimeService.RuntimePath("excluded.log");
				using ExcludedLocsJob excludedLocsJob = new ExcludedLocsJob(hardwareService);
				excludedLocsJob.Run();
				using StreamWriter streamWriter9 = new StreamWriter(path11);
				IList<Location> excludedLocations = excludedLocsJob.ExcludedLocations;
				if (excludedLocations.Count == 0)
				{
					streamWriter9.WriteLine("There are no excluded locations.");
					continue;
				}
				streamWriter9.WriteLine("There are {0} locations total.", excludedLocsJob.Results.Count);
				foreach (Location item3 in excludedLocations)
				{
					streamWriter9.WriteLine("Excluded location deck = {0} slot = {1}", item3.Deck, item3.Slot);
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
				HardwareCorrectionStatistic optionVal13 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal13 != HardwareCorrectionStatistic.None)
				{
					using DumpHardwareStatisticsExecutor dumpHardwareStatisticsExecutor2 = new DumpHardwareStatisticsExecutor(hardwareService, optionVal13);
					dumpHardwareStatisticsExecutor2.Run();
				}
			}
			else if (args[i].StartsWith("--resetStats"))
			{
				HardwareCorrectionStatistic optionVal14 = CommandLineOption.GetOptionVal(args[i], HardwareCorrectionStatistic.None);
				if (optionVal14 != HardwareCorrectionStatistic.None)
				{
					using ResetHardwareStatisticsExecutor resetHardwareStatisticsExecutor = new ResetHardwareStatisticsExecutor(hardwareService, optionVal14);
					resetHardwareStatisticsExecutor.Run();
				}
			}
			else if (args[i] == "-resetTouchscreen")
			{
				new ConsoleLogger(enableDebug: true);
				if (hardwareService.ResetTouchscreenController(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job7).Success)
				{
					clientHelper.WaitForJob(job7, out var endStatus8);
					Console.WriteLine("Reset touchscreen controller ended with status {0}", endStatus8.ToString());
				}
				else
				{
					Console.WriteLine("Unable to schedule reset touchscreen job.");
				}
			}
			else if (args[i].StartsWith("--dumpKFCSessions"))
			{
				OutputModes optionVal15 = CommandLineOption.GetOptionVal(args[i], OutputModes.Console);
				using KioskFunctionCheckHelper kioskFunctionCheckHelper = new KioskFunctionCheckHelper(hardwareService);
				kioskFunctionCheckHelper.DumpSessions(optionVal15);
			}
			else if (args[i] == "-ipcClient")
			{
				IpcClientTester.RunTest(GetProgramArgs(args, i));
				Environment.Exit(0);
			}
			else if (args[i] == "-powerhouseBackup")
			{
				PowerhouseUtilities.Run(PowerhouseOperations.Backup, runtimeService);
			}
			else if (args[i] == "-powerhouseRestore")
			{
				PowerhouseUtilities.Run(PowerhouseOperations.Restore, runtimeService);
			}
			else if (args[i].StartsWith("--hashFile"))
			{
				string optionValue4 = CommandLineOption.GetOptionValue(args[i], "=");
				if (string.IsNullOrEmpty(optionValue4))
				{
					Console.WriteLine("Invalid file specified - correct the command line.");
					continue;
				}
				TripleDesEncryptionService tripleDesEncryptionService = new TripleDesEncryptionService();
				try
				{
					Console.WriteLine("{0} --> {1}", optionValue4, tripleDesEncryptionService.HashFile(optionValue4));
				}
				catch (Exception ex3)
				{
					Console.WriteLine("Hash failed with an exception:");
					Console.WriteLine(ex3.Message);
					Console.WriteLine(ex3.StackTrace);
				}
			}
			else if (args[i].StartsWith("-initSys"))
			{
				new ConsoleLogger(enableDebug: true);
				int exitCode2 = 0;
				using (InitJob initJob = new InitJob(hardwareService))
				{
					initJob.Job.EventRaised += delegate(HardwareJob hardwareJob3, DateTime eventTime, string eventMessage)
					{
						LogHelper.Instance.Log("{0}: {1}", eventTime, eventMessage);
					};
					initJob.Run();
					initJob.Results.ForEach(delegate(ProgramResult pr)
					{
						LogHelper.Instance.Log("  Code:{0} Message:{1} ", pr.Code, pr.Message);
					});
					LogHelper.Instance.Log("Init job {0} ends with status {1}", initJob.ID, initJob.EndStatus);
					exitCode2 = ((HardwareJobStatus.Completed != initJob.EndStatus) ? (-1) : 0);
				}
				Environment.Exit(exitCode2);
			}
			else if (args[i].StartsWith("-powerCycleRouter"))
			{
				using PowerCycleRouterExecutor powerCycleRouterExecutor = new PowerCycleRouterExecutor(hardwareService);
				powerCycleRouterExecutor.Run();
				Console.WriteLine("Power cycle router job {0} ended with status {1}", powerCycleRouterExecutor.ID, powerCycleRouterExecutor.EndStatus);
			}
			else if (args[i] == "-qlmUnload")
			{
				HardwareCommandResult hardwareCommandResult3 = hardwareService.Unload(new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.High
				}, out var job8);
				if (hardwareCommandResult3.Success)
				{
					job8.Pend();
					Console.WriteLine("{0} ( ID = {1} ) started", job8.ProgramName, job8.ID);
				}
				else
				{
					Console.WriteLine("Failed to schedule job");
					hardwareCommandResult3.Errors.Dump(Console.Out);
				}
			}
			else if (args[i] == "-qlmThin")
			{
				List<string> list3 = new List<string>();
				using (new DisposeableList<string>(list3))
				{
					for (int num7 = i + 1; num7 < args.Length; num7++)
					{
						list3.Add(args[num7]);
					}
					HardwareCommandResult hardwareCommandResult4 = hardwareService.Thin(list3.ToArray(), new HardwareJobSchedule
					{
						Priority = HardwareJobPriority.High
					}, out var job9);
					if (hardwareCommandResult4.Success)
					{
						job9.Pend();
						Console.WriteLine("{0} ( ID = {1} ) started", job9.ProgramName, job9.ID);
					}
					else
					{
						Console.WriteLine("Failed to schedule job");
						hardwareCommandResult4.Errors.Dump(Console.Out);
					}
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
