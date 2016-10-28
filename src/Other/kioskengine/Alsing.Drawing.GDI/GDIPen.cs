using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Alsing.Windows;

namespace Alsing.Drawing.GDI;

[ComVisible(true)]
public class GDIPen : GDIObject
{
	public IntPtr hPen;

	public GDIPen(Color color, int width)
	{
		hPen = NativeMethods.CreatePen(0, width, NativeMethods.ColorToInt(color));
		Create();
	}

	protected override void Destroy()
	{
		if (hPen != (IntPtr)0)
		{
			NativeMethods.DeleteObject(hPen);
		}
		base.Destroy();
		hPen = (IntPtr)0;
	}
}
