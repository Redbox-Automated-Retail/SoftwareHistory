namespace Skybound.Windows.Forms;

internal class ThemeComboBox
{
	private const int ClassID = 768;

	public const int DropDownButtonNormal = 1;

	public const int DropDownButtonHot = 2;

	public const int DropDownButtonPressed = 3;

	public const int DropDownButtonDisabled = 4;

	public static readonly ThemeRenderer DropDownButton = new ThemeRenderer(769);
}
