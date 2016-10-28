using System;
using System.Runtime.InteropServices;
using Alsing.Windows;

namespace Alsing.Drawing.GDI;

[ComVisible(true)]
public class GDIFont : GDIObject
{
	public bool Bold;

	public byte Charset;

	public string FontName;

	public IntPtr hFont;

	public bool Italic;

	public float Size;

	public bool Strikethrough;

	public bool Underline;

	public GDIFont()
	{
		Create();
	}

	public GDIFont(string fontname, float size)
	{
		Init(fontname, size, bold: false, italic: false, underline: false, strikethrough: false);
		Create();
	}

	public GDIFont(string fontname, float size, bool bold, bool italic, bool underline, bool strikethrough)
	{
		Init(fontname, size, bold, italic, underline, strikethrough);
		Create();
	}

	protected void Init(string fontname, float size, bool bold, bool italic, bool underline, bool strikethrough)
	{
		FontName = fontname;
		Size = size;
		Bold = bold;
		Italic = italic;
		Underline = underline;
		Strikethrough = strikethrough;
		LogFont logFont = new LogFont();
		logFont.lfItalic = (byte)(Italic ? 1u : 0u);
		logFont.lfStrikeOut = (byte)(Strikethrough ? 1u : 0u);
		logFont.lfUnderline = (byte)(Underline ? 1u : 0u);
		logFont.lfWeight = (Bold ? 700 : 400);
		logFont.lfWidth = 0;
		logFont.lfHeight = (int)((double)(0f - Size) * 1.3333333333333);
		logFont.lfCharSet = 1;
		logFont.lfFaceName = FontName;
		LogFont logFont2 = logFont;
		hFont = NativeMethods.CreateFontIndirect(logFont2);
	}

	~GDIFont()
	{
		Destroy();
	}

	protected override void Destroy()
	{
		if (hFont != (IntPtr)0)
		{
			NativeMethods.DeleteObject(hFont);
		}
		base.Destroy();
		hFont = (IntPtr)0;
	}
}
