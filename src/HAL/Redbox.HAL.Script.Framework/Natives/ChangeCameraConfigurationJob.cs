using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "change-camera-configuration")]
internal sealed class ChangeCameraConfigurationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ScannerServices scannerServices = base.Context.PopTop<ScannerServices>();
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		service.SetPropertyByName("CAMERA", "ScannerService", new object[1] { scannerServices });
		IScannerDeviceService service2 = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		IScannerDevice configuredDevice = service2.GetConfiguredDevice();
		if (configuredDevice.Service != scannerServices)
		{
			AddError("Failed to toggle service.");
		}
	}

	internal ChangeCameraConfigurationJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
