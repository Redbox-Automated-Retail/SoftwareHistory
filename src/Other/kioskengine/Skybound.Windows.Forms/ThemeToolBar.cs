namespace Skybound.Windows.Forms;

internal class ThemeToolBar
{
	private const int ClassID = 5120;

	public const int Normal = 1;

	public const int Hot = 2;

	public const int Pressed = 3;

	public const int Disabled = 4;

	public const int Checked = 5;

	public const int HotChecked = 6;

	public static readonly ThemeRenderer Button = new ThemeRenderer(5121);

	public static readonly ThemeRenderer DropDownButton = new ThemeRenderer(5122);

	public static readonly ThemeRenderer SplitButton = new ThemeRenderer(5123);

	public static readonly ThemeRenderer SplitButtonDropDown = new ThemeRenderer(5124);

	public static readonly ThemeRenderer Separator = new ThemeRenderer(5125);

	public static readonly ThemeRenderer SeparatorVert = new ThemeRenderer(5126);
}
