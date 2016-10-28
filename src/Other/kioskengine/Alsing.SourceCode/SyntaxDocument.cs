using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Alsing.SourceCode.SyntaxDocumentParsers;

namespace Alsing.SourceCode;

[ComVisible(true)]
public class SyntaxDocument : Component, IEnumerable
{
	private readonly RowList rows = new RowList();

	public RowList KeywordQueue = new RowList();

	public RowList ParseQueue = new RowList();

	private UndoBlockCollection captureBlock;

	private bool captureMode;

	private bool folding = true;

	private bool isParsed = true;

	private bool modified;

	private string mSyntaxFile = "";

	public bool NeedResetRows;

	public IParser Parser = new DefaultParser();

	public object Tag;

	public readonly UndoBuffer UndoBuffer = new UndoBuffer();

	public RowList VisibleRows = new RowList();

	private int _UndoStep;

	private long _Version = long.MinValue;

	public int UndoStep
	{
		get
		{
			if (_UndoStep > UndoBuffer.Count)
			{
				_UndoStep = UndoBuffer.Count;
			}
			return _UndoStep;
		}
		set
		{
			_UndoStep = value;
		}
	}

	public int MaxUndoBufferSize
	{
		get
		{
			return UndoBuffer.MaxSize;
		}
		set
		{
			UndoBuffer.MaxSize = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

	public bool Modified
	{
		get
		{
			return modified;
		}
		set
		{
			modified = value;
			OnModifiedChanged();
		}
	}

	[DefaultValue("")]
	public string SyntaxFile
	{
		get
		{
			return mSyntaxFile;
		}
		set
		{
			mSyntaxFile = value;
			Parser.Init(value);
			Text = Text;
		}
	}

	[DefaultValue(true)]
	public bool Folding
	{
		get
		{
			return folding;
		}
		set
		{
			folding = value;
			if (!value)
			{
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							Row row = (Row)enumerator.Current;
							row.Expanded = true;
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
				}
			}
			ResetVisibleRows();
			OnChange();
		}
	}

	[Browsable(false)]
	public bool IsParsed => isParsed;

	public Row this[int index]
	{
		get
		{
			if (index < 0 || index >= rows.Count)
			{
				return null;
			}
			return rows[index];
		}
		set
		{
			rows[index] = value;
		}
	}

	[Browsable(false)]
	public int Count => rows.Count;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Text
	{
		get
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			ParseAll(ParseKeywords: true);
			foreach (Row row in rows)
			{
				if (num > 0)
				{
					stringBuilder.Append(Environment.NewLine);
				}
				row.MatchCase();
				stringBuilder.Append(row.Text);
				num++;
			}
			return stringBuilder.ToString();
		}
		set
		{
			clear();
			Add("");
			InsertText(value, 0, 0);
			UndoBuffer.Clear();
			UndoStep = 0;
			Modified = false;
			isParsed = false;
			InvokeChange();
		}
	}

	public string[] Lines
	{
		get
		{
			return Text.Split("\n".ToCharArray());
		}
		set
		{
			string text = "";
			foreach (string text2 in value)
			{
				text = text + text2 + "\n";
			}
			Text = text.Substring(0, text.Length - 1);
		}
	}

	public event EventHandler ParsingCompleted;

	public event EventHandler UndoBufferChanged;

	public event EventHandler Parsing;

	public event EventHandler Change;

	public event RowEventHandler BreakPointAdded;

	public event RowEventHandler BreakPointRemoved;

	public event RowEventHandler BookmarkAdded;

	public event RowEventHandler BookmarkRemoved;

	public event EventHandler ModifiedChanged;

	public event ParserEventHandler RowParsed;

	public event ParserEventHandler RowDeleted;

	protected virtual void OnBreakPointAdded(Row r)
	{
		if (BreakPointAdded != null)
		{
			BreakPointAdded(this, new RowEventArgs(r));
		}
	}

