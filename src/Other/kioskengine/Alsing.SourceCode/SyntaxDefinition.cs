using System.Collections.Generic;

namespace Alsing.SourceCode;

public class SyntaxDefinition
{
	private string _Separators = ".,:;{}()[]+-*/\\ \t=&%$#@!|&";

	private long _Version = long.MinValue;

	private readonly Dictionary<SpanDefinition, SpanDefinition> spanDefinitionLookup = new Dictionary<SpanDefinition, SpanDefinition>();

	private readonly Dictionary<TextStyle, TextStyle> styleLookup = new Dictionary<TextStyle, TextStyle>();

	public List<FileType> FileTypes = new List<FileType>();

	public SpanDefinition mainSpanDefinition;

	public string Name = "";

	public string Separators
	{
		get
		{
			return _Separators;
		}
		set
		{
			_Separators = value;
		}
	}

	public long Version
	{
		get
		{
			return _Version;
		}
		set
		{
			_Version = value;
		}
	}

	public SpanDefinition[] SpanDefinitions
	{
		get
		{
			spanDefinitionLookup.Clear();
			FillBlocks(mainSpanDefinition);
			SpanDefinition[] array = new SpanDefinition[spanDefinitionLookup.Values.Count];
			int num = 0;
			foreach (SpanDefinition value in spanDefinitionLookup.Values)
			{
				array[num] = value;
				num++;
			}
			return array;
		}
	}

	public TextStyle[] Styles
	{
		get
		{
			styleLookup.Clear();
			SpanDefinition[] spanDefinitions = SpanDefinitions;
			SpanDefinition[] array = spanDefinitions;
			foreach (SpanDefinition spanDefinition in array)
			{
				styleLookup[spanDefinition.Style] = spanDefinition.Style;
				foreach (Scope scopePattern in spanDefinition.ScopePatterns)
				{
					if (scopePattern.Style != null)
					{
						styleLookup[scopePattern.Style] = scopePattern.Style;
					}
				}
				foreach (PatternList keywords in spanDefinition.KeywordsList)
				{
					if (keywords.Style != null)
					{
						styleLookup[keywords.Style] = keywords.Style;
					}
				}
				foreach (PatternList operators in spanDefinition.OperatorsList)
				{
					if (operators.Style != null)
					{
						styleLookup[operators.Style] = operators.Style;
					}
				}
			}
			TextStyle[] array2 = new TextStyle[styleLookup.Values.Count];
			int num = 0;
			foreach (TextStyle value in styleLookup.Values)
			{
				array2[num] = value;
				num++;
			}
			return array2;
		}
	}

	public void UpdateLists()
	{
		SpanDefinition[] spanDefinitions = SpanDefinitions;
		SpanDefinition[] array = spanDefinitions;
		foreach (SpanDefinition spanDefinition in array)
		{
			spanDefinition.Parent = this;
			spanDefinition.ResetLookupTable();
			spanDefinition.KeywordsList.Parent = spanDefinition;
			foreach (PatternList keywords in spanDefinition.KeywordsList)
			{
				keywords.Parent = spanDefinition.KeywordsList;
				foreach (Pattern item in keywords)
				{
					spanDefinition.AddToLookupTable(item);
				}
			}
			spanDefinition.OperatorsList.Parent = spanDefinition;
			foreach (PatternList operators in spanDefinition.OperatorsList)
			{
				operators.Parent = spanDefinition.OperatorsList;
				foreach (Pattern item2 in operators)
				{
					spanDefinition.AddToLookupTable(item2);
				}
			}
			spanDefinition.BuildLookupTable();
		}
	}

	public void ChangeVersion()
	{
		Version++;
		if (Version > 9223372036854775797L)
		{
			Version = long.MinValue;
		}
	}

	public static SyntaxDefinition FromSyntaxXml(string xml)
	{
		SyntaxDefinitionLoader syntaxDefinitionLoader = new SyntaxDefinitionLoader();
		return syntaxDefinitionLoader.LoadXML(xml);
	}

	public static SyntaxDefinition FromSyntaxFile(string filename)
	{
		SyntaxDefinitionLoader syntaxDefinitionLoader = new SyntaxDefinitionLoader();
		return syntaxDefinitionLoader.Load(filename);
	}

	public void MergeByMainBlock(SyntaxDefinition Target)
	{
		SpanDefinition[] spanDefinitions = SpanDefinitions;
		SpanDefinition[] array = spanDefinitions;
		foreach (SpanDefinition spanDefinition in array)
		{
			spanDefinition.childSpanDefinitions.Insert(0, Target.mainSpanDefinition);
		}
	}

	public void MergeByChildBlocks(SyntaxDefinition Target)
	{
		SpanDefinition[] spanDefinitions = SpanDefinitions;
		SpanDefinition[] array = spanDefinitions;
		foreach (SpanDefinition spanDefinition in array)
		{
			for (int num = Target.mainSpanDefinition.childSpanDefinitions.Count - 1; num >= 0; num--)
			{
				SpanDefinition item = Target.mainSpanDefinition.childSpanDefinitions[num];
				spanDefinition.childSpanDefinitions.Insert(0, item);
			}
		}
	}

	private void FillBlocks(SpanDefinition bt)
	{
		if (bt == null || spanDefinitionLookup.ContainsKey(bt))
		{
			return;
		}
		spanDefinitionLookup.Add(bt, bt);
		foreach (SpanDefinition childSpanDefinition in bt.childSpanDefinitions)
		{
			FillBlocks(childSpanDefinition);
		}
		foreach (Scope scopePattern in bt.ScopePatterns)
		{
			FillBlocks(scopePattern.spawnSpanOnEnd);
			FillBlocks(scopePattern.spawnSpanOnStart);
		}
	}
}
