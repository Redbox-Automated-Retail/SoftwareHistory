using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public class SpanDefinition
{
	private readonly List<Pattern> tmpSimplePatterns = new List<Pattern>();

	public Color BackColor = Color.Transparent;

	public SpanDefinitionList childSpanDefinitions = new SpanDefinitionList();

	public PatternCollection ComplexPatterns = new PatternCollection();

	public PatternListList KeywordsList;

	public Hashtable LookupTable = new Hashtable();

	public bool MultiLine;

	public string Name = "";

	public PatternListList OperatorsList;

	public ScopeList ScopePatterns;

	public TextStyle Style;

	public bool TerminateChildren;

	public SyntaxDefinition Parent { get; set; }

	public bool Transparent => BackColor.A == 0;

	public SpanDefinition(SyntaxDefinition parent)
		: this()
	{
		Parent = parent;
		Parent.ChangeVersion();
	}

	public SpanDefinition()
	{
		KeywordsList = new PatternListList(this);
		OperatorsList = new PatternListList(this);
		Style = new TextStyle();
		KeywordsList.Parent = this;
		KeywordsList.IsKeyword = true;
		OperatorsList.Parent = this;
		OperatorsList.IsOperator = true;
		ScopePatterns = new ScopeList(this);
	}

	public void ResetLookupTable()
	{
		LookupTable.Clear();
		tmpSimplePatterns.Clear();
		ComplexPatterns.Clear();
	}

	public void AddToLookupTable(Pattern pattern)
	{
		if (pattern.IsComplex)
		{
			ComplexPatterns.Add(pattern);
		}
		else
		{
			tmpSimplePatterns.Add(pattern);
		}
	}

	public void BuildLookupTable()
	{
		tmpSimplePatterns.Sort(new PatternComparer());
		foreach (Pattern tmpSimplePattern in tmpSimplePatterns)
		{
			if (tmpSimplePattern.StringPattern.Length <= 2)
			{
				char c = tmpSimplePattern.StringPattern[0];
				if (!tmpSimplePattern.Parent.CaseSensitive)
				{
					char c2 = char.ToLowerInvariant(c);
					if (LookupTable[c2] == null)
					{
						LookupTable[c2] = new PatternCollection();
					}
					if (LookupTable[c2] is PatternCollection patternCollection && !patternCollection.Contains(tmpSimplePattern))
					{
						patternCollection.Add(tmpSimplePattern);
					}
					char c3 = char.ToUpper(c);
					if (LookupTable[c3] == null)
					{
						LookupTable[c3] = new PatternCollection();
					}
					if (LookupTable[c3] is PatternCollection patternCollection2 && !patternCollection2.Contains(tmpSimplePattern))
					{
						patternCollection2.Add(tmpSimplePattern);
					}
				}
				else
				{
					if (LookupTable[c] == null)
					{
						LookupTable[c] = new PatternCollection();
					}
					if (LookupTable[c] is PatternCollection patternCollection3 && !patternCollection3.Contains(tmpSimplePattern))
					{
						patternCollection3.Add(tmpSimplePattern);
					}
				}
			}
			else
			{
				string key = tmpSimplePattern.StringPattern.Substring(0, 3).ToLowerInvariant();
				if (LookupTable[key] == null)
				{
					LookupTable[key] = new PatternCollection();
				}
				if (LookupTable[key] is PatternCollection patternCollection4 && !patternCollection4.Contains(tmpSimplePattern))
				{
					patternCollection4.Add(tmpSimplePattern);
				}
			}
		}
	}
}
