namespace Skybound.Windows.Forms;

internal class ThemeTrackBar
{
	private const int ClassID = 5632;

	public const int TrackBarNormal = 1;

	public const int TrackNormal = 1;

	public const int ThumbNormal = 1;

	public const int ThumbHot = 2;

	public const int ThumbPressed = 3;

	public const int ThumbFocused = 4;

	public const int ThumbDisabled = 5;

	public const int TicsNormal = 1;

	public const int TicsVertNormal = 1;

	public static readonly ThemeRenderer Track = new ThemeRenderer(5633);

	public static readonly ThemeRenderer TrackVert = new ThemeRenderer(5634);

	public static readonly ThemeRenderer Thumb = new ThemeRenderer(5635);

	public static readonly ThemeRenderer ThumbBottom = new ThemeRenderer(5636);

	public static readonly ThemeRenderer ThumbTop = new ThemeRenderer(5637);

	public static readonly ThemeRenderer ThumbVert = new ThemeRenderer(5638);

	public static readonly ThemeRenderer ThumbLeft = new ThemeRenderer(5639);

	public static readonly ThemeRenderer ThumbRight = new ThemeRenderer(5640);

	public static readonly ThemeRenderer Tics = new ThemeRenderer(5641);

	public static readonly ThemeRenderer TicsVert = new ThemeRenderer(5642);
}
