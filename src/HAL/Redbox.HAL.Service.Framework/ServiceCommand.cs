using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("service")]
public sealed class ServiceCommand
{
	private static Random m_random = new Random((int)DateTime.Now.Ticks);

	[Usage("SERVICE hardware-status")]
	[CommandForm(Name = "hardware-status")]
	[Description("Queries the state of the hardware.")]
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

	[Usage("SERVICE merch-mode status: enabled")]
	[CommandForm(Name = "merch-mode")]
	[Description("")]
	public void ChangeMerchModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		ExecutionEngine.Instance.ChangeMode((!enabled) ? EngineModes.Normal : EngineModes.Merchandizing);
	}

	[Usage("SERVICE diagnostic-mode status: enabled")]
	[Description("")]
	[CommandForm(Name = "diagnostic-mode")]
	public void ChangeDiagnosticModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		ExecutionEngine.Instance.ChangeMode((!enabled) ? EngineModes.Normal : EngineModes.Diagnostic);
	}

	[CommandForm(Name = "sync-unknowns")]
	[Usage("SERVICE sync-unknowns")]
	[Description("")]
	public void SyncUnknowns(CommandContext context)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		IList<ILocation> unknowns = service.GetUnknowns();
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

	[Description("")]
	[CommandForm(Name = "sync-empty")]
	[Usage("SERVICE sync-empty")]
	public void SyncEmpty(CommandContext context)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		IList<ILocation> emptySlots = service.GetEmptySlots();
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

	[Usage("SERVICE full-sync")]
	[CommandForm(Name = "full-sync")]
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
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		service.ForAllDecksDo(delegate(IDeck deck)
		{
			List<int> list = new List<int> { deck.YOffset };
			foreach (IQuadrant quadrant in deck.Quadrants)
			{
				list.Add(quadrant.Offset);
			}
			newData.Add(list);
			return true;
		});
		GampHelper gampHelper = new GampHelper();
		GampBackupResult gampBackupResult = gampHelper.WriteSlotDataFile(newData, createBackup: true);
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

	[Description("")]
	[CommandForm(Name = "test-comm")]
	[Usage("SERVICE test-comm")]
	public void TestIPCTransfer(CommandContext context)
	{
		context.Messages.Add("ACK");
	}

	[Usage("SERVICE last-user-job")]
	[CommandForm(Name = "last-user-job")]
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

	[Description("")]
	[CommandForm(Name = "get-kiosk-id")]
	[Usage("SERVICE get-kiosk-id")]
	public void GetKioskID(CommandContext context)
	{
		RegistryKey registryKey = null;
		string item = "UNKNOWN";
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("ID");
				if (value != null)
				{
					item = (string)ConversionHelper.ChangeType(value, typeof(string));
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to read secret option 'ID'", e);
			item = "UNKNOWN";
		}
		finally
		{
			registryKey?.Close();
		}
		context.Messages.Add(item);
	}

	[CommandForm(Name = "mark-kiosk-empty")]
	[Description("")]
	[Usage("SERVICE mark-kiosk-empty")]
	public void MarkKioskEmpty(CommandContext context)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		service.ForAllDecksDo(delegate(IDeck deck)
		{
			inventoryService.ResetAndMark(deck, "EMPTY");
			return true;
		});
	}

	[Description("")]
	[CommandForm(Name = "mark-kiosk-unknown")]
	[Usage("SERVICE mark-kiosk-unknown")]
	public void MarkKioskUnknown(CommandContext context)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		service.ForAllDecksDo(delegate(IDeck deck)
		{
			inventoryService.ResetAndMark(deck, "UNKNOWN");
			return true;
		});
	}

	[CommandForm(Name = "dump-inventory-store")]
	[Description("")]
	[Usage("SERVICE dump-inventory-store")]
	public void DumpInventoryStore(CommandContext context)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		string path = string.Format("StoreDump-{0}", FileHelper.GenerateUniqueFile("txt"));
		string path2 = Path.Combine("c:\\Program Files\\Redbox\\KioskLogs\\Service", path);
		using StreamWriter writer = new StreamWriter(path2);
		service.DumpStore(writer);
	}

	[Description("")]
	[CommandForm(Name = "power-down")]
	[Usage("SERVICE power-down")]
	public void PowerDownService(CommandContext context)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		service.Suspend();
	}

	[CommandForm(Name = "power-up")]
	[Description("")]
	[Usage("SERVICE power-up")]
	public void PowerUpService(CommandContext context)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		service.Restart();
	}

	[Description("")]
	[CommandForm(Name = "dump-arcus-correction-stats")]
	[Usage("SERVICE dump-arcus-correction-stats")]
	public void DumpArcusCorrectionStats(CommandContext context)
	{
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service.CreateSubpath("Service");
		string path2 = Path.Combine(path, "ArcusCorrectionStats.txt");
		if (File.Exists(path2))
		{
			FileHelper.CreateBackup(path2, BackupAction.Move);
		}
		IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		using StreamWriter log = new StreamWriter(path2);
		service2.LogStats(HardwareCorrectionStatistic.Arcus, log);
	}

	[Usage("SERVICE clear-arcus-correction-stats")]
	[Description("")]
	[CommandForm(Name = "clear-arcus-correction-stats")]
	public void ClearArcusCorrectionStats(CommandContext context)
	{
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		service.RemoveAll(HardwareCorrectionStatistic.Arcus);
	}

	[CommandForm(Name = "get-fraud-post-statistics")]
	[Description("")]
	[Usage("SERVICE get-fraud-post-statistics")]
	public void GetFraudPostStatistics(CommandContext context)
	{
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service.CreateSubpath("Service");
		string path2 = Path.Combine(path, "FraudPostStatistics.txt");
		if (File.Exists(path2))
		{
			FileHelper.SafeDeleteFile(path2);
		}
		using (StreamWriter streamWriter = new StreamWriter(path2))
		{
			IDiskValidatorService service2 = ServiceLocator.Instance.GetService<IDiskValidatorService>();
			if (service2.IsConfigured)
			{
				IFraudPostStatistics statistics = service2.FraudSensor.GetStatistics();
				streamWriter.WriteLine("POST statistics sample {0}", statistics.LastSampleDate.ToString());
				streamWriter.WriteLine("  Failed post: {0}", statistics.Failures);
				streamWriter.WriteLine("  Successful post: {0}", statistics.Successes);
			}
			else
			{
				streamWriter.WriteLine("Fraud service not configured.");
			}
		}
		context.Messages.Add("ACK");
	}

	[Description("")]
	[CommandForm(Name = "reset-fraud-post-counters")]
	[Usage("SERVICE reset-fraud-post-counters")]
	public void ResetFraudPostCounters(CommandContext context)
	{
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		if (service.IsConfigured)
		{
			service.FraudSensor.ResetPostStatistics();
			context.Messages.Add("OK");
		}
		else
		{
			context.Messages.Add("NOT CONFIGURED");
		}
	}
}
