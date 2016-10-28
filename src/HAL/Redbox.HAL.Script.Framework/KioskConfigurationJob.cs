using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "kiosk-configuration-job", Operand = "KIOSK-CONFIGURATION-JOB")]
internal sealed class KioskConfigurationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IScannerDeviceService service = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		IScannerDevice configuredDevice = service.GetConfiguredDevice();
		IDoorSensorService service2 = ServiceLocator.Instance.GetService<IDoorSensorService>();
		IDeck first = base.DecksService.First;
		base.Context.CreateInfoResult("MachineConfig", (first.NumberOfSlots == 90) ? "Dense" : "Sparse");
		base.Context.CreateInfoResult("CameraConfig", configuredDevice.Name);
		base.Context.CreateInfoResult("AuxRelayBoard", "Not Configured");
		base.Context.CreateInfoResult("MerchMode", ControllerConfiguration.Instance.IsVMZMachine ? "VMZ" : "QLM");
		base.Context.CreateInfoResult("DoorSensors", service2.SensorsEnabled ? "On" : "Off");
		if (service2.SensorsEnabled)
		{
			base.Context.CreateInfoResult("DoorSensorStatus", service2.Query().ToString().ToUpper());
		}
		base.Context.CreateInfoResult("QuickReturn", QuickReturnDevice.Instance.IsConfigured ? "Configured" : "Not configured");
		IAirExchangerService service3 = ServiceLocator.Instance.GetService<IAirExchangerService>();
		base.Context.CreateInfoResult("AirExchangerConfigured", service3.Configured ? "Configured" : "Not configured");
		IDiskValidatorService service4 = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		base.Context.CreateInfoResult("FraudSensorEnabled", service4.IsConfigured ? "Enabled" : "Not Enabled");
		AbeDeviceDescriptor abeDeviceDescriptor = new AbeDeviceDescriptor(debug: true);
		base.Context.CreateInfoResult("AbeDeviceStatus", abeDeviceDescriptor.Locate() ? "ATTACHED" : "NOT ATTACHED");
	}

	internal KioskConfigurationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
