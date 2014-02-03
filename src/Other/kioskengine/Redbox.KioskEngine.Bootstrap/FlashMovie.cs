using System.Drawing;
using AxShockwaveFlashObjects;

namespace Redbox.KioskEngine.Bootstrap;

public class FlashMovie
{
	public Rectangle Zone { get; set; }

	public string FileName { get; set; }

	public AxShockwaveFlash Player { get; set; }
}