	protected virtual void OnBreakPointRemoved(Row r)
	{
		if (BreakPointRemoved != null)
		{
			BreakPointRemoved(this, new RowEventArgs(r));
		}
	}

	protected virtual void OnBookmarkAdded(Row r)
	{
		if (BookmarkAdded != null)
		{
			BookmarkAdded(this, new RowEventArgs(r));
		}
	}

	protected virtual void OnBookmarkRemoved(Row r)
	{
		if (BookmarkRemoved != null)
		{
			BookmarkRemoved(this, new RowEventArgs(r));
		}
	}

	protected virtual void OnUndoBufferChanged()
	{
		if (UndoBufferChanged != null)
		{
			UndoBufferChanged(this, EventArgs.Empty);
		}
	}

	public virtual void InvokeBreakPointAdded(Row r)
	{
		OnBreakPointAdded(r);
	}

	public virtual void InvokeBreakPointRemoved(Row r)
	{
		OnBreakPointRemoved(r);
	}

	public virtual void InvokeBookmarkAdded(Row r)
	{
		OnBookmarkAdded(r);
	}

	public virtual void InvokeBookmarkRemoved(Row r)
	{
		OnBookmarkRemoved(r);
	}

	public SyntaxDocument(IContainer container)
		: this()
	{
		container.Add(this);
		InitializeComponent();
	}

	public SyntaxDocument()
	{
		Parser.Document = this;
		Text = "";
		ResetVisibleRows();
		Init();
	}

	private void InitializeComponent()
	{
		new Container();
	}

	public IEnumerator GetEnumerator()
	{
		return rows.GetEnumerator();
	}

	public void ChangeVersion()
	{
		Version++;
		if (Version > 9223372036854775797L)
		{
			Version = long.MinValue;
		}
	}

	public void StartUndoCapture()
	{
		captureMode = true;
		captureBlock = new UndoBlockCollection();
	}

	public UndoBlockCollection EndUndoCapture()
	{
		captureMode = false;
		AddToUndoList(captureBlock);
		return captureBlock;
	}

	public void ReParse()
	{
		Text = Text;
	}

