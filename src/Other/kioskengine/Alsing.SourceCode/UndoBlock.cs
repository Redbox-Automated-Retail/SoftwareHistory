namespace Alsing.SourceCode;

public sealed class UndoBlock
{
	public UndoAction Action;

	public TextPoint Position = new TextPoint(0, 0);

	public string Text = "";
}
