using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-inventory-job", Operand = "RESET-INVENTORY-JOB")]
internal sealed class ResetInventoryJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string text = GetTop<string>();
		if (!File.Exists(text))
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			text = Path.Combine(service.StoreDirectory, text);
			if (!File.Exists(text))
			{
				AddError("The file {0} doesn't exist.");
				return;
			}
		}
		base.Context.ServiceLog("Reset inventory from file '{0}'", text);
		try
		{
			string xml = File.ReadAllText(text);
			IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
			service2.ResetInventoryState(xml, base.Result.Errors);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Reset-inventory caught an exception.", e);
		}
	}

	internal ResetInventoryJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
