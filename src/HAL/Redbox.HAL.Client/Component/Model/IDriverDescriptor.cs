using System;

namespace Redbox.HAL.Component.Model;

internal interface IDriverDescriptor
{
	Version DriverVersion { get; }

	string Provider { get; }
}
