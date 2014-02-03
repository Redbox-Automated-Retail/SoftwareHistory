namespace Alsing.SourceCode.SyntaxDocumentParsers;

public interface IParser
{
	SyntaxDocument Document { get; set; }

	SyntaxDefinition SyntaxDefinition { get; set; }

	string Separators { get; set; }

	void Init(string syntaxDefinitionPath);

	void Init(string syntaxDefinitionPath, string separators);

	void Init(SyntaxDefinition syntaxDefinition);

	void ParseRow(int RowIndex, bool ParseKeywords);

	void ParsePreviewLine(int RowIndex);
}
