using System;
using System.Drawing;
using Alsing.Windows;

namespace Alsing.Globalization;

public class IMEWindow
{
	private const int CFS_POINT = 2;

	private const byte FF_MODERN = 48;

	private const byte FIXED_PITCH = 1;

	private const int IMC_SETCOMPOSITIONFONT = 10;

	private const int IMC_SETCOMPOSITIONWINDOW = 12;

	private readonly IntPtr hIMEWnd;

	private Font _Font;

	private Point _Loation;

	public Font Font
	{
		get
		{
			return _Font;
		}
		set
		{
			if (!_Font.Equals(value))
			{
				SetFont(value);
				_Font = value;
			}
		}
	}

	public Point Loation
	{
		get
		{
			return _Loation;
		}
		set
		{
			_Loation = value;
			APIPoint ptCurrentPos = new APIPoint
			{
				x = value.X,
				y = value.Y
			};
			COMPOSITIONFORM cOMPOSITIONFORM = new COMPOSITIONFORM();
			cOMPOSITIONFORM.dwStyle = 2;
			cOMPOSITIONFORM.ptCurrentPos = ptCurrentPos;
			cOMPOSITIONFORM.rcArea = default(APIRect);
			COMPOSITIONFORM lParam = cOMPOSITIONFORM;
			NativeMethods.SendMessage(hIMEWnd, 643, 12, lParam);
		}
	}

	public IMEWindow(IntPtr hWnd, string fontname, float fontsize)
	{
		hIMEWnd = NativeMethods.ImmGetDefaultIMEWnd(hWnd);
		SetFont(fontname, fontsize);
	}

	public void SetFont(Font font)
	{
		LogFont logFont = new LogFont();
		font.ToLogFont(logFont);
		logFont.lfPitchAndFamily = 49;
		NativeMethods.SendMessage(hIMEWnd, 643, 10, logFont);
	}

	public void SetFont(string fontname, float fontsize)
	{
		LogFont logFont = new LogFont();
		logFont.lfItalic = 0;
		logFont.lfStrikeOut = 0;
		logFont.lfUnderline = 0;
		logFont.lfWeight = 400;
		logFont.lfWidth = 0;
		logFont.lfHeight = (int)((double)(0f - fontsize) * 1.3333333333333);
		logFont.lfCharSet = 1;
		logFont.lfPitchAndFamily = 49;
		logFont.lfFaceName = fontname;
		LogFont logFont2 = logFont;
		LogFont lParam = logFont2;
		NativeMethods.SendMessage(hIMEWnd, 643, 10, lParam);
	}
}
