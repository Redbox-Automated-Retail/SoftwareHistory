namespace Alsing.SourceCode.SyntaxDocumentParsers;

public sealed class DefaultParser : IParser
{
	private SyntaxDefinition mSyntaxDefinition;

	private long Version;

	public SyntaxDefinition SyntaxDefinition
	{
		get
		{
			return mSyntaxDefinition;
		}
		set
		{
			mSyntaxDefinition = value;
			if (mSyntaxDefinition == null)
			{
				SyntaxDefinition syntaxDefinition = new SyntaxDefinition();
				syntaxDefinition.mainSpanDefinition = new SpanDefinition(syntaxDefinition)
				{
					MultiLine = true
				};
				mSyntaxDefinition = syntaxDefinition;
			}
			Version = long.MinValue;
			mSyntaxDefinition.Version = -9223372036854775807L;
			Document.ReParse();
		}
	}

	public string Separators
	{
		get
		{
			return SyntaxDefinition.Separators;
		}
		set
		{
			SyntaxDefinition.Separators = value;
		}
	}

	public SyntaxDocument Document { get; set; }

	public DefaultParser()
	{
		mSyntaxDefinition = null;
	}

	public void ParseRow(int index, bool ParseKeywords)
	{
		InternalParseLine(index, ParseKeywords);
		if (SyntaxDefinition != null && Version != SyntaxDefinition.Version)
		{
			SyntaxDefinition.UpdateLists();
			Version = SyntaxDefinition.Version;
		}
		Document.InvokeRowParsed(Document[index]);
	}

	public void Init(string SyntaxFile)
	{
		try
		{
			if (!SyntaxFile.ToLowerInvariant().EndsWith(".syn"))
			{
				SyntaxFile += ".syn";
			}
			SyntaxDefinition = new SyntaxDefinitionLoader().Load(SyntaxFile);
		}
		catch
		{
		}
	}

	public void Init(string syntaxFile, string separators)
	{
		try
		{
			if (!syntaxFile.ToLowerInvariant().EndsWith(".syn"))
			{
				syntaxFile += ".syn";
			}
			SyntaxDefinition = new SyntaxDefinitionLoader().Load(syntaxFile, separators);
		}
		catch
		{
		}
	}

	public void Init(SyntaxDefinition syntaxDefinition)
	{
		SyntaxDefinition = syntaxDefinition;
	}

	public void ParsePreviewLine(int RowIndex)
	{
		Row row = Document[RowIndex];
		row.Clear();
		row.Add(row.Text);
		row.RowState = RowState.NotParsed;
	}

	private void MakeSame(int RowIndex)
	{
		Row row = Document[RowIndex];
		Span span = row.endSpan;
		Span span2 = Document[RowIndex + 1].startSpan;
		while (span != null)
		{
			foreach (Word item in row)
			{
				if (item.Span == span)
				{
					if (item.Span.StartWord == item)
					{
						span2.StartWord = item;
					}
					if (item.Span.EndWord == item)
					{
						span2.EndWord = item;
					}
					item.Span = span2;
				}
			}
			if (span == row.startSpan)
			{
				row.startSpan = span2;
			}
			if (span == row.endSpan)
			{
				row.endSpan = span2;
			}
			if (row.startSpans.IndexOf(span) >= 0)
			{
				row.startSpans[row.startSpans.IndexOf(span)] = span2;
			}
			if (row.endSpans.IndexOf(span) >= 0)
			{
				row.endSpans[row.endSpans.IndexOf(span)] = span2;
			}
			span = span.Parent;
			span2 = span2.Parent;
		}
		row.SetExpansionSegment();
	}

	private bool IsSameButDifferent(int RowIndex, Span oldStartSpan)
	{
		if (RowIndex >= Document.Count - 1)
		{
			return false;
		}
		Row row = Document[RowIndex];
		Span span = row.endSpan;
		Span startSpan = Document[RowIndex + 1].startSpan;
		Span span2 = startSpan;
		bool flag = false;
		while (span != null)
		{
			if (span2 == null)
			{
				flag = true;
				break;
			}
			if (span.spanDefinition != span2.spanDefinition)
			{
				flag = true;
				break;
			}
			if (span.Parent != span2.Parent)
			{
				flag = true;
				break;
			}
			span = span.Parent;
			span2 = span2.Parent;
		}
		if (flag || row.startSpan != oldStartSpan)
		{
			return false;
		}
		return true;
	}

