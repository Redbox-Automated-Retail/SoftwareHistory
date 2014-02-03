using System;
using System.Collections;
using System.Drawing;
using Alsing.Windows;

namespace Alsing.Drawing.GDI;

public class FontEnum
{
	private Hashtable Fonts;

	public ICollection EnumFonts()
	{
		Bitmap bitmap = new Bitmap(10, 10);
		Graphics graphics = Graphics.FromImage(bitmap);
		IntPtr hdc = graphics.GetHdc();
		Fonts = new Hashtable();
		LogFont logFont = new LogFont();
		logFont.lfCharSet = 1;
		LogFont lf = logFont;
		FONTENUMPROC proc = CallbackFunc;
		NativeMethods.EnumFontFamiliesEx(hdc, lf, proc, 0L, 0L);
		graphics.ReleaseHdc(hdc);
		graphics.Dispose();
		bitmap.Dispose();
		return Fonts.Keys;
	}

	private int CallbackFunc(ENUMLOGFONTEX f, int a, int b, int LParam)
	{
		Fonts[f.elfLogFont.lfFaceName] = "abc";
		return 1;
	}
}
