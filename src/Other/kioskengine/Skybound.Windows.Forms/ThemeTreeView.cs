namespace Skybound.Windows.Forms;

internal class ThemeTreeView
{
	private const int ClassID = 6144;

	public const int GlyphClosed = 1;

	public const int GlyphOpened = 2;

	public static readonly ThemeRenderer Item = new ThemeRenderer(6145);

	public static readonly ThemeRenderer Glyph = new ThemeRenderer(6146);

	public static readonly ThemeRenderer Branch = new ThemeRenderer(6147);
}
