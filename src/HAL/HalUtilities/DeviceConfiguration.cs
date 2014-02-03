namespace HALUtilities;

public class DeviceConfiguration
{
	private static DeviceConfiguration m_instance;

	public static DeviceConfiguration Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new DeviceConfiguration();
			}
			return m_instance;
		}
	}

	public int DevicePixelWidth { get; set; }

	public int DeviceHeight { get; set; }

	private DeviceConfiguration()
	{
	}
}
