using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public class Span
{
	public SpanDefinition spanDefinition;

	public int Depth;

	public Row EndRow;

	public Word EndWord;

	public bool Expanded = true;

	public Span Parent;

	public Scope Scope;

	public Row StartRow;

	public Word StartWord;

	public Span(Row startrow)
	{
		StartRow = startrow;
	}

	public Span()
	{
	}
}
