using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class DeviceDescriptor : IDeviceDescriptor
{
	private string m_deviceRep;

	public string Vendor { get; private set; }

	public string Product { get; private set; }

	public string Friendlyname { get; private set; }

	public DeviceClass DeviceClazz { get; private set; }

	public override string ToString()
	{
		if (m_deviceRep != null)
		{
			return m_deviceRep;
		}
		return m_deviceRep = $"vid_{Vendor}&pid_{Product}";
	}

	public bool MatchDriver()
	{
		return false;
	}

	public bool Locate()
	{
		return false;
	}

	public DeviceStatus GetStatus()
	{
		return DeviceStatus.Unknown;
	}

	public bool ResetDriver()
	{
		return false;
	}

	public DeviceDescriptor(string v, string p, string friendly, DeviceClass clazz)
	{
		Vendor = v.ToLower();
		Product = p.ToLower();
		Friendlyname = friendly;
		DeviceClazz = clazz;
	}
}
