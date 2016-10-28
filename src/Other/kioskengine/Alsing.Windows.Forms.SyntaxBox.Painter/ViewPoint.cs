using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms.SyntaxBox.Painter;

[ComVisible(true)]
public struct ViewPoint
{
	public EditAction Action;

	public int CharWidth;

	public int ClientAreaStart;

	public int ClientAreaWidth;

	public int FirstVisibleColumn;

	public int FirstVisibleRow;

	public int GutterMarginWidth;

	public int LineNumberMarginWidth;

	public int RowHeight;

	public int TextMargin;

	public int TotalMarginWidth;

	public int VisibleRowCount;

	public int YOffset;
}
