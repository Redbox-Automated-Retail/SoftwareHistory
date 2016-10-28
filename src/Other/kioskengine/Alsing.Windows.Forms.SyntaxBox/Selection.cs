using System;
using System.Runtime.InteropServices;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

[ComVisible(true)]
public class Selection
{
	private TextRange _Bounds;

	private readonly EditViewControl Control;

	public string Text
	{
		get
		{
			if (!IsValid)
			{
				return "";
			}
			return Control.Document.GetRange(LogicalBounds);
		}
		set
		{
			if (!(Text == value))
			{
				string text = value.Replace(Environment.NewLine, "\n");
				text = text.Replace("\n", Environment.NewLine);
				value = text;
				TextPoint position = Control.Caret.Position;
				int x = position.X;
				int y = position.Y;
				Control.Document.StartUndoCapture();
				DeleteSelection();
				Control.Document.InsertText(value, position.X, position.Y);
				SelLength = value.Length;
				if (x != position.X || y != position.Y)
				{
					Control.Caret.Position = new TextPoint(Bounds.LastColumn, Bounds.LastRow);
				}
				Control.Document.EndUndoCapture();
				Control.Document.InvokeChange();
			}
		}
	}

	public TextRange LogicalBounds
	{
		get
		{
			TextRange textRange = new TextRange();
			if (Bounds.FirstRow < Bounds.LastRow)
			{
				return Bounds;
			}
			if (Bounds.FirstRow == Bounds.LastRow && Bounds.FirstColumn < Bounds.LastColumn)
			{
				return Bounds;
			}
			textRange.FirstColumn = Bounds.LastColumn;
			textRange.FirstRow = Bounds.LastRow;
			textRange.LastColumn = Bounds.FirstColumn;
			textRange.LastRow = Bounds.FirstRow;
			return textRange;
		}
	}

	public bool IsValid
	{
		get
		{
			if (LogicalBounds.FirstColumn == LogicalBounds.LastColumn)
			{
				return LogicalBounds.FirstRow != LogicalBounds.LastRow;
			}
			return true;
		}
	}

	public int SelLength
	{
		get
		{
			TextPoint pos = new TextPoint(Bounds.FirstColumn, Bounds.FirstRow);
			TextPoint pos2 = new TextPoint(Bounds.LastColumn, Bounds.LastRow);
			int num = Control.Document.PointToIntPos(pos);
			int num2 = Control.Document.PointToIntPos(pos2);
			return num2 - num;
		}
		set
		{
			SelEnd = SelStart + value;
		}
	}

	public int SelEnd
	{
		get
		{
			TextPoint pos = new TextPoint(Bounds.LastColumn, Bounds.LastRow);
			return Control.Document.PointToIntPos(pos);
		}
		set
		{
			TextPoint textPoint = Control.Document.IntPosToPoint(value);
			Bounds.LastColumn = textPoint.X;
			Bounds.LastRow = textPoint.Y;
		}
	}

	public int SelStart
	{
		get
		{
			TextPoint pos = new TextPoint(Bounds.FirstColumn, Bounds.FirstRow);
			return Control.Document.PointToIntPos(pos);
		}
		set
		{
			TextPoint textPoint = Control.Document.IntPosToPoint(value);
			Bounds.FirstColumn = textPoint.X;
			Bounds.FirstRow = textPoint.Y;
		}
	}

	public int LogicalSelStart
	{
		get
		{
			TextPoint pos = new TextPoint(LogicalBounds.FirstColumn, LogicalBounds.FirstRow);
			return Control.Document.PointToIntPos(pos);
		}
		set
		{
			TextPoint textPoint = Control.Document.IntPosToPoint(value);
			Bounds.FirstColumn = textPoint.X;
			Bounds.FirstRow = textPoint.Y;
		}
	}

	public TextRange Bounds
	{
		get
		{
			return _Bounds;
		}
		set
		{
			if (_Bounds != null)
			{
				_Bounds.Change -= Bounds_Change;
			}
			_Bounds = value;
			_Bounds.Change += Bounds_Change;
			OnChange();
		}
	}

	public event EventHandler Change;

	private void PositionChange(object s, EventArgs e)
	{
		OnChange();
	}

	private void OnChange()
	{
		if (Change != null)
		{
			Change(this, null);
		}
	}

	public Selection(EditViewControl control)
	{
		Control = control;
		Bounds = new TextRange();
	}

	public void Indent()
	{
		if (IsValid)
		{
			Row row = null;
			UndoBlockCollection undoBlockCollection = new UndoBlockCollection();
			for (int i = LogicalBounds.FirstRow; i <= LogicalBounds.LastRow; i++)
			{
				row = Control.Document[i];
				row.Text = "\t" + row.Text;
				UndoBlock undoBlock = new UndoBlock();
				undoBlock.Action = UndoAction.InsertRange;
				undoBlock.Text = "\t";
				undoBlock.Position.X = 0;
				undoBlock.Position.Y = i;
				undoBlockCollection.Add(undoBlock);
			}
			if (undoBlockCollection.Count > 0)
			{
				Control.Document.AddToUndoList(undoBlockCollection);
			}
			Bounds = LogicalBounds;
			Bounds.FirstColumn = 0;
			Bounds.LastColumn = row.Text.Length;
			Control.Caret.Position.X = LogicalBounds.LastColumn;
			Control.Caret.Position.Y = LogicalBounds.LastRow;
		}
	}

