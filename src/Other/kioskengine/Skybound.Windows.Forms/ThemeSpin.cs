namespace Skybound.Windows.Forms;

internal class ThemeSpin
{
	private const int ClassID = 3584;

	public const int Normal = 1;

	public const int Hot = 2;

	public const int Pressed = 3;

	public const int Disabled = 4;

	public static readonly ThemeRenderer Up = new ThemeRenderer(3585);

	public static readonly ThemeRenderer Down = new ThemeRenderer(3586);

	public static readonly ThemeRenderer UpHorz = new ThemeRenderer(3587);

	public static readonly ThemeRenderer DownHorz = new ThemeRenderer(3588);
}
