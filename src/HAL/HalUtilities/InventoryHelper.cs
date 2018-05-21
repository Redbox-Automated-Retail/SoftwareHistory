using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class InventoryHelper : IDisposable
{
	private class FraudLocInfo
	{
		internal readonly List<DecodeInfo> Dumps = new List<DecodeInfo>();

		internal readonly Location Location;

		public override string ToString()
		{
			return $"Deck = {Location.Deck} Slot = {Location.Slot}";
		}

		internal bool HasSecureRead()
		{
			return Dumps.FindAll((DecodeInfo each) => each.IsSecure).Count > 0;
		}

		internal FraudLocInfo(int deck, int slot)
		{
			Location = new Location
			{
				Deck = deck,
				Slot = slot
			};
		}
	}

	private class DecodeInfo
	{
		internal readonly bool IsSecure;

		internal string Matrix;

		internal int Count;

		internal DecodeInfo(string matrix, int count)
		{
			Matrix = matrix;
			Count = count;
			IsSecure = matrix.Equals("YES", StringComparison.CurrentCultureIgnoreCase);
		}
	}

	private bool Disposed;

	private int completedSyncs;

	private readonly ManualResetEvent Event = new ManualResetEvent(initialState: false);

	private readonly HardwareService Service;

	private readonly ClientHelper Helper;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Helper.Dispose();
		}
	}

	internal void DumpInventoryToFile(string path)
	{
		HardwareCommandResult inventoryState = Service.GetInventoryState();
		if (!inventoryState.Errors.ContainsError())
		{
			File.WriteAllText(path, inventoryState.CommandMessages[0]);
		}
	}

	internal bool ImportInventoryFromFile(string fileName)
	{
		if (!File.Exists(fileName))
		{
			Console.WriteLine("The file '{0}' doesn't exist.", fileName);
			return false;
		}
		try
		{
			return Service.SetInventoryState(File.ReadAllText(fileName)).Success;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return false;
		}
	}

	internal void CompareSyncResults(HardwareJob syncJob, bool outputSuccessfulCompare)
	{
		if (!syncJob.GetResults(out var results).Success)
		{
			LogHelper.Instance.Log("Can't get job results for {0}", syncJob.ID);
			return;
		}
		int inventoryMatches = 0;
		int inventoryMismatches = 0;
		Array.ForEach(results, delegate(ProgramResult _r)
		{
			if (_r.Code == "InventoryState")
			{
				if (InventoryItemsEqual(_r, outputSuccessfulCompare))
				{
					int num = inventoryMatches + 1;
					inventoryMatches = num;
				}
				else
				{
					int num = inventoryMismatches + 1;
					inventoryMismatches = num;
				}
			}
		});
		LogHelper.Instance.Log("Analysis summary:");
		LogHelper.Instance.Log("  {0} matches.", inventoryMatches);
		LogHelper.Instance.Log("  {0} mismatches.", inventoryMismatches);
		if (inventoryMatches + inventoryMismatches != 717)
		{
			LogHelper.Instance.Log("!! Analysis Error !! expected 717, found {0}", inventoryMatches + inventoryMismatches);
		}
	}

	internal bool FixType(SyncType type)
	{
		return type switch
		{
			SyncType.Empty => Service.ServiceEmptySync().Success, 
			SyncType.Unknown => Service.ServiceUnknownSync().Success, 
			_ => false, 
		};
	}

	internal bool ScheduleFullSync()
	{
		return ScheduleFullSync(SyncOptions.None);
	}

	internal bool ScheduleFullSync(SyncOptions options)
	{
		LogHelper.Instance.Log("Schedule full sync with options: ");
		if (options == SyncOptions.None)
		{
			LogHelper.Instance.Log("  None");
		}
		else
		{
			foreach (SyncOptions value in Enum.GetValues(typeof(SyncOptions)))
			{
				if ((options & value) != SyncOptions.None)
				{
					LogHelper.Instance.Log("  {0}", value.ToString());
				}
			}
		}
		Thread thread = null;
		if ((options & SyncOptions.AsyncSoftSync) != SyncOptions.None)
		{
			thread = new Thread(RunSoftSync);
			thread.Start();
		}
		HardwareCommandResult hardwareCommandResult = Service.FullSync();
		if ((options & SyncOptions.Attach) != SyncOptions.None && hardwareCommandResult.Success)
		{
			string[] array = hardwareCommandResult.CommandMessages[0].Split('|');
			if (Service.GetJob(array[0], out var job).Success)
			{
				job.EventRaised += delegate(HardwareJob hardwareJob, DateTime eventTime, string eventMessage)
				{
					LogHelper.Instance.Log("{0}: {1}", eventTime, eventMessage);
				};
				Helper.WaitForJob(job, out var endStatus);
				LogHelper.Instance.Log("{0} Sync job {1} ended with status {2}", DateTime.Now, job.ID, endStatus.ToString());
				if ((options & SyncOptions.Analyze) != SyncOptions.None)
				{
					CompareSyncResults(job, (options & SyncOptions.OutputSuccessfulSyncCompare) != 0);
				}
				if ((options & SyncOptions.ParseFrauds) != SyncOptions.None)
				{
					ParseFraudsFromId(job.ID, vend: false);
				}
				if (thread != null)
				{
					Event.Set();
					LogHelper.Instance.Log(" ** Completed {0} soft syncs ** ", completedSyncs);
				}
			}
		}
		return hardwareCommandResult.Success;
	}

	internal void ParseFraudsFromFile(string fullPath, TextWriter output)
	{
		ParseFraudsFromFile(fullPath, output, vendFrauds: false);
	}

	internal void ParseFraudsFromFile(string fullPath, TextWriter output, bool vendFrauds)
	{
		List<FraudLocInfo> list = new List<FraudLocInfo>();
		using (new DisposeableList<FraudLocInfo>(list))
		{
			using (TextReader textReader = new StreamReader(fullPath))
			{
				FraudLocInfo fraudLocInfo = null;
				char[] separator = new char[1] { ' ' };
				while (true)
				{
					string text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					if (!text.Contains("Vend time is set") && !text.Contains("Random sync") && !text.Contains("Sync Deck") && !text.Contains("PUT Deck") && !text.Contains("-- read dump --"))
					{
						if (text.Contains("GET Deck"))
						{
							string[] array = text.Split(separator);
							int deck = Convert.ToInt32(array[6]);
							int slot = Convert.ToInt32(array[9]);
							fraudLocInfo = new FraudLocInfo(deck, slot);
							list.Add(fraudLocInfo);
						}
						else if (text.Contains("Matrix"))
						{
							string[] array2 = text.Split(separator);
							DecodeInfo item = new DecodeInfo(array2[6], Convert.ToInt32(array2[8]));
							fraudLocInfo.Dumps.Add(item);
						}
						else
						{
							Console.WriteLine("Unrecognized line '{0}'", text);
						}
					}
				}
			}
			output.WriteLine("Found {0} total sync locations", list.Count);
			List<FraudLocInfo> list2 = list.FindAll((FraudLocInfo each) => !each.HasSecureRead());
			output.WriteLine(" There are {0} locations WITH a secure read", list.Count - list2.Count);
			output.WriteLine(" There are {0} locations without a secure read", list2.Count);
			if (vendFrauds)
			{
				VendFrauds(list2);
			}
		}
	}

	internal void ParseFraudsFromId(string programId, bool vend)
	{
		string fullPath = $"c:\\Program Files\\Redbox\\KioskLogs\\Sync\\sync-{programId}.log";
		using StreamWriter output = new StreamWriter($"fraud-analysis-{programId}.log");
		ParseFraudsFromFile(fullPath, output, vend);
	}

	internal InventoryHelper(HardwareService service, ClientHelper helper)
	{
		Service = service;
		Helper = helper;
	}

	private void RunSoftSync()
	{
		while (!Event.WaitOne(30000))
		{
			if (Service.SoftSync(new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out var job).Success)
			{
				job.Pend();
				Helper.WaitForJob(job, out var endStatus);
				if (HardwareJobStatus.Completed == endStatus)
				{
					completedSyncs++;
				}
			}
		}
	}

	private void VendFrauds(List<FraudLocInfo> without)
	{
		KioskInventory ij = new KioskInventory(Service);
		try
		{
			List<Location> vendLocs = new List<Location>();
			without.ForEach(delegate(FraudLocInfo each)
			{
				IInventoryLocation inventoryLocation = ij.DeckInventory.Find((IInventoryLocation z) => z.Location.Deck == each.Location.Deck && z.Location.Slot == each.Location.Slot);
				if (inventoryLocation != null && inventoryLocation.Matrix != "EMPTY")
				{
					vendLocs.Add(each.Location);
				}
			});
			HardwareCommandResult hardwareCommandResult = Service.LoadBin(vendLocs, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.High
			}, out var job);
			if (!hardwareCommandResult.Success)
			{
				Console.WriteLine("Failed to schedule load-bin job.");
				hardwareCommandResult.Errors.Dump(Console.Out);
			}
			else
			{
				Console.WriteLine("Starting load-bin job {0}", job.ID);
				Helper.WaitForJob(job, out var _);
			}
		}
		finally
		{
			if (ij != null)
			{
				((IDisposable)ij).Dispose();
			}
		}
	}

	private bool InventoryItemsEqual(ProgramResult result, bool logSuccess)
	{
		if (result.ItemID.ToString() == result.PreviousItem.ToString())
		{
			if (logSuccess)
			{
				LogHelper.Instance.Log("Sync: inventory at deck {0} slot {1} ID = {2} was the same", result.Deck, result.Slot, result.PreviousItem.ToString());
			}
			return true;
		}
		LogHelper.Instance.Log("Sync: inventory changed at deck {0} slot {1} ( went from {2} -> {3} )", result.Deck, result.Slot, result.PreviousItem.ToString(), result.ItemID.ToString());
		return false;
	}
}
