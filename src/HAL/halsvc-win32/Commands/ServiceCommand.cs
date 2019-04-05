using System;
using System.Collections.Generic;
using System.ComponentModel;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands;

[Command("service")]
public sealed class ServiceCommand
{
	[CommandForm(Name = "hardware-status")]
	[Description("Queries the state of the hardware.")]
	[Usage("SERVICE hardware-status")]
	public void Status(CommandContext context)
	{
		context.Errors.Add(Error.NewError("E221", "Deprecated command", "The hardware-status command is deprecated."));
	}

	[Description("")]
	[Usage("SERVICE mm-status status: enabled")]
	[CommandForm(Name = "mm-status")]
	public void ChangeMMStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		OnChangeMode(enabled, EngineModes.Maintenance);
	}

	[CommandForm(Name = "merch-mode")]
	[Description("")]
	[Usage("SERVICE merch-mode status: enabled")]
	public void ChangeMerchModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		OnChangeMode(enabled, EngineModes.Merchandizing);
	}

	[CommandForm(Name = "diagnostic-mode")]
	[Description("")]
	[Usage("SERVICE diagnostic-mode status: enabled")]
	public void ChangeDiagnosticModeStatus(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "status")] bool enabled)
	{
		OnChangeMode(enabled, EngineModes.Diagnostic);
	}

	[Usage("SERVICE sync-unknowns")]
	[CommandForm(Name = "sync-unknowns")]
	[Description("")]
	public void SyncUnknowns(CommandContext context)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		List<ILocation> unknowns = service.GetUnknowns();
		if (unknowns.Count == 0)
		{
			LogHelper.Instance.Log("Service unknown sync: there are no UNKNOWNS. Nice job!");
			return;
		}
		IExecutionService service2 = ServiceLocator.Instance.GetService<IExecutionService>();
		IExecutionContext executionContext = service2.ScheduleJob("sync-locations", "UNKNOWN inventory sync.", DateTime.Now, ProgramPriority.Low);
		using (new DisposeableList<ILocation>(unknowns))
		{
			executionContext.Push(unknowns);
			LogHelper.Instance.Log("Service unknown sync: {0} locations ( job ID = {1} ).", unknowns.Count, executionContext.ID);
		}
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[CommandForm(Name = "sync-empty")]
	[Description("")]
	[Usage("SERVICE sync-empty")]
	public void SyncEmpty(CommandContext context)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		List<ILocation> emptySlots = service.GetEmptySlots();
		if (emptySlots.Count == 0)
		{
			LogHelper.Instance.Log("SyncEmpty: there are no empty slots.");
			return;
		}
		IExecutionService service2 = ServiceLocator.Instance.GetService<IExecutionService>();
		IExecutionContext executionContext = service2.ScheduleJob("sync-locations", "EMPTY inventory sync.", DateTime.Now, ProgramPriority.Low);
		using (new DisposeableList<ILocation>(emptySlots))
		{
			executionContext.Push(emptySlots);
			LogHelper.Instance.Log("Service empty sync: {0} locations ( job ID = {1} ).", emptySlots.Count, executionContext.ID);
		}
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Description("")]
	[CommandForm(Name = "full-sync")]
	[Usage("SERVICE full-sync")]
	public void FullSync(CommandContext context)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		IExecutionContext executionContext = service.ScheduleJob("sync", "Full inventory sync.", DateTime.Now, ProgramPriority.Low);
		IDecksService service2 = ServiceLocator.Instance.GetService<IDecksService>();
		IDeck deck = service2.Last;
		if (deck.IsQlm)
		{
			deck = service2.GetByNumber(deck.Number - 1);
		}
		executionContext.Push(deck.NumberOfSlots, StackEnd.Top);
		executionContext.Push(deck.Number, StackEnd.Top);
		IDeck first = service2.First;
		executionContext.Push(1, StackEnd.Top);
		executionContext.Push(first.Number, StackEnd.Top);
		LogHelper.Instance.Log("Service executing full sync ( job ID = {0} ).", executionContext.ID);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Description("")]
	[CommandForm(Name = "update-gamp")]
	[Usage("SERVICE update-gamp [backup-directory: 'c:\\backup\\path' ]")]
	public void UpdateGamp(CommandContext context, [CommandKeyValue(KeyName = "backup-directory")] string backupDirectory)
	{
		context.Errors.Add(Error.NewError("C999", "Unsupported Client API", "The update-gamp client API is no longer supported."));
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		context.Messages.Add(service.KioskId);
	}

	[Usage("SERVICE mark-kiosk-empty")]
	[Description("")]
	[CommandForm(Name = "mark-kiosk-empty")]
	public void MarkKioskEmpty(CommandContext context)
	{
		IExecutionContext executionContext = ScheduleHighPriority("mark-kiosk-inventory");
		executionContext.Push("EMPTY", StackEnd.Top);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Description("")]
	[CommandForm(Name = "mark-kiosk-unknown")]
	[Usage("SERVICE mark-kiosk-unknown")]
	public void MarkKioskUnknown(CommandContext context)
	{
		IExecutionContext executionContext = ScheduleHighPriority("mark-kiosk-inventory");
		executionContext.Push("UNKNOWN", StackEnd.Top);
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Description("")]
	[Usage("SERVICE dump-inventory-store")]
	[CommandForm(Name = "dump-inventory-store")]
	public void DumpInventoryStore(CommandContext context)
	{
		IExecutionContext executionContext = ScheduleHighPriority("dump-inventory-store-job");
		executionContext.Pend();
		CommandHelper.FormatJob(executionContext, context);
	}

	[Usage("SERVICE power-down")]
	[CommandForm(Name = "power-down")]
	[Description("")]
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

	[CommandForm(Name = "test-comm")]
	[Usage("SERVICE test-comm")]
	[Description("")]
	public void TestConnection(CommandContext context)
	{
		context.Messages.Add("ACK");
	}

	private IExecutionContext ScheduleHighPriority(string name)
	{
		return ScheduleHighPriority(name, string.Empty);
	}

	private IExecutionContext ScheduleHighPriority(string name, string label)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		return service.ScheduleJob(name, label, DateTime.Now, ProgramPriority.High);
	}

	private void OnChangeMode(bool enter, EngineModes mode)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		if (enter)
		{
			service.Enter(mode);
		}
		else
		{
			service.Exit(mode);
		}
	}
}
