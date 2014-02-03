namespace Skybound.Windows.Forms;

internal class ThemeButton
{
	private const int ClassID = 256;

	public const int PushButtonNormal = 1;

	public const int PushButtonHot = 2;

	public const int PushButtonPressed = 3;

	public const int PushButtonDisabled = 4;

	public const int PushButtonDefaulted = 5;

	public const int RadioButtonUncheckedNormal = 1;

	public const int RadioButtonUncheckedHot = 2;

	public const int RadioButtonUncheckedPressed = 3;

	public const int RadioButtonUncheckedDisabled = 4;

	public const int RadioButtonCheckedNormal = 5;

	public const int RadioButtonCheckedHot = 6;

	public const int RadioButtonCheckedPressed = 7;

	public const int RadioButtonCheckedDisabled = 8;

	public const int CheckBoxUncheckedNormal = 1;

	public const int CheckBoxUncheckedHot = 2;

	public const int CheckBoxUncheckedPressed = 3;

	public const int CheckBoxUncheckedDisabled = 4;

	public const int CheckBoxCheckedNormal = 5;

	public const int CheckBoxCheckedHot = 6;

	public const int CheckBoxCheckedPressed = 7;

	public const int CheckBoxCheckedDisabled = 8;

	public const int CheckBoxMixedNormal = 9;

	public const int CheckBoxMixedHot = 10;

	public const int CheckBoxMixedPressed = 11;

	public const int CheckBoxMixedDisabled = 12;

	public const int GroupBoxNormal = 1;

	public const int GroupBoxDisabled = 2;

	public static readonly ThemeRenderer PushButton = new ThemeRenderer(257);

	public static readonly ThemeRenderer RadioButton = new ThemeRenderer(258);

	public static readonly ThemeRenderer CheckBox = new ThemeRenderer(259);

	public static readonly ThemeRenderer GroupBox = new ThemeRenderer(260);

	public static readonly ThemeRenderer UserButton = new ThemeRenderer(261);
}