	private ScanResultWord GetNextWord(string Text, Span currentSpan, int StartPos, ref bool HasComplex)
	{
		SpanDefinition spanDefinition = currentSpan.spanDefinition;
		int num = -1;
		Pattern pattern = null;
		string token = "";
		ScanResultWord scanResultWord = new ScanResultWord();
		if (HasComplex)
		{
			foreach (Pattern complexPattern in spanDefinition.ComplexPatterns)
			{
				PatternScanResult patternScanResult = complexPattern.IndexIn(Text, StartPos, complexPattern.Parent.CaseSensitive, Separators);
				if (patternScanResult.Token != "" && (patternScanResult.Index < num || num == -1))
				{
					num = patternScanResult.Index;
					pattern = complexPattern;
					token = patternScanResult.Token;
				}
			}
			if (pattern != null)
			{
				scanResultWord.HasContent = true;
				scanResultWord.ParentList = pattern.Parent;
				scanResultWord.Pattern = pattern;
				scanResultWord.Position = num;
				scanResultWord.Token = token;
			}
			else
			{
				HasComplex = false;
			}
		}
		ScanResultWord scanResultWord2 = new ScanResultWord();
		for (int i = StartPos; i < Text.Length && (i <= scanResultWord.Position || !scanResultWord.HasContent); i++)
		{
			if (i <= Text.Length - 3)
			{
				string key = Text.Substring(i, 3).ToLowerInvariant();
				PatternCollection patternCollection = (PatternCollection)spanDefinition.LookupTable[key];
				if (patternCollection != null)
				{
					foreach (Pattern item in patternCollection)
					{
						int length = item.StringPattern.Length;
						if (i + length > Text.Length)
						{
							continue;
						}
						char c = char.ToLower(item.StringPattern[length - 1]);
						char c2 = char.ToLower(Text[i + length - 1]);
						if (c != c2 || (item.IsKeyword && (!item.IsKeyword || !item.HasSeparators(Text, i))))
						{
							continue;
						}
						if (!item.Parent.CaseSensitive)
						{
							string text = Text.Substring(i, length).ToLowerInvariant();
							if (text == item.StringPattern.ToLowerInvariant())
							{
								scanResultWord2.HasContent = true;
								scanResultWord2.ParentList = item.Parent;
								scanResultWord2.Pattern = item;
								scanResultWord2.Position = i;
								scanResultWord2.Token = (item.Parent.NormalizeCase ? item.StringPattern : Text.Substring(i, length));
								break;
							}
						}
						else
						{
							string text2 = Text.Substring(i, length);
							if (text2 == item.StringPattern)
							{
								scanResultWord2.HasContent = true;
								scanResultWord2.ParentList = item.Parent;
								scanResultWord2.Pattern = item;
								scanResultWord2.Position = i;
								scanResultWord2.Token = item.StringPattern;
								break;
							}
						}
					}
				}
			}
			if (scanResultWord2.HasContent)
			{
				break;
			}
			char c3 = Text[i];
			PatternCollection patternCollection2 = (PatternCollection)spanDefinition.LookupTable[c3];
			if (patternCollection2 == null)
			{
				continue;
			}
			foreach (Pattern item2 in patternCollection2)
			{
				int length2 = item2.StringPattern.Length;
				if (i + length2 > Text.Length)
				{
					continue;
				}
				char c4 = item2.StringPattern[length2 - 1];
				char c5 = Text[i + length2 - 1];
				if (!item2.Parent.CaseSensitive)
				{
					if (char.ToLower(c4) == char.ToLower(c5) && (!item2.IsKeyword || (item2.IsKeyword && item2.HasSeparators(Text, i))))
					{
						string text3 = Text.Substring(i, length2).ToLowerInvariant();
						if (text3 == item2.StringPattern.ToLowerInvariant())
						{
							scanResultWord2.HasContent = true;
							scanResultWord2.ParentList = item2.Parent;
							scanResultWord2.Pattern = item2;
							scanResultWord2.Position = i;
							scanResultWord2.Token = (item2.Parent.NormalizeCase ? item2.StringPattern : Text.Substring(i, length2));
							break;
						}
					}
				}
				else if (c4 == c5 && (!item2.IsKeyword || (item2.IsKeyword && item2.HasSeparators(Text, i))))
				{
					string text4 = Text.Substring(i, length2);
					if (text4 == item2.StringPattern)
					{
						scanResultWord2.HasContent = true;
						scanResultWord2.ParentList = item2.Parent;
						scanResultWord2.Pattern = item2;
						scanResultWord2.Position = i;
						scanResultWord2.Token = item2.StringPattern;
						break;
					}
				}
			}
			if (scanResultWord2.HasContent)
			{
				break;
			}
		}
		if (scanResultWord.HasContent && scanResultWord2.HasContent)
		{
			if (scanResultWord2.Position == scanResultWord.Position)
			{
				if (scanResultWord2.Token.Length >= scanResultWord.Token.Length)
				{
					return scanResultWord2;
				}
				return scanResultWord;
			}
			if (scanResultWord2.Position < scanResultWord.Position)
			{
				return scanResultWord2;
			}
			if (scanResultWord2.Position > scanResultWord.Position)
			{
				return scanResultWord;
			}
		}
		if (scanResultWord2.HasContent)
		{
			return scanResultWord2;
		}
		if (scanResultWord.HasContent)
		{
			return scanResultWord;
		}
		return new ScanResultWord();
	}

