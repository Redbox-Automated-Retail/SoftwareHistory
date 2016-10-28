using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public sealed class UndoBlock
{
	public UndoAction Action;

	public TextPoint Position = new TextPoint(0, 0);

	public string Text = "";
}
