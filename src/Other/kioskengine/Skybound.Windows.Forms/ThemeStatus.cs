namespace Skybound.Windows.Forms;

internal class ThemeStatus
{
	private const int ClassID = 4096;

	public static readonly ThemeRenderer Pane = new ThemeRenderer(4097);

	public static readonly ThemeRenderer GripperPane = new ThemeRenderer(4098);

	public static readonly ThemeRenderer Gripper = new ThemeRenderer(4099);
}
