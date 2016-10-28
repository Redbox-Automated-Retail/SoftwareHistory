using System.Runtime.InteropServices;
using Alsing.Drawing.GDI;

namespace Alsing.Windows.Forms.SyntaxBox.Painter;

[ComVisible(true)]
public struct RenderItems
{
	public GDISurface BackBuffer;

	public GDIBrush BackgroundBrush;

	public GDIFont FontBold;

	public GDIFont FontBoldItalic;

	public GDIFont FontBoldItalicUnderline;

	public GDIFont FontBoldUnderline;

	public GDIFont FontItalic;

	public GDIFont FontItalicUnderline;

	public GDIFont FontNormal;

	public GDIFont FontUnderline;

	public GDIBrush GutterMarginBorderBrush;

	public GDIBrush GutterMarginBrush;

	public GDIBrush HighLightLineBrush;

	public GDIBrush LineNumberMarginBorderBrush;

	public GDIBrush LineNumberMarginBrush;

	public GDIBrush OutlineBrush;

	public GDISurface SelectionBuffer;

	public GDISurface StringBuffer;
}
