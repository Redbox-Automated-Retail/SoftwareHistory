using System;

namespace DeviceService.ComponentModel.Commands;

public class DeviceServiceCanShutDownCommand : BaseCommandRequest
{
	public override string SignalRCommand => "ExecuteDeviceServiceCanShutDownCommand";

	public bool CanShutDown { get; set; }

	public DeviceServiceCanShutDownCommand(Version deviceServiceClientVersion)
		: base("DeviceServiceCanShutDown", deviceServiceClientVersion)
	{
	}
}
