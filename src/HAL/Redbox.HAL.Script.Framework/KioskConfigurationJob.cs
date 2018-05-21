using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "kiosk-configuration-job", Operand = "KIOSK-CONFIGURATION-JOB")]
internal sealed class KioskConfigurationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		IDeck first = base.DecksService.First;
		base.Context.CreateInfoResult("MachineConfig", (first.NumberOfSlots == 90) ? "Dense" : "Sparse");
		base.Context.CreateInfoResult("CameraConfig", configuredDevice.Name);
		base.Context.CreateInfoResult("AuxRelayBoard", "Not Configured");
		base.Context.CreateInfoResult("MerchMode", ControllerConfiguration.Instance.IsVMZMachine ? "VMZ" : "QLM");
		base.Context.CreateInfoResult("DoorSensors", service.SensorsEnabled ? "On" : "Off");
		if (service.SensorsEnabled)
		{
			base.Context.CreateInfoResult("DoorSensorStatus", service.Query().ToString().ToUpper());
		}
		base.Context.CreateInfoResult("QuickReturn", "Not configured");
		IAirExchangerService service2 = ServiceLocator.Instance.GetService<IAirExchangerService>();
		base.Context.CreateInfoResult("AirExchangerConfigured", service2.Configured ? "Configured" : "Not configured");
		base.Context.CreateInfoResult("AirExchangerStatus", service2.CheckStatus().ToString());
		base.Context.CreateInfoResult("AirExchangerFanStatus", service2.FanStatus.ToString());
		ServiceLocator.Instance.GetService<IDiskValidatorService>();
		IUsbDeviceService service3 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		base.Context.CreateInfoResult("FraudSensorEnabled", ControllerConfiguration.Instance.EnableSecureDiskValidator ? "Enabled" : "Not Enabled");
		AbeDeviceDescriptor abeDeviceDescriptor = new AbeDeviceDescriptor(service3, debug: true);
		base.Context.CreateInfoResult("AbeDeviceStatus", abeDeviceDescriptor.Locate() ? "ATTACHED" : "NOT ATTACHED");
		base.Context.CreateInfoResult("QuickReturnStatus", DeviceStatus.None.ToString());
		base.Context.CreateInfoResult("RouterPowerRelay", (ControllerConfiguration.Instance.RouterPowerCyclePause > 0) ? "Configured" : "Not Configured");
		base.Context.CreateInfoResult("SupportsArcusReset", (ControllerConfiguration.Instance.RestartControllerDuringUserJobs && ControllerConfiguration.Instance.TrackHardwareCorrections) ? "Configured" : "Not Configured");
		IScannerDeviceService service4 = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		base.Context.CreateInfoResult("SupportsFraudScan", "Not Configured");
		base.Context.CreateInfoResult("IRCameraHardwareInstall", service4.IRHardwareInstall.HasValue ? service4.IRHardwareInstall.ToString() : "NONE");
	}

	internal KioskConfigurationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
