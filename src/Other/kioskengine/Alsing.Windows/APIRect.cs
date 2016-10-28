using System.Drawing;
using System.Runtime.InteropServices;

namespace Alsing.Windows;

[ComVisible(true)]
public struct APIRect
{
	public int left;

	public int top;

	public int right;

	public int bottom;

	public int Width => right - left;

	public int Height => bottom - top;

	public APIRect(Rectangle rect)
	{
		bottom = rect.Bottom;
		left = rect.Left;
		right = rect.Right;
		top = rect.Top;
	}

	public APIRect(int left, int top, int right, int bottom)
	{
		this.bottom = bottom;
		this.left = left;
		this.right = right;
		this.top = top;
	}
}
