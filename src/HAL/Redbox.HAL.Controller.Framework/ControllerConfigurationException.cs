using System;

namespace Redbox.HAL.Controller.Framework;

public class ControllerConfigurationException : Exception
{
	public ControllerConfigurationException()
	{
	}

	public ControllerConfigurationException(string msg)
		: base(msg)
	{
	}
}
