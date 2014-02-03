namespace Skybound.Windows.Forms;

internal class ThemeEdit
{
	private const int ClassID = 1024;

	public const int TextNormal = 1;

	public const int TextHot = 2;

	public const int TextSelected = 3;

	public const int TextDisabled = 4;

	public const int TextFocused = 5;

	public const int TextReadOnly = 6;

	public const int TextAssist = 7;

	public static readonly ThemeRenderer Text = new ThemeRenderer(1025);

	public static readonly ThemeRenderer Caret = new ThemeRenderer(1026);
}
