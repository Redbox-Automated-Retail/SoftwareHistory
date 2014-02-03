namespace Alsing.SourceCode.SyntaxDocumentParsers;

public sealed class NewParser : ParserBase
{
	public override void ParseRow(int rowIndex, bool parseKeywords)
	{
		if (!parseKeywords)
		{
			ParseLineStructure(rowIndex);
		}
		else
		{
			ParseLineFully(rowIndex);
		}
	}

	private void ParseLineFully(int rowIndex)
	{
	}

	private void ParseLineStructure(int rowIndex)
	{
	}
}