	public void Outdent()
	{
		if (!IsValid)
		{
			return;
		}
		Row row = null;
		UndoBlockCollection undoBlockCollection = new UndoBlockCollection();
		for (int i = LogicalBounds.FirstRow; i <= LogicalBounds.LastRow; i++)
		{
			row = Control.Document[i];
			UndoBlock undoBlock = new UndoBlock();
			undoBlock.Action = UndoAction.DeleteRange;
			undoBlock.Position.X = 0;
			undoBlock.Position.Y = i;
			undoBlockCollection.Add(undoBlock);
			string text = row.Text;
			if (text.StartsWith("\t"))
			{
				undoBlock.Text = text.Substring(0, 1);
				text = text.Substring(1);
			}
			if (text.StartsWith("    "))
			{
				undoBlock.Text = text.Substring(0, 4);
				text = text.Substring(4);
			}
			row.Text = text;
		}
		if (undoBlockCollection.Count > 0)
		{
			Control.Document.AddToUndoList(undoBlockCollection);
		}
		Bounds = LogicalBounds;
		Bounds.FirstColumn = 0;
		Bounds.LastColumn = row.Text.Length;
		Control.Caret.Position.X = LogicalBounds.LastColumn;
		Control.Caret.Position.Y = LogicalBounds.LastRow;
	}

	public void Indent(string Pattern)
	{
		if (IsValid)
		{
			Row row = null;
			UndoBlockCollection undoBlockCollection = new UndoBlockCollection();
			for (int i = LogicalBounds.FirstRow; i <= LogicalBounds.LastRow; i++)
			{
				row = Control.Document[i];
				row.Text = Pattern + row.Text;
				UndoBlock undoBlock = new UndoBlock();
				undoBlock.Action = UndoAction.InsertRange;
				undoBlock.Text = Pattern;
				undoBlock.Position.X = 0;
				undoBlock.Position.Y = i;
				undoBlockCollection.Add(undoBlock);
			}
			if (undoBlockCollection.Count > 0)
			{
				Control.Document.AddToUndoList(undoBlockCollection);
			}
			Bounds = LogicalBounds;
			Bounds.FirstColumn = 0;
			Bounds.LastColumn = row.Text.Length;
			Control.Caret.Position.X = LogicalBounds.LastColumn;
			Control.Caret.Position.Y = LogicalBounds.LastRow;
		}
	}

	public void Outdent(string Pattern)
	{
		if (!IsValid)
		{
			return;
		}
		Row row = null;
		UndoBlockCollection undoBlockCollection = new UndoBlockCollection();
		for (int i = LogicalBounds.FirstRow; i <= LogicalBounds.LastRow; i++)
		{
			row = Control.Document[i];
			UndoBlock undoBlock = new UndoBlock();
			undoBlock.Action = UndoAction.DeleteRange;
			undoBlock.Position.X = 0;
			undoBlock.Position.Y = i;
			undoBlockCollection.Add(undoBlock);
			string text = row.Text;
			if (text.StartsWith(Pattern))
			{
				undoBlock.Text = text.Substring(0, Pattern.Length);
				text = text.Substring(Pattern.Length);
			}
			row.Text = text;
		}
		if (undoBlockCollection.Count > 0)
		{
			Control.Document.AddToUndoList(undoBlockCollection);
		}
		Bounds = LogicalBounds;
		Bounds.FirstColumn = 0;
		Bounds.LastColumn = row.Text.Length;
		Control.Caret.Position.X = LogicalBounds.LastColumn;
		Control.Caret.Position.Y = LogicalBounds.LastRow;
	}

	public void DeleteSelection()
	{
		TextRange logicalBounds = LogicalBounds;
		int firstColumn = logicalBounds.FirstColumn;
		int firstRow = logicalBounds.FirstRow;
		Control.Document.DeleteRange(logicalBounds);
		Control.Caret.Position.X = firstColumn;
		Control.Caret.Position.Y = firstRow;
		ClearSelection();
		Control.ScrollIntoView();
	}

	public void ClearSelection()
	{
		Bounds.FirstColumn = Control.Caret.Position.X;
		Bounds.FirstRow = Control.Caret.Position.Y;
		Bounds.LastColumn = Control.Caret.Position.X;
		Bounds.LastRow = Control.Caret.Position.Y;
	}

	public void MakeSelection()
	{
		Bounds.LastColumn = Control.Caret.Position.X;
		Bounds.LastRow = Control.Caret.Position.Y;
	}

	public void SelectAll()
	{
		Bounds.FirstColumn = 0;
		Bounds.FirstRow = 0;
		Bounds.LastColumn = Control.Document[Control.Document.Count - 1].Text.Length;
		Bounds.LastRow = Control.Document.Count - 1;
		Control.Caret.Position.X = Bounds.LastColumn;
		Control.Caret.Position.Y = Bounds.LastRow;
		Control.ScrollIntoView();
	}

	private void Bounds_Change(object s, EventArgs e)
	{
		OnChange();
	}
}