	private void ParseText(Row Row, Span currentSpan, string Text)
	{
		int num = 0;
		bool HasComplex = true;
		while (true)
		{
			ScanResultWord nextWord = GetNextWord(Text, currentSpan, num, ref HasComplex);
			if (!nextWord.HasContent)
			{
				break;
			}
			ParseTools.AddString(Text.Substring(num, nextWord.Position - num), Row, currentSpan.spanDefinition.Style, currentSpan);
			ParseTools.AddPatternString(nextWord.Token, Row, nextWord.Pattern, nextWord.ParentList.Style, currentSpan, HasError: false);
			num = nextWord.Position + nextWord.Token.Length;
		}
		ParseTools.AddString(Text.Substring(num), Row, currentSpan.spanDefinition.Style, currentSpan);
	}

	private void InternalParseLine(int index, bool ParseKeywords)
	{
		if (mSyntaxDefinition == null)
		{
			return;
		}
		SyntaxDocument document = Document;
		Row row = document[index];
		Span endSpan = row.endSpan;
		Span startSpan = row.startSpan;
		bool expanded = !row.IsCollapsed;
		if (row.IsCollapsedEndPart)
		{
			row.expansion_EndSpan.EndWord = null;
		}
		if (index > 0)
		{
			row.startSpan = Document[index - 1].endSpan;
		}
		else if (row.startSpan == null)
		{
			row.startSpan = new Span(row)
			{
				spanDefinition = mSyntaxDefinition.mainSpanDefinition
			};
		}
		int num = 0;
		Span span = row.startSpan;
		row.endSpans.Clear();
		row.startSpans.Clear();
		row.Clear();
		while (true)
		{
			ScanResultSegment nextChildSegment = GetNextChildSegment(row, span, num);
			ScanResultSegment endSegment = GetEndSegment(row, span, num);
			if ((endSegment.HasContent && nextChildSegment.HasContent && endSegment.Position <= nextChildSegment.Position) || (endSegment.HasContent && !nextChildSegment.HasContent))
			{
				if (ParseKeywords)
				{
					string text = row.Text.Substring(num, endSegment.Position - num);
					ParseText(row, span, text);
				}
				Span span2 = span;
				while (span != endSegment.span)
				{
					row.endSpans.Add(span);
					span = span.Parent;
				}
				row.endSpans.Add(span);
				TextStyle style = span.Scope.Style;
				ParseTools.AddPatternString(endSegment.Token, row, endSegment.Pattern, style, span, HasError: false);
				while (span2 != endSegment.span)
				{
					span2.EndRow = row;
					span2.EndWord = row[row.Count - 1];
					span2 = span2.Parent;
				}
				span.EndRow = row;
				span.EndWord = row[row.Count - 1];
				if (span.Parent != null)
				{
					span = span.Parent;
				}
				num = endSegment.Position + endSegment.Token.Length;
				continue;
			}
			if (!nextChildSegment.HasContent)
			{
				break;
			}
			if (ParseKeywords)
			{
				string text2 = row.Text.Substring(num, nextChildSegment.Position - num);
				ParseText(row, span, text2);
			}
			Span span3 = new Span();
			span3.Parent = span;
			span3.spanDefinition = nextChildSegment.spanDefinition;
			span3.Scope = nextChildSegment.Scope;
			Span span4 = span3;
			row.startSpans.Add(span4);
			TextStyle style2 = span4.Scope.Style;
			ParseTools.AddPatternString(nextChildSegment.Token, row, nextChildSegment.Pattern, style2, span4, HasError: false);
			span4.StartRow = row;
			span4.StartWord = row[row.Count - 1];
			span = span4;
			num = nextChildSegment.Position + nextChildSegment.Token.Length;
			if (nextChildSegment.Scope.spawnSpanOnStart != null)
			{
				Span span5 = new Span();
				span5.Parent = span4;
				span5.spanDefinition = nextChildSegment.Scope.spawnSpanOnStart;
				span5.Scope = new Scope();
				span5.StartWord = span4.StartWord;
				Span span6 = span5;
				row.startSpans.Add(span6);
				span = span6;
			}
		}
		if (num < row.Text.Length && ParseKeywords)
		{
			string text3 = row.Text.Substring(num);
			ParseText(row, span, text3);
		}
		while (!span.spanDefinition.MultiLine)
		{
			row.endSpans.Add(span);
			span = span.Parent;
		}
		row.endSpan = span;
		row.SetExpansionSegment();
		row.RowState = ((!ParseKeywords) ? RowState.SpanParsed : RowState.AllParsed);
		if (IsSameButDifferent(index, startSpan))
		{
			MakeSame(index);
		}
		if (row.CanFold)
		{
			row.expansion_StartSpan.Expanded = expanded;
		}
		if (!ParseKeywords)
		{
			if (endSpan != null)
			{
				if (row.endSpan != endSpan && index <= Document.Count - 2)
				{
					Document[index + 1].AddToParseQueue();
					Document.NeedResetRows = true;
				}
			}
			else if (index <= Document.Count - 2)
			{
				Document[index + 1].AddToParseQueue();
				Document.NeedResetRows = true;
			}
		}
		if (endSpan != null && endSpan.EndWord == null)
		{
			endSpan.Expanded = true;
		}
	}

