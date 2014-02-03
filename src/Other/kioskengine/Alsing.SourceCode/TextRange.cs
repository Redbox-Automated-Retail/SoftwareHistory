using System;

namespace Alsing.SourceCode;

public class TextRange
{
	private int firstRow;

	private int firstColumn;

	private int lastRow;

	private int lastColumn;

	public int FirstRow
	{
		get
		{
			return firstRow;
		}
		set
		{
			firstRow = value;
			OnChange();
		}
	}

	public int FirstColumn
	{
		get
		{
			return firstColumn;
		}
		set
		{
			firstColumn = value;
			OnChange();
		}
	}

	public int LastRow
	{
		get
		{
			return lastRow;
		}
		set
		{
			lastRow = value;
			OnChange();
		}
	}

	public int LastColumn
	{
		get
		{
			return lastColumn;
		}
		set
		{
			lastColumn = value;
			OnChange();
		}
	}

	public event EventHandler Change;

	public TextRange()
	{
	}

	public TextRange(int firstColumn, int firstRow, int lastColumn, int lastRow)
	{
		this.firstColumn = firstColumn;
		this.firstRow = firstRow;
		this.lastColumn = lastColumn;
		this.lastRow = lastRow;
	}

	protected virtual void OnChange()
	{
		if (Change != null)
		{
			Change(this, EventArgs.Empty);
		}
	}

	public void SetBounds(int firstColumn, int firstRow, int lastColumn, int lastRow)
	{
		this.firstColumn = firstColumn;
		this.firstRow = firstRow;
		this.lastColumn = lastColumn;
		this.lastRow = lastRow;
		OnChange();
	}
}
