using System;
using System.Windows.Forms;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

public class WordMouseEventArgs : EventArgs
{
	public MouseButtons Button;

	public Cursor Cursor;

	public Pattern Pattern;

	public Word Word;
}
