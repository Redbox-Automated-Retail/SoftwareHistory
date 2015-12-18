using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("service")]
public sealed class ServiceCommand
{
	private static Random m_random = new Random((int)DateTime.Now.Ticks);

	[Description("Queries the state of the hardware.")]
	[Usage("SERVICE hardware-status")]
	[CommandForm(Name = "hardware-status")]
	public void Status(CommandContext context)
	{
		if (ExecutionEngine.Instance.IsImmediateJobPending())
		{
			ExecutionEngine.Instance.GetJob("$$$immediate$$$")?.WaitForCompletion();
		}
		using MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes("HARDWARE STATUS"));
		ExecutionResult executionResult = ExecutionEngine.Instance.Compile(stream, "$$$immediate$$$");
		if (executionResult.Errors.ContainsError())
		{
			context.Messages.Add("ERROR");
			return;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ExecuteImmediate();
		executionContext.WaitForCompletion();
		string item = executionContext.PopTop() as string;
		context.Messages.Add(item);
	}

	[CommandForm(Name = "mm-status")]
	[Usage("SERVICE mm-status status: enabled")]
	[Description("")]
	public void ChangeMMStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		ExecutionEngine.Instance.ChangeMode((!enabled) ? EngineModes.Normal : EngineModes.Maintenance);
	}

	[Description("")]
	[CommandForm(Name = "merch-mode")]
	[Usage("SERVICE merch-mode status: enabled")]
	public void ChangeMerchModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		ExecutionEngine.Instance.ChangeMode((!enabled) ? EngineModes.Normal : EngineModes.Merchandizing);
	}

	[Usage("SERVICE diagnostic-mode status: enabled")]
	[CommandForm(Name = "diagnostic-mode")]
	[Description("")]
	public void ChangeDiagnosticModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		ExecutionEngine.Instance.ChangeMode((!enabled) ? EngineModes.Normal : EngineModes.Diagnostic);
	}

	[CommandForm(Name = "sync-unknowns")]
	[Usage("SERVICE sync-unknowns")]
	[Description("")]
	public void SyncUnknowns(CommandContext context)
	{
		List<Location> unknowns = InventoryService.Instance.GetUnknowns();
		if (unknowns.Count == 0)
		{
			LogHelper.Instance.Log("Service unknown sync: there are no UNKNOWNS. Nice job!");
			return;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync-locations", "UNKNOWN inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		using (new DisposeableList<Location>(unknowns))
		{
			foreach (Location item in unknowns)
			{
				executionContext.PushTop(item.Slot);
				executionContext.PushTop(item.Deck);
			}
			executionContext.PushTop(unknowns.Count);
			LogHelper.Instance.Log("Service unknown sync: {0} locations ( job ID = {1} ).", unknowns.Count, executionContext.ID);
		}
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Description("")]
	[Usage("SERVICE sync-empty")]
	[CommandForm(Name = "sync-empty")]
	public void SyncEmpty(CommandContext context)
	{
		List<Location> emptySlots = InventoryService.Instance.GetEmptySlots();
		if (emptySlots.Count == 0)
		{
			LogHelper.Instance.Log("SyncEmpty: there are no empty slots.");
			return;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync-locations", "EMPTY inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		using (new DisposeableList<Location>(emptySlots))
		{
			foreach (Location item in emptySlots)
			{
				executionContext.PushTop(item.Slot);
				executionContext.PushTop(item.Deck);
			}
			executionContext.PushTop(emptySlots.Count);
			LogHelper.Instance.Log("Service empty sync: {0} locations ( job ID = {1} ).", emptySlots.Count, executionContext.ID);
		}
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Usage("SERVICE full-sync")]
	[CommandForm(Name = "full-sync")]
	[Description("")]
	public void FullSync(CommandContext context)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync", "Full inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		Deck deck = DecksManager.Instance.Last();
		if (deck.IsQlmDeck)
		{
			deck = DecksManager.Instance.GetDeck(deck.Number - 1);
		}
		executionContext.PushTop(deck.NumberOfSlots);
		executionContext.PushTop(deck.Number);
		Deck deck2 = DecksManager.Instance.First();
		executionContext.PushTop(1);
		executionContext.PushTop(deck2.Number);
		LogHelper.Instance.Log("Service executing full sync ( job ID = {0} ).", executionContext.ID);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Description("")]
	[Usage("SERVICE update-gamp [backup-directory: 'c:\\backup\\path' ]")]
	[CommandForm(Name = "update-gamp")]
	public void UpdateGamp(CommandContext context, [CommandKeyValue(KeyName = "backup-directory")] string backupDirectory)
	{
		List<List<int>> newData = new List<List<int>>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			List<int> list = new List<int> { deck.YOffset };
			Quadrant[] quadrants = deck.Quadrants;
			foreach (Quadrant quadrant in quadrants)
			{
				list.Add(quadrant.Offset);
			}
			newData.Add(list);
			return true;
		});
		GampBackupResult gampBackupResult = GampHelper.Instance.WriteSlotDataFile(newData, createBackup: true);
		if (!string.IsNullOrEmpty(backupDirectory) && Directory.Exists(backupDirectory))
		{
			try
			{
				string fileName = Path.GetFileName(gampBackupResult.BackupFile);
				File.Move(gampBackupResult.BackupFile, Path.Combine(backupDirectory, fileName));
			}
			catch
			{
			}
		}
	}

	[Description("")]
	[CommandForm(Name = "test-ipc-xfer")]
	[Usage("SERVICE test-ipc-xfer size: payloadSize")]
	public void TestIPCTransfer(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "size")] int payloadSize)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < payloadSize; i++)
		{
			char value = Convert.ToChar(Convert.ToInt32(Math.Floor(26.0 * m_random.NextDouble() + 65.0)));
			stringBuilder.Append(value);
		}
		string text = stringBuilder.ToString();
		LogHelper.Instance.Log("String: ");
		LogHelper.Instance.Log(text);
		context.Messages.Add(text);
	}

	[Usage("SERVICE test-comm")]
	[CommandForm(Name = "test-comm")]
	[Description("")]
	public void TestIPCTransfer(CommandContext context)
	{
		context.Messages.Add("ACK");
	}

	[CommandForm(Name = "quick-sync")]
	[Usage("SERVICE quick-sync")]
	[Description("")]
	public void QuickSync(CommandContext context)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("quick-sync-test", "Quick full sync", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		LogHelper.Instance.Log("Service executing quick sync ( job ID = {0} ).", executionContext.ID);
		executionContext.Pend();
	}

	[Description("")]
	[CommandForm(Name = "last-user-job")]
	[Usage("SERVICE last-user-job")]
	public void LastUserJob(CommandContext context)
	{
		context.Messages.Add("NONE");
	}

	[CommandForm(Name = "get-version")]
	[Description("")]
	[Usage("SERVICE get-version")]
	public void GetServiceVersion(CommandContext context)
	{
		context.Messages.Add(typeof(ServiceCommand).Assembly.GetName().Version.ToString());
	}

	[Description("")]
	[CommandForm(Name = "get-kiosk-id")]
	[Usage("SERVICE get-kiosk-id")]
	public void GetKioskID(CommandContext context)
	{
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore(PersistentMapTypes.Engine);
		string value = dataStore.GetValue("ID", "UKNOWN");
		context.Messages.Add(value);
	}

	[Description("")]
	[Usage("SERVICE reset-support-counters")]
	[CommandForm(Name = "reset-support-counters")]
	public void ResetSupportCounters(CommandContext context)
	{
		HardwareSupport.ResetCounters();
	}

	[Usage("SERVICE mark-kiosk-empty")]
	[Description("")]
	[CommandForm(Name = "mark-kiosk-empty")]
	public void MarkKioskEmpty(CommandContext context)
	{
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			deck.Reset("EMPTY");
			return true;
		});
	}

	[CommandForm(Name = "mark-kiosk-unknown")]
	[Description("")]
	[Usage("SERVICE mark-kiosk-unknown")]
	public void MarkKioskUnknown(CommandContext context)
	{
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			deck.Reset("UNKNOWN");
			return true;
		});
	}
}
