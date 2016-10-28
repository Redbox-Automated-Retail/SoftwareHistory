using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

[ComVisible(true)]
public class RowMouseEventArgs : EventArgs
{
	public RowArea Area;

	public MouseButtons Button;

	public int MouseX;

	public int MouseY;

	public Row Row;
}
