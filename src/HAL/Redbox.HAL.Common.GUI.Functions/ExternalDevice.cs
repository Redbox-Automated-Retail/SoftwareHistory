using System.Collections.Generic;

namespace Redbox.HAL.Common.GUI.Functions;

public class ExternalDevice
{
	public string Name { get; set; }

	public List<ExternalDeviceProperty> Properties { get; private set; }

	public bool IsInstalled { get; set; }

	public ExternalDevice(string name)
	{
		Name = name;
		Properties = new List<ExternalDeviceProperty>();
	}
}
