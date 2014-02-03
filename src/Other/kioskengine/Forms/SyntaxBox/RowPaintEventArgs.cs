using System;
using System.Drawing;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

public class RowPaintEventArgs : EventArgs
{
	public Rectangle Bounds;

	public Graphics Graphics;

	public Row Row;
}
