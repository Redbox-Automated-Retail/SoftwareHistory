using System;

namespace Redbox.HAL.Component.Model;

internal interface IDeviceSetupClass
{
	DeviceClass Class { get; }

	Guid Guid { get; }
}
