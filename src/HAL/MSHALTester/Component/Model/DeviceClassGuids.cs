using System;

namespace Redbox.HAL.Component.Model;

public static class DeviceClassGuids
{
	public static readonly Guid Image = new Guid("6BDD1FC6-810F-11D0-BEC7-08002BE2092F");

	public static readonly Guid USB = new Guid("36FC9E60-C465-11CF-8056-444553540000");

	public static readonly Guid HID = new Guid("745a17a0-74d3-11d0-b6fe-00a0c90f57da");

	public static readonly Guid Mouse = new Guid("4D36E96F-E325-11CE-BFC1-08002BE10318");

	public static readonly Guid Ports = new Guid("4D36E978-E325-11CE-BFC1-08002BE10318");
}
