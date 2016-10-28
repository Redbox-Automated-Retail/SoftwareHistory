using System.Runtime.InteropServices;

namespace Alsing.SourceCode.SyntaxDocumentParsers;

[ComVisible(true)]
public abstract class ParserBase : IParser
{
	public SyntaxDocument Document { get; set; }

	public SyntaxDefinition SyntaxDefinition { get; set; }

	public string Separators { get; set; }

	public void Init(string syntaxDefinitionPath)
	{
		if (!syntaxDefinitionPath.ToLowerInvariant().EndsWith(".syn"))
		{
			syntaxDefinitionPath += ".syn";
		}
		SyntaxDefinitionLoader syntaxDefinitionLoader = new SyntaxDefinitionLoader();
		SyntaxDefinition syntaxDefinition = syntaxDefinitionLoader.Load(syntaxDefinitionPath);
		Init(syntaxDefinition);
	}

	public void Init(string syntaxDefinitionPath, string separators)
	{
		Separators = separators;
		Init(syntaxDefinitionPath);
	}

	public void Init(SyntaxDefinition syntaxDefinition)
	{
		SyntaxDefinition = syntaxDefinition;
	}

	public abstract void ParseRow(int rowIndex, bool parseKeywords);

	public void ParsePreviewLine(int rowIndex)
	{
		Row row = Document[rowIndex];
		row.Clear();
		row.Add(row.Text);
		row.RowState = RowState.NotParsed;
	}
}
