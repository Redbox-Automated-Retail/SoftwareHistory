using System.Runtime.InteropServices;

namespace Alsing.SourceCode.SyntaxDocumentParsers;

[ComVisible(true)]
public class ScanResultWord
{
	public bool HasContent;

	public PatternList ParentList;

	public Pattern Pattern;

	public int Position;

	public string Token = "";
}
