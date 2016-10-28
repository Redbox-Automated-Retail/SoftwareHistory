using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox;

[ComVisible(true)]
public sealed class Caret
{
	private readonly EditViewControl Control;

	private TextPoint _Position;

	public bool Blink;

	private int OldLogicalXPos;

	public TextPoint Position
	{
		get
		{
			return _Position;
		}
		set
		{
			_Position = value;
			_Position.Change += PositionChange;
			OnChange();
		}
	}

	public Word CurrentWord => Control.Document.GetWordFromPos(Position);

	public Row CurrentRow => Control.Document[Position.Y];

	public TextPoint LogicalPosition
	{
		get
		{
			if (Position.X < 0)
			{
				return new TextPoint(0, Position.Y);
			}
			Row currentRow = CurrentRow;
			int num = 0;
			if (currentRow == null)
			{
				return new TextPoint(0, 0);
			}
			int count = Math.Max(Position.X - currentRow.Text.Length, 0);
			string text = new string(' ', count);
			string text2 = currentRow.Text + text;
			char[] array = text2.ToCharArray(0, Position.X);
			char[] array2 = array;
			foreach (char c in array2)
			{
				num = ((c != '\t') ? (num + 1) : (num + (Control.TabSize - num % Control.TabSize)));
			}
			return new TextPoint(num, Position.Y);
		}
		set
		{
			Row currentRow = CurrentRow;
			int num = 0;
			int num2 = 0;
			if (value.X > 0)
			{
				char[] array = currentRow.Text.ToCharArray();
				int num3 = 0;
				while (num < value.X)
				{
					char c = ((num3 < array.Length) ? array[num3] : ' ');
					num2++;
					num = ((c != '\t') ? (num + 1) : (num + (Control.TabSize - num % Control.TabSize)));
					num3++;
				}
			}
			Position.Y = value.Y;
			Position.X = num2;
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

	public Caret(EditViewControl control)
	{
		Position = new TextPoint(0, 0);
		Control = control;
	}

	private void RememberXPos()
	{
		OldLogicalXPos = LogicalPosition.X;
	}

	public void CropPosition()
	{
		if (Position.X < 0)
		{
			Position.X = 0;
		}
		if (Position.Y >= Control.Document.Count)
		{
			Position.Y = Control.Document.Count - 1;
		}
		if (Position.Y < 0)
		{
			Position.Y = 0;
		}
		Row currentRow = CurrentRow;
		if (Position.X > currentRow.Text.Length && !Control.VirtualWhitespace)
		{
			Position.X = currentRow.Text.Length;
		}
	}

	public void MoveRight(bool Select)
	{
		CropPosition();
		Position.X++;
		if (CurrentRow.IsCollapsed)
		{
			if (Position.X > CurrentRow.Expansion_EndChar)
			{
				Position.Y = CurrentRow.Expansion_EndRow.Index;
				Position.X = CurrentRow.Expansion_EndRow.Expansion_StartChar;
				CropPosition();
			}
			RememberXPos();
			CaretMoved(Select);
			return;
		}
		Row currentRow = CurrentRow;
		if (Position.X > currentRow.Text.Length && !Control.VirtualWhitespace)
		{
			if (Position.Y < Control.Document.Count - 1)
			{
				MoveDown(Select);
				Position.X = 0;
				CropPosition();
			}
			else
			{
				CropPosition();
			}
		}
		RememberXPos();
		CaretMoved(Select);
	}

	public void MoveUp(bool Select)
	{
		CropPosition();
		int x = OldLogicalXPos;
		try
		{
			if (CurrentRow != null && CurrentRow.PrevVisibleRow != null)
			{
				Position.Y = CurrentRow.PrevVisibleRow.Index;
				if (CurrentRow.IsCollapsed)
				{
					x = 0;
				}
			}
		}
		catch
		{
		}
		finally
		{
			CropPosition();
			LogicalPosition = new TextPoint(x, Position.Y);
			CropPosition();
			CaretMoved(Select);
		}
	}

	public void MoveUp(int rows, bool Select)
	{
		CropPosition();
		int x = OldLogicalXPos;
		try
		{
			int visibleIndex = CurrentRow.VisibleIndex;
			visibleIndex -= rows;
			if (visibleIndex < 0)
			{
				visibleIndex = 0;
			}
			Row row = Control.Document.VisibleRows[visibleIndex];
			visibleIndex = row.Index;
			Position.Y = visibleIndex;
			if (CurrentRow.IsCollapsed)
			{
				x = 0;
			}
		}
		catch
		{
		}
		CropPosition();
		LogicalPosition = new TextPoint(x, Position.Y);
		CropPosition();
		CaretMoved(Select);
	}

	public void MoveDown(int rows, bool Select)
	{
		int x = OldLogicalXPos;
		CropPosition();
		try
		{
			int visibleIndex = CurrentRow.VisibleIndex;
			visibleIndex += rows;
			if (visibleIndex > Control.Document.VisibleRows.Count - 1)
			{
				visibleIndex = Control.Document.VisibleRows.Count - 1;
			}
			Row row = Control.Document.VisibleRows[visibleIndex];
			visibleIndex = row.Index;
			Position.Y = visibleIndex;
			if (CurrentRow.IsCollapsed)
			{
				x = 0;
			}
		}
		catch
		{
		}
		finally
		{
			CropPosition();
			LogicalPosition = new TextPoint(x, Position.Y);
			CropPosition();
			CaretMoved(Select);
		}
	}

	public void MoveDown(bool Select)
	{
		CropPosition();
		int x = OldLogicalXPos;
		try
		{
			Row currentRow = CurrentRow;
			Row nextVisibleRow = currentRow.NextVisibleRow;
			if (nextVisibleRow != null)
			{
				Position.Y = nextVisibleRow.Index;
				if (CurrentRow.IsCollapsed)
				{
					x = 0;
				}
			}
		}
		catch
		{
		}
		finally
		{
			CropPosition();
			LogicalPosition = new TextPoint(x, Position.Y);
			CropPosition();
			CaretMoved(Select);
		}
	}

	public void MoveLeft(bool Select)
	{
		CropPosition();
		Position.X--;
		if (CurrentRow.IsCollapsedEndPart)
		{
			if (Position.X < CurrentRow.Expansion_StartChar)
			{
				if (CurrentRow.Expansion_StartRow.Index == -1)
				{
					Debugger.Break();
				}
				Position.Y = CurrentRow.Expansion_StartRow.Index;
				Position.X = CurrentRow.Expansion_StartRow.Expansion_EndChar;
				CropPosition();
			}
			RememberXPos();
			CaretMoved(Select);
			return;
		}
		if (Position.X < 0)
		{
			if (Position.Y > 0)
			{
				MoveUp(Select);
				CropPosition();
				Row currentRow = CurrentRow;
				Position.X = currentRow.Text.Length;
				if (CurrentRow.IsCollapsed)
				{
					Position.Y = CurrentRow.Expansion_EndRow.Index;
					Position.X = CurrentRow.Text.Length;
				}
			}
			else
			{
				CropPosition();
			}
		}
		RememberXPos();
		CaretMoved(Select);
	}

	public void MoveHome(bool Select)
	{
		CropPosition();
		if (CurrentRow.IsCollapsedEndPart)
		{
			Position.Y = CurrentRow.Expansion_StartRow.Index;
			MoveHome(Select);
			return;
		}
		int length = CurrentRow.GetLeadingWhitespace().Length;
		Position.X = ((Position.X != length) ? length : 0);
		RememberXPos();
		CaretMoved(Select);
	}

	public void MoveEnd(bool Select)
	{
		if (CurrentRow.IsCollapsed)
		{
			Position.Y = CurrentRow.Expansion_EndRow.Index;
			MoveEnd(Select);
			return;
		}
		CropPosition();
		Row currentRow = CurrentRow;
		Position.X = currentRow.Text.Length;
		RememberXPos();
		CaretMoved(Select);
	}

	public void CaretMoved(bool Select)
	{
		Control.ScrollIntoView();
		if (!Select)
		{
			Control.Selection.ClearSelection();
		}
		else
		{
			Control.Selection.MakeSelection();
		}
	}

	public void MoveAbsoluteHome(bool Select)
	{
		Position.X = 0;
		Position.Y = 0;
		RememberXPos();
		CaretMoved(Select);
	}

	public void MoveAbsoluteEnd(bool Select)
	{
		Position.X = Control.Document[Control.Document.Count - 1].Text.Length;
		Position.Y = Control.Document.Count - 1;
		RememberXPos();
		CaretMoved(Select);
	}

	public Span CurrentSegment()
	{
		return Control.Document.GetSegmentFromPos(Position);
	}

	public void SetPos(TextPoint pos)
	{
		Position = pos;
		RememberXPos();
	}
}
