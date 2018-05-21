using System;

namespace Redbox.HAL.Component.Model;

public interface IScannerDeviceService
{
	DateTime? IRHardwareInstall { get; }

	IScannerDevice GetConfiguredDevice();
}
