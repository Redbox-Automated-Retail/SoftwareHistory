namespace Redbox.HAL.Core;

internal class DeviceDescriptor
{
	private string m_deviceRep;

	public string Vendor { get; private set; }

	public string Product { get; private set; }

	public string Friendlyname { get; private set; }

	public override string ToString()
	{
		if (m_deviceRep != null)
		{
			return m_deviceRep;
		}
		return m_deviceRep = $"vid_{Vendor}&pid_{Product}";
	}

	public DeviceDescriptor(string v, string p, string friendly)
	{
		Vendor = v;
		Product = p;
		Friendlyname = friendly;
	}
}
