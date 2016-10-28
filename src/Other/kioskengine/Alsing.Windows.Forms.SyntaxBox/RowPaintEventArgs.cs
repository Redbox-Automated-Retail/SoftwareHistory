using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

[ComVisible(true)]
public class RowPaintEventArgs : EventArgs
{
	public Rectangle Bounds;

	public Graphics Graphics;

	public Row Row;
}
