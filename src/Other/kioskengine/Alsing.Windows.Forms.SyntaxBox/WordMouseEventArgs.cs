using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

[ComVisible(true)]
public class WordMouseEventArgs : EventArgs
{
	public MouseButtons Button;

	public Cursor Cursor;

	public Pattern Pattern;

	public Word Word;
}
