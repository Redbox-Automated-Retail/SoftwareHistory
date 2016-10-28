using System.Drawing;
using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public sealed class Word
{
	public Color ErrorColor = Color.Red;

	public bool HasError;

	public string InfoTip;

	public Pattern Pattern;

	public Row Row;

	public Span Span;

	public TextStyle Style;

	public string Text;

	public WordType Type;

	public int Index => Row.IndexOf(this);

	public int Column
	{
		get
		{
			int num = 0;
			foreach (Word item in Row)
			{
				if (item == this)
				{
					return num;
				}
				num += item.Text.Length;
			}
			return -1;
		}
	}
}
