using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("service")]
public sealed class ServiceCommand
{
	private static Random m_random = new Random((int)DateTime.Now.Ticks);

	[CommandForm(Name = "hardware-status")]
	[Usage("SERVICE hardware-status")]
	[Description("Queries the state of the hardware.")]
	public void Status(CommandContext context)
	{
		if (ExecutionEngine.Instance.IsImmediateJobPending())
		{
			ExecutionEngine.Instance.GetJob("$$$immediate$$$")?.WaitForCompletion();
		}
		using MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes("HARDWARE STATUS"));
		if (ExecutionEngine.Instance.Compile(stream, "$$$immediate$$$").Errors.ContainsError())
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

	[CommandForm(Name = "merch-mode")]
	[Usage("SERVICE merch-mode status: enabled")]
	[Description("")]
	public void ChangeMerchModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		ExecutionEngine.Instance.ChangeMode((!enabled) ? EngineModes.Normal : EngineModes.Merchandizing);
	}

	[CommandForm(Name = "diagnostic-mode")]
	[Usage("SERVICE diagnostic-mode status: enabled")]
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
		IList<ILocation> unknowns = ServiceLocator.Instance.GetService<IInventoryService>().GetUnknowns();
		if (unknowns.Count == 0)
		{
			LogHelper.Instance.Log("Service unknown sync: there are no UNKNOWNS. Nice job!");
			return;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync-locations", "UNKNOWN inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		using (new DisposeableList<ILocation>(unknowns))
		{
			foreach (ILocation item in unknowns)
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

	[CommandForm(Name = "sync-empty")]
	[Usage("SERVICE sync-empty")]
	[Description("")]
	public void SyncEmpty(CommandContext context)
	{
		IList<ILocation> emptySlots = ServiceLocator.Instance.GetService<IInventoryService>().GetEmptySlots();
		if (emptySlots.Count == 0)
		{
			LogHelper.Instance.Log("SyncEmpty: there are no empty slots.");
			return;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync-locations", "EMPTY inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		using (new DisposeableList<ILocation>(emptySlots))
		{
			foreach (ILocation item in emptySlots)
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

	[CommandForm(Name = "full-sync")]
	[Usage("SERVICE full-sync")]
	[Description("")]
	public void FullSync(CommandContext context)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync", "Full inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IDeck deck = service.Last;
		if (deck.IsQlm)
		{
			deck = service.GetByNumber(deck.Number - 1);
		}
		executionContext.PushTop(deck.NumberOfSlots);
		executionContext.PushTop(deck.Number);
		IDeck first = service.First;
		executionContext.PushTop(1);
		executionContext.PushTop(first.Number);
		LogHelper.Instance.Log("Service executing full sync ( job ID = {0} ).", executionContext.ID);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "update-gamp")]
	[Usage("SERVICE update-gamp [backup-directory: 'c:\\backup\\path' ]")]
	[Description("")]
	public void UpdateGamp(CommandContext context, [CommandKeyValue(KeyName = "backup-directory")] string backupDirectory)
	{
		List<List<int>> newData = new List<List<int>>();
		ServiceLocator.Instance.GetService<IDecksService>().ForAllDecksDo(delegate(IDeck deck)
		{
			List<int> list = new List<int> { deck.YOffset };
			foreach (IQuadrant quadrant in deck.Quadrants)
			{
				list.Add(quadrant.Offset);
			}
			newData.Add(list);
			return true;
		});
		GampBackupResult gampBackupResult = new GampHelper().WriteSlotDataFile(newData, createBackup: true);
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

	[CommandForm(Name = "last-user-job")]
	[Usage("SERVICE last-user-job")]
	[Description("")]
	public void LastUserJob(CommandContext context)
	{
		context.Messages.Add("NONE");
	}

	[CommandForm(Name = "get-version")]
	[Usage("SERVICE get-version")]
	[Description("")]
	public void GetServiceVersion(CommandContext context)
	{
		context.Messages.Add(typeof(ServiceCommand).Assembly.GetName().Version.ToString());
	}

	[CommandForm(Name = "get-kiosk-id")]
	[Usage("SERVICE get-kiosk-id")]
	[Description("")]
	public void GetKioskID(CommandContext context)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		context.Messages.Add(service.KioskId);
	}

	[CommandForm(Name = "mark-kiosk-empty")]
	[Usage("SERVICE mark-kiosk-empty")]
	[Description("")]
	public void MarkKioskEmpty(CommandContext context)
	{
		ExecutionContext executionContext = ScheduleHighPriority("mark-kiosk-inventory");
		executionContext.PushTop("EMPTY");
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "mark-kiosk-unknown")]
	[Usage("SERVICE mark-kiosk-unknown")]
	[Description("")]
	public void MarkKioskUnknown(CommandContext context)
	{
		ExecutionContext executionContext = ScheduleHighPriority("mark-kiosk-inventory");
		executionContext.PushTop("UNKNOWN");
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "dump-inventory-store")]
	[Usage("SERVICE dump-inventory-store")]
	[Description("")]
	public void DumpInventoryStore(CommandContext context)
	{
		ExecutionContext executionContext = ScheduleHighPriority("dump-inventory-store-job");
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "power-down")]
	[Usage("SERVICE power-down")]
	[Description("")]
	public void PowerDownService(CommandContext context)
	{
		ServiceLocator.Instance.GetService<IExecutionService>().Suspend();
	}

	[CommandForm(Name = "power-up")]
	[Usage("SERVICE power-up")]
	[Description("")]
	public void PowerUpService(CommandContext context)
	{
		ServiceLocator.Instance.GetService<IExecutionService>().Restart();
	}

	[CommandForm(Name = "test-comm")]
	[Usage("SERVICE test-comm")]
	[Description("")]
	public void TestConnection(CommandContext context)
	{
		context.Messages.Add("ACK");
	}

	private ExecutionContext ScheduleHighPriority(string name)
	{
		return ScheduleHighPriority(name, string.Empty);
	}

	private ExecutionContext ScheduleHighPriority(string name, string label)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob(name, label, DateTime.Now, ProgramPriority.High);
		executionContext.EnableDebugging = false;
		return executionContext;
	}
}
