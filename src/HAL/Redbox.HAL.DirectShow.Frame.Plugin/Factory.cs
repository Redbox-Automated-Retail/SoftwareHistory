using Redbox.HAL.Core;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

public sealed class Factory
{
	private static Service m_instance;

	public static ICameraPlugin GetCameraPlugin()
	{
		if (m_instance == null)
		{
			m_instance = new Service();
		}
		return m_instance;
	}
}
