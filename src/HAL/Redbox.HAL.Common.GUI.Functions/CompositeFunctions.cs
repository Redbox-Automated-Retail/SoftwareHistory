using System;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;

namespace Redbox.HAL.Common.GUI.Functions;

public static class CompositeFunctions
{
	public static void VendDisk(HardwareService service, OutputBox box)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		VendDiskInPickerJob val = new VendDiskInPickerJob(service);
		try
		{
			((JobExecutor)val).Run();
			foreach (ProgramResult result in ((JobExecutor)val).Results)
			{
				box.Write(result.Code);
				if (result.Code.Equals("ItemVended", StringComparison.CurrentCultureIgnoreCase))
				{
					box.Write("Disk was taken.");
					return;
				}
				if (result.Code.Equals("HardwareError", StringComparison.CurrentCultureIgnoreCase))
				{
					box.Write(result.Message);
				}
			}
			box.Write("Disk not taken - pulled back in.");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static bool GetItem(int deck, int slot, OutputBox box, HardwareService service)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		GetAndCenterResult val = new GetAndCenterResult(service, deck, slot, false);
		try
		{
			((JobExecutor)val).Run();
			box.Write("GetDVD ended with status {0}", ((object)((JobExecutor)val).EndStatus/*cast due to constrained. prefix*/).ToString());
			int num = 0;
			foreach (ProgramResult result in ((JobExecutor)val).Results)
			{
				if (result.Code == "ErrorMessage")
				{
					num++;
				}
				box.Write(result.Message);
			}
			return num == 0;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static bool PutItem(HardwareService service, int deck, int slot, OutputBox box)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Expected O, but got Unknown
		PutInLocationResult val = new PutInLocationResult(service, deck, slot);
		try
		{
			((JobExecutor)val).Run();
			int num = 0;
			foreach (ProgramResult result in ((JobExecutor)val).Results)
			{
				if (result.Code == "JobError")
				{
					num++;
				}
				box.Write(result.Message);
			}
			return num == 0;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
