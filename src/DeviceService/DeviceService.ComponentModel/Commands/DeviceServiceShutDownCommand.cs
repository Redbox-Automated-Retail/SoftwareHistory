using System;

namespace DeviceService.ComponentModel.Commands;

public class DeviceServiceShutDownCommand : BaseCommandRequest
{
	public override string SignalRCommand => "ExecuteDeviceServiceShutDownCommand";

	public bool ForceShutDown { get; set; }

	public ShutDownReason Reason { get; set; }

	public DeviceServiceShutDownCommand(Version deviceServiceClientVersion)
		: base("DeviceServiceShutDown", deviceServiceClientVersion)
	{
	}
}
