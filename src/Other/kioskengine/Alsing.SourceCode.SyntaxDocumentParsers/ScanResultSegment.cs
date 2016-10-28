using System.Runtime.InteropServices;

namespace Alsing.SourceCode.SyntaxDocumentParsers;

[ComVisible(true)]
public class ScanResultSegment
{
	public SpanDefinition spanDefinition;

	public bool HasContent;

	public bool IsEndSegment;

	public Pattern Pattern;

	public int Position;

	public Scope Scope;

	public Span span;

	public string Token = "";
}