	public void ClearBookmarks()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Row row = (Row)enumerator.Current;
				row.Bookmarked = false;
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
		InvokeChange();
	}

	public void ClearBreakpoints()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Row row = (Row)enumerator.Current;
				row.Breakpoint = false;
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
		InvokeChange();
	}

	public void EnsureParsed(Row Row)
	{
		ParseAll();
		Parser.ParseRow(Row.Index, ParseKeywords: true);
	}

	private void Init()
	{
		SyntaxDefinition syntaxDefinition = new SyntaxDefinition();
		syntaxDefinition.mainSpanDefinition = new SpanDefinition(syntaxDefinition)
		{
			MultiLine = true
		};
		Parser.Init(syntaxDefinition);
	}

	public void InvokeChange()
	{
		OnChange();
	}

	public void ParseAll()
	{
		while (ParseQueue.Count > 0)
		{
			ParseSome();
		}
		ParseQueue.Clear();
	}

	public void ParseAll(bool ParseKeywords)
	{
		ParseAll();
		if (!ParseKeywords)
		{
			return;
		}
		for (int i = 0; i < Count; i++)
		{
			if (this[i].RowState != RowState.AllParsed)
			{
				Parser.ParseRow(i, ParseKeywords: true);
			}
		}
		ParseQueue.Clear();
		KeywordQueue.Clear();
	}

	public void FoldAll()
	{
		ParseAll(ParseKeywords: false);
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Row row = (Row)enumerator.Current;
				row.Expanded = false;
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
		ResetVisibleRows();
		OnChange();
	}

	public void UnFoldAll()
	{
		ParseAll(ParseKeywords: false);
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Row row = (Row)enumerator.Current;
				row.Expanded = true;
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
		ResetVisibleRows();
		OnChange();
	}

	public void ParseSome()
	{
		ParseSome(1000);
	}

	public void ParseSome(int RowCount)
	{
		if (ParseQueue.Count > 0)
		{
			isParsed = false;
			Row row;
			for (int i = 0; i < RowCount; i += ParseRows(row))
			{
				if (ParseQueue.Count <= 0)
				{
					break;
				}
				row = ParseQueue[0];
			}
			if (NeedResetRows)
			{
				ResetVisibleRows();
			}
			if (Parsing != null)
			{
				Parsing(this, new EventArgs());
			}
		}
		else if (!isParsed && !Modified)
		{
			isParsed = true;
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Row row2 = (Row)enumerator.Current;
						if (row2.expansion_StartSpan != null && row2.Expansion_EndRow != null && !row2.expansion_StartSpan.Scope.DefaultExpanded)
						{
							row2.Expanded = false;
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
			}
			ResetVisibleRows();
			if (ParsingCompleted != null)
			{
				ParsingCompleted(this, new EventArgs());
			}
		}
		if (ParseQueue.Count != 0 || KeywordQueue.Count <= 0)
		{
			return;
		}
		Row row3;
		for (int j = 0; j < RowCount / 20; j += ParseRows(row3, Keywords: true))
		{
			if (KeywordQueue.Count <= 0)
			{
				break;
			}
			row3 = KeywordQueue[0];
		}
	}

	public Row Add(string text)
	{
		return Add(text, StoreUndo: true);
	}

	public Row Add(string text, bool StoreUndo)
	{
		Row row = new Row();
		rows.Add(row);
		row.Document = this;
		row.Text = text;
		return row;
	}

	public Row Insert(string text, int index)
	{
		return Insert(text, index, storeUndo: true);
	}

	public Row Insert(string text, int index, bool storeUndo)
	{
		Row row = new Row();
		row.Document = this;
		Row row2 = row;
		rows.Insert(index, row2);
		row2.Text = text;
		if (storeUndo)
		{
			UndoBlock undoBlock = new UndoBlock();
			undoBlock.Text = text;
			UndoBlock undoBlock2 = undoBlock;
			undoBlock2.Position.Y = IndexOf(row2);
			AddToUndoList(undoBlock2);
		}
		return row2;
	}

	public void Remove(int index)
	{
		Remove(index, StoreUndo: true);
	}

	public void Remove(int index, bool StoreUndo)
	{
		Remove(index, StoreUndo, raiseChanged: true);
	}

	public void Remove(int index, bool storeUndo, bool raiseChanged)
	{
		Row row = this[index];
		if (storeUndo)
		{
			TextRange textRange = new TextRange();
			if (index != Count - 1)
			{
				textRange.FirstColumn = 0;
				textRange.FirstRow = index;
				textRange.LastRow = index + 1;
				textRange.LastColumn = 0;
			}
			else
			{
				textRange.FirstColumn = row.PrevRow.Text.Length;
				textRange.FirstRow = index - 1;
				textRange.LastRow = index;
				textRange.LastColumn = row.Text.Length;
			}
			PushUndoBlock(UndoAction.DeleteRange, GetRange(textRange), textRange.FirstColumn, textRange.FirstRow);
		}
		rows.RemoveAt(index);
		if (row.InKeywordQueue)
		{
			KeywordQueue.Remove(row);
		}
		if (row.InQueue)
		{
			ParseQueue.Remove(row);
		}
		OnRowDeleted(row);
		if (raiseChanged)
		{
			OnChange();
		}
	}

	public void DeleteRange(TextRange Range)
	{
		DeleteRange(Range, StoreUndo: true);
	}

	private int ParseRows(Row row)
	{
		return ParseRows(row, Keywords: false);
	}

	private int ParseRows(Row row, bool Keywords)
	{
		if (!Keywords)
		{
			int num = IndexOf(row);
			int num2 = 0;
			try
			{
				while (row.InQueue && num2 < 100)
				{
					if (num >= 0)
					{
						if (num > 0 && this[num - 1].InQueue)
						{
							ParseRow(this[num - 1]);
						}
						Parser.ParseRow(num, ParseKeywords: false);
					}
					int num3 = ParseQueue.IndexOf(row);
					if (num3 >= 0)
					{
						ParseQueue.RemoveAt(num3);
					}
					row.InQueue = false;
					num++;
					num2++;
					row = this[num];
					if (row == null)
					{
						break;
					}
				}
			}
			catch
			{
			}
			return num2;
		}
		int num4 = IndexOf(row);
		if (num4 == -1 || !row.InKeywordQueue)
		{
			KeywordQueue.Remove(row);
			return 0;
		}
		int num5 = 0;
		try
		{
			while (row.InKeywordQueue && num5 < 100)
			{
				if (num4 >= 0)
				{
					if (num4 > 0 && this[num4 - 1].InQueue)
					{
						ParseRow(this[num4 - 1]);
					}
					Parser.ParseRow(num4, ParseKeywords: true);
				}
				num4++;
				num5++;
				row = this[num4];
				if (row == null)
				{
					break;
				}
			}
		}
		catch
		{
		}
		return num5;
	}

	public void ParseRow(Row r, bool ParseKeywords)
	{
		int num = IndexOf(r);
		if (num >= 0)
		{
			if (num > 0 && this[num - 1].InQueue)
			{
				ParseRow(this[num - 1]);
			}
			Parser.ParseRow(num, ParseKeywords: false);
			if (ParseKeywords)
			{
				Parser.ParseRow(num, ParseKeywords: true);
			}
		}
		int num2 = ParseQueue.IndexOf(r);
		if (num2 >= 0)
		{
			ParseQueue.RemoveAt(num2);
		}
		r.InQueue = false;
	}

	public void ParseRow(Row r)
	{
		ParseRow(r, ParseKeywords: false);
	}

	public int GetNextBookmark(int StartIndex)
	{
		for (int i = StartIndex + 1; i < Count; i++)
		{
			Row row = this[i];
			if (row.Bookmarked)
			{
				return i;
			}
		}
		for (int j = 0; j < StartIndex; j++)
		{
			Row row2 = this[j];
			if (row2.Bookmarked)
			{
				return j;
			}
		}
		return StartIndex;
	}

	public int GetPreviousBookmark(int StartIndex)
	{
		for (int num = StartIndex - 1; num >= 0; num--)
		{
			Row row = this[num];
			if (row.Bookmarked)
			{
				return num;
			}
		}
		for (int num2 = Count - 1; num2 >= StartIndex; num2--)
		{
			Row row2 = this[num2];
			if (row2.Bookmarked)
			{
				return num2;
			}
		}
		return StartIndex;
	}

	public void DeleteRange(TextRange Range, bool StoreUndo)
	{
		Modified = true;
		if (StoreUndo)
		{
			string range = GetRange(Range);
			PushUndoBlock(UndoAction.DeleteRange, range, Range.FirstColumn, Range.FirstRow);
		}
		if (Range.FirstRow == Range.LastRow)
		{
			Row row = this[Range.FirstRow];
			int length = Math.Min(Range.FirstColumn, row.Text.Length);
			string text = row.Text.Substring(0, length);
			string text2 = "";
			if (row.Text.Length >= Range.LastColumn)
			{
				text2 = row.Text.Substring(Range.LastColumn);
			}
			row.Text = text + text2;
		}
		else
		{
			if (Range.LastRow > Count - 1)
			{
				Range.LastRow = Count - 1;
			}
			Row row2 = this[Range.FirstRow];
			if (Range.FirstColumn > row2.Text.Length)
			{
				int count = Range.FirstColumn - row2.Text.Length;
				string text3 = new string(' ', count);
				InsertText(text3, row2.Text.Length, Range.FirstRow, StoreUndo: true);
			}
			string text4 = row2.Text.Substring(0, Range.FirstColumn);
			Row row3 = this[Range.LastRow];
			int startIndex = Math.Min(row3.Text.Length, Range.LastColumn);
			string text5 = row3.Text.Substring(startIndex);
			string text6 = text4 + text5;
			int firstRow = Range.FirstRow;
			int lastRow = Range.LastRow;
			for (int num = lastRow - 1; num >= firstRow; num--)
			{
				Remove(num, storeUndo: false, raiseChanged: false);
			}
			Row row4 = this[firstRow];
			row4.Expanded = true;
			row4.Text = text6;
			row4.startSpans.Clear();
			row4.endSpans.Clear();
			row4.startSpan = null;
			row4.endSpan = null;
			row4.Parse();
		}
		ResetVisibleRows();
		OnChange();
	}

	public string GetRange(TextRange Range)
	{
		if (Range.FirstRow >= Count)
		{
			Range.FirstRow = Count;
		}
		if (Range.LastRow >= Count)
		{
			Range.LastRow = Count;
		}
		if (Range.FirstRow != Range.LastRow)
		{
			Row row = this[Range.FirstRow];
			int startIndex = Math.Min(row.Text.Length, Range.FirstColumn);
			string text = row.Text.Substring(startIndex) + Environment.NewLine;
			Row row2 = this[Range.LastRow];
			if (row2 == null)
			{
				return "";
			}
			int length = Math.Min(row2.Text.Length, Range.LastColumn);
			string text2 = row2.Text.Substring(0, length);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = Range.FirstRow + 1; i <= Range.LastRow - 1; i++)
			{
				Row row3 = this[i];
				stringBuilder.Append(row3.Text + Environment.NewLine);
			}
			string text3 = stringBuilder.ToString();
			return text + text3 + text2;
		}
		Row row4 = this[Range.FirstRow];
		int num = Math.Min(row4.Text.Length, Range.LastColumn);
		int num2 = num - Range.FirstColumn;
		if (num2 <= 0)
		{
			return "";
		}
		return row4.Text.Substring(Range.FirstColumn, num - Range.FirstColumn);
	}

	public int IndexOf(Row xtr)
	{
		return rows.IndexOf(xtr);
	}

	public void clear()
	{
		foreach (Row row in rows)
		{
			OnRowDeleted(row);
		}
		rows.Clear();
		ParseQueue.Clear();
		KeywordQueue.Clear();
		UndoBuffer.Clear();
		UndoStep = 0;
	}

	public void Clear()
	{
		Text = "";
	}

	public TextPoint InsertText(string text, int xPos, int yPos)
	{
		return InsertText(text, xPos, yPos, StoreUndo: true);
	}

	public TextPoint InsertText(string text, int xPos, int yPos, bool StoreUndo)
	{
		Modified = true;
		Row row = this[yPos];
		if (xPos > row.Text.Length)
		{
			int num = xPos - row.Text.Length;
			string text2 = new string(' ', num);
			text = text2 + text;
			xPos -= num;
		}
		string text3 = row.Text.Substring(0, xPos);
		string text4 = row.Text.Substring(xPos);
		string text5 = text3 + text + text4;
		string text6 = text5.Replace(Environment.NewLine, "\n");
		string[] array = text6.Split('\n');
		row.Text = array[0];
		Row row2 = row;
		row.Parse();
		if (!row.InQueue)
		{
			ParseQueue.Add(row);
		}
		row.InQueue = true;
		int num2 = IndexOf(row);
		for (int i = 1; i <= array.GetUpperBound(0); i++)
		{
			row2 = Insert(array[i], i + num2, storeUndo: false);
		}
		if (StoreUndo)
		{
			PushUndoBlock(UndoAction.InsertRange, text, xPos, yPos);
		}
		ResetVisibleRows();
		OnChange();
		return new TextPoint(row2.Text.Length - text4.Length, IndexOf(row2));
	}

	private void OnModifiedChanged()
	{
		if (ModifiedChanged != null)
		{
			ModifiedChanged(this, new EventArgs());
		}
	}

	private void OnChange()
	{
		if (Change != null)
		{
			Change(this, new EventArgs());
		}
	}

	private void OnRowParsed(Row r)
	{
		if (RowParsed != null)
		{
			RowParsed(this, new RowEventArgs(r));
		}
		OnApplyFormatRanges(r);
	}

	private void OnRowDeleted(Row r)
	{
		if (RowDeleted != null)
		{
			RowDeleted(this, new RowEventArgs(r));
		}
	}

	public void PushUndoBlock(UndoAction Action, string text, int x, int y)
	{
		UndoBlock undoBlock = new UndoBlock();
		undoBlock.Action = Action;
		undoBlock.Text = text;
		UndoBlock undoBlock2 = undoBlock;
		undoBlock2.Position.Y = y;
		undoBlock2.Position.X = x;
		if (captureMode)
		{
			captureBlock.Add(undoBlock2);
		}
		else
		{
			AddToUndoList(undoBlock2);
		}
	}

	public TextRange GetRangeFromText(string text, int xPos, int yPos)
	{
		string text2 = text.Replace(Environment.NewLine, "\n");
		string[] array = text2.Split("\n".ToCharArray());
		TextRange textRange = new TextRange();
		textRange.FirstColumn = xPos;
		textRange.FirstRow = yPos;
		textRange.LastRow = array.Length - 1 + yPos;
		textRange.LastColumn = array[array.Length - 1].Length;
		TextRange textRange2 = textRange;
		if (textRange2.FirstRow == textRange2.LastRow)
		{
			textRange2.LastColumn += textRange2.FirstColumn;
		}
		return textRange2;
	}

	public void AddToUndoList(UndoBlock undo)
	{
		UndoBlockCollection undoBlockCollection = new UndoBlockCollection();
		undoBlockCollection.Add(undo);
		UndoBlockCollection actionGroup = undoBlockCollection;
		AddToUndoList(actionGroup);
	}

	public void AddToUndoList(UndoBlockCollection ActionGroup)
	{
		UndoBuffer.ClearFrom(UndoStep);
		UndoBuffer.Add(ActionGroup);
		UndoStep++;
		OnUndoBufferChanged();
	}

	public TextPoint Undo()
	{
		if (UndoStep == 0)
		{
			return new TextPoint(-1, -1);
		}
		UndoBlockCollection undoBlockCollection = UndoBuffer[UndoStep - 1];
		UndoBlock undoBlock = undoBlockCollection[0];
		for (int num = undoBlockCollection.Count - 1; num >= 0; num--)
		{
			undoBlock = undoBlockCollection[num];
			switch (undoBlock.Action)
			{
			case UndoAction.DeleteRange:
				InsertText(undoBlock.Text, undoBlock.Position.X, undoBlock.Position.Y, StoreUndo: false);
				break;
			case UndoAction.InsertRange:
			{
				TextRange rangeFromText = GetRangeFromText(undoBlock.Text, undoBlock.Position.X, undoBlock.Position.Y);
				DeleteRange(rangeFromText, StoreUndo: false);
				break;
			}
			}
		}
		UndoStep--;
		ResetVisibleRows();
		if (UndoStep == 0)
		{
			Modified = false;
		}
		TextPoint result = new TextPoint(undoBlock.Position.X, undoBlock.Position.Y);
		OnUndoBufferChanged();
		return result;
	}

	public void AutoIndentSegment(Span span)
	{
		if (span == null)
		{
			span = this[0].startSpan;
		}
		Row row = span.StartRow;
		Row row2 = span.EndRow;
		if (row == null)
		{
			row = this[0];
		}
		if (row2 == null)
		{
			row2 = this[Count - 1];
		}
		for (int i = row.Index; i <= row2.Index; i++)
		{
			Row row3 = this[i];
			int indent = row3.Indent;
			string text = row3.Text.Substring(row3.GetLeadingWhitespace().Length);
			string text2 = new string('\t', indent);
			row3.Text = text2 + text;
		}
		ResetVisibleRows();
	}

	public Span GetSegmentFromPos(TextPoint p)
	{
		Row row = this[p.Y];
		int num = 0;
		if (row.Count == 0)
		{
			return row.startSpan;
		}
		Span result = row.startSpan;
		foreach (Word item in row)
		{
			if (item.Text.Length + num > p.X)
			{
				if (num == p.X)
				{
					return result;
				}
				return item.Span;
			}
			num += item.Text.Length;
			result = item.Span;
		}
		return row.endSpan;
	}

	public Word GetWordFromPos(TextPoint p)
	{
		Row row = this[p.Y];
		int num = 0;
		Word word = null;
		foreach (Word item in row)
		{
			if (word != null)
			{
				if (item.Text == "")
				{
					return item;
				}
				return word;
			}
			if (item.Text.Length + num > p.X || item == row[row.Count - 1])
			{
				word = item;
			}
			else
			{
				num += item.Text.Length;
			}
		}
		return word;
	}

	public Word GetFormatWordFromPos(TextPoint p)
	{
		Row row = this[p.Y];
		int num = 0;
		Word word = null;
		foreach (Word formattedWord in row.FormattedWords)
		{
			if (word != null)
			{
				if (formattedWord.Text == "")
				{
					return formattedWord;
				}
				return word;
			}
			if (formattedWord.Text.Length + num > p.X || formattedWord == row[row.Count - 1])
			{
				word = formattedWord;
			}
			else
			{
				num += formattedWord.Text.Length;
			}
		}
		return word;
	}

	public void InvokeRowParsed(Row row)
	{
		OnRowParsed(row);
	}

	public void ResetVisibleRows()
	{
		InternalResetVisibleRows();
	}

	private void InternalResetVisibleRows()
	{
		if (!folding)
		{
			VisibleRows = rows;
			NeedResetRows = false;
			return;
		}
		NeedResetRows = false;
		VisibleRows = new RowList();
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			Row row = this[num];
			VisibleRows.Add(row);
			bool flag = false;
			if (row.CanFold && !row.expansion_StartSpan.Expanded && row.expansion_StartSpan.EndWord != null)
			{
				row = row.Expansion_EndRow;
				flag = true;
			}
			num = (flag ? (IndexOf(row) + 1) : (num + 1));
			if (num >= Count)
			{
				break;
			}
		}
	}

	public int PointToIntPos(TextPoint pos)
	{
		int num = 0;
		int num2 = 0;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Row row = (Row)enumerator.Current;
				if (num != pos.Y)
				{
					num2 += row.Text.Length + Environment.NewLine.Length;
					num++;
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
		return num2 + Math.Min(pos.X, this[pos.Y].Text.Length);
	}

	public TextPoint IntPosToPoint(int pos)
	{
		int num = 0;
		int num2 = 0;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Row row = (Row)enumerator.Current;
				num += row.Text.Length + Environment.NewLine.Length;
				if (num > pos)
				{
					num -= row.Text.Length + Environment.NewLine.Length;
					int x = pos - num;
					return new TextPoint(x, num2);
				}
				num2++;
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
		return new TextPoint(-1, -1);
	}

	public void ToggleRow(Row r)
	{
		if (folding && r.Expansion_EndRow != null && r.Expansion_StartRow != null)
		{
			if (r.CanFold)
			{
				r.Expanded = !r.Expanded;
			}
			ResetVisibleRows();
			OnChange();
		}
	}

	public TextPoint Redo()
	{
		if (UndoStep >= UndoBuffer.Count)
		{
			return new TextPoint(-1, -1);
		}
		UndoBlockCollection undoBlockCollection = UndoBuffer[UndoStep];
		UndoBlock undoBlock = undoBlockCollection[0];
		for (int i = 0; i < undoBlockCollection.Count; i++)
		{
			undoBlock = undoBlockCollection[i];
			switch (undoBlock.Action)
			{
			case UndoAction.InsertRange:
				InsertText(undoBlock.Text, undoBlock.Position.X, undoBlock.Position.Y, StoreUndo: false);
				break;
			case UndoAction.DeleteRange:
			{
				TextRange rangeFromText = GetRangeFromText(undoBlock.Text, undoBlock.Position.X, undoBlock.Position.Y);
				DeleteRange(rangeFromText, StoreUndo: false);
				break;
			}
			}
		}
		TextRange rangeFromText2 = GetRangeFromText(undoBlock.Text, undoBlock.Position.X, undoBlock.Position.Y);
		UndoStep++;
		ResetVisibleRows();
		OnUndoBufferChanged();
		return new TextPoint(rangeFromText2.LastColumn, rangeFromText2.LastRow);
	}

	public Word GetStartBracketWord(Word Start, Pattern End, Span FindIn)
	{
		if (Start == null || Start.Pattern == null || Start.Span == null)
		{
			return null;
		}
		int num = Start.Row.Index;
		int index = FindIn.StartRow.Index;
		int num2 = Start.Index;
		int num3 = 0;
		while (num >= index)
		{
			for (int num4 = num2; num4 >= 0; num4--)
			{
				Word word = this[num][num4];
				if (word.Span == FindIn && word.Type == WordType.Word)
				{
					if (word.Pattern == Start.Pattern)
					{
						num3++;
					}
					if (word.Pattern == End)
					{
						num3--;
					}
					if (num3 == 0)
					{
						return word;
					}
				}
			}
			if (!Start.Pattern.IsMultiLineBracket)
			{
				break;
			}
			num--;
			if (num >= 0)
			{
				num2 = this[num].Count - 1;
			}
		}
		return null;
	}

	public Word GetEndBracketWord(Word Start, Pattern End, Span FindIn)
	{
		if (Start == null || Start.Pattern == null || Start.Span == null)
		{
			return null;
		}
		int num = Start.Row.Index;
		int num2 = Count - 1;
		if (FindIn.EndRow != null)
		{
			num2 = FindIn.EndRow.Index;
		}
		int num3 = Start.Index;
		int num4 = 0;
		while (num <= num2)
		{
			for (int i = num3; i < this[num].Count; i++)
			{
				Word word = this[num][i];
				if (word.Span == FindIn && word.Type == WordType.Word)
				{
					if (word.Pattern == Start.Pattern)
					{
						num4++;
					}
					if (word.Pattern == End)
					{
						num4--;
					}
					if (num4 == 0)
					{
						return word;
					}
				}
			}
			if (!Start.Pattern.IsMultiLineBracket)
			{
				break;
			}
			num++;
			num3 = 0;
		}
		return null;
	}

	public void SetSyntaxFromEmbeddedResource(Assembly assembly, string resourceName)
	{
		if ((object)assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		if (string.IsNullOrEmpty(resourceName))
		{
			throw new ArgumentNullException("resourceName");
		}
		Stream manifestResourceStream = assembly.GetManifestResourceStream(resourceName);
		if (manifestResourceStream != null)
		{
			manifestResourceStream.Seek(0L, SeekOrigin.Begin);
			StreamReader streamReader = new StreamReader(manifestResourceStream);
			string xml = streamReader.ReadToEnd();
			manifestResourceStream.Close();
			Parser.Init(SyntaxDefinition.FromSyntaxXml(xml));
			Text = Text;
		}
	}

	public void OnApplyFormatRanges(Row row)
	{
		row.FormattedWords = row.words;
	}
}
