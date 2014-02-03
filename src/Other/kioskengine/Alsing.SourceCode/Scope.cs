namespace Alsing.SourceCode;

public sealed class Scope
{
	public bool CaseSensitive;

	public bool CauseIndent;

	public bool DefaultExpanded = true;

	public PatternCollection EndPatterns = new PatternCollection();

	public string ExpansionText = "";

	public bool NormalizeCase = true;

	public SpanDefinition Parent;

	public SpanDefinition spawnSpanOnEnd;

	public SpanDefinition spawnSpanOnStart;

	public Pattern Start;

	public TextStyle Style;
}
