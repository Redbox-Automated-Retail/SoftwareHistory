using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "one-disk-random-sync", HideFromList = true)]
internal sealed class OneDiskRandomSyncJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "RandomSync", flushOnWrite: true, null);
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IControllerService service3 = ServiceLocator.Instance.GetService<IControllerService>();
		base.Context.AppLog.WriteFormatted("Vend time is set to {0}s, vend frequency = {1}", num2, num);
		base.Context.AppLog.Write("One disk random sync test start.");
		RandomLocationSelector randomLocationSelector = new RandomLocationSelector(base.DecksService, base.InventoryService);
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		int num3 = 0;
		bool flag = false;
		if (service2.MoveTo(1, 1, MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
		{
			flag = true;
		}
		else if (!service3.Get(base.Context.AppLog).Success)
		{
			flag = true;
		}
		while (!flag)
		{
			ILocation location = randomLocationSelector.SelectTargetLocation();
			if (location == null)
			{
				break;
			}
			if (location.Excluded)
			{
				base.Context.AppLog.WriteFormatted("Not syncing excluded location {0}", location);
				continue;
			}
			if (!isVMZMachine && location.Deck == 8)
			{
				if (base.ControlSystem.EngageQlm(base.Context.AppLog) != ErrorCodes.Success)
				{
					flag = true;
					continue;
				}
				Thread.Sleep(1500);
				if (base.ControlSystem.DisengageQlm(base.Context.AppLog) != ErrorCodes.Success)
				{
					flag = true;
				}
				continue;
			}
			base.Context.AppLog.WriteFormatted("Sync {0}", location.ToString());
			num3++;
			if (num != 0)
			{
				_ = num3 % num == 0;
			}
			else
				_ = 0;
			ScanResult scanResult = null;
			using (Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, service))
			{
				scanResult = scanner.Read();
				if (!scanResult.SnapOk)
				{
					base.Context.CreateResult("MachineError", "The CAMERA CAPTURE request timed out.", null, null, null, null, null);
					flag = true;
				}
			}
			if (service2.MoveTo(location, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
			{
				flag = true;
			}
			else if (!service3.Put(scanResult.ScannedMatrix, base.Context.AppLog).Success)
			{
				flag = true;
			}
			else if (service2.MoveTo(location, MoveMode.Get, base.Context.AppLog) != ErrorCodes.Success)
			{
				flag = true;
			}
			else if (!service3.Get(base.Context.AppLog).Success)
			{
				flag = true;
			}
			else if (OnVendReceive(num2) != ErrorCodes.Success)
			{
				flag = true;
			}
		}
		if (service2.MoveTo(1, 1, MoveMode.Put, base.Context.AppLog) != ErrorCodes.Success)
		{
			flag = true;
		}
		else if (!service3.Put("UNKNOWN", base.Context.AppLog).Success)
		{
			flag = true;
		}
		applicationLog.WriteFormatted("Job visited {0} locations", num3);
		if (flag)
		{
			AddError("Job error");
		}
	}

	internal OneDiskRandomSyncJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	private ErrorCodes OnVendReceive(int vendTime)
	{
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.None, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		using (VendItemOperation vendItemOperation = new VendItemOperation(vendTime))
		{
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (!vendItemResult.Presented)
			{
				base.Context.AppLog.Write("The disk was not presented to the user from the vend door.");
				return ErrorCodes.PickerObstructed;
			}
			if (vendItemResult.Status == ErrorCodes.PickerEmpty)
			{
				base.Context.AppLog.Write("The disk is no longer present in the picker. This is not good.");
				return vendItemResult.Status;
			}
		}
		using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
		{
			ErrorCodes errorCodes2 = acceptDiskOperation.Execute();
			if (ErrorCodes.PickerFull != errorCodes2)
			{
				base.Context.AppLog.WriteFormatted("Accept disk opertaion returned status {0}", errorCodes2.ToString());
				return errorCodes2;
			}
		}
		return ErrorCodes.Success;
	}
}
