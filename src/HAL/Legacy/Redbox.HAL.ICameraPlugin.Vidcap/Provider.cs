using Redbox.HAL.Core;

namespace Redbox.HAL.ICameraPlugin.Vidcap;

public class Provider
{
	public static Redbox.HAL.Core.ICameraPlugin GetCameraPlugin()
	{
		return VidcapService.Instance;
	}
}