	private ScanResultSegment GetEndSegment(Row Row, Span currentSpan, int StartPos)
	{
		if (StartPos >= Row.Text.Length || currentSpan.Scope == null)
		{
			return new ScanResultSegment();
		}
		ScanResultSegment scanResultSegment = new ScanResultSegment();
		scanResultSegment.HasContent = false;
		scanResultSegment.IsEndSegment = false;
		ScanResultSegment scanResultSegment2 = scanResultSegment;
		for (Span span = currentSpan; span != null; span = span.Parent)
		{
			if (span == currentSpan || span.spanDefinition.TerminateChildren)
			{
				foreach (Pattern endPattern in span.Scope.EndPatterns)
				{
					PatternScanResult patternScanResult = endPattern.IndexIn(Row.Text, StartPos, span.Scope.CaseSensitive, Separators);
					int index = patternScanResult.Index;
					if (patternScanResult.Token != "" && ((patternScanResult.Index < scanResultSegment2.Position && scanResultSegment2.HasContent) || !scanResultSegment2.HasContent))
					{
						scanResultSegment2.Pattern = endPattern;
						scanResultSegment2.Position = index;
						scanResultSegment2.Token = patternScanResult.Token;
						scanResultSegment2.HasContent = true;
						scanResultSegment2.span = span;
						scanResultSegment2.Scope = null;
						if (!endPattern.IsComplex && span.Scope.NormalizeCase && !span.Scope.Start.IsComplex)
						{
							scanResultSegment2.Token = endPattern.StringPattern;
						}
					}
				}
			}
		}
		if (!scanResultSegment2.HasContent)
		{
			return new ScanResultSegment();
		}
		return scanResultSegment2;
	}

	private ScanResultSegment GetNextChildSegment(Row Row, Span currentSpan, int StartPos)
	{
		if (StartPos >= Row.Text.Length)
		{
			return new ScanResultSegment();
		}
		ScanResultSegment scanResultSegment = new ScanResultSegment();
		scanResultSegment.HasContent = false;
		scanResultSegment.IsEndSegment = false;
		ScanResultSegment scanResultSegment2 = scanResultSegment;
		foreach (SpanDefinition childSpanDefinition in currentSpan.spanDefinition.childSpanDefinitions)
		{
			foreach (Scope scopePattern in childSpanDefinition.ScopePatterns)
			{
				PatternScanResult patternScanResult = scopePattern.Start.IndexIn(Row.Text, StartPos, scopePattern.CaseSensitive, Separators);
				int index = patternScanResult.Index;
				if ((!scanResultSegment2.HasContent || index < scanResultSegment2.Position) && patternScanResult.Token != "")
				{
					scanResultSegment2.Pattern = scopePattern.Start;
					scanResultSegment2.Position = index;
					scanResultSegment2.Token = patternScanResult.Token;
					scanResultSegment2.HasContent = true;
					scanResultSegment2.spanDefinition = childSpanDefinition;
					scanResultSegment2.Scope = scopePattern;
					if (scopePattern.NormalizeCase && !scopePattern.Start.IsComplex)
					{
						scanResultSegment2.Token = scopePattern.Start.StringPattern;
					}
				}
			}
		}
		if (!scanResultSegment2.HasContent)
		{
			return new ScanResultSegment();
		}
		return scanResultSegment2;
	}
}
