using System.Runtime.InteropServices;

namespace x1b802d6029d7be4a;

internal sealed class xc1ab0cc8a4568e7b
{
	private const int x33c00bbcdd1a07e2 = 4118;

	private const int xf9440d839b8d0898 = 4120;

	public static bool x6b254b18d1dfb65f
	{
		get
		{
			SystemParametersInfo(4118, 0, out var lpvParam, 0);
			return lpvParam != 0;
		}
	}

	public static bool xe9b7df98d555f9fe
	{
		get
		{
			SystemParametersInfo(4120, 0, out var lpvParam, 0);
			return lpvParam != 0;
		}
	}

	public static bool x003e94eb365fa7c9 => true;

	[DllImport("user32")]
	private static extern int SystemParametersInfo(int uAction, int uParam, out int lpvParam, int fuWinIni);

	private xc1ab0cc8a4568e7b()
	{
	}
}
