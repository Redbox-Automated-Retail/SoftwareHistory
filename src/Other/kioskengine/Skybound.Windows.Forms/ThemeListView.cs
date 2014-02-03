namespace Skybound.Windows.Forms;

internal class ThemeListView
{
	private const int ClassID = 1792;

	public const int ListItemNormal = 1;

	public const int ListItemHot = 2;

	public const int ListItemSelected = 3;

	public const int ListItemDisabled = 4;

	public const int ListItemSelectedNotFocus = 5;

	public static readonly ThemeRenderer ListItem = new ThemeRenderer(1793);

	public static readonly ThemeRenderer ListGroup = new ThemeRenderer(1794);

	public static readonly ThemeRenderer ListDetail = new ThemeRenderer(1795);

	public static readonly ThemeRenderer ListSortedDetail = new ThemeRenderer(1796);

	public static readonly ThemeRenderer EmptyText = new ThemeRenderer(1797);
}
