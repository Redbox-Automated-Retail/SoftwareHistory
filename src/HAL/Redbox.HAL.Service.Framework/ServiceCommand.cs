using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
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
	public void Status(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		if (enabled)
		{
			ExecutionEngine.Instance.SuspendDevices();
		}
		else
		{
			ExecutionEngine.Instance.RestartDevices();
		}
	}

	[CommandForm(Name = "sync-unknowns")]
	[Description("")]
	[Usage("SERVICE sync-unknowns")]
	public void SyncUnknowns(CommandContext context)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync-locations", "UNKNOWN inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		List<Location> unknowns = ControllerConfiguration.Instance.GetUnknowns();
		try
		{
			foreach (Location item in unknowns)
			{
				executionContext.PushTop(item.Slot);
				executionContext.PushTop(item.Deck);
			}
			executionContext.PushTop(unknowns.Count);
			LogHelper.Instance.Log("Service unknown sync: sync {0} locations ( job ID = {1} ).", unknowns.Count, executionContext.ID);
		}
		finally
		{
			unknowns.Clear();
		}
		executionContext.Pend();
	}

	[Description("")]
	[CommandForm(Name = "sync-empty")]
	[Usage("SERVICE sync-empty")]
	public void SyncEmpty(CommandContext context)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("sync-locations", "EMPTY inventory sync.", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		List<Location> emptySlots = ControllerConfiguration.Instance.GetEmptySlots();
		try
		{
			foreach (Location item in emptySlots)
			{
				executionContext.PushTop(item.Slot);
				executionContext.PushTop(item.Deck);
			}
			executionContext.PushTop(emptySlots.Count);
			LogHelper.Instance.Log("Service empty sync: sync {0} locations ( job ID = {1} ).", emptySlots.Count, executionContext.ID);
		}
		finally
		{
			emptySlots.Clear();
		}
		executionContext.Pend();
	}

	[CommandForm(Name = "full-sync")]
	[Usage("SERVICE full-sync")]
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
	}

	[Description("")]
	[Usage("SERVICE update-gamp [backup-directory: 'c:\\backup\\path' ]")]
	[CommandForm(Name = "update-gamp")]
	public void UpdateGamp(CommandContext context, [CommandKeyValue(KeyName = "backup-directory")] string backupDirectory)
	{
		ControllerConfiguration.Instance.UpdateGamp(backupDirectory);
	}

	[Usage("SERVICE test-ipc-xfer size: payloadSize")]
	[CommandForm(Name = "test-ipc-xfer")]
	[Description("")]
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

	[CommandForm(Name = "test-comm")]
	[Description("")]
	[Usage("SERVICE test-comm")]
	public void TestIPCTransfer(CommandContext context)
	{
		context.Messages.Add("ACK");
	}

	[Description("")]
	[CommandForm(Name = "quick-sync")]
	[Usage("SERVICE quick-sync")]
	public void QuickSync(CommandContext context)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("quick-sync-test", "Quick full sync", DateTime.Now, ProgramPriority.Low);
		executionContext.EnableDebugging = false;
		LogHelper.Instance.Log("Service executing quick sync ( job ID = {0} ).", executionContext.ID);
		executionContext.Pend();
	}

	[CommandForm(Name = "last-user-job")]
	[Description("")]
	[Usage("SERVICE last-user-job")]
	public void LastUserJob(CommandContext context)
	{
		context.Messages.Add((!UserJobMonitor.Instance.LastJob.HasValue) ? "NONE" : UserJobMonitor.Instance.LastJob.ToString());
	}
}
