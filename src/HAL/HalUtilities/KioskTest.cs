using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class KioskTest : IDisposable
{
	private HardwareService m_hardwareService;

	private readonly NamedLock m_instanceLock;

	private Guid ProgramLock = new Guid("{7B780DBE-F37B-4f4a-83FA-C59F5B2C87D2}");

	private List<TestLocation> TestLocations = new List<TestLocation>();

	private readonly List<DeckConfig> Decks = new List<DeckConfig>();

	private readonly ClientHelper Helper;

	private readonly double RunTime;

	private readonly int VendFrequency;

	private readonly string DeckList;

	private readonly bool RunOneDiskQuickTestOnly;

	private readonly bool ExcludeSlotsAroundDumpBin;

	private readonly bool ExecuteQuickTest;

	private readonly bool RewriteDataOnSuccess = true;

	internal HardwareService Service
	{
		get
		{
			if (m_hardwareService == null)
			{
				m_hardwareService = Helper.AutoConnect();
				if (m_hardwareService == null)
				{
					Console.WriteLine("Can't connect to hardware service.");
					return m_hardwareService;
				}
			}
			return m_hardwareService;
		}
	}

	private bool ConfiguredForVMZ { get; set; }

	private bool Disposed { get; set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			if (m_instanceLock != null)
			{
				m_instanceLock.Dispose();
			}
		}
	}

	internal KioskTest(string[] args)
	{
		if (args.Length > 0)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("-runtime", StringComparison.CurrentCultureIgnoreCase))
				{
					RunTime = CommandLineOption.GetOptionVal(args[i], 6.5);
					continue;
				}
				if (args[i].StartsWith("-vend"))
				{
					VendFrequency = CommandLineOption.GetOptionVal(args[i], 5);
					continue;
				}
				if (args[i].StartsWith("-decks"))
				{
					DeckList = CommandLineOption.GetOptionVal(args[i], string.Empty);
					continue;
				}
				if (args[i].StartsWith("-excludeAroundDump"))
				{
					ExcludeSlotsAroundDumpBin = true;
					continue;
				}
				if (args[i].StartsWith("-quickTest"))
				{
					ExecuteQuickTest = CommandLineOption.GetOptionVal(args[i], defVal: true);
					continue;
				}
				if (args[i].StartsWith("-rewriteDataOnSuccess"))
				{
					RewriteDataOnSuccess = CommandLineOption.GetOptionVal(args[i], defVal: true);
					continue;
				}
				if (args[i] == "-oneDiskQuickTest")
				{
					RunOneDiskQuickTestOnly = true;
					continue;
				}
				Console.WriteLine("\nList of Commands:\n--------------------\n");
				Console.WriteLine("\t-decks <number>,<number>,<number>,...\tUse only the decks listed");
				Console.WriteLine("\t-runTime <number>\tSet the number of hours for test to run");
				Console.WriteLine("\t-vend <number>\tSet the number of moves between each vend attempt, 0 prevents vends from being attempted");
				Console.WriteLine("\t-help\t\t\tPrints this screen");
				Environment.Exit(0);
			}
		}
		Helper = new ClientHelper();
		m_instanceLock = new NamedLock(ProgramLock.ToString(), LockScope.Local);
	}

	internal int Run()
	{
		string path = Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\KioskTest", "kioskTest.log");
		using (StreamWriter streamWriter = new StreamWriter(path, append: true))
		{
			if (!ConfigureDecks(streamWriter))
			{
				printAndLog("Unable to retrieve decks information from service.", streamWriter);
				return -1;
			}
			if (RunOneDiskQuickTestOnly)
			{
				printAndLog(" ** scheduling one disk quick deck test ** ", streamWriter);
				using OneDiskQuickTest oneDiskQuickTest = new OneDiskQuickTest(m_hardwareService);
				oneDiskQuickTest.Run();
				if (oneDiskQuickTest.EndStatus != HardwareJobStatus.Completed)
				{
					return -1;
				}
				if (!RewriteDataOnSuccess)
				{
					return 0;
				}
				HardwareCommandResult hardwareCommandResult = Service.ExecuteServiceCommand("SERVICE update-gamp backup-directory: @'c:\\gamp\\KentBackup'");
				streamWriter.WriteLine("Update gamp file data: {0}.", hardwareCommandResult.Success ? "OK" : "EPIC FAIL");
				return (!hardwareCommandResult.Success) ? (-1) : 0;
			}
			if (ExecuteQuickTest)
			{
				using QuickDeckTest quickDeckTest = new QuickDeckTest(m_hardwareService);
				printAndLog(" ** scheduling quick deck test ** ", streamWriter);
				quickDeckTest.Run();
				if (HardwareJobStatus.Completed != quickDeckTest.EndStatus)
				{
					streamWriter.WriteLine("The quick test failed - bailing test.");
					return -1;
				}
				printAndLog(" ** quick test SUCCEEDED; schedule adjacent bin test ** ", streamWriter);
				using AdjacentBinTest adjacentBinTest = new AdjacentBinTest(m_hardwareService);
				adjacentBinTest.Run();
				if (adjacentBinTest.EndStatus == HardwareJobStatus.Completed)
				{
					printAndLog("The quick test succeeded.", streamWriter);
				}
			}
			if (!m_instanceLock.IsOwned)
			{
				Console.WriteLine("Only one instance of the Kiosk Test is allowed to run.");
				PerfFunctions.Wait(10000);
				return -1;
			}
			printAndLog($"Kiosk Test Starting - {DateTime.Now}", streamWriter);
			printAndLog("Deck Details", streamWriter);
			printAndLog("-------------------------------------", streamWriter);
			foreach (DeckConfig deck in Decks)
			{
				printAndLog($"Deck {deck.Deck} - Number of Slots {deck.Slots} - Is QLM {deck.IsQlm}", streamWriter);
			}
			printAndLog("-------------------------------------", streamWriter);
			DateTime dateTime = DateTime.Now.AddHours(RunTime);
			printAndLog($"Run until {dateTime}", streamWriter);
			int num = 1;
			int num2 = 0;
			bool flag = false;
			Location location = null;
			Location location2 = null;
			while (DateTime.Now <= dateTime)
			{
				flag = location != null && VendFrequency != 0 && TestLocations.Count % VendFrequency == 0;
				if (TestLocations.Count < 1 || location == null)
				{
					if (location != null)
					{
						Service.CollectGarbage(force: false);
						Console.WriteLine("{4} - Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {5}", location.Deck, location.Slot, Decks[num2].Deck, 1, TestLocations.Count, flag);
						Schedule(location, new Location
						{
							Deck = Decks[num2].Deck,
							Slot = 1
						}, flag, streamWriter);
					}
					num = 1;
					num2 = 0;
					streamWriter.WriteLine("{0} - Populating Location Information", DateTime.Now);
					streamWriter.Flush();
					PopulateLocations();
					Location location3 = new Location();
					location3.Deck = Decks[num2].Deck;
					location3.Slot = 1;
					location = location3;
					location2 = null;
					Console.WriteLine("============Start Fresh : Time Remaining {0}============", dateTime - DateTime.Now);
				}
				else if (num >= Decks[num2].Slots)
				{
					Service.CollectGarbage(force: false);
					Console.WriteLine("{4} - Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {5}", location.Deck, location.Slot, Decks[num2].Deck, 1, TestLocations.Count + 1, flag);
					Schedule(location, new Location
					{
						Deck = Decks[num2].Deck,
						Slot = 1
					}, flag, streamWriter);
					num2++;
					Location location4 = new Location();
					location4.Deck = Decks[num2].Deck;
					location4.Slot = 1;
					location = location4;
					num = 1;
					streamWriter.WriteLine("{0} - Deck {1} finished", DateTime.Now, Decks[num2].Deck);
					streamWriter.Flush();
					Console.WriteLine("----------Iteration {0} : Time Remaining {1}-----------", num2, dateTime - DateTime.Now);
				}
				location2 = SelectTargetLocation();
				Console.WriteLine("{4} - Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {5}", location.Deck, location.Slot, location2.Deck, location2.Slot, TestLocations.Count + 1, flag);
				if (!Schedule(location, location2, flag, streamWriter))
				{
					streamWriter.WriteLine("Job failed to schedule properly");
					streamWriter.Flush();
					Environment.Exit(-1);
				}
				num++;
				location = location2;
			}
			Console.WriteLine("return disk - Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {4}", location.Deck, location.Slot, Decks[num2].Deck, 1, false);
			Schedule(location, new Location
			{
				Deck = Decks[num2].Deck,
				Slot = 1
			}, vend: false, streamWriter);
			if (ConfiguredForVMZ)
			{
				streamWriter.WriteLine("Kiosk is configured for VMZ - schedule load bin job.");
				List<Location> list = new List<Location>();
				foreach (DeckConfig deck2 in Decks)
				{
					Location location5 = new Location();
					location5.Deck = deck2.Deck;
					location5.Slot = 1;
					Location item = location5;
					list.Add(item);
				}
				ScheduleAndBlockLoadBinJob(list);
			}
			if (RewriteDataOnSuccess)
			{
				HardwareCommandResult hardwareCommandResult2 = Service.ExecuteServiceCommand("SERVICE update-gamp backup-directory: @'c:\\gamp\\KentBackup'");
				streamWriter.WriteLine("Update gamp file data: {0}.", hardwareCommandResult2.Success ? "OK" : "EPIC FAIL");
			}
			printAndLog("Kiosk Test Completed", streamWriter);
		}
		return 0;
	}

	private bool ConfigureDecks(StreamWriter log)
	{
		HardwareCommandResult machineInfo = Service.GetMachineInfo(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		}, out var job);
		if (!machineInfo.Success)
		{
			return false;
		}
		Helper.WaitForJob(job, out var endStatus);
		if (endStatus != HardwareJobStatus.Completed)
		{
			log.WriteLine("Get-machine-info returned an error status.");
			return false;
		}
		if (!job.GetResults(out var results).Success)
		{
			log.WriteLine("Unable to get results for get-machine-info job.");
			return false;
		}
		int qlmDeck = -1;
		ConfiguredForVMZ = true;
		ProgramResult[] array = results;
		foreach (ProgramResult programResult in array)
		{
			switch (programResult.Code)
			{
			case "QlmInfo":
				qlmDeck = programResult.Deck;
				break;
			case "DeckInfo":
				Decks.Add(new DeckConfig
				{
					Deck = programResult.Deck,
					Slots = programResult.Slot
				});
				break;
			}
		}
		if (qlmDeck != -1)
		{
			DeckConfig deckConfig = Decks.Find((DeckConfig each) => each.Deck == qlmDeck);
			if (deckConfig != null)
			{
				deckConfig.IsQlm = true;
			}
		}
		if (string.IsNullOrEmpty(DeckList))
		{
			return true;
		}
		List<string> list = new List<string>(DeckList.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
		list.Sort();
		foreach (string item in list)
		{
			if (!int.TryParse(item, out var _d0))
			{
				printAndLog("Unable to parse deck list option.", log);
				Environment.Exit(-1);
			}
			DeckConfig deckConfig2 = Decks.Find((DeckConfig each) => each.Deck == _d0);
			if (deckConfig2 != null)
			{
				Decks.Remove(deckConfig2);
			}
		}
		return true;
	}

	private void PopulateLocations()
	{
		foreach (DeckConfig deck in Decks)
		{
			if (deck.IsQlm)
			{
				for (int i = 2; i < deck.Slots; i += 8)
				{
					TestLocations.Add(new TestLocation(deck, i));
				}
				continue;
			}
			for (int j = 2; j <= deck.Slots; j++)
			{
				if (deck.Deck == 7 && deck.Slots == 72 && j % 6 == 0)
				{
					continue;
				}
				if (deck.Deck == 8)
				{
					if (j != 82 && j != 83 && j != 84 && (!ExcludeSlotsAroundDumpBin || (j != 81 && j != 85)))
					{
						TestLocations.Add(new TestLocation(deck, j));
					}
				}
				else
				{
					TestLocations.Add(new TestLocation(deck, j));
				}
			}
		}
	}

	private Location SelectTargetLocation()
	{
		if (0 >= TestLocations.Count)
		{
			Environment.Exit(-1);
		}
		Random random = new Random(DateTime.Now.Millisecond);
		int index = random.Next(0, TestLocations.Count - 1);
		TestLocation testLocation = TestLocations[index];
		TestLocations.RemoveAt(index);
		return testLocation.Location;
	}

	private bool IsFinished(HardwareJob job)
	{
		if (!job.Status.Equals(HardwareJobStatus.Stopped) && !job.Status.Equals(HardwareJobStatus.Completed))
		{
			return job.Status.Equals(HardwareJobStatus.Errored);
		}
		return true;
	}

	private bool ScheduleAndBlockLoadBinJob(List<Location> locs)
	{
		HardwareJob job = null;
		try
		{
			HardwareCommandResult hardwareCommandResult = Service.LoadBin(locs, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out job);
			if (!hardwareCommandResult.Success)
			{
				Console.WriteLine("Failed to create bin job.");
				return false;
			}
			if (!Helper.WaitForJob(job, out var endStatus))
			{
				Console.WriteLine("Error waiting for load bin job to complete.");
				return false;
			}
			if (endStatus == HardwareJobStatus.Completed)
			{
				return true;
			}
			Environment.Exit(-1);
			return false;
		}
		finally
		{
			job?.Trash();
		}
	}

	private void printAndLog(string message, StreamWriter log)
	{
		Console.WriteLine(message);
		log.WriteLine(message);
	}

	private bool Schedule(Location start, Location end, bool vend, StreamWriter log)
	{
		using (KioskTestJob kioskTestJob = new KioskTestJob(m_hardwareService, start, end, vend))
		{
			kioskTestJob.Run();
			if (HardwareJobStatus.Completed != kioskTestJob.EndStatus)
			{
				log.WriteLine("Job {0} Errored out", kioskTestJob.ID);
				log.Flush();
				Environment.Exit(-1);
			}
		}
		return true;
	}
}
