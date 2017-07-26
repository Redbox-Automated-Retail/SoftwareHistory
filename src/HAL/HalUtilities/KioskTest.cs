using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class KioskTest : IDisposable
{
	private class DeckConfig
	{
		internal int Deck { get; set; }

		internal int Slots { get; set; }

		internal bool IsQlm { get; set; }
	}

	private class TestLocation
	{
		internal Location Location { get; private set; }

		internal bool IsQlm { get; private set; }

		internal TestLocation(DeckConfig config, int slot)
		{
			Location = new Location
			{
				Deck = config.Deck,
				Slot = slot
			};
			IsQlm = config.IsQlm;
		}
	}

	private int Transfers;

	private bool ConfiguredForVMZ;

	private bool Disposed;

	private volatile bool KeepRunningTest = true;

	private readonly StreamWriter Log;

	private readonly List<TestLocation> TestLocations = new List<TestLocation>();

	private readonly List<int> ConfiguredDecks = new List<int>();

	private readonly List<DeckConfig> Decks = new List<DeckConfig>();

	private readonly ClientHelper Helper;

	private readonly double RunTime;

	private readonly int VendFrequency;

	private readonly bool RunOneDiskQuickTestOnly;

	private readonly bool ExcludeSlotsAroundDumpBin;

	private readonly bool ExecuteQuickTest;

	private readonly bool RewriteDataOnSuccess = true;

	private readonly NamedLock m_instanceLock;

	private readonly Guid ProgramLock = new Guid("{7B780DBE-F37B-4f4a-83FA-C59F5B2C87D2}");

	private readonly HardwareService Service;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Helper.Dispose();
			if (m_instanceLock != null)
			{
				m_instanceLock.Dispose();
			}
			Log.Dispose();
		}
	}

	internal KioskTest(string[] args, HardwareService service)
	{
		if (args.Length != 0)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("--runtime", StringComparison.CurrentCultureIgnoreCase))
				{
					RunTime = CommandLineOption.GetOptionVal(args[i], 6.5);
				}
				else if (args[i].StartsWith("--vend"))
				{
					VendFrequency = CommandLineOption.GetOptionVal(args[i], 5);
				}
				else if (args[i].StartsWith("--decks"))
				{
					string optionVal = CommandLineOption.GetOptionVal(args[i], string.Empty);
					if (!string.IsNullOrEmpty(optionVal))
					{
						string[] array = optionVal.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string text in array)
						{
							if (!int.TryParse(text, out var result))
							{
								Console.WriteLine("Unable to parse deck list option '{0}'.", text);
								Environment.Exit(-1);
							}
							ConfiguredDecks.Add(result);
						}
					}
					ConfiguredDecks.Sort();
				}
				else if (args[i].StartsWith("-excludeAroundDump"))
				{
					ExcludeSlotsAroundDumpBin = true;
				}
				else if (args[i].StartsWith("--quickTest"))
				{
					ExecuteQuickTest = CommandLineOption.GetOptionVal(args[i], defVal: true);
				}
				else if (args[i].StartsWith("--rewriteDataOnSuccess"))
				{
					RewriteDataOnSuccess = CommandLineOption.GetOptionVal(args[i], defVal: true);
				}
				else if (args[i] == "-oneDiskQuickTest")
				{
					RunOneDiskQuickTestOnly = true;
				}
				else
				{
					Console.WriteLine("\nList of Commands:\n--------------------\n");
					Console.WriteLine("\t--decks:<number>,<number>,<number>,...\tUse only the decks listed");
					Console.WriteLine("\t--runTime:<number>\tSet the number of hours for test to run");
					Console.WriteLine("\t--vend:<number>\tSet the number of moves between each vend attempt, 0 prevents vends from being attempted");
					Console.WriteLine("\t-excludeAroundDump\tIgnores slots 81 & 85 on deck 8 VMZ machines.");
					Console.WriteLine("\t--quickTest:{true|false}\tTests disk at slots 1, 30 and 60 on each deck. Assumes a disk at 1,1 on each deck.");
					Console.WriteLine("\t--rewriteDataOnSuccess:{true|false}\tOn successful test, updates GAMP data with new values.");
					Console.WriteLine("\t-oneDiskQuickTest\tPerforms a quick test at slots 1, 30 and 60 on each deck. Assumes single disk at 1,1.");
					Console.WriteLine("\t-help\t\t\tPrints this screen");
					Environment.Exit(0);
				}
			}
		}
		Service = service;
		Helper = new ClientHelper(service);
		IRuntimeService service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
		m_instanceLock = new NamedLock(ProgramLock.ToString(), LockScope.Local);
		string path = string.Format("kioskTest-{0}", service2.GenerateUniqueFile(".log"));
		string path2 = Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\KioskTest", path);
		Log = new StreamWriter(path2)
		{
			AutoFlush = true
		};
	}

	internal int Run()
	{
		if (RunOneDiskQuickTestOnly)
		{
			printAndLog(" ** scheduling one disk quick deck test ** ");
			using OneDiskQuickTest oneDiskQuickTest = new OneDiskQuickTest(Service);
			oneDiskQuickTest.Run();
			if (oneDiskQuickTest.EndStatus != HardwareJobStatus.Completed)
			{
				return -1;
			}
			return (!UpdateGamp()) ? (-1) : 0;
		}
		if (ExecuteQuickTest)
		{
			using QuickDeckTest quickDeckTest = new QuickDeckTest(Service);
			printAndLog(" ** scheduling quick deck test ** ");
			quickDeckTest.Run();
			if (HardwareJobStatus.Completed != quickDeckTest.EndStatus)
			{
				printAndLog("The quick test failed - bailing test.");
				return -1;
			}
			printAndLog(" ** quick test SUCCEEDED; schedule adjacent bin test ** ");
			using AdjacentBinTest adjacentBinTest = new AdjacentBinTest(Service);
			adjacentBinTest.Run();
			if (adjacentBinTest.EndStatus == HardwareJobStatus.Completed)
			{
				printAndLog("The quick test succeeded.");
			}
		}
		if (!ConfigureDecks())
		{
			printAndLog("!! Unable to retrieve decks information from service !!");
			return -1;
		}
		return RunKioskTest();
	}

	private void OnTimerFired(object sender, ElapsedEventArgs e)
	{
		KeepRunningTest = false;
	}

	private int RunKioskTest()
	{
		if (!m_instanceLock.IsOwned)
		{
			printAndLog("Only one instance of the Kiosk Test is allowed to run - bailing test.");
			return -1;
		}
		printAndLog("Kiosk Test Starting - Deck details:");
		printAndLog("-------------------------------------");
		foreach (DeckConfig deck in Decks)
		{
			printAndLog("  Deck {0} - Number of Slots {1} - Is QLM {2}", deck.Deck, deck.Slots, deck.IsQlm);
		}
		printAndLog("-------------------------------------");
		DateTime dateTime = DateTime.Now.AddHours(RunTime);
		printAndLog("Run until {0}", dateTime);
		new RedboxTimer("KioskTest", OnTimerFired).FireOn(dateTime);
		while (KeepRunningTest)
		{
			foreach (DeckConfig deck2 in Decks)
			{
				printAndLog("## Start test on deck {0} : Time Remaining {1} ##", deck2.Deck, dateTime - DateTime.Now);
				RunOnDeck(deck2, dateTime);
			}
		}
		if (ConfiguredForVMZ)
		{
			printAndLog("## Kiosk is configured for VMZ - schedule load bin job ##");
			List<Location> list = new List<Location>();
			foreach (DeckConfig deck3 in Decks)
			{
				Location item = new Location
				{
					Deck = deck3.Deck,
					Slot = 1
				};
				list.Add(item);
			}
			ScheduleAndBlockLoadBinJob(list);
		}
		UpdateGamp();
		printAndLog("## Kiosk Test Completed ##");
		return 0;
	}

	private void RunOnDeck(DeckConfig deck, DateTime stopTime)
	{
		bool flag = false;
		Location location = new Location
		{
			Deck = deck.Deck,
			Slot = 1
		};
		Location location2 = null;
		for (int i = 1; i <= deck.Slots; i++)
		{
			if (!KeepRunningTest)
			{
				break;
			}
			if (VendFrequency > 0)
			{
				flag = Transfers % VendFrequency == 0;
			}
			if (TestLocations.Count == 0)
			{
				printAndLog("## Populating Location Information ##");
				PopulateLocations();
			}
			int index = new Random(DateTime.Now.Millisecond).Next(0, TestLocations.Count - 1);
			location2 = TestLocations[index].Location;
			TestLocations.RemoveAt(index);
			printAndLog("## Move Disc From Deck {0}, Slot {1} to Deck {2}, Slot {3} - Vend {4} ##", location.Deck, location.Slot, location2.Deck, location2.Slot, flag);
			if (!Schedule(location, location2, flag))
			{
				printAndLog("Job failed to schedule properly");
				Environment.Exit(-1);
			}
			Transfers++;
			location = location2;
		}
		DeckConfig deckConfig = Decks.Find((DeckConfig each) => each.Deck == deck.Deck + 1);
		if (deckConfig == null)
		{
			deckConfig = Decks.Find((DeckConfig each) => each.Deck == 1);
		}
		printAndLog("## Deck {0} iteration complete return disk to Deck = {1} Slot = 1 ##", deck.Deck, deckConfig.Deck);
		Schedule(location, new Location
		{
			Deck = deckConfig.Deck,
			Slot = 1
		}, vend: false);
	}

	private bool UpdateGamp()
	{
		if (RewriteDataOnSuccess)
		{
			HardwareCommandResult hardwareCommandResult = Service.ExecuteServiceCommand("SERVICE update-gamp backup-directory: @'c:\\gamp\\KentBackup'");
			printAndLog("Update gamp file data: {0}.", hardwareCommandResult.Success ? "OK" : "EPIC FAIL");
			return hardwareCommandResult.Success;
		}
		return true;
	}

	private bool ConfigureDecks()
	{
		if (!Service.GetMachineInfo(new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.High
		}, out var job).Success)
		{
			return false;
		}
		Helper.WaitForJob(job, out var endStatus);
		if (endStatus != HardwareJobStatus.Completed)
		{
			printAndLog("!! Get-machine-info ended with error status {0} !!", endStatus.ToString());
			return false;
		}
		if (!job.GetResults(out var results).Success)
		{
			printAndLog("!! Unable to get results for get-machine-info job !!");
			return false;
		}
		int qlmDeck = -1;
		ProgramResult[] array = results;
		foreach (ProgramResult programResult in array)
		{
			string code = programResult.Code;
			if (!(code == "QlmInfo"))
			{
				if (code == "DeckInfo" && (ConfiguredDecks.Count == 0 || ConfiguredDecks.Contains(programResult.Deck)))
				{
					Decks.Add(new DeckConfig
					{
						Deck = programResult.Deck,
						Slots = programResult.Slot
					});
				}
			}
			else
			{
				qlmDeck = programResult.Deck;
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
		ConfiguredForVMZ = -1 == qlmDeck;
		Decks.Sort((DeckConfig x, DeckConfig y) => x.Deck.CompareTo(y.Deck));
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

	private bool ScheduleAndBlockLoadBinJob(List<Location> locs)
	{
		HardwareJob job = null;
		try
		{
			if (!Service.LoadBin(locs, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out job).Success)
			{
				printAndLog("!! Failed to create bin job !!");
				return false;
			}
			if (!Helper.WaitForJob(job, out var endStatus))
			{
				printAndLog("!! Error waiting for load bin job to complete !!");
				return false;
			}
			return endStatus == HardwareJobStatus.Completed;
		}
		finally
		{
			job?.Trash();
		}
	}

	private void printAndLog(string message)
	{
		string value = $"{DateTime.Now.ToString()} {message}";
		Console.WriteLine(value);
		Log.WriteLine(value);
	}

	private void printAndLog(string fmt, params object[] args)
	{
		printAndLog(string.Format(fmt, args));
	}

	private bool Schedule(Location start, Location end, bool vend)
	{
		using (KioskTestJob kioskTestJob = new KioskTestJob(Service, start, end, vend))
		{
			kioskTestJob.Run();
			if (HardwareJobStatus.Completed != kioskTestJob.EndStatus)
			{
				printAndLog("!! Job {0} failed with status {1} !!", kioskTestJob.ID, kioskTestJob.EndStatus.ToString());
				Environment.Exit(-1);
			}
		}
		return true;
	}
}
