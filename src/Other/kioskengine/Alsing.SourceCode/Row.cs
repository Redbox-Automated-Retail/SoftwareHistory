using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public sealed class Row : IEnumerable
{
	private RowState _RowState;

	public SyntaxDocument Document;

	public Span endSpan;

	public SpanList endSpans = new SpanList();

	public int Expansion_EndChar;

	public Span expansion_EndSpan;

	public int Expansion_PixelEnd;

	public int Expansion_PixelStart;

	public int Expansion_StartChar;

	public Span expansion_StartSpan;

	public WordList FormattedWords = new WordList();

	public ImageIndexList Images = new ImageIndexList();

	public int Indent;

	public bool InKeywordQueue;

	public bool InQueue;

	private bool mBookmarked;

	private bool mBreakpoint;

	private string mText = "";

	internal WordList words = new WordList();

	public Span startSpan;

	public SpanList startSpans = new SpanList();

	public object Tag;

	private Color _BackColor = Color.Transparent;

	public Color BackColor
	{
		get
		{
			return _BackColor;
		}
		set
		{
			_BackColor = value;
		}
	}

	public int Depth
	{
		get
		{
			int num = 0;
			for (Span parent = startSpan; parent != null; parent = parent.Parent)
			{
				if (parent.Scope != null && parent.Scope.CauseIndent)
				{
					num++;
				}
			}
			if (ShouldOutdent)
			{
				num--;
			}
			return num;
		}
	}

	public bool ShouldOutdent
	{
		get
		{
			if (startSpan.EndRow == this && startSpan.Scope.CauseIndent)
			{
				return true;
			}
			return false;
		}
	}

	public RowState RowState
	{
		get
		{
			return _RowState;
		}
		set
		{
			if (value != _RowState)
			{
				if (value == RowState.SpanParsed && !InKeywordQueue)
				{
					Document.KeywordQueue.Add(this);
					InKeywordQueue = true;
				}
				if ((value == RowState.AllParsed || value == RowState.NotParsed) && InKeywordQueue)
				{
					Document.KeywordQueue.Remove(this);
					InKeywordQueue = false;
				}
				_RowState = value;
			}
		}
	}

	public bool Bookmarked
	{
		get
		{
			return mBookmarked;
		}
		set
		{
			mBookmarked = value;
			if (value)
			{
				Document.InvokeBookmarkAdded(this);
			}
			else
			{
				Document.InvokeBookmarkRemoved(this);
			}
			Document.InvokeChange();
		}
	}

	public bool Breakpoint
	{
		get
		{
			return mBreakpoint;
		}
		set
		{
			mBreakpoint = value;
			if (value)
			{
				Document.InvokeBreakPointAdded(this);
			}
			else
			{
				Document.InvokeBreakPointRemoved(this);
			}
			Document.InvokeChange();
		}
	}

	public int Count => words.Count;

	public string Text
	{
		get
		{
			return mText;
		}
		set
		{
			bool flag = false;
			if (mText != value)
			{
				flag = true;
				Document.Modified = true;
			}
			mText = value;
			if (Document != null)
			{
				if (flag)
				{
					Document.Parser.ParsePreviewLine(Document.IndexOf(this));
					Document.OnApplyFormatRanges(this);
				}
				AddToParseQueue();
			}
		}
	}

	public Word this[int index]
	{
		get
		{
			if (index >= 0)
			{
				return words[index];
			}
			return new Word();
		}
	}

	public int StartWordIndex
	{
		get
		{
			if (expansion_StartSpan == null)
			{
				return 0;
			}
			Word startWord = expansion_StartSpan.StartWord;
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Word word = (Word)enumerator.Current;
					if (word != startWord)
					{
						num += word.Text.Length;
						continue;
					}
					break;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return num;
		}
	}

	public Word FirstNonWsWord
	{
		get
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Word word = (Word)enumerator.Current;
					if (word.Type == WordType.Word)
					{
						return word;
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return null;
		}
	}

	public int Index => Document.IndexOf(this);

	public int VisibleIndex
	{
		get
		{
			int num = Document.VisibleRows.IndexOf(this);
			if (num == -1)
			{
				if (startSpan != null && startSpan.StartRow != null && startSpan.StartRow != this)
				{
					return startSpan.StartRow.VisibleIndex;
				}
				return Index;
			}
			return Document.VisibleRows.IndexOf(this);
		}
	}

	public Row NextVisibleRow
	{
		get
		{
			int visibleIndex = VisibleIndex;
			if (visibleIndex > Document.VisibleRows.Count)
			{
				return null;
			}
			if (visibleIndex + 1 < Document.VisibleRows.Count)
			{
				return Document.VisibleRows[visibleIndex + 1];
			}
			return null;
		}
	}

	public Row NextRow
	{
		get
		{
			int index = Index;
			if (index + 1 <= Document.Lines.Length - 1)
			{
				return Document[index + 1];
			}
			return null;
		}
	}

	public Row PrevVisibleRow
	{
		get
		{
			int visibleIndex = VisibleIndex;
			if (visibleIndex < 0)
			{
				return null;
			}
			if (visibleIndex - 1 >= 0)
			{
				return Document.VisibleRows[visibleIndex - 1];
			}
			return null;
		}
	}

	public bool IsCollapsed
	{
		get
		{
			if (expansion_StartSpan != null && !expansion_StartSpan.Expanded)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsCollapsedEndPart
	{
		get
		{
			if (expansion_EndSpan != null && !expansion_EndSpan.Expanded)
			{
				return true;
			}
			return false;
		}
	}

	public bool CanFold
	{
		get
		{
			if (expansion_StartSpan != null && expansion_StartSpan.EndRow != null)
			{
				return Document.IndexOf(expansion_StartSpan.EndRow) != 0;
			}
			return false;
		}
	}

	public bool Expanded
	{
		get
		{
			if (CanFold)
			{
				return expansion_StartSpan.Expanded;
			}
			return false;
		}
		set
		{
			if (CanFold)
			{
				expansion_StartSpan.Expanded = value;
			}
		}
	}

	public string ExpansionText
	{
		get
		{
			return expansion_StartSpan.Scope.ExpansionText;
		}
		set
		{
			Scope scope = expansion_StartSpan.Scope;
			Scope scope2 = new Scope();
			scope2.CaseSensitive = scope.CaseSensitive;
			scope2.CauseIndent = scope.CauseIndent;
			scope2.DefaultExpanded = scope.DefaultExpanded;
			scope2.EndPatterns = scope.EndPatterns;
			scope2.NormalizeCase = scope.NormalizeCase;
			scope2.Parent = scope.Parent;
			scope2.spawnSpanOnEnd = scope.spawnSpanOnEnd;
			scope2.spawnSpanOnStart = scope.spawnSpanOnStart;
			scope2.Start = scope.Start;
			scope2.Style = scope.Style;
			scope2.ExpansionText = value;
			Scope scope3 = scope2;
			expansion_StartSpan.Scope = scope3;
			Document.InvokeChange();
		}
	}

	public bool CanFoldEndPart => expansion_EndSpan != null;

	public bool HasExpansionLine => endSpan.Parent != null;

	public Row Expansion_EndRow
	{
		get
		{
			if (CanFold)
			{
				return expansion_StartSpan.EndRow;
			}
			return this;
		}
	}

	public Row Expansion_StartRow
	{
		get
		{
			if (CanFoldEndPart)
			{
				return expansion_EndSpan.StartRow;
			}
			return this;
		}
	}

	public Row VirtualCollapsedRow
	{
		get
		{
			Row row = new Row();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Word word = (Word)enumerator.Current;
					if (expansion_StartSpan != word.Span)
					{
						row.Add(word);
						continue;
					}
					break;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			Word word2 = row.Add(CollapsedText);
			word2.Style = new TextStyle
			{
				BackColor = Color.Silver,
				ForeColor = Color.DarkBlue,
				Bold = true
			};
			bool flag = false;
			if (Expansion_EndRow != null)
			{
				foreach (Word item in Expansion_EndRow)
				{
					if (flag)
					{
						row.Add(item);
					}
					if (item == Expansion_EndRow.expansion_EndSpan.EndWord)
					{
						flag = true;
					}
				}
			}
			return row;
		}
	}

	public string CollapsedText
	{
		get
		{
			string text = "";
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Word word = (Word)enumerator.Current;
					num += word.Text.Length;
					if (word.Span == expansion_StartSpan)
					{
						text = Text.Substring(num).Trim();
						break;
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			if (expansion_StartSpan.Scope.ExpansionText != "")
			{
				text = expansion_StartSpan.Scope.ExpansionText.Replace("***", text);
			}
			return text;
		}
	}

	public Row PrevRow
	{
		get
		{
			int index = Index;
			if (index - 1 >= 0)
			{
				return Document[index - 1];
			}
			return null;
		}
	}

	public IEnumerator GetEnumerator()
	{
		return words.GetEnumerator();
	}

	public void Clear()
	{
		words.Clear();
	}

	public void EnsureVisible()
	{
		if (RowState != RowState.NotParsed)
		{
			for (Span parent = startSpan; parent != null; parent = parent.Parent)
			{
				parent.Expanded = true;
			}
			Document.ResetVisibleRows();
		}
	}

	public Word Add(string text)
	{
		Word word = new Word();
		word.Row = this;
		word.Text = text;
		Word word2 = word;
		words.Add(word2);
		return word2;
	}

	public void AddToParseQueue()
	{
		if (!InQueue)
		{
			Document.ParseQueue.Add(this);
		}
		InQueue = true;
		RowState = RowState.NotParsed;
	}

	public void SetText(string text)
	{
		Document.StartUndoCapture();
		TextPoint textPoint = new TextPoint(0, Index);
		TextRange textRange = new TextRange();
		textRange.FirstColumn = 0;
		textRange.FirstRow = textPoint.Y;
		textRange.LastColumn = Text.Length;
		textRange.LastRow = textPoint.Y;
		TextRange textRange2 = textRange;
		Document.StartUndoCapture();
		Document.PushUndoBlock(UndoAction.DeleteRange, Document.GetRange(textRange2), textRange2.FirstColumn, textRange2.FirstRow);
		Document.PushUndoBlock(UndoAction.InsertRange, text, textPoint.X, textPoint.Y);
		Text = text;
		Document.EndUndoCapture();
		Document.InvokeChange();
	}

	public void MatchCase()
	{
		string text = "";
		foreach (Word word in words)
		{
			text += word.Text;
		}
		mText = text;
	}

	public void Parse()
	{
		Document.ParseRow(this);
	}

	public void Parse(bool ParseKeywords)
	{
		Document.ParseRow(this, ParseKeywords);
	}

	public void SetExpansionSegment()
	{
		expansion_StartSpan = null;
		expansion_EndSpan = null;
		foreach (Span startSpan in startSpans)
		{
			if (!endSpans.Contains(startSpan))
			{
				expansion_StartSpan = startSpan;
				break;
			}
		}
		foreach (Span endSpan in endSpans)
		{
			if (!startSpans.Contains(endSpan))
			{
				expansion_EndSpan = endSpan;
				break;
			}
		}
		if (expansion_EndSpan != null)
		{
			expansion_StartSpan = null;
		}
	}

	public string GetLeadingWhitespace()
	{
		string text = mText;
		text = text.Replace("\t", " ");
		int i;
		for (i = 0; i < text.Length && text.Substring(i, 1) == " "; i++)
		{
		}
		return mText.Substring(0, i);
	}

	public string GetVirtualLeadingWhitespace()
	{
		int num = StartWordIndex;
		string text = "";
		string text2 = Text;
		foreach (char c in text2)
		{
			text = ((c != '\t') ? (text + ' ') : (text + c));
			num--;
			if (num <= 0)
			{
				break;
			}
		}
		return text;
	}

	public void Add(Word word)
	{
		word.Row = this;
		words.Add(word);
	}

	public int IndexOf(Word word)
	{
		return words.IndexOf(word);
	}

	public Word FindRightWordByPatternList(PatternList PatternList, Word StartWord, bool IgnoreStartWord)
	{
		int i = StartWord.Index;
		if (IgnoreStartWord)
		{
			i++;
		}
		for (; i < words.Count; i++)
		{
			Word word = this[i];
			if (word.Pattern != null && word.Pattern.Parent != null && word.Pattern.Parent == PatternList && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
		}
		return null;
	}

	public Word FindRightWordByPatternListName(string PatternListName, Word StartWord, bool IgnoreStartWord)
	{
		int i = StartWord.Index;
		if (IgnoreStartWord)
		{
			i++;
		}
		for (; i < words.Count; i++)
		{
			Word word = this[i];
			if (word.Pattern != null && word.Pattern.Parent != null && word.Pattern.Parent.Name == PatternListName && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
		}
		return null;
	}

	public Word FindLeftWordByPatternList(PatternList PatternList, Word StartWord, bool IgnoreStartWord)
	{
		int num = StartWord.Index;
		if (IgnoreStartWord)
		{
			num--;
		}
		while (num >= 0)
		{
			Word word = this[num];
			if (word.Pattern != null && word.Pattern.Parent != null && word.Pattern.Parent == PatternList && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
			num--;
		}
		return null;
	}

	public Word FindLeftWordByPatternListName(string PatternListName, Word StartWord, bool IgnoreStartWord)
	{
		int num = StartWord.Index;
		if (IgnoreStartWord)
		{
			num--;
		}
		while (num >= 0)
		{
			Word word = this[num];
			if (word.Pattern != null && word.Pattern.Parent != null && word.Pattern.Parent.Name == PatternListName && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
			num--;
		}
		return null;
	}

	public Word FindLeftWordByBlockType(SpanDefinition spanDefinition, Word StartWord, bool IgnoreStartWord)
	{
		int num = StartWord.Index;
		if (IgnoreStartWord)
		{
			num--;
		}
		while (num >= 0)
		{
			Word word = this[num];
			if (word.Span.spanDefinition == spanDefinition && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
			num--;
		}
		return null;
	}

	public Word FindRightWordByBlockType(SpanDefinition spanDefinition, Word StartWord, bool IgnoreStartWord)
	{
		int i = StartWord.Index;
		if (IgnoreStartWord)
		{
			i++;
		}
		for (; i < words.Count; i++)
		{
			Word word = this[i];
			if (word.Span.spanDefinition == spanDefinition && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
		}
		return null;
	}

	public Word FindLeftWordByBlockTypeName(string BlockTypeName, Word StartWord, bool IgnoreStartWord)
	{
		int num = StartWord.Index;
		if (IgnoreStartWord)
		{
			num--;
		}
		while (num >= 0)
		{
			Word word = this[num];
			if (word.Span.spanDefinition.Name == BlockTypeName && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
			num--;
		}
		return null;
	}

	public Word FindRightWordByBlockTypeName(string BlockTypeName, Word StartWord, bool IgnoreStartWord)
	{
		int i = StartWord.Index;
		if (IgnoreStartWord)
		{
			i++;
		}
		for (; i < words.Count; i++)
		{
			Word word = this[i];
			if (word.Span.spanDefinition.Name == BlockTypeName && word.Type != WordType.Space && word.Type != WordType.Tab)
			{
				return word;
			}
		}
		return null;
	}
}
