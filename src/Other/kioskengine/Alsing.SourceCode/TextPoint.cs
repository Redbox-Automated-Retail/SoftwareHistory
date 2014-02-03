using System;

namespace Alsing.SourceCode;

public class TextPoint
{
	private int x;

	private int y;

	public int X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
			OnChange();
		}
	}

	public int Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
			OnChange();
		}
	}

	public event EventHandler Change;

	public TextPoint()
	{
	}

	public TextPoint(int X, int Y)
	{
		this.X = X;
		this.Y = Y;
	}

	private void OnChange()
	{
		if (Change != null)
		{
			Change(this, new EventArgs());
		}
	}
}
