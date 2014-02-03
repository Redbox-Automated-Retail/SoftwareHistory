namespace Skybound.Windows.Forms;

internal class ThemeHeader
{
	private const int ClassID = 1536;

	public const int ItemNormal = 1;

	public const int ItemHot = 2;

	public const int ItemPressed = 3;

	public const int ItemLeftNormal = 1;

	public const int ItemLeftHot = 2;

	public const int ItemLeftPressed = 3;

	public const int ItemRightNormal = 1;

	public const int ItemRightHot = 2;

	public const int ItemRightPressed = 3;

	public const int SortArrowSortedUp = 1;

	public const int SortArrowSortedDown = 2;

	public static readonly ThemeRenderer Item = new ThemeRenderer(1537);

	public static readonly ThemeRenderer ItemLeft = new ThemeRenderer(1538);

	public static readonly ThemeRenderer ItemRight = new ThemeRenderer(1539);

	public static readonly ThemeRenderer SortArrow = new ThemeRenderer(1540);
}
