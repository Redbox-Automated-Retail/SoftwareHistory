namespace Skybound.Windows.Forms;

internal class ThemeExplorerBar
{
	private const int ClassID = 1280;

	public const int HeaderCloseNormal = 1;

	public const int HeaderCloseHot = 2;

	public const int HeaderClosePressed = 3;

	public const int HeaderPinNormal = 1;

	public const int HeaderPinHot = 2;

	public const int HeaderPinPressed = 3;

	public const int HeaderPinSelectedNormal = 4;

	public const int HeaderPinSelectedHot = 5;

	public const int HeaderPinSelectedPressed = 6;

	public const int IEBarMenuNormal = 1;

	public const int IEBarMenuHot = 2;

	public const int IEBarMenuPressed = 3;

	public const int NormalGroupCollapseNormal = 1;

	public const int NormalGroupCollapseHot = 2;

	public const int NormalGroupCollapsePressed = 3;

	public const int NormalGroupExpandNormal = 1;

	public const int NormalGroupExpandHot = 2;

	public const int NormalGroupExpandPressed = 3;

	public const int SpecialGroupCollapseNormal = 1;

	public const int SpecialGroupCollapseHot = 2;

	public const int SpecialGroupCollapsePressed = 3;

	public const int SpecialGroupExpandNormal = 1;

	public const int SpecialGroupExpandHot = 2;

	public const int SpecialGroupExpandPressed = 3;

	public static readonly ThemeRenderer HeaderBackground = new ThemeRenderer(1281);

	public static readonly ThemeRenderer HeaderClose = new ThemeRenderer(1282);

	public static readonly ThemeRenderer HeaderPin = new ThemeRenderer(1283);

	public static readonly ThemeRenderer IEBarMenu = new ThemeRenderer(1284);

	public static readonly ThemeRenderer NormalGroupBackground = new ThemeRenderer(1285);

	public static readonly ThemeRenderer NormalGroupCollapse = new ThemeRenderer(1286);

	public static readonly ThemeRenderer NormalGroupExpand = new ThemeRenderer(1287);

	public static readonly ThemeRenderer NormalGroupHead = new ThemeRenderer(1288);

	public static readonly ThemeRenderer SpecialGroupBackground = new ThemeRenderer(1289);

	public static readonly ThemeRenderer SpecialGroupCollapse = new ThemeRenderer(1290);

	public static readonly ThemeRenderer SpecialGroupExpand = new ThemeRenderer(1291);

	public static readonly ThemeRenderer SpecialGroupHead = new ThemeRenderer(1292);
}
