using System;
using System.Collections;

namespace Alsing.SourceCode;

public sealed class PatternList : IEnumerable
{
	private readonly PatternCollection patterns = new PatternCollection();

	public bool CaseSensitive;

	public PatternCollection ComplexPatterns = new PatternCollection();

	public string Name = "";

	public bool NormalizeCase;

	public PatternListList Parent;

	public SpanDefinition parentSpanDefinition;

	public Hashtable SimplePatterns = new Hashtable();

	public Hashtable SimplePatterns1Char = new Hashtable();

	public Hashtable SimplePatterns2Char = new Hashtable();

	public TextStyle Style = new TextStyle();

	public PatternList()
	{
		SimplePatterns = new Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
	}

	public IEnumerator GetEnumerator()
	{
		return patterns.GetEnumerator();
	}

	public Pattern Add(Pattern Pattern)
	{
		if (Parent != null && Parent.Parent != null && Parent.Parent.Parent != null)
		{
			Pattern.Separators = Parent.Parent.Parent.Separators;
			Parent.Parent.Parent.ChangeVersion();
		}
		if (!Pattern.IsComplex && !Pattern.ContainsSeparator)
		{
			string text = ((Pattern.StringPattern.Length < 2) ? (Pattern.StringPattern.Substring(0, 1) + " ") : Pattern.StringPattern.Substring(0, 2));
			text = text.ToLowerInvariant();
			if (Pattern.StringPattern.Length == 1)
			{
				SimplePatterns1Char[Pattern.StringPattern] = Pattern;
			}
			else
			{
				if (SimplePatterns2Char[text] == null)
				{
					SimplePatterns2Char[text] = new PatternCollection();
				}
				PatternCollection patternCollection = (PatternCollection)SimplePatterns2Char[text];
				patternCollection.Add(Pattern);
			}
			if (CaseSensitive)
			{
				SimplePatterns[Pattern.LowerStringPattern] = Pattern;
			}
			else
			{
				SimplePatterns[Pattern.StringPattern] = Pattern;
			}
		}
		else
		{
			ComplexPatterns.Add(Pattern);
		}
		patterns.Add(Pattern);
		if (Pattern.Parent == null)
		{
			Pattern.Parent = this;
			return Pattern;
		}
		throw new Exception("Pattern already assigned to another PatternList");
	}

	public void Clear()
	{
		patterns.Clear();
	}
}
