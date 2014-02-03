namespace Skybound.Windows.Forms;

internal class ThemeRebar
{
	private const int ClassID = 3072;

	public const int ChevronNormal = 1;

	public const int ChevronHot = 2;

	public const int ChevronPressed = 3;

	public static readonly ThemeRenderer Gripper = new ThemeRenderer(3073);

	public static readonly ThemeRenderer GripperVert = new ThemeRenderer(3074);

	public static readonly ThemeRenderer Band = new ThemeRenderer(3075);

	public static readonly ThemeRenderer Chevron = new ThemeRenderer(3076);

	public static readonly ThemeRenderer ChevronVert = new ThemeRenderer(3077);
}
