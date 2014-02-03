namespace Skybound.Windows.Forms;

internal class ThemeTab
{
	private const int ClassID = 4352;

	public const int ItemNormal = 1;

	public const int ItemHot = 2;

	public const int ItemSelected = 3;

	public const int ItemDisabled = 4;

	public const int ItemFocused = 5;

	public static readonly ThemeRenderer Item = new ThemeRenderer(4353);

	public static readonly ThemeRenderer ItemLeftEdge = new ThemeRenderer(4354);

	public static readonly ThemeRenderer ItemRightEdge = new ThemeRenderer(4355);

	public static readonly ThemeRenderer ItemBothEdge = new ThemeRenderer(4356);

	public static readonly ThemeRenderer TopTabItem = new ThemeRenderer(4357);

	public static readonly ThemeRenderer TopTabItemLeftEdge = new ThemeRenderer(4358);

	public static readonly ThemeRenderer TopTabItemRightEdge = new ThemeRenderer(4359);

	public static readonly ThemeRenderer TopTabItemBothEdge = new ThemeRenderer(4360);

	public static readonly ThemeRenderer Pane = new ThemeRenderer(4361);

	public static readonly ThemeRenderer Body = new ThemeRenderer(4362);
}
