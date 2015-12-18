using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "kiosk-configuration-job", Operand = "KIOSK-CONFIGURATION-JOB")]
internal sealed class KioskConfigurationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		Deck deck = DecksManager.Instance.GetDeck(1);
		base.Context.CreateInfoResult("MachineConfig", (deck.NumberOfSlots == 90) ? "Dense" : "Sparse");
		base.Context.CreateInfoResult("CameraConfig", BarcodeConfiguration.Instance.ScannerService.ToString());
		base.Context.CreateInfoResult("MerchMode", ControllerConfiguration.Instance.IsVMZMachine ? "VMZ" : "QLM");
		base.Context.CreateInfoResult("DoorSensors", DoorSensors.Instance.SensorsEnabled ? "On" : "Off");
		if (DoorSensors.Instance.SensorsEnabled)
		{
			base.Context.CreateInfoResult("DoorSensorStatus", DoorSensors.Instance.Query().ToString().ToUpper());
		}
		base.Context.CreateInfoResult("QuickReturn", QuickReturnDevice.Instance.IsConfigured ? "Configured" : "Not configured");
		base.Context.CreateInfoResult("AirExchangerConfigured", IceQubeBoard.Instance.Configured ? "Configured" : "Not configured");
	}

	internal KioskConfigurationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
