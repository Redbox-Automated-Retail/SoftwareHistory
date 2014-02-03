namespace Skybound.Windows.Forms;

internal class ThemeScrollBar
{
	private const int ClassID = 3328;

	public const int ArrowBtnUpNormal = 1;

	public const int ArrowBtnUpHot = 2;

	public const int ArrowBtnUpPressed = 3;

	public const int ArrowBtnUpDisabled = 4;

	public const int ArrowBtnDownNormal = 5;

	public const int ArrowBtnDownHot = 6;

	public const int ArrowBtnDownPressed = 7;

	public const int ArrowBtnDownDisabled = 8;

	public const int ArrowBtnLeftNormal = 9;

	public const int ArrowBtnLeftHot = 10;

	public const int ArrowBtnLeftPressed = 11;

	public const int ArrowBtnLeftDisabled = 12;

	public const int ArrowBtnRightNormal = 13;

	public const int ArrowBtnRightHot = 14;

	public const int ArrowBtnRightPressed = 15;

	public const int ArrowBtnRightDisabled = 16;

	public const int Normal = 1;

	public const int Hot = 2;

	public const int Pressed = 3;

	public const int Disabled = 4;

	public const int SizeBoxRightAlign = 1;

	public const int SizeBoxLeftAlign = 2;

	public static readonly ThemeRenderer ArrowBtn = new ThemeRenderer(3329);

	public static readonly ThemeRenderer ThumbBtnHorz = new ThemeRenderer(3330);

	public static readonly ThemeRenderer ThumbBtnVert = new ThemeRenderer(3331);

	public static readonly ThemeRenderer LowerTrackHorz = new ThemeRenderer(3332);

	public static readonly ThemeRenderer UpperTrackHorz = new ThemeRenderer(3333);

	public static readonly ThemeRenderer LowerTrackVert = new ThemeRenderer(3334);

	public static readonly ThemeRenderer UpperTrackVert = new ThemeRenderer(3335);

	public static readonly ThemeRenderer GripperHorz = new ThemeRenderer(3336);

	public static readonly ThemeRenderer GripperVert = new ThemeRenderer(3337);

	public static readonly ThemeRenderer SizeBox = new ThemeRenderer(3338);
}
