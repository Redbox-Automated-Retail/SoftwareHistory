using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Alsing.Windows;

namespace Alsing.Drawing.GDI;

[ComVisible(true)]
public class GDIBrush : GDIObject
{
	public IntPtr hBrush;

	protected bool mSystemBrush;

	public GDIBrush(Color color)
	{
		hBrush = NativeMethods.CreateSolidBrush(NativeMethods.ColorToInt(color));
		Create();
	}

	public GDIBrush(Bitmap pattern)
	{
		hBrush = NativeMethods.CreatePatternBrush(pattern.GetHbitmap());
		Create();
	}

	public GDIBrush(IntPtr hBMP_Pattern)
	{
		hBrush = NativeMethods.CreatePatternBrush(hBMP_Pattern);
		Create();
	}

	public GDIBrush(int Style, Color color)
	{
		hBrush = NativeMethods.CreateHatchBrush(Style, NativeMethods.ColorToInt(color));
		Create();
	}

	public GDIBrush(int BrushIndex)
	{
		hBrush = (IntPtr)BrushIndex;
		mSystemBrush = true;
		Create();
	}

	protected override void Destroy()
	{
		if (!mSystemBrush && hBrush != (IntPtr)0)
		{
			NativeMethods.DeleteObject(hBrush);
		}
		base.Destroy();
		hBrush = (IntPtr)0;
	}
}
